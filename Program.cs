using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace GuvenlikKilidi;

// Ayarlar (PIN, ntfy konusu vb.) Ayar.cs dosyasında - örnek: Ayar.ornek.txt

static class Program
{
    [STAThread]
    static void Main()
    {
        var app = new Application();
        app.Run(new KilitPenceresi());
    }
}

// ====================== KLAVYE ENGELLEME ======================
static class KlavyeEngel
{
    const int WH_KEYBOARD_LL = 13;
    delegate IntPtr Proc(int n, IntPtr w, IntPtr l);
    static readonly Proc _proc = Hook;
    static IntPtr _id = IntPtr.Zero;

    [StructLayout(LayoutKind.Sequential)]
    struct KBD { public int vk, scan, flags, time; public IntPtr extra; }

    [DllImport("user32.dll")] static extern IntPtr SetWindowsHookEx(int id, Proc p, IntPtr mod, uint tid);
    [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(IntPtr h);
    [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr h, int n, IntPtr w, IntPtr l);
    [DllImport("kernel32.dll")] static extern IntPtr GetModuleHandle(string? name);
    [DllImport("user32.dll")] static extern short GetAsyncKeyState(int vk);

    public static void Baslat()
    {
        if (_id == IntPtr.Zero)
            _id = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(Process.GetCurrentProcess().MainModule!.ModuleName), 0);
    }

    public static void Durdur()
    {
        if (_id != IntPtr.Zero) { UnhookWindowsHookEx(_id); _id = IntPtr.Zero; }
    }

    static IntPtr Hook(int n, IntPtr w, IntPtr l)
    {
        if (n >= 0)
        {
            var k = Marshal.PtrToStructure<KBD>(l);
            bool alt = (k.flags & 0x20) != 0;
            bool ctrl = (GetAsyncKeyState(0x11) & 0x8000) != 0;

            bool engelle =
                (alt && k.vk == 0x73) ||            // Alt+F4
                (alt && k.vk == 0x09) ||            // Alt+Tab
                (alt && k.vk == 0x1B) ||            // Alt+Esc
                (ctrl && k.vk == 0x1B) ||           // Ctrl+Esc (Başlat)
                k.vk == 0x5B || k.vk == 0x5C ||     // Windows tuşları
                k.vk == 0x5D;                       // Menü tuşu
            if (engelle) return (IntPtr)1;
        }
        return CallNextHookEx(_id, n, w, l);
    }
}

// ====================== ALARM ======================
static class Alarm
{
    static volatile bool _calisiyor;
    static Thread? _t;

    public static bool Caliyor => _calisiyor;

    public static void Baslat()
    {
        if (_calisiyor) return;
        _calisiyor = true;
        _t = new Thread(() =>
        {
            while (_calisiyor)
            {
                try { Console.Beep(1800, 250); Console.Beep(900, 250); }
                catch { System.Media.SystemSounds.Hand.Play(); Thread.Sleep(400); }
            }
        }) { IsBackground = true };
        _t.Start();
    }

    public static void Durdur() => _calisiyor = false;
}

// ====================== TELEFON BİLDİRİMİ (ntfy) ======================
static class Telefon
{
    static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(10) };

    public static async Task Gonder(string baslik, string mesaj, string oncelik = "high")
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"https://ntfy.sh/{Ayar.NtfyKonu}")
            {
                Content = new StringContent(mesaj, Encoding.UTF8, "text/plain")
            };
            req.Headers.Add("Title", baslik);
            req.Headers.Add("Priority", oncelik);
            req.Headers.Add("Tags", "rotating_light");
            await _http.SendAsync(req);
        }
        catch { /* internet yoksa sessizce geç */ }
    }
}

// ====================== ANA PENCERE ======================
class KilitPenceresi : Window
{
    readonly TextBlock _saat = new(), _tarih = new(), _hava = new(), _durum = new();
    readonly PasswordBox _pin = new();
    readonly DateTime _baslangic = DateTime.Now;
    bool _acik, _alarmBildirildi;
    int _hatali;
    static readonly HttpClient _http = new();

    public KilitPenceresi()
    {
        WindowStyle = WindowStyle.None;
        WindowState = WindowState.Maximized;
        ResizeMode = ResizeMode.NoResize;
        Topmost = true;
        ShowInTaskbar = false;
        Cursor = Cursors.Arrow;
        Background = new LinearGradientBrush(
            (Color)ColorConverter.ConvertFromString("#0B1220"),
            (Color)ColorConverter.ConvertFromString("#1B2A4A"), 45);

        var tr = new CultureInfo("tr-TR");
        var kok = new Grid();
        var panel = new StackPanel { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

        _saat.FontSize = 140; _saat.FontWeight = FontWeights.Thin; _saat.Foreground = Brushes.White;
        _saat.HorizontalAlignment = HorizontalAlignment.Center;
        _tarih.FontSize = 28; _tarih.Foreground = new SolidColorBrush(Color.FromRgb(160, 180, 220));
        _tarih.HorizontalAlignment = HorizontalAlignment.Center;

        var havaKart = new Border
        {
            CornerRadius = new CornerRadius(18), Padding = new Thickness(28, 14, 28, 14),
            Margin = new Thickness(0, 24, 0, 40), HorizontalAlignment = HorizontalAlignment.Center,
            Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)),
            Child = _hava
        };
        _hava.FontSize = 26; _hava.Foreground = Brushes.White; _hava.Text = "Ankara: yükleniyor...";

        var kilit = new TextBlock { Text = "🔒  Bu bilgisayar kilitli", FontSize = 22, Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 14) };

        _pin.Width = 280; _pin.FontSize = 32; _pin.HorizontalContentAlignment = HorizontalAlignment.Center;
        _pin.MaxLength = 12; _pin.Padding = new Thickness(8);
        _pin.Background = new SolidColorBrush(Color.FromArgb(70, 255, 255, 255));
        _pin.Foreground = Brushes.White; _pin.BorderBrush = Brushes.Transparent; _pin.CaretBrush = Brushes.White;
        _pin.KeyDown += (s, e) => { if (e.Key == Key.Enter) Dene(); };

        _durum.FontSize = 18; _durum.Foreground = new SolidColorBrush(Color.FromRgb(255, 120, 120));
        _durum.HorizontalAlignment = HorizontalAlignment.Center; _durum.Margin = new Thickness(0, 12, 0, 0);

        panel.Children.Add(_saat); panel.Children.Add(_tarih); panel.Children.Add(havaKart);
        panel.Children.Add(kilit); panel.Children.Add(_pin); panel.Children.Add(_durum);
        kok.Children.Add(panel);
        Content = kok;

        // Saat
        var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        t.Tick += (s, e) =>
        {
            _saat.Text = DateTime.Now.ToString("HH:mm:ss", tr);
            _tarih.Text = DateTime.Now.ToString("dddd, d MMMM yyyy", tr);
            Topmost = true;
            if (!IsActive) { Activate(); }
            _pin.Focus();
        };
        t.Start();

        // Hava durumu (10 dk'da bir)
        var h = new DispatcherTimer { Interval = TimeSpan.FromMinutes(10) };
        h.Tick += async (s, e) => await HavaGuncelle();
        h.Start();

        // Hareket algılama -> alarm
        PreviewMouseMove += (s, e) => Hareket();
        PreviewKeyDown += (s, e) => { if (e.Key != Key.Enter) Hareket(); };
        PreviewMouseDown += (s, e) => Hareket();

        Loaded += async (s, e) =>
        {
            KlavyeEngel.Baslat();
            _pin.Focus();
            _ = Telefon.Gonder("Kilit aktif", $"Bilgisayar kilitlendi: {DateTime.Now:HH:mm:ss}", "default");
            await HavaGuncelle();
            if (Ayar.AlarmHemenBaslasin) AlarmVer("Alarm başladı");
        };
        Closing += (s, e) => { if (!_acik) e.Cancel = true; };   // Alt+F4 vb. kapatamaz
    }

    void Hareket()
    {
        if (_acik) return;
        if ((DateTime.Now - _baslangic).TotalSeconds < Ayar.SilahlanmaSaniye) return;
        AlarmVer("Kilitli bilgisayarda hareket algılandı!");
    }

    void AlarmVer(string sebep)
    {
        Alarm.Baslat();
        if (!_alarmBildirildi)
        {
            _alarmBildirildi = true;
            _ = Telefon.Gonder("ALARM!", $"{sebep} ({DateTime.Now:HH:mm:ss})", "urgent");
        }
    }

    void Dene()
    {
        if (_pin.Password == Ayar.Pin)
        {
            _acik = true;
            Alarm.Durdur();
            KlavyeEngel.Durdur();
            _ = Telefon.Gonder("Kilit açıldı", $"PIN ile açıldı: {DateTime.Now:HH:mm:ss}", "default");
            Thread.Sleep(300);
            Close();
            Application.Current.Shutdown();
        }
        else
        {
            _hatali++;
            _pin.Clear();
            _durum.Text = $"Yanlış PIN ({_hatali})";
            AlarmVer($"Yanlış PIN denemesi: {_hatali}");
            _ = Telefon.Gonder("Yanlış PIN", $"{_hatali}. hatalı deneme", "high");
        }
    }

    async Task HavaGuncelle()
    {
        try
        {
            var url = "https://api.open-meteo.com/v1/forecast?latitude=39.93&longitude=32.86&current=temperature_2m,weather_code&timezone=Europe%2FIstanbul";
            using var doc = JsonDocument.Parse(await _http.GetStringAsync(url));
            var c = doc.RootElement.GetProperty("current");
            double derece = c.GetProperty("temperature_2m").GetDouble();
            int kod = c.GetProperty("weather_code").GetInt32();
            _hava.Text = $"{Ikon(kod)}  Ankara  {derece:0}°C  •  {Aciklama(kod)}";
        }
        catch { _hava.Text = "Ankara: hava durumu alınamadı"; }
    }

    static string Aciklama(int k) => k switch
    {
        0 => "Açık", 1 or 2 or 3 => "Parçalı bulutlu", 45 or 48 => "Sisli",
        >= 51 and <= 67 => "Yağmurlu", >= 71 and <= 77 => "Karlı",
        >= 80 and <= 82 => "Sağanak", >= 95 => "Fırtına", _ => "Değişken"
    };

    static string Ikon(int k) => k switch
    {
        0 => "☀️", 1 or 2 or 3 => "⛅", 45 or 48 => "🌫️",
        >= 51 and <= 67 => "🌧️", >= 71 and <= 77 => "❄️",
        >= 80 and <= 82 => "🌦️", >= 95 => "⛈️", _ => "🌡️"
    };
}
