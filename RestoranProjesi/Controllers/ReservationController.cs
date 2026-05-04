using Microsoft.AspNetCore.Mvc;
using RestoranProjesi.Data;
using RestoranProjesi.Models.Entities;

namespace RestoranProjesi.Controllers
{
    public class ReservationController : Controller
    {
        private readonly AppDbContext _context;

        public ReservationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Rezervasyonunuz başarıyla alındı!";
                return RedirectToAction("Index", "Home");
            }
            return View(reservation);
        }
    }
}
