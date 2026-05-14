using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestoranProjesi.Data;
using RestoranProjesi.Models.Entities;

namespace RestoranProjesi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            
            var stats = new
            {
                TodayOrders = await _context.Orders.CountAsync(o => o.OrderDate.Date == today),
                TodayEarnings = await _context.Orders.Where(o => o.OrderDate.Date == today && o.Status == "Tamamlandı").SumAsync(o => o.TotalAmount),
                TotalProducts = await _context.Products.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync()
            };

            // En çok satan ürün (top 5)
            var topProducts = await _context.OrderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { 
                    ProductId = g.Key, 
                    Count = g.Sum(x => x.Quantity) 
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .Join(_context.Products, 
                    fav => fav.ProductId, 
                    p => p.Id, 
                    (fav, p) => new { p.Name, fav.Count })
                .ToListAsync();

            // Son 7 günlük satış verisi (Chart.js için)
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            var salesData = new List<decimal>();
            var labels = new List<string>();

            foreach (var date in last7Days)
            {
                var dayTotal = await _context.Orders
                    .Where(o => o.OrderDate.Date == date && o.Status == "Tamamlandı")
                    .SumAsync(o => o.TotalAmount);
                salesData.Add(dayTotal);
                labels.Add(date.ToString("dd MMM"));
            }

            ViewBag.Stats = stats;
            ViewBag.TopProducts = topProducts;
            ViewBag.SalesData = salesData;
            ViewBag.SalesLabels = labels;

            return View();
        }

        public async Task<IActionResult> ProductList()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ProductList));
            }
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ProductList));
            }
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleProductStatus(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsActive = !product.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ProductList));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFeaturedStatus(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsFeatured = !product.IsFeatured;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ProductList));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ProductList));
        }

        // Category Management
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategoryInline(string name, string description)
        {
            if (!string.IsNullOrEmpty(name))
            {
                _context.Categories.Add(new Category { Name = name, Description = description });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Categories));
        }

        // Hızlı Kategori Ekleme (Yeni Yemek Sayfasından)
        [HttpPost]
        public async Task<IActionResult> AddCategoryFromProduct(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                _context.Categories.Add(new Category { Name = name, Description = "Hızlı eklendi" });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(AddProduct));
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(int id, string name, string description)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null && !string.IsNullOrEmpty(name))
            {
                category.Name = name;
                category.Description = description;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Categories));
        }

        // --- ORDER MANAGEMENT ---
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null && !string.IsNullOrEmpty(status))
            {
                if (status == "Geri Yüklendi")
                {
                    order.Status = "Bekliyor";
                    order.IsRestored = true;
                }
                else
                {
                    order.Status = status;
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Orders));
        }
        // --- USER MANAGEMENT ---
        public async Task<IActionResult> AllUsers(bool showDeleted = false)
        {
            var users = await _context.Users
                .Where(u => u.IsDeleted == showDeleted)
                .ToListAsync();
            ViewBag.ShowDeleted = showDeleted;
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id, [FromServices] Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager)
        {
            var user = await userManager.FindByIdAsync(id);
            // Admin hesabı silinemesin
            if (user != null && user.Email != "admin@restoran.com" && User.Identity?.Name != user.Email)
            {
                user.IsDeleted = true;
                user.IsRestored = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(AllUsers));
        }

        [HttpPost]
        public async Task<IActionResult> RestoreUser(string id, [FromServices] Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user != null && user.IsDeleted)
            {
                user.IsDeleted = false;
                user.IsRestored = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(AllUsers), new { showDeleted = true });
        }

        // --- EMPLOYEE MANAGEMENT ---
        public async Task<IActionResult> Employees()
        {
            var employees = await _context.Employees.ToListAsync();
            return View(employees);
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee(string firstName, string lastName, string position, string phoneNumber, string imageUrl)
        {
            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
            {
                var employee = new Employee
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Position = position,
                    PhoneNumber = phoneNumber,
                    ImageUrl = imageUrl,
                    IsActive = true
                };
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Employees));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Employees));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleEmployeeStatus(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                employee.IsActive = !employee.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Employees));
        }

        [HttpGet]
        public async Task<IActionResult> EditEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        [HttpPost]
        public async Task<IActionResult> EditEmployee(Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Employees));
            }
            return View(employee);
        }

        // --- RESERVATION MANAGEMENT ---
        public async Task<IActionResult> Reservations()
        {
            var reservations = await _context.Reservations
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
            return View(reservations);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReservationStatus(int id, string status)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null && !string.IsNullOrEmpty(status))
            {
                reservation.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Reservations));
        }

        [HttpGet]
        public async Task<IActionResult> SeedInitialData()
        {
            // Kategoriler
            var categories = new List<Category>
            {
                new Category { Name = "Ana Yemekler", Description = "Geleneksel ev lezzetleri ve şefin imzaları" },
                new Category { Name = "Ev Yemekleri", Description = "Geleneksel ev lezzetleri" },
                new Category { Name = "Çorbalar", Description = "Sıcacık ve şifalı başlangıçlar" },
                new Category { Name = "Kebap Çeşitleri", Description = "Ustasından özel kebaplar" },
                new Category { Name = "Pide Çeşitleri", Description = "Taş fırında çıtır pideler" },
                new Category { Name = "Lahmacunlar", Description = "Geleneksel ve özel lahmacunlar" },
                new Category { Name = "Tatlılar", Description = "Günün tatlı sonu" },
                new Category { Name = "İçecekler", Description = "Ferahlatıcı seçenekler" }
            };

            foreach (var cat in categories)
            {
                var existing = await _context.Categories.FirstOrDefaultAsync(c => c.Name == cat.Name);
                if (existing != null)
                {
                    existing.Description = cat.Description;
                }
                else
                {
                    _context.Categories.Add(cat);
                }
            }
            await _context.SaveChangesAsync();

            // Kategori ID'lerini al
            var anaYemeklerId = (await _context.Categories.FirstAsync(c => c.Name == "Ana Yemekler")).Id;
            var evYemekleriId = (await _context.Categories.FirstAsync(c => c.Name == "Ev Yemekleri")).Id;
            var corbalarId = (await _context.Categories.FirstAsync(c => c.Name == "Çorbalar")).Id;
            var kebapId = (await _context.Categories.FirstAsync(c => c.Name == "Kebap Çeşitleri")).Id;
            var pideId = (await _context.Categories.FirstAsync(c => c.Name == "Pide Çeşitleri")).Id;
            var lahmacunId = (await _context.Categories.FirstAsync(c => c.Name == "Lahmacunlar")).Id;
            var tatliId = (await _context.Categories.FirstAsync(c => c.Name == "Tatlılar")).Id;
            var icecekId = (await _context.Categories.FirstAsync(c => c.Name == "İçecekler")).Id;

            // Yemekler
            var products = new List<Product>
            {
                // --- KEBAPLAR ---
                new Product { Name = "Adana Kebap", Price = 350, Description = "Zırh kıyması ile hazırlanan acılı gerçek Adana lezzeti.", CategoryId = kebapId, IsActive = true, IsFeatured = true, ImageUrl = "https://encrypted-tbn2.gstatic.com/images?q=tbn:ANd9GcQ1hoeD7klxWgpOfr-JIOUO21g8o6W28WNqadKQJg4tY05DTI37UMvyl7mfFylT" },
                new Product { Name = "Urfa Kebap", Price = 340, Description = "Acısız, özel baharatlarla harmanlanmış yumuşak kebap.", CategoryId = kebapId, IsActive = true, ImageUrl = "https://encrypted-tbn3.gstatic.com/images?q=tbn:ANd9GcT1EvaGP7XBOmmP3epjJgH2ulnu1L5MUWVGSiBoeDJ8T9P8uLrm0-2pazrGNsiI" },
                new Product { Name = "Beyti Sarma", Price = 420, Description = "Lavaş içinde kebap, yoğurt ve özel sos ile.", CategoryId = kebapId, IsActive = true, IsFeatured = true, ImageUrl = "https://encrypted-tbn3.gstatic.com/images?q=tbn:ANd9GcT1EvaGP7XBOmmP3epjJgH2ulnu1L5MUWVGSiBoeDJ8T9P8uLrm0-2pazrGNsiI" },
                new Product { Name = "Ali Nazik", Price = 450, Description = "Süzme yoğurtlu patlıcan yatağında lokum gibi kuzu kuşbaşı.", CategoryId = kebapId, IsActive = true, IsFeatured = true, ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQNNm90-7AaqiRvpXva5_fug4bL4GiTQDXJ2y8k60sOSwQKx6OYDXMHZq2h5K-X" },
                new Product { Name = "Patlıcan Kebap", Price = 380, Description = "Sıralı patlıcan ve özel harçlı kebap köfteleri.", CategoryId = kebapId, IsActive = true, ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSQ1jue4OoqLABqSOiM1z2ZHmAXoP9Ojgw58DU-jjbiHmUCXVQBCFyQVMKCD23d" },
                new Product { Name = "Tavuk Şiş", Price = 280, Description = "Özel marinasyonlu, yumuşacık tavuk göğsü şiş.", CategoryId = kebapId, IsActive = true, ImageUrl = "https://encrypted-tbn3.gstatic.com/images?q=tbn:ANd9GcTcv5TNqVIZasiMlR5QVPeL0zKHynuJ873tZ68rtGJSBhvhn-FgFMgp6EogEERE" },
                new Product { Name = "Kuzu Şiş", Price = 460, Description = "Terbiyeli kuzu but etinden enfes şiş kebap.", CategoryId = kebapId, IsActive = true, ImageUrl = "https://encrypted-tbn3.gstatic.com/images?q=tbn:ANd9GcRd55Z3HXHqZWjJ4RChYferQQ2MLyBfhHpu8sj7-EaYHO19NvvMo9RGHo6xqgvv" },
                new Product { Name = "İskender Kebap", Price = 480, Description = "Döner eti, pide parçaları, özel domates sosu ve kızgın tereyağ ile.", CategoryId = kebapId, IsActive = true, IsFeatured = true, ImageUrl = "https://images.unsplash.com/photo-1626074353765-517a681e40be?auto=format&fit=crop&w=800&q=80" },

                // --- EV YEMEKLERİ & ANA YEMEKLER ---
                new Product { Name = "İmambayıldı", Price = 230, Description = "Patlıcanın en güzel hallerinden biri (soğan, sarımsak ve domates dolgulu).", CategoryId = evYemekleriId, IsActive = true, ImageUrl = "https://images.unsplash.com/photo-1541518763669-27fef04b14ea?auto=format&fit=crop&w=800&q=80" },
                new Product { Name = "Karnıyarık", Price = 260, Description = "Patlıcanın kıymalı harç ile buluştuğu eşsiz lezzet.", CategoryId = evYemekleriId, IsActive = true, IsDailyMeal = true, ImageUrl = "https://images.unsplash.com/photo-1541518763669-27fef04b14ea?auto=format&fit=crop&w=800&q=80" },
                new Product { Name = "Hünkar Beğendi", Price = 450, Description = "Köz patlıcan yatağında servis edilen yumuşacık kuşbaşı et.", CategoryId = anaYemeklerId, IsActive = true, IsFeatured = true, ImageUrl = "https://images.unsplash.com/photo-1541518763669-27fef04b14ea?auto=format&fit=crop&w=800&q=80" },
                new Product { Name = "İzmir Köfte", Price = 240, Description = "Patates ve köftenin domates sosuyla fırınlanmış hali.", CategoryId = evYemekleriId, IsActive = true, IsDailyMeal = true, ImageUrl = "https://images.unsplash.com/photo-1529006557810-274b9b2fc783?auto=format&fit=crop&w=800&q=80" },
                new Product { Name = "Orman Kebabı", Price = 310, Description = "Bol sebzeli ve kuşbaşı etli bir tencere yemeği.", CategoryId = anaYemeklerId, IsActive = true, ImageUrl = "https://images.unsplash.com/photo-1541518763669-27fef04b14ea?auto=format&fit=crop&w=800&q=80" },
                new Product { Name = "Zeytinyağlı Yaprak Sarma", Price = 160, Description = "Ev usulü, bol baharatlı ve zeytinyağlı sarma.", CategoryId = evYemekleriId, IsActive = true, ImageUrl = "https://encrypted-tbn2.gstatic.com/images?q=tbn:ANd9GcQzyk8TBC1gCLhzQ47DDjDliZ-NO1H8kNDcGWsVU_dgqhZS0D1F7MP6mZDE7GiU" },

                // --- TATLILAR ---
                new Product { Name = "Fırın Sütlaç", Price = 120, Description = "Üstü kızarmış, tam kıvamında geleneksel sütlaç.", CategoryId = tatliId, IsActive = true, ImageUrl = "https://encrypted-tbn2.gstatic.com/images?q=tbn:ANd9GcQbxCjXpX4EBUoGb_To7RWVqTZEvSi6uLcVGcp5Le3vDWENFnN3DAidh8EDNZxv" },
                new Product { Name = "Künefe", Price = 180, Description = "Bol peynirli, antep fıstıklı sıcak servis.", CategoryId = tatliId, IsActive = true, ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR1kFAD4nlcPZursik2EaKav3yVV0PddCGtr7Fr0iIfeEUyOxpuuJbLAC0OT9wP" },

                // --- İÇECEKLER ---
                new Product { Name = "Ayran", Price = 40, Description = "Bol köpüklü, yayık usulü taze ayran.", CategoryId = icecekId, IsActive = true, ImageUrl = "https://encrypted-tbn1.gstatic.com/images?q=tbn:ANd9GcTSr6begWppuby9jw-Yf5KkSmHSx1dIiECKKr4nzU2zhCJIeNfnBT8Spuru6TH5" },
                new Product { Name = "Şalgam Suyu", Price = 45, Description = "Acılı veya acısız, buz gibi şalgam suyu.", CategoryId = icecekId, IsActive = true, ImageUrl = "https://encrypted-tbn2.gstatic.com/images?q=tbn:ANd9GcQzyk8TBC1gCLhzQ47DDjDliZ-NO1H8kNDcGWsVU_dgqhZS0D1F7MP6mZDE7GiU" },

                // --- ÇORBALAR ---
                new Product { Name = "Mercimek Çorbası", Price = 80, Description = "Süzme mercimek, tereyağlı sos ile.", CategoryId = corbalarId, IsActive = true, ImageUrl = "https://images.unsplash.com/photo-1547592166-23ac45744acd?auto=format&fit=crop&w=800&q=80" }
            };

            foreach (var p in products)
            {
                var existing = await _context.Products.FirstOrDefaultAsync(pr => pr.Name == p.Name);
                if (existing != null)
                {
                    existing.Price = p.Price;
                    existing.Description = p.Description;
                    existing.ImageUrl = p.ImageUrl;
                    existing.CategoryId = p.CategoryId;
                }
                else
                {
                    _context.Products.Add(p);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ProductList));
        }

        public IActionResult QRManagement()
        {
            return View();
        }

        public async Task<IActionResult> DailyLog(string? date)
        {
            DateTime selectedDate;
            if (string.IsNullOrEmpty(date) || !DateTime.TryParse(date, out selectedDate))
            {
                selectedDate = DateTime.Today;
            }
            
            // Mevcut kayıtlı günleri getir
            var recordedDays = await _context.DailyLogRecords
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderDate.Date == selectedDate.Date)
                .ToListAsync();

            var stats = new
            {
                Date = selectedDate,
                TotalOrders = orders.Count,
                CompletedOrders = orders.Count(o => o.Status == "Tamamlandı"),
                CancelledOrders = orders.Count(o => o.Status == "İptal Edildi"),
                PendingOrders = orders.Count(o => o.Status == "Bekliyor"),
                TotalEarnings = orders.Where(o => o.Status == "Tamamlandı").Sum(o => o.TotalAmount)
            };

            ViewBag.Date = selectedDate;
            ViewBag.LogTitle = date ?? selectedDate.ToString("dd MMMM yyyy");
            ViewBag.RecordedDays = recordedDays;
            ViewBag.DailyOrders = orders;
            return View(stats);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDailyLog(string date)
        {
            if (!string.IsNullOrEmpty(date) && !await _context.DailyLogRecords.AnyAsync(r => r.LogDate == date))
            {
                _context.DailyLogRecords.Add(new DailyLogRecord { LogDate = date });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(DailyLog), new { date = date });
        }
        public async Task<IActionResult> PrintReceipt(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }
        public async Task<IActionResult> AllTimeStats()
        {
            var orders = await _context.Orders.ToListAsync();
            var stats = new
            {
                TotalOrders = orders.Count,
                CompletedOrders = orders.Count(o => o.Status == "Tamamlandı"),
                CancelledOrders = orders.Count(o => o.Status == "İptal Edildi"),
                PendingOrders = orders.Count(o => o.Status == "Bekliyor"),
                TotalEarnings = orders.Where(o => o.Status == "Tamamlandı").Sum(o => o.TotalAmount)
            };
            return View(stats);
        }
    }
}
