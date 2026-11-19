using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("EarFlip")]
[assembly: AssemblyDescription("Tiny Windows tray tool that flips audio between speakers and Bluetooth headphones with one hotkey")]
[assembly: AssemblyCompany("Ali Alhumedi")]
[assembly: AssemblyProduct("EarFlip")]
[assembly: AssemblyCopyright("Copyright (c) 2026 Ali Alhumedi")]
[assembly: AssemblyVersion("1.2.0.0")]
[assembly: AssemblyFileVersion("1.2.0.0")]
[assembly: AssemblyInformationalVersion("1.2.0")]

static class Devices
{
    // picked by the user and kept in HKCU\Software\EarFlip: nothing is compiled in any more
    public static string Speakers;
    public static string Buds;
    static string speakersName;
    static string budsName;

    const int Render = 0;
    const int Multimedia = 1;
    const int StateActive = 1;
    const int StateUnplugged = 8;

    public sealed class Endpoint
    {
        public string Id;
        public string Name;
        public bool Active;
        public bool Bluetooth;
        public bool HandsFree;
    }

    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    class MMDeviceEnumerator { }

    [ComImport, Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceEnumerator
    {
        int EnumAudioEndpoints(int flow, int stateMask, [MarshalAs(UnmanagedType.Interface)] out IMMDeviceCollection collection);
        int GetDefaultAudioEndpoint(int flow, int role, [MarshalAs(UnmanagedType.Interface)] out IMMDevice device);
        int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, [MarshalAs(UnmanagedType.Interface)] out IMMDevice device);
        int RegisterEndpointNotificationCallback(IntPtr client);
        int UnregisterEndpointNotificationCallback(IntPtr client);
    }

    [ComImport, Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceCollection
    {
        int GetCount(out int count);
        int Item(int index, [MarshalAs(UnmanagedType.Interface)] out IMMDevice device);
    }

    [ComImport, Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDevice
    {
        int Activate(ref Guid iid, int clsCtx, IntPtr activationParams, [MarshalAs(UnmanagedType.IUnknown)] out object iface);
        int OpenPropertyStore(int access, out IntPtr store);
        int GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);
        int GetState(out int state);
    }

    [ComImport, Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
    class PolicyConfigClient { }

    [ComImport, Guid("F8679F50-850A-41CF-9C72-430F290290C8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPolicyConfig
    {
        // vtable order matters: SetDefaultEndpoint is the 11th slot, so the 10 before it are declared as placeholders
        int GetMixFormat([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr format);
        int GetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string id, int isDefault, IntPtr format);
        int ResetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string id);
        int SetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr endpoint, IntPtr mix);
        int GetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string id, int isDefault, IntPtr a, IntPtr b);
        int SetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr value);
        int GetShareMode([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr mode);
        int SetShareMode([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr mode);
        int GetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr key, IntPtr value);
        int SetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr key, IntPtr value);
        int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string id, int role);
        int SetEndpointVisibility([MarshalAs(UnmanagedType.LPWStr)] string id, int visible);
    }

    public static string CurrentId()
    {
        var en = (IMMDeviceEnumerator)new MMDeviceEnumerator();
        try
        {
            IMMDevice dev;
            if (en.GetDefaultAudioEndpoint(Render, Multimedia, out dev) != 0 || dev == null) return null;
            try
            {
                string id;
                return dev.GetId(out id) == 0 ? id : null;
            }
            finally { Marshal.ReleaseComObject(dev); }
        }
        catch (COMException) { return null; }
        finally { Marshal.ReleaseComObject(en); }
    }

    public static bool IsActive(string id)
    {
        var en = (IMMDeviceEnumerator)new MMDeviceEnumerator();
        try
        {
            IMMDevice dev;
            if (en.GetDevice(id, out dev) != 0 || dev == null) return false;
            try
            {
                int state;
                return dev.GetState(out state) == 0 && state == StateActive;
            }
            finally { Marshal.ReleaseComObject(dev); }
        }
        catch (COMException) { return false; }
        finally { Marshal.ReleaseComObject(en); }
    }

    public static void SetDefault(string id)
    {
        var policy = (IPolicyConfig)new PolicyConfigClient();
        try
        {
            for (int role = 0; role <= 2; role++) policy.SetDefaultEndpoint(id, role);
        }
        finally { Marshal.ReleaseComObject(policy); }
    }

    // active plus unplugged, so a paired-but-disconnected Bluetooth headset still shows up
    public static List<Endpoint> List()
    {
        var list = new List<Endpoint>();
        var en = (IMMDeviceEnumerator)new MMDeviceEnumerator();
        IMMDeviceCollection col = null;
        try
        {
            if (en.EnumAudioEndpoints(Render, StateActive | StateUnplugged, out col) != 0 || col == null) return list;
            int count;
            if (col.GetCount(out count) != 0) return list;
            for (int i = 0; i < count; i++)
            {
                IMMDevice dev;
                if (col.Item(i, out dev) != 0 || dev == null) continue;
                try
                {
                    string id;
                    if (dev.GetId(out id) != 0 || string.IsNullOrEmpty(id)) continue;
                    int state;
                    bool bluetooth, handsFree;
                    BluetoothFlags(id, out bluetooth, out handsFree);
                    list.Add(new Endpoint
                    {
                        Id = id,
                        Name = NameOf(id),
                        Active = dev.GetState(out state) == 0 && state == StateActive,
                        Bluetooth = bluetooth,
                        HandsFree = handsFree
                    });
                }
                finally { Marshal.ReleaseComObject(dev); }
            }
            return list;
        }
        catch (COMException) { return list; }
        finally
        {
            if (col != null) Marshal.ReleaseComObject(col);
            Marshal.ReleaseComObject(en);
        }
    }

    public static void Load()
    {
        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\EarFlip"))
        {
            if (key != null)
            {
                Speakers = key.GetValue("Speakers") as string;
                speakersName = key.GetValue("SpeakersName") as string;
                Buds = key.GetValue("Buds") as string;
                budsName = key.GetValue("BudsName") as string;
            }
        }
        Ensure();
    }

    public static void Save()
    {
        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\EarFlip"))
        {
            key.SetValue("Speakers", Speakers ?? "", RegistryValueKind.String);
            key.SetValue("SpeakersName", NameOf(Speakers), RegistryValueKind.String);
            key.SetValue("Buds", Buds ?? "", RegistryValueKind.String);
            key.SetValue("BudsName", NameOf(Buds), RegistryValueKind.String);
        }
    }

    // fills an empty slot (first run) or one whose saved device Windows no longer lists
    public static void Ensure()
    {
        List<Endpoint> list = List();

        // a re-paired headset gets a new id: match it back by its saved name
        string speakers = Resolve(list, Speakers, speakersName);
        string buds = Resolve(list, Buds, budsName);

        if (speakers == null)
        {
            Endpoint current = Find(list, CurrentId());
            if (current != null && current.Active && !current.Bluetooth) speakers = current.Id;
            if (speakers == null)
            {
                Endpoint first = list.Find(delegate (Endpoint e) { return e.Active && !e.Bluetooth; });
                if (first != null) speakers = first.Id;
            }
        }
        if (buds == null)
        {
            Endpoint first = list.Find(delegate (Endpoint e) { return e.Bluetooth && !e.HandsFree; });
            if (first != null) buds = first.Id;
        }

        if (speakers == Speakers && buds == Buds) return;
        Speakers = speakers;
        Buds = buds;
        Save();
    }

    static Endpoint Find(List<Endpoint> list, string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return list.Find(delegate (Endpoint e) { return e.Id == id; });
    }

    static string Resolve(List<Endpoint> list, string id, string savedName)
    {
        if (Find(list, id) != null) return id;
        if (string.IsNullOrEmpty(savedName)) return null;
        Endpoint byName = list.Find(delegate (Endpoint e) { return e.Name == savedName; });
        return byName == null ? null : byName.Id;
    }

    static RegistryKey PropertiesKey(string id)
    {
        if (id == null) return null;
        int brace = id.LastIndexOf('{');
        if (brace < 0) return null;
        string key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Render\" + id.Substring(brace) + @"\Properties";
        return Registry.LocalMachine.OpenSubKey(key);
    }

    // Windows stores the device interface path in the properties: BTHENUM is A2DP, BTHHFENUM is hands-free
    static void BluetoothFlags(string id, out bool bluetooth, out bool handsFree)
    {
        bluetooth = false;
        handsFree = false;
        using (RegistryKey props = PropertiesKey(id))
        {
            if (props == null) return;
            foreach (string name in props.GetValueNames())
            {
                string value = props.GetValue(name) as string;
                if (value == null) continue;
                if (value.IndexOf("BTHHFENUM", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    handsFree = true;
                    bluetooth = true;
                }
                else if (value.IndexOf("BTHENUM", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    bluetooth = true;
                }
            }
        }
    }

    public static string NameOf(string id)
    {
        using (RegistryKey k = PropertiesKey(id))
        {
            if (k != null)
            {
                string friendly = k.GetValue("{a45c254e-df1c-4efd-8020-67d146a850e0},2") as string;
                string desc = k.GetValue("{b3f8fa53-0004-438e-9003-51a46e139bfc},6") as string;
                if (!string.IsNullOrEmpty(desc) && desc != friendly) return friendly + " (" + desc + ")";
                if (!string.IsNullOrEmpty(friendly)) return friendly;
                if (!string.IsNullOrEmpty(desc)) return desc;
            }
        }
        return "EarFlip";
    }
}

static class Art
{
    static readonly Color HeadsetColor = Color.FromArgb(0, 162, 255);
    static readonly Color SpeakerColor = Color.FromArgb(255, 148, 26);

    public static Bitmap Headphones(int size)
    {
        var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(HeadsetColor, Math.Max(1.5f, size * 0.14f)))
                g.DrawArc(pen, size * 0.14f, size * 0.10f, size * 0.72f, size * 0.72f, 180, 180);
            using (var brush = new SolidBrush(HeadsetColor))
            using (GraphicsPath left = RoundedRect(size * 0.03f, size * 0.44f, size * 0.25f, size * 0.42f, size * 0.09f))
            using (GraphicsPath right = RoundedRect(size * 0.72f, size * 0.44f, size * 0.25f, size * 0.42f, size * 0.09f))
            {
                g.FillPath(brush, left);
                g.FillPath(brush, right);
            }
        }
        return bmp;
    }

    public static Bitmap Speaker(int size)
    {
        var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new SolidBrush(SpeakerColor))
            {
                var cone = new[]
                {
                    new PointF(size * 0.06f, size * 0.36f),
                    new PointF(size * 0.24f, size * 0.36f),
                    new PointF(size * 0.50f, size * 0.10f),
                    new PointF(size * 0.50f, size * 0.90f),
                    new PointF(size * 0.24f, size * 0.64f),
                    new PointF(size * 0.06f, size * 0.64f)
                };
                g.FillPolygon(brush, cone);
            }
            using (var pen = new Pen(SpeakerColor, Math.Max(1.5f, size * 0.11f)))
            {
                g.DrawArc(pen, size * 0.34f, size * 0.26f, size * 0.44f, size * 0.48f, -55, 110);
                g.DrawArc(pen, size * 0.30f, size * 0.12f, size * 0.66f, size * 0.76f, -50, 100);
            }
        }
        return bmp;
    }

    public static Icon ToIcon(Bitmap bmp)
    {
        IntPtr handle = bmp.GetHicon();
        try
        {
            using (Icon raw = Icon.FromHandle(handle)) return (Icon)raw.Clone();
        }
        finally { DestroyIcon(handle); }
    }

    static GraphicsPath RoundedRect(float x, float y, float w, float h, float r)
    {
        var path = new GraphicsPath();
        path.AddArc(x, y, r * 2, r * 2, 180, 90);
        path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
        path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
        path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
        path.CloseFigure();
        return path;
    }

    [DllImport("user32.dll")]
    static extern bool DestroyIcon(IntPtr handle);
}

sealed class TrayApp : Form
{
    const int HotkeyId = 1;
    const int WmHotkey = 0x0312;
    const int ModControlAlt = 0x0003;
    const int ModNoRepeat = 0x4000;
    const int VkF12 = 0x7B;
    [DllImport("user32.dll")]
    static extern bool RegisterHotKey(IntPtr hwnd, int id, int modifiers, int key);
    [DllImport("user32.dll")]
    static extern bool UnregisterHotKey(IntPtr hwnd, int id);

    readonly NotifyIcon tray = new NotifyIcon();
    readonly System.Windows.Forms.Timer poll = new System.Windows.Forms.Timer();
    readonly ToolStripMenuItem speakersItem = new ToolStripMenuItem("سماعة الجهاز");
    readonly ToolStripMenuItem budsItem = new ToolStripMenuItem("سماعة البلوتوث");
    ToolStripMenuItem startupItem;
    Icon currentIcon;
    string shownId;
    int hotMods = ModControlAlt;
    int hotKey = VkF12;

    public TrayApp()
    {
        ShowInTaskbar = false;
        LoadHotkey();
        Devices.Load();
        EnsureStartupLink();

        startupItem = new ToolStripMenuItem("التشغيل عند بدء ويندوز", null, delegate { ToggleStartup(); });
        startupItem.Checked = File.Exists(StartupLink);

        var menu = new ContextMenuStrip();
        menu.Items.Add(speakersItem);
        menu.Items.Add(budsItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("تبديل الاختصار", null, delegate { ChangeHotkey(); });
        menu.Items.Add(startupItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("خروج", null, delegate { Quit(); });
        menu.Opening += delegate { FillDevices(); };
        tray.ContextMenuStrip = menu;

        // left click flips, right click still opens the menu
        tray.MouseClick += delegate (object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) Toggle();
        };

        RefreshTray();
        tray.Visible = true;

        RegisterHotKey(Handle, HotkeyId, hotMods | ModNoRepeat, hotKey);

        poll.Interval = 2000;
        // shortcut: polling بدل IMMNotificationClient، يُرقّى إن ظهر بطء
        poll.Tick += delegate { RefreshTray(); };
        poll.Start();
    }

    // tray-only app: the message loop needs a window, but the window must never show
    protected override void SetVisibleCore(bool value)
    {
        if (!IsHandleCreated) CreateHandle();
        base.SetVisibleCore(false);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotkey && m.WParam.ToInt32() == HotkeyId) Toggle();
        base.WndProc(ref m);
    }

    void Toggle()
    {
        if (Devices.Speakers == null || Devices.Buds == null) Devices.Ensure();

        string id = Devices.CurrentId();
        string target;
        if (id == Devices.Buds) target = Devices.Speakers;
        else if (Devices.Buds != null && Devices.IsActive(Devices.Buds)) target = Devices.Buds;
        else target = id == Devices.Speakers ? null : Devices.Speakers;

        if (target == null) return; // silent on purpose: the app must never make a sound
        Devices.SetDefault(target);
        RefreshTray();
    }

    void FillDevices()
    {
        FillDeviceMenu(speakersItem, false, Devices.Speakers);
        FillDeviceMenu(budsItem, true, Devices.Buds);
    }

    void FillDeviceMenu(ToolStripMenuItem parent, bool bluetooth, string selected)
    {
        parent.DropDownItems.Clear();
        int added = 0;
        foreach (Devices.Endpoint e in Devices.List())
        {
            if (e.Bluetooth != bluetooth) continue;
            string id = e.Id;
            var item = new ToolStripMenuItem(e.Name + (e.Active ? "" : " (غير متصل)"), null,
                delegate { ChooseDevice(bluetooth, id); });
            item.Checked = id == selected;
            parent.DropDownItems.Add(item);
            added++;
        }
        if (added == 0)
        {
            var none = new ToolStripMenuItem("لا توجد أجهزة");
            none.Enabled = false;
            parent.DropDownItems.Add(none);
        }
    }

    void ChooseDevice(bool buds, string id)
    {
        if (buds) Devices.Buds = id;
        else Devices.Speakers = id;
        Devices.Save();
        shownId = null; // force the icon and the tooltip to follow the new pair
        RefreshTray();
    }

    void RefreshTray()
    {
        string id = Devices.CurrentId();
        if (id == shownId) return;
        shownId = id;

        int size = Math.Max(16, SystemInformation.SmallIconSize.Width);
        using (Bitmap bmp = id == Devices.Buds ? Art.Headphones(size) : Art.Speaker(size))
        {
            Icon icon = Art.ToIcon(bmp);
            if (currentIcon != null) currentIcon.Dispose();
            currentIcon = icon;
            tray.Icon = icon;
        }

        string text = Devices.NameOf(id);
        tray.Text = text.Length > 63 ? text.Substring(0, 63) : text;
    }

    void Quit()
    {
        UnregisterHotKey(Handle, HotkeyId);
        poll.Stop();
        tray.Visible = false;
        tray.Dispose();
        if (currentIcon != null) currentIcon.Dispose();
        Application.Exit();
    }

    void ChangeHotkey()
    {
        using (var dialog = new HotkeyForm(hotMods, hotKey))
        {
            if (dialog.ShowDialog() != DialogResult.OK) return;

            UnregisterHotKey(Handle, HotkeyId);
            if (RegisterHotKey(Handle, HotkeyId, dialog.Mods | ModNoRepeat, dialog.Key))
            {
                hotMods = dialog.Mods;
                hotKey = dialog.Key;
                SaveHotkey();
            }
            else
            {
                // someone else owns that combo: keep the old one instead of running with none
                RegisterHotKey(Handle, HotkeyId, hotMods | ModNoRepeat, hotKey);
            }
        }
    }

    static string StartupLink
    {
        get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "EarFlip.lnk"); }
    }

    // repair a shortcut Windows can no longer launch (folder moved, or an old test build wrote it)
    void EnsureStartupLink()
    {
        string link = StartupLink;
        if (!File.Exists(link)) return; // no shortcut means the user turned it off
        try
        {
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            object shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null,
                Activator.CreateInstance(shellType), new object[] { link });
            object target = shortcut.GetType().InvokeMember("TargetPath", BindingFlags.GetProperty, null, shortcut, null);
            if (string.Equals(target as string, Application.ExecutablePath, StringComparison.OrdinalIgnoreCase)) return;
        }
        catch
        {
            return;
        }
        try
        {
            CreateStartupLink();
        }
        catch
        {
            // silent on purpose: the app must never make a sound
        }
    }

    static void CreateStartupLink()
    {
        string link = StartupLink;
        // WScript.Shell via late binding keeps the build reference-free
        Type shellType = Type.GetTypeFromProgID("WScript.Shell");
        object shell = Activator.CreateInstance(shellType);
        object shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { link });
        Type shortcutType = shortcut.GetType();
        shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, new object[] { Application.ExecutablePath });
        shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, new object[] { Path.GetDirectoryName(Application.ExecutablePath) });
        shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
    }

    void ToggleStartup()
    {
        try
        {
            string link = StartupLink;
            if (File.Exists(link))
            {
                File.Delete(link);
            }
            else
            {
                CreateStartupLink();
            }
        }
        catch (Exception)
        {
            // silent on purpose: the app must never make a sound
        }
        startupItem.Checked = File.Exists(StartupLink);
    }

    void LoadHotkey()
    {
        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\EarFlip"))
        {
            if (key == null) return;
            object mods = key.GetValue("Modifiers");
            object vk = key.GetValue("Key");
            if (mods is int && vk is int)
            {
                hotMods = (int)mods;
                hotKey = (int)vk;
            }
        }
    }

    void SaveHotkey()
    {
        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\EarFlip"))
        {
            key.SetValue("Modifiers", hotMods, RegistryValueKind.DWord);
            key.SetValue("Key", hotKey, RegistryValueKind.DWord);
        }
    }
}

sealed class HotkeyForm : Form
{
    public int Mods;
    public int Key;

    public HotkeyForm(int mods, int key)
    {
        Mods = mods;
        Key = key;
        Text = "EarFlip";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MinimizeBox = false;
        MaximizeBox = false;
        TopMost = true;
        ClientSize = new Size(320, 80);
        Controls.Add(new Label
        {
            Text = "اضغط الاختصار الجديد (Esc للإلغاء)",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        });
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        Keys key = keyData & Keys.KeyCode;
        if (key == Keys.Escape)
        {
            DialogResult = DialogResult.Cancel;
            Close();
            return true;
        }
        if (key == Keys.ControlKey || key == Keys.Menu || key == Keys.ShiftKey || key == Keys.LWin || key == Keys.RWin)
            return true;

        Keys held = keyData & Keys.Modifiers;
        if (held == Keys.None) return true; // a bare key would shut normal typing out of the whole desktop

        Mods = 0;
        if ((held & Keys.Control) != 0) Mods |= 0x0002;
        if ((held & Keys.Alt) != 0) Mods |= 0x0001;
        if ((held & Keys.Shift) != 0) Mods |= 0x0004;
        Key = (int)key;
        DialogResult = DialogResult.OK;
        Close();
        return true;
    }
}

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--make-icon")
        {
            MakeIcon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EarFlip.ico"));
            return;
        }

        bool isFirst;
        using (var mutex = new Mutex(true, @"Local\EarFlip", out isFirst))
        {
            if (!isFirst) return;
            Application.EnableVisualStyles();
            Application.Run(new TrayApp());
        }
    }

    static void MakeIcon(string path)
    {
        int[] sizes = { 16, 24, 32, 48, 256 };
        var pngs = new byte[sizes.Length][];
        for (int i = 0; i < sizes.Length; i++)
        {
            using (Bitmap bmp = Art.Headphones(sizes[i]))
            using (var buffer = new MemoryStream())
            {
                bmp.Save(buffer, ImageFormat.Png);
                pngs[i] = buffer.ToArray();
            }
        }

        using (var stream = File.Create(path))
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write((short)0);
            writer.Write((short)1);
            writer.Write((short)sizes.Length);

            int offset = 6 + 16 * sizes.Length;
            for (int i = 0; i < sizes.Length; i++)
            {
                int size = sizes[i];
                writer.Write((byte)(size >= 256 ? 0 : size));
                writer.Write((byte)(size >= 256 ? 0 : size));
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((short)1);
                writer.Write((short)32);
                writer.Write(pngs[i].Length);
                writer.Write(offset);
                offset += pngs[i].Length;
            }

            foreach (byte[] png in pngs) writer.Write(png);
        }
    }
}
