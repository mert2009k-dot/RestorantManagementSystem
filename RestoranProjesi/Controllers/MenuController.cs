using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using RestoranProjesi.Data;
using RestoranProjesi.Models.Entities;
using System.Security.Claims;

namespace RestoranProjesi.Controllers
{
    public class MenuController : Controller
    {
        private readonly AppDbContext _context;

        public MenuController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? table)
        {
            // 🔒 Masa numarası doğrulama (1-100 arası)
            if (table.HasValue && table.Value > 0 && table.Value <= 100)
            {
                HttpContext.Session.SetInt32("TableNumber", table.Value);
                ViewBag.TableMessage = $"Hoş geldiniz! Şu an Masa {table.Value} üzerinden sipariş vermektesiniz.";
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var productsList = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .ToListAsync();

            if (userId != null)
            {
                ViewBag.FavoriteIds = await _context.Favorites
                    .Where(f => f.UserId == userId)
                    .Select(f => f.ProductId)
                    .ToListAsync();
            }

            return View(productsList);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            if (existingFavorite != null)
            {
                _context.Favorites.Remove(existingFavorite);
            }
            else
            {
                _context.Favorites.Add(new Favorite { UserId = userId, ProductId = productId });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddToFavorites(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            var exists = await _context.Favorites.AnyAsync(f => f.UserId == userId && f.ProductId == id);
            if (!exists)
            {
                _context.Favorites.Add(new Favorite { UserId = userId, ProductId = id });
                await _context.SaveChangesAsync();
                TempData["CartMessage"] = "Ürün favorilerinize eklendi!";
            }
            else
            {
                TempData["CartMessage"] = "Ürün zaten favorilerinizde.";
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> MyFavorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            var favorites = await _context.Favorites
                .Include(f => f.Product)
                .ThenInclude(p => p!.Category)
                .Where(f => f.UserId == userId)
                .ToListAsync();

            return View(favorites);
        }

        [HttpPost]
        public IActionResult ExitTable()
        {
            HttpContext.Session.Remove("TableNumber");
            return RedirectToAction("Index", "Home");
        }
    }
}
