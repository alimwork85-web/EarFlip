# EarFlip: خطة التنفيذ

## الهدف
أداة صغيرة لـ Windows بوظيفة واحدة: اختصار **Ctrl+Alt+F12** يبدّل الصوت بين سماعات الجهاز وسماعة البلوتوث. هي بديل لـ SoundSwitch، الذي صار مزعجًا بإشعاراته وميزاته الكثيرة.

## الجهازان (معرّفات ثابتة داخل الكود)
| الجهاز | المعرّف |
|---|---|
| `Speakers (Senary Audio)` | `{0.0.0.00000000}.{81f5809c-6c48-4d9d-a509-6edec89d8082}` |
| `Headphones (Ali's Buds4 Pro)` | `{0.0.0.00000000}.{d96537a1-4fe0-486f-a805-6fb778567048}` |

المصدر: `%APPDATA%\SoundSwitch\SoundSwitchConfiguration.json`.

## التقنية
- ملف C# واحد: `EarFlip.cs` (WinForms + P/Invoke + COM interop).
- البناء بالمترجم الموجود أصلًا في Windows:
  `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`
- الناتج `EarFlip.exe` بلا أي اعتماديات، ولا NuGet، ولا تثبيت حزم.

## الملفات (كلها داخل هذا المجلد فقط)
```
EarFlip\
  PLAN.md       (هذا الملف، لا يُعدَّل)
  AGENTS.md     (تعليمات الوكيل، لا يُعدَّل)
  EarFlip.cs
  build.cmd
  .gitignore    (يستثني *.exe و*.ico)
```

## السلوك المطلوب
1. **الاختصار:** `RegisterHotKey` بـ `MOD_CONTROL | MOD_ALT` (0x0003) و`VK_F12` (0x7B). يُضاف `MOD_NOREPEAT` (0x4000).
2. **التبديل:** عند الضغط:
   - قراءة الجهاز الافتراضي الحالي عبر `IMMDeviceEnumerator.GetDefaultAudioEndpoint(eRender, eMultimedia)`.
   - إن كان Speakers، تُضبط Buds افتراضيًا، وإلا تُضبط Speakers.
   - الضبط عبر `IPolicyConfig.SetDefaultEndpoint` لكل الأدوار الثلاثة: `eConsole` و`eMultimedia` و`eCommunications`.
   - المعرّفات المعروفة (Windows 7 فما بعد):
     - `MMDeviceEnumerator` CLSID: `BCDE0395-E52F-467C-8E3D-C4579291692E`
     - `IMMDeviceEnumerator` IID: `A95664D2-9614-4F35-A746-DE8DB63617E6`
     - `PolicyConfigClient` CLSID: `870AF99C-171D-4F9E-AF0D-E63DF40C2BC9`
     - `IPolicyConfig` IID: `F8679F50-850A-41CF-9C72-430F290290C8`
3. **السماعة غير متصلة:** قبل التبديل إلى Buds يُفحص أن حالتها `DEVICE_STATE_ACTIVE` (عبر `GetDevice` ثم `GetState`). إن لم تكن نشطة: لا تبديل، و`SystemSounds.Beep` فقط، بلا رسالة ولا استثناء.
4. **أيقونة شريط المهام (tray)، وتتغير حسب الجهاز:**
   - رسمتان أصليتان بـ GDI+ داخل الكود: **سماعة رأس** حين تكون Buds نشطة، و**مكبّر صوت بموجتين** حين تكون Speakers نشطة.
   - لونان مختلفان، وخطوط سميكة واضحة في مقاس 16px، وخلفية شفافة.
   - عند التمرير يظهر اسم الجهاز الحالي (`NotifyIcon.Text`).
   - قائمة يمين بخيار واحد: «خروج».
   - تتحدث الأيقونة أيضًا إن غيّر Windows الجهاز بنفسه: `Timer` كل 2000ms يقارن الجهاز الحالي.
     تعليق في الكود: `// shortcut: polling بدل IMMNotificationClient، يُرقّى إن ظهر بطء`
5. **نسخة واحدة:** `Mutex` باسم `Local\EarFlip`. إن كانت هناك نسخة تعمل، تخرج النسخة الثانية فورًا بصمت.
6. **بلا نوافذ وبلا إشعارات:** يُبنى بـ `/target:winexe`.
7. **الخروج:** `UnregisterHotKey`، ثم إخفاء الأيقونة والتخلص منها.
8. **أيقونة EXE:** `EarFlip.exe --make-icon` يحفظ `EarFlip.ico` بجانبه، فيه رسمة السماعة بمقاسات 16 و24 و32 و48 و256 (صيغة PNG داخل ICO، مع كتابة ترويسة ICO يدويًا). كود الرسم واحد، وتستخدمه الأيقونة في الـ tray وملف الـ ico معًا.

## build.cmd
1. بناء أول بلا أيقونة → `EarFlip.exe`.
2. تشغيل `EarFlip.exe --make-icon` → `EarFlip.ico`.
3. بناء ثانٍ مع `/win32icon:EarFlip.ico`.
4. يتوقف عند أي خطأ (`if errorlevel 1 exit /b 1`).

مراجع البناء: `System.dll` و`System.Drawing.dll` و`System.Windows.Forms.dll`.

## التثبيت المحلي (بعد نجاح البناء والاختبار)
1. إيقاف SoundSwitch وإلغاء تشغيله مع Windows، حتى لا يتعارض الاختصاران. **لا يُحذف SoundSwitch ولا إعداده.**
2. إنشاء اختصار `EarFlip.lnk` في مجلد Startup (`shell:startup`) يشير إلى `EarFlip.exe` في هذا المجلد.

## خارج النطاق (لا يُنفَّذ)
اختيار الأجهزة من واجهة، تغيير الاختصار من الإعدادات، الميكروفون، الإشعارات، التحديث التلقائي، المثبّت (installer)، النشر على GitHub.

## التحقق
1. `build.cmd` يكتمل بلا أخطاء، والـ EXE يحمل الأيقونة.
2. مع السماعة متصلة: الضغط على Ctrl+Alt+F12 مرتين يبدّل ذهابًا وإيابًا. يُتحقق من ذلك بقراءة الجهاز الافتراضي من PowerShell، وأيقونة الـ tray تتبدل معه.
3. مع السماعة مفصولة: الضغط يُصدر Beep ويبقى الصوت على Speakers.
4. تغيير الجهاز يدويًا من إعدادات Windows يُحدّث الأيقونة خلال ثانيتين.
5. تشغيل نسخة ثانية: تخرج فورًا.
6. «خروج» يحرر الاختصار، وتشغيل الأداة من جديد يسجله بلا خطأ.

## git
- `git init` في هذا المجلد.
- commit بعد أول بناء ناجح، وcommit بعد اكتمال التحقق.

## التراجع
1. حذف مجلد `EarFlip` واختصار Startup.
2. إعادة تفعيل SoundSwitch مع Windows، وإعداده محفوظ كما هو.
