using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestoranProjesi.Models.Entities;

namespace RestoranProjesi.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<DailyLogRecord> DailyLogRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- IDENTITY (GÜVENLİK/KULLANICI) TABLOLARI ---
            // Görüntüdeki 'musteribilgi' tablosunu kullanıcı tablosu yapıyoruz
            builder.Entity<AppUser>().ToTable("musteribilgi");
            builder.Entity<IdentityRole>().ToTable("roller");
            builder.Entity<IdentityUserRole<string>>().ToTable("kullanicirol");
            builder.Entity<IdentityUserClaim<string>>().ToTable("kullanicihaklari");
            builder.Entity<IdentityUserLogin<string>>().ToTable("kullanicigirisleri");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("rolhaklari");
            builder.Entity<IdentityUserToken<string>>().ToTable("kullanicitokenlari");

            // --- RESTORAN İŞLEYİŞ TABLOLARI ---
            builder.Entity<Category>().ToTable("kategoriler");
            builder.Entity<Product>().ToTable("yemekler");
            builder.Entity<Order>().ToTable("siparisler");
            builder.Entity<OrderItem>().ToTable("siparis_detaylari");
            builder.Entity<CartItem>().ToTable("sepet_icerikleri");
            builder.Entity<Reservation>().ToTable("rezervasyonlar");
            builder.Entity<Employee>().ToTable("calisanlar");
            builder.Entity<Favorite>().ToTable("favoriler");
        }
    }
}
