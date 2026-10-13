"""Telefon bildirimini test eder. Kullanım: python telefon_test.py
Konu adı Ayar.cs içindeki Ayar.NtfyKonu değerinden okunur."""
import pathlib
import re
import urllib.request

ayar = pathlib.Path(__file__).with_name("Ayar.cs").read_text(encoding="utf-8")
KONU = re.search(r'NtfyKonu\s*=\s*"([^"]+)"', ayar).group(1)

req = urllib.request.Request(
    f"https://ntfy.sh/{KONU}",
    data="Test bildirimi: sistem çalışıyor".encode("utf-8"),
    headers={"Title": "Guvenlik Testi", "Priority": "high", "Tags": "rotating_light"},
)
urllib.request.urlopen(req, timeout=10)
print("Gönderildi, telefonu kontrol et.")
