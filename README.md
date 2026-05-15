# 🍽️ Restoran Yönetim Sistemi (RestoranProjesi)

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core%209.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![MySQL](https://img.shields.io/badge/MySQL-4479A1?style=for-the-badge&logo=mysql&logoColor=white)

Modern, hızlı ve güvenli bir restoran yönetim paneli. Bu proje, bir restoranın tüm dijital operasyonlarını (siparişler, kullanıcı yönetimi, istatistikler) yönetmek için geliştirilmiştir.

## 🚀 Öne Çıkan Özellikler

*   **🛡️ Gelişmiş Kimlik Doğrulama (Identity):** ASP.NET Core Identity ile güvenli giriş, rol yönetimi (Admin/User) ve güçlü şifre politikaları.
*   **📊 Canlı İstatistik Paneli:** Günlük cirolar, tüm zamanların satış verileri ve kullanıcı istatistikleri.
*   **👥 Kullanıcı Yönetimi:** Yumuşak silme (Soft Delete) özelliği ile kullanıcıları silmeden pasifize etme ve geri yükleme imkanı.
*   **🔐 Üst Düzey Güvenlik:** 
    *   CSRF (Anti-Forgery) koruması.
    *   XSS saldırılarına karşı güvenli header yapılandırması.
    *   HTTPS zorunluluğu ve güvenli Cookie ayarları.
*   **☁️ Bulut Entegrasyonu:** Aiven MySQL veritabanı ve Render bulut sunucu desteği.

## 🛠️ Kullanılan Teknolojiler

*   **Backend:** ASP.NET Core 9.0 MVC
*   **Database:** MySQL (Pomelo Entity Framework Core)
*   **Frontend:** HTML5, CSS3 (Vanilla), JavaScript, Razor Pages
*   **Deployment:** Docker, Render, Aiven Cloud MySQL

## ⚙️ Kurulum ve Çalıştırma

### Yerel Ortam (Local)
1.  Projeyi klonlayın: `git clone https://github.com/mert2009k-dot/RestorantManagementSystem.git`
2.  `appsettings.json` dosyasındaki ConnectionString'i kendi yerel MySQL bilgilerinizle güncelleyin.
3.  Terminalde `dotnet run` komutunu çalıştırın.

### Canlı Ortam (Cloud)
Bu proje Dockerize edilmiştir ve Render üzerinde çalışacak şekilde yapılandırılmıştır. Canlıya almak için `Dockerfile` üzerinden build edilmesi yeterlidir.

## 📂 Proje Yapısı
*   **Controllers:** İş mantığının yönetildiği kontrolcüler.
*   **Models:** Veritabanı tabloları ve veri yapıları.
*   **Views:** Kullanıcı arayüzü (Admin & User sayfaları).
*   **Services:** İş süreçlerini destekleyen servis katmanı.

---
👤 **Geliştirici:** [Mert]
📧 **İletişim:** admin@restoran.com
