<div align="center">

![Güvenlik Kilidi](docs/afis.svg)

[![.NET 8](https://img.shields.io/badge/.NET-8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Windows](https://img.shields.io/badge/Windows-10_/_11-0078D6?style=for-the-badge&logo=windows&logoColor=white)](#kurulum)
[![Lisans](https://img.shields.io/badge/lisans-MIT-e63946?style=for-the-badge)](LICENSE)

</div>

# 🔒 Güvenlik Kilidi

Windows için tam ekran, PIN korumalı kilit ekranı. Bilgisayarın başından kalktığında ekranı kilitler; biri dokunursa alarm çalar ve telefonuna bildirim gönderir.

## Özellikler

- **Tam ekran kilit** — her zaman üstte, görev çubuğunda görünmez, PIN girilmeden kapanmaz
- **Kısayol engelleme** — Alt+F4, Alt+Tab, Alt+Esc, Ctrl+Esc, Windows ve Menü tuşları devre dışı
- **Hareket alarmı** — kilitlendikten birkaç saniye sonra fare/klavye hareketinde sesli alarm
- **Telefon bildirimi** — kilitlenme, alarm, yanlış PIN ve kilit açılma anında [ntfy](https://ntfy.sh) üzerinden anlık bildirim
- **Saat, tarih ve hava durumu** — Ankara için canlı hava durumu ([Open-Meteo](https://open-meteo.com), API anahtarı gerekmez)

## Kurulum

Gereken: Windows 10/11 (x64). .NET 8 SDK yoksa `EXE_OLUSTUR.bat` winget ile kurar.

1. Depoyu indir:
   ```
   git clone https://github.com/Enskygr234/GuvenlikKilidi.git
   ```
2. `Ayar.ornek.txt` dosyasını `Ayar.cs` adıyla kopyala, `Pin` ve `NtfyKonu` değerlerini değiştir.
   (Kopyalamazsan `EXE_OLUSTUR.bat` ilk çalıştırmada senin yerine kopyalar.)
3. `EXE_OLUSTUR.bat` dosyasına çift tıkla.
4. `EXE\GuvenlikKilidi.exe` oluşur. Çalıştırınca ekran kilitlenir.

### Telefon bildirimi

1. Google Play / App Store'dan **ntfy** uygulamasını indir.
2. **+** tuşuna bas, `Ayar.cs` içindeki `NtfyKonu` adını gir, abone ol.
3. Test et:
   ```
   python telefon_test.py
   ```

## Ayarlar

Tüm ayarlar `Ayar.cs` içinde; değiştirdikten sonra yeniden derlemek gerekir.

| Ayar | Açıklama |
| --- | --- |
| `Pin` | Kilidi açan PIN |
| `NtfyKonu` | Bildirimlerin gönderildiği ntfy konusu |
| `AlarmHemenBaslasin` | `true` ise kilit açılır açılmaz alarm çalar |
| `SilahlanmaSaniye` | Kilitten sonra hareket algılamanın başlaması için beklenen süre |

`Ayar.cs` `.gitignore` içindedir; kendi PIN'in ve konu adın depoya gitmez.

## Bilinmesi gerekenler

- **Ctrl+Alt+Del Windows tarafından engellenemez.** Bu uygulama caydırıcı bir kilittir; Windows oturum kilidinin (Win+L) yerini tutmaz.
- ntfy.sh konuları herkese açıktır: konu adını bilen herkes bildirimleri okuyabilir. Tahmin edilemez bir ad seç.
- PIN, derlenen EXE'nin içine gömülür; EXE'yi başkalarıyla paylaşma.
- İlk denemede PIN'i unutma. 🙂

## Proje yapısı

| Dosya | Görevi |
| --- | --- |
| `Program.cs` | Kilit penceresi, klavye engelleme, alarm, bildirim, hava durumu |
| `Ayar.ornek.txt` | Ayar şablonu (`Ayar.cs` olarak kopyalanır) |
| `EXE_OLUSTUR.bat` | Tek dosyalık, kendi kendine yeten EXE üretir |
| `telefon_test.py` | Telefon bildirimini test eder |

## Lisans

[MIT](LICENSE)
