using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestoranProjesi.Data;
using RestoranProjesi.Models.Entities;

namespace RestoranProjesi.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // Get CartId from Session or Cookie (Simple implementation using Session)
        private string GetCartId()
        {
            var cartId = HttpContext.Session.GetString("CartId");
            if (string.IsNullOrEmpty(cartId))
            {
                cartId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("CartId", cartId);
            }
            return cartId;
        }

        public async Task<IActionResult> Index()
        {
            var cartId = GetCartId();
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.CartId == cartId)
                .ToListAsync();

            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            // 🔒 Girdi doğrulama
            if (quantity < 1) quantity = 1;
            if (quantity > 50) quantity = 50;

            // 🔒 Ürün kontrolü - sahte ürün ID'si engelle
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);
            if (product == null) return BadRequest("Geçersiz ürün.");

            var cartId = GetCartId();
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.CartId == cartId && c.ProductId == productId);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartId = cartId,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity;
                // 🔒 Maksimum miktar sınırı
                if (cartItem.Quantity > 50) cartItem.Quantity = 50;
            }

            await _context.SaveChangesAsync();
            
            TempData["CartMessage"] = "Başarıyla eklendi";
            
            // 🔒 Open Redirect koruması
            string referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && Url.IsLocalUrl(new Uri(referer).PathAndQuery))
            {
                return Redirect(referer);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var cartId = GetCartId();
            // 🔒 Sahiplik kontrolü - sadece kendi sepetindeki ürünü silebilir
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.Id == id && c.CartId == cartId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            var cartId = GetCartId();
            // 🔒 Sahiplik kontrolü + miktar sınırı
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.Id == id && c.CartId == cartId);
            if (cartItem != null && quantity > 0 && quantity <= 50)
            {
                cartItem.Quantity = quantity;
                await _context.SaveChangesAsync();
            }
            else if (cartItem != null && quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
