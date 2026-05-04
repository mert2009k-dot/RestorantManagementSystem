using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestoranProjesi.Data;
using System.Diagnostics;
using RestoranProjesi.Models;

namespace RestoranProjesi.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsFeatured && p.IsActive)
            .OrderByDescending(p => p.Id)
            .Take(8)
            .ToListAsync();

        if (!products.Any())
        {
            products = await _context.Products.Include(p => p.Category).Where(p => p.IsActive).Take(8).ToListAsync();
        }

        return View(products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public async Task<IActionResult> About()
    {
        var employees = await _context.Employees.Where(e => e.IsActive).ToListAsync();
        return View(employees);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
