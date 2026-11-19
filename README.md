<div align="center">

<img src="docs/icon.png" width="112" alt="EarFlip icon" />

# EarFlip

**One hotkey or one click flips your Windows audio between speakers and headphones.**<br/>
Tiny, silent, no installer, no settings window.

[![Release](https://img.shields.io/github/v/release/alimwork85-web/EarFlip?label=release&color=0aa2ff)](https://github.com/alimwork85-web/EarFlip/releases/latest)
[![Build](https://github.com/alimwork85-web/EarFlip/actions/workflows/build.yml/badge.svg)](https://github.com/alimwork85-web/EarFlip/actions/workflows/build.yml)
![Platform](https://img.shields.io/badge/platform-Windows%2011%20%7C%2010-lightgrey)
![Size](https://img.shields.io/badge/exe-34.5%20KB-brightgreen)
![RAM](https://img.shields.io/badge/RAM-~47%20MB-yellow)
![CPU](https://img.shields.io/badge/CPU-idle%20%E2%89%A40.01%25-orange)

**English** · [العربية](#العربية)

</div>

---

## Features

- **Flip in one move.** `Ctrl + Alt + F12` or a left click on the tray icon switches the default output for all three Windows roles (console, multimedia, communications).
- **Works on any PC.** On first run EarFlip picks your current output and your first Bluetooth headset. Change either one from the tray menu; every output is listed, USB headsets and HDMI included.
- **Battery at a glance.** Hover the icon to see the device and its battery, e.g. `Headphones (Buds) · 66%`. A red dot appears on the icon below 20%.
- **An icon that tells the truth.** Blue headset when headphones play, orange speaker otherwise.
- **Silent by design.** No sound, no toast. If the headphones are off, the icon blinks grey three times and the tooltip says why.
- **Follows Windows instantly** through device notifications, with no polling.
- **Speaks your language.** The menu is Arabic on Arabic Windows, English everywhere else.
- **Remembers your choice.** A re-paired headset is found again by its name, and an unplugged USB or HDMI device keeps its slot.

## Screenshots

| Tray menu | Change shortcut |
|---|---|
| <img src="docs/menu.png" width="260" alt="EarFlip tray menu" /> | <img src="docs/hotkey-dialog.png" width="360" alt="Change shortcut dialog" /> |

| Headphones active | Speakers active |
|---|---|
| <img src="docs/icon.png" width="96" alt="headset icon" /> | <img src="docs/icon-speaker.png" width="96" alt="speaker icon" /> |

## Install

1. Download `EarFlip.exe` from the [latest release](https://github.com/alimwork85-web/EarFlip/releases/latest).
2. Put it anywhere and run it. It lives in the notification area.
3. Press `Ctrl + Alt + F12`, or left-click the icon.

Windows SmartScreen may warn about an unsigned exe: *More info → Run anyway*.

## Usage

| Action | Result |
|---|---|
| `Ctrl + Alt + F12` or left click | Flip the output |
| Hover the icon | Current device and its battery |
| Right-click → *Speakers ▸* | Choose the first device |
| Right-click → *Bluetooth headphones ▸* | Choose the second device |
| Right-click → *Change shortcut* | Press a new combination; Esc cancels |
| Right-click → *Run at Windows startup* | Turn autostart on or off |
| Right-click → *Exit* | Quit and release the hotkey |

## Resource usage

Measured on Windows 11 with `EarFlip.exe` 1.4.0:

| Metric | Value |
|---|---|
| Executable size | **34.5 KB** (35,328 bytes) |
| Working set (RAM) | **~47 MB** (private ~26 MB) |
| CPU while idle | **0.00 s over 15 s**, below 0.01% of one core |
| Threads | 15 |
| Background work | device notifications; a battery check every 2 minutes (~30 ms) |
| Dependencies | none, in-box .NET Framework 4.x only |

## Build from source

Uses the C# compiler that ships with Windows. No SDK, no NuGet.

```cmd
build.cmd
```

It compiles `EarFlip.cs`, writes `EarFlip.ico` with `EarFlip.exe --make-icon`, then recompiles with the icon. GitHub Actions does the same on every tag and attaches the exe to the release.

## Notes

- Settings live in `HKCU\Software\EarFlip`: the two devices (`Speakers`, `SpeakersName`, `Buds`, `BudsName`) and the shortcut (`Modifiers`, `Key`). Delete the key to start fresh.
- The battery is the level Windows itself shows in Settings. Most Bluetooth headsets report it; USB dongle headsets usually don't, and then only the name is shown.
- If Windows ever refuses the device callback, EarFlip falls back to checking every 2 seconds.

## License

© 2026 **Ali Alhumedi**. Free to use and modify; keep the credit.

`#EarFlip` `#Windows` `#CSharp` `#TrayApp` `#AudioSwitcher` `#Bluetooth` `#Headphones` `#Battery` `#Productivity` `#OpenSource` `#SoundSwitchAlternative` `#Arabic`

---

<div dir="rtl">

## العربية

**تبديل الصوت في ويندوز بين مكبّر الجهاز والسماعة باختصار واحد أو نقرة واحدة.**<br/>
صغير وصامت، بلا مثبّت وبلا نافذة إعدادات.

### المزايا

- **تبديل بحركة واحدة:** `Ctrl + Alt + F12` أو نقرة يسرى على الأيقونة تغيّر جهاز الإخراج الافتراضي للأدوار الثلاثة في ويندوز.
- **يعمل على أي جهاز:** في أول تشغيل يختار EarFlip المخرج الحالي وأول سماعة بلوتوث. يمكنك تغيير أي منهما من القائمة، وكل أجهزة الإخراج تظهر فيها ومنها سماعات USB وHDMI.
- **البطارية بنظرة:** مرّر المؤشر على الأيقونة لترى الجهاز ونسبة بطاريته، مثل `Headphones (Buds) · 66%`. وتظهر نقطة حمراء على الأيقونة تحت 20%.
- **أيقونة صادقة:** سماعة زرقاء حين تعمل السماعة، ومكبّر برتقالي في غير ذلك.
- **صامت بالتصميم:** بلا صوت ولا إشعارات. إن كانت السماعة مغلقة تومض الأيقونة رمادية ثلاث مرات ويوضح التلميح السبب.
- **يتبع ويندوز فورًا** عبر إشعارات الأجهزة، بلا فحص دوري.
- **بلغتك:** القائمة عربية على ويندوز العربي، وإنجليزية في غيره.
- **يحفظ اختيارك:** السماعة المعاد إقرانها يتعرف عليها باسمها، وجهاز USB أو HDMI المفصول يبقى في مكانه.

### التركيب

1. حمّل `EarFlip.exe` من [أحدث إصدار](https://github.com/alimwork85-web/EarFlip/releases/latest).
2. ضعه في أي مكان وشغّله، فيظهر في منطقة الإشعارات.
3. اضغط `Ctrl + Alt + F12` أو انقر الأيقونة بالزر الأيسر.

قد يحذّر SmartScreen من ملف غير موقّع: *مزيد من المعلومات ← التشغيل على أي حال*.

### الاستخدام

| الإجراء | النتيجة |
|---|---|
| `Ctrl + Alt + F12` أو نقرة يسرى | تبديل المخرج |
| تمرير المؤشر على الأيقونة | الجهاز الحالي ونسبة بطاريته |
| يمين ← **سماعة الجهاز ▸** | اختيار الجهاز الأول |
| يمين ← **سماعة البلوتوث ▸** | اختيار الجهاز الثاني |
| يمين ← **تبديل الاختصار** | اضغط الاختصار الجديد، وEsc للإلغاء |
| يمين ← **التشغيل عند بدء ويندوز** | تفعيل التشغيل التلقائي أو إيقافه |
| يمين ← **خروج** | إغلاق التطبيق وتحرير الاختصار |

### الحجم والاستهلاك

قيس على Windows 11 بالإصدار 1.4.0:

| القياس | القيمة |
|---|---|
| حجم الملف التنفيذي | **34.5 كيلوبايت** |
| الذاكرة | **~47 ميجابايت** (الخاصة ~26) |
| المعالج في السكون | **0.00 ثانية خلال 15 ثانية**، أقل من 0.01% من نواة |
| الخيوط | 15 |
| العمل في الخلفية | إشعارات الأجهزة، وفحص للبطارية كل دقيقتين (~30 ms) |
| الاعتماديات | لا شيء، .NET Framework الموجود في ويندوز فقط |

### البناء من المصدر

يستخدم مترجم C# الموجود في ويندوز، بلا SDK وبلا حزم.

```cmd
build.cmd
```

### ملاحظات

- الإعدادات في `HKCU\Software\EarFlip`: الجهازان (`Speakers` و`SpeakersName` و`Buds` و`BudsName`) والاختصار (`Modifiers` و`Key`). احذف المفتاح لتبدأ من جديد.
- نسبة البطارية هي التي يعرضها ويندوز نفسه في الإعدادات. أغلب سماعات البلوتوث تبلّغ عنها، أما سماعات USB ذات الدونجل فغالبًا لا، وعندها يظهر الاسم فقط.
- إن رفض ويندوز تسجيل الإشعارات يعود EarFlip إلى الفحص كل ثانيتين.

### الترخيص

© 2026 **Ali Alhumedi**. حر الاستخدام والتعديل مع إبقاء الإشارة إلى المطوّر.

</div>
