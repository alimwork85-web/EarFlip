@echo off
setlocal
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
set REFS=/r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll

"%CSC%" /nologo /codepage:65001 /target:winexe /out:EarFlip.exe %REFS% EarFlip.cs
if errorlevel 1 exit /b 1

EarFlip.exe --make-icon
if errorlevel 1 exit /b 1

"%CSC%" /nologo /codepage:65001 /target:winexe /win32icon:EarFlip.ico /out:EarFlip.exe %REFS% EarFlip.cs
if errorlevel 1 exit /b 1
