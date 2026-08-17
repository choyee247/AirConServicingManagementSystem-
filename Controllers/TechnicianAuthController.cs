using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;

public class TechnicianAuthController : Controller
{
    private readonly DBContext _context;

    public TechnicianAuthController(DBContext context)
    {
        _context = context;
    }


    // GET Login
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
    string username,
    string password)
    {

        if (string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "Username and Password are required";
            return View();
        }



        var passwordHash = ComputeSha256Hash(password);



        var user = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.Username == username &&
                x.PasswordHash == passwordHash &&
                x.Role != "Admin" &&
                x.IsActive == true &&
                x.IsDeleted != true);



        if (user == null)
        {
            ViewBag.Error = "Invalid Username or Password";
            return View();
        }



        // Save Login Session

        HttpContext.Session.SetInt32(
            "UserId",
            user.Id
        );


        HttpContext.Session.SetString(
            "Username",
            user.Username
        );


        HttpContext.Session.SetString(
            "TechnicianRole",
            user.Role ?? ""
        );



        // First Login Check

        if (user.TechnicianId == null)
        {
            return RedirectToAction(
                "Create",
                "Technicians"
            );
        }



        // Existing Technician

        HttpContext.Session.SetInt32(
            "TechnicianId",
            user.TechnicianId.Value
        );

        HttpContext.Session.SetString(
             "TechnicianName",
             user?.Username ?? "Technician"
         );

        return RedirectToAction(
            "Dashboard",
            "TechnicianService"
        );

    }


    public IActionResult Logout()
    {
        HttpContext.Session.Clear();  
        return RedirectToAction("Login", "Login");
    }

    private string ComputeSha256Hash(string rawData)
    {
        using var sha256 = SHA256.Create();


        var bytes = sha256.ComputeHash(
            Encoding.UTF8.GetBytes(rawData)
        );


        return Convert.ToBase64String(bytes);
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
