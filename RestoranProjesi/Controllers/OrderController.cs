using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestoranProjesi.Data;
using RestoranProjesi.Models.Entities;

namespace RestoranProjesi.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public OrderController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(string paymentMethod)
        {
            var user = await _userManager.GetUserAsync(User);
            var tableNumber = HttpContext.Session.GetInt32("TableNumber");

            if (user == null && !tableNumber.HasValue)
            {
                return Challenge();
            }

            if (string.IsNullOrEmpty(paymentMethod))
            {
                TempData["ErrorMessage"] = "Lütfen bir ödeme yöntemi seçiniz.";
                return RedirectToAction("Index", "Cart");
            }

            // 🔒 Ödeme yöntemi doğrulama - sadece izin verilen değerler
            var allowedMethods = new[] { "Nakit", "Kredi Kartı" };
            if (!allowedMethods.Contains(paymentMethod))
            {
                TempData["ErrorMessage"] = "Geçersiz ödeme yöntemi.";
                return RedirectToAction("Index", "Cart");
            }

            var cartId = HttpContext.Session.GetString("CartId");
            if (string.IsNullOrEmpty(cartId)) return RedirectToAction("Index", "Cart");

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.CartId == cartId)
                .ToListAsync();

            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            var total = cartItems.Sum(c => c.Quantity * (c.Product?.Price ?? 0));
            
            var order = new Order
            {
                UserId = user?.Id,
                OrderDate = DateTime.Now,
                TotalAmount = total,
                Status = "Bekliyor",
                DeliveryType = tableNumber.HasValue ? "Masa Servisi" : "Gel Al",
                TableNumber = tableNumber,
                PaymentMethod = paymentMethod,
                OrderItems = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    UnitPrice = c.Product?.Price ?? 0
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            if (user != null)
            {
                TempData["SuccessMessage"] = "Siparişiniz başarıyla alındı!";
                return RedirectToAction(nameof(MyOrders));
            }
            else
            {
                ViewBag.OrderTotal = total;
                ViewBag.TableNo = tableNumber;
                return View("OrderSuccessGuest");
            }
        }
    }
}
