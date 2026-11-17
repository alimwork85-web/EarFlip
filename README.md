# EarFlip

<img src="docs/icon.png" width="96" align="left" alt="EarFlip icon" />

One hotkey to flip your Windows audio between speakers and Bluetooth headphones.
A tiny replacement for SoundSwitch: no installer, no notifications, no sound, no settings window.

<br clear="left" />

[![Release](https://img.shields.io/github/v/release/alimwork85-web/EarFlip?label=release&color=0aa2ff)](https://github.com/alimwork85-web/EarFlip/releases/latest)
[![Build](https://github.com/alimwork85-web/EarFlip/actions/workflows/build.yml/badge.svg)](https://github.com/alimwork85-web/EarFlip/actions/workflows/build.yml)
![Platform](https://img.shields.io/badge/platform-Windows%2011%20%7C%2010-lightgrey)
![Size](https://img.shields.io/badge/exe-24.5%20KB-brightgreen)
![RAM](https://img.shields.io/badge/RAM-~39%20MB-yellow)
![CPU](https://img.shields.io/badge/CPU-idle%20%E2%89%A40.01%25-orange)

---

## Features

- **`Ctrl + Alt + F12`** switches the default playback device between speakers and your Bluetooth headphones — all three Windows roles (console, multimedia, communications) at once.
- **Tray icon that tells the truth:** a blue headset when the headphones are the active output, an orange speaker otherwise. Hover to see the current device name.
- **Silent by design.** No balloon, no beep, no toast. If the headphones are disconnected, the hotkey simply does nothing.
- **Watches Windows too:** change the device from Settings and the icon follows within 2 seconds.
- **Right-click menu:** *Change shortcut* · *Run at Windows startup* · *Exit*.
- **Single instance**, and the hotkey is released on exit.
- **One 24.5 KB exe**, no framework install, no dependencies, no registry clutter (only your chosen shortcut is saved, under `HKCU\Software\EarFlip`).

## Screenshots

| Tray menu | Change shortcut |
|---|---|
| <img src="docs/menu.png" width="260" alt="EarFlip tray menu" /> | <img src="docs/hotkey-dialog.png" width="360" alt="Change shortcut dialog" /> |

| Headphones active | Speakers active |
|---|---|
| <img src="docs/icon.png" width="96" alt="headset icon" /> | <img src="docs/icon-speaker.png" width="96" alt="speaker icon" /> |

## Resource usage

Measured on Windows 11 with the released `EarFlip.exe` (1.2.0):

| Metric | Value |
|---|---|
| Executable size | **24.5 KB** (25,088 bytes) |
| Working set (RAM) | **~39 MB** (private ~24 MB) |
| CPU while idle | **0.00 s over 15 s** of sampling — below 0.01% of one core |
| Threads | 13 |
| Background polling | one 2000 ms timer, one COM call per tick |
| Dependencies | none — in-box .NET Framework 4.x only |

## Install

1. Download `EarFlip.exe` from the [latest release](../../releases/latest).
2. Put it anywhere and run it once. It lives in the notification area and starts with Windows (tick *Run at Windows startup* in the tray menu).
3. Press `Ctrl + Alt + F12`.

Windows SmartScreen may warn about an unsigned exe — *More info → Run anyway*.

## Usage

| Action | Result |
|---|---|
| `Ctrl + Alt + F12` | Switch output: speakers ⇄ Bluetooth headphones |
| Tray icon (hover) | Name of the current output device |
| Right-click → *Change shortcut* | Press the new combination (needs Ctrl/Alt/Shift); Esc cancels |
| Right-click → *Run at Windows startup* | Tick/untick autostart (a shortcut in your Startup folder; a stale one is rebuilt on the next launch) |
| Right-click → *Exit* | Quits and releases the hotkey |

## Build from source

The build uses the C# compiler that already ships with Windows — no SDK, no NuGet.

```cmd
build.cmd
```

It compiles `EarFlip.cs` → runs `EarFlip.exe --make-icon` (writes `EarFlip.ico` from the same GDI+ routine the tray uses) → recompiles with `/win32icon`. CI does the same thing on every tag and attaches the exe to the release.

## Notes

- The two device IDs are compiled in (they are this machine's speakers and headphones). Pointing it at another pair means editing the two constants at the top of `EarFlip.cs` and rebuilding.
- Global polling instead of `IMMNotificationClient` is a deliberate shortcut; it costs one COM call every 2 seconds.
- Version resources: *Properties → Details* on the exe shows **Ali Alhumedi** as company/publisher and the current version.

## License

Released by **Ali Alhumedi**. Free to use and modify; keep the credit.

`#EarFlip` `#Windows` `#CSharp` `#WinForms` `#TrayApp` `#AudioSwitcher` `#Bluetooth` `#Productivity` `#OpenSource` `#TinyTools` `#NoDependencies` `#SilentByDesign`

---

<div dir="rtl">

## بالعربية

**EarFlip** أداة صغيرة لـ Windows وظيفتها واحدة: اختصار `Ctrl + Alt + F12` يبدّل الصوت بين مكبّر صوت الجهاز وسماعة البلوتوث.

### المزايا
- تبديل الجهاز الافتراضي للأدوار الثلاثة (console وmultimedia وcommunications) بضغطة واحدة.
- أيقونة في شريط النظام تتبع الحالة: **سماعة زرقاء** حين تكون السماعة هي المخرج، و**مكبّر برتقالي** في غير ذلك، واسم الجهاز الحالي عند التمرير.
- **صامت تمامًا:** بلا إشعارات وبلا صوت. إن كانت السماعة مفصولة لا يحدث شيء.
- يتابع تغييرك للجهاز من إعدادات Windows ويحدّث الأيقونة خلال ثانيتين.
- قائمة يمين: **تبديل الاختصار** · **التشغيل عند بدء ويندوز** · **خروج**.
- نسخة واحدة فقط تعمل، والاختصار يُحرَّر عند الخروج.

### الحجم والاستهلاك
| القياس | القيمة |
|---|---|
| حجم الملف التنفيذي | **24.5 كيلوبايت** |
| الذاكرة عند التشغيل | **~39 ميجابايت** |
| المعالج في وضع السكون | **أقل من 0.01%** من نواة واحدة |
| الاعتماديات | لا شيء — .NET Framework الموجود في Windows |

### التركيب
1. حمّل `EarFlip.exe` من [أحدث إصدار](../../releases/latest).
2. شغّله مرة واحدة، وفعّل **التشغيل عند بدء ويندوز** من قائمة الأيمن إن أردته دائمًا.
3. اضغط `Ctrl + Alt + F12`.

### البناء من المصدر
```cmd
build.cmd
```
يستخدم `csc.exe` الموجود في Windows؛ لا SDK ولا حزم.

### ملاحظة
معرّفا الجهازين مضمّنان في الكود (مكبّر وسماعة هذا الجهاز). لتغييرهما عدّل الثابتين في أعلى `EarFlip.cs` وأعد البناء.

</div>

<sub>Made by Ali Alhumedi · 2026</sub>
