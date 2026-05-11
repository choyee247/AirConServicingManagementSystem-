using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class TechnicianAuthController : Controller
{
    private readonly DBContext _context;

    public TechnicianAuthController(DBContext context)
    {
        _context = context;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string phoneNumber)
    {
        var tech = await _context.Technicians
            .FirstOrDefaultAsync(t =>
                t.Email == email &&
                t.PhoneNumber == phoneNumber &&
                t.IsDeleted == false);

        if (tech == null)
        {
            ViewBag.Error = "Invalid Email or Phone Number";
            return View();
        }

        HttpContext.Session.SetInt32("TechnicianId", tech.TechnicianId);
        HttpContext.Session.SetString("TechnicianName", tech.Name);
        HttpContext.Session.SetString("TechnicianRole", tech.TechnicianRole);

        return RedirectToAction("Dashboard", "TechnicianService");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();  
        return RedirectToAction("Login", "TechnicianAuth");
    }
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(Technician tech)
    {
        tech.CreatedAt = DateTime.Now;
        tech.IsAvailable = true;
        tech.IsDeleted = false;

        _context.Technicians.Add(tech);
        await _context.SaveChangesAsync();

        return RedirectToAction("Login");
    }
}
