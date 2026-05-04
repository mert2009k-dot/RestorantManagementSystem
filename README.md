# RestorantManagementSystem
# 🍽️ Hanedan Restoran - Akıllı QR Menü & Yönetim Sistemi
11/A Mert İnan

Hanedan Restoran, modern Restorant ihtiyaçları için geliştirilmiş, hem müşteri hem de calısanların tarafını kapsayan kapsamlı bir **ASP.NET Core MVC** web uygulamasıdır.

## 🚀 Öne Çıkan Özellikler

### 📱 Müşteri (QR Menü) Deneyimi
- **Hesapsız Sipariş:** Müşteriler üye olmadan, sadece masadaki QR kodu okutarak anında sipariş verebilir.
- **Masa Takibi:** Siparişler otomatik olarak masa numarası ile eşleşir.
- **Ödeme Seçimi:** Sepet aşamasında Nakit veya Kredi Kartı ile ödeme tercihi yapılabilir.
- **Dinamik Menü:** Kategorize edilmiş, görsel destekli ve etkileşimli menü arayüzü.

### 🛠️ Yönetici (Admin) Paneli
- **Canlı Sipariş Takibi:** Gelen siparişleri "Hazırlanıyor", "Yolda" veya "Tamamlandı" olarak anlık yönetme.
- **Ürün & Kategori Yönetimi:** Menüye yeni ürün ekleme, düzenleme ve stok durumunu kontrol etme.
- **Günlük Log Kayıtları:** Günlük kazanç, sipariş sayısı ve işlem geçmişini detaylı raporlama.
- **Personel Yönetimi:** Restoran çalışanlarını ve rollerini yönetme.
- **QR Yönetimi:** Masalar için dinamik QR kod oluşturma ve yönetim.

### 🔒 Güvenlik Katmanları
- **Identity Framework:** Güvenli kullanıcı yetkilendirme ve rol yönetimi.
- **Brute-Force Koruması:** Hatalı giriş denemelerinde hesap kilitleme sistemi.
- **Global CSRF Koruması:** Sahte form gönderimlerine karşı tam koruma.
- **Secure Cookies:** HTTPOnly ve SameSite politikaları ile korunan oturumlar.
- **Input Validation:** Tüm kullanıcı girdileri için sıkı doğrulama kuralları.

## 🛠️ Kullanılan Teknolojiler
- **Backend:** ASP.NET Core 10.0 MVC
- **Database:** MySQL / Entity Framework Core
- **Frontend:** HTML5, CSS3, JavaScript, Bootstrap 5, FontAwesome 6
- **Auth:** Microsoft Identity Framework



## 💻 KURULUM VE CALISTIRMA İCİN YAPMANIZ GEREKENLER
1. **Veritabanı Yapılandırması:** `appsettings.json` dosyasındaki bağlantı bilgilerini güncelleyin.
2. **Migration Uygulayın:** `dotnet ef database update`
3. **Projeyi Çalıştırın:** `dotnet run`
4. Eğer sorun olursa tarayıcınızdan manuel girmek icin "https://localhost:7278" adresini kullanabilirsiniz
