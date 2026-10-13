@echo off
chcp 65001 >nul
echo ===== Guvenlik Kilidi - EXE olusturuluyor =====
where dotnet >nul 2>nul
if errorlevel 1 (
  echo .NET SDK bulunamadi. Kuruluyor...
  winget install --id Microsoft.DotNet.SDK.8 -e --accept-source-agreements --accept-package-agreements
  echo Kurulum bitti. Bu dosyayi KAPATIP tekrar calistirin.
  pause
  exit /b
)
if not exist "%~dp0Ayar.cs" (
  copy "%~dp0Ayar.ornek.txt" "%~dp0Ayar.cs" >nul
  echo Ayar.cs olusturuldu. Icindeki Pin ve NtfyKonu degerlerini degistirip tekrar calistirin.
  pause
  exit /b
)
dotnet publish "%~dp0GuvenlikKilidi.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%~dp0EXE"
if errorlevel 1 (
  echo HATA olustu.
  pause
  exit /b
)
echo.
echo Hazir: %~dp0EXE\GuvenlikKilidi.exe
explorer "%~dp0EXE"
pause
