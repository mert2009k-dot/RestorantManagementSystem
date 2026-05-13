using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestoranProjesi.Data;
using RestoranProjesi.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// MySQL Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion, mySqlOptions =>
        mySqlOptions.EnableRetryOnFailure()));

// ==========================================
// 🔒 GÜVENLİK: Güçlü Şifre Politikası
// ==========================================
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Şifre kuralları - Güçlü şifre zorunlu
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // 🔒 Hesap kilitleme - Brute force koruması
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Kullanıcı ayarları
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ==========================================
// 🔒 GÜVENLİK: Güvenli Cookie Ayarları
// ==========================================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;                    // JavaScript erişimi engellendi
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Sadece HTTPS üzerinden
    options.Cookie.SameSite = SameSiteMode.Strict;     // CSRF koruması
    options.ExpireTimeSpan = TimeSpan.FromHours(2);    // 2 saat sonra oturum sona erer
    options.SlidingExpiration = true;                   // Aktif kullanımda süre yenilenir
});

// ==========================================
// 🔒 GÜVENLİK: Güvenli Session Ayarları
// ==========================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// ==========================================
// 🔒 GÜVENLİK: Global CSRF (Anti-Forgery) Koruması
// ==========================================
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

// Seed Roles and Admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    
    // Veritabanını otomatik oluştur/güncelle
    await context.Database.MigrateAsync();

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();

    string[] roleNames = { "Admin", "User" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    var adminEmail = "admin@restoran.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new AppUser { UserName = adminEmail, Email = adminEmail, FirstName = "Sistem", LastName = "Yöneticisi" };
        var adminPassword = builder.Configuration["AdminSettings:DefaultPassword"] ?? "Admin123!";
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// ==========================================
// 🔒 GÜVENLİK: HTTP Güvenlik Başlıkları
// ==========================================
app.Use(async (context, next) =>
{
    // XSS saldırılarını engelle
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    // Clickjacking (iframe ile gizli tıklama) engelle
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    // XSS filtresi aktif
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    // Referrer bilgisini sınırla
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    // İzin verilmeyen tarayıcı özelliklerini kapat
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    // Sunucu bilgisini gizle
    context.Response.Headers.Remove("Server");
    context.Response.Headers.Remove("X-Powered-By");

    await next();
});

app.UseStaticFiles(); // Bu satır eklendi (CSS, JS, Resimlerin yüklenmesi için kritik)
app.UseRouting();

// Enable session
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
