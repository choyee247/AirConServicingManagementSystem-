using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;
public class UsersController : Controller
{
    private readonly DBContext _context;

    public UsersController(DBContext context)
    {
        _context = context;
    }


    // ==========================
    // INDEX - User List
    // ==========================
    public async Task<IActionResult> Index()
    {
        var users = await _context.Users
            .Include(x => x.Technician)
            .Where(x => x.IsDeleted == false)
            .OrderByDescending(x => x.Id)
            .ToListAsync();


        return View(users);
    }



    // ==========================
    // CREATE GET
    // ==========================
   



    // GET: Users/Create
    public IActionResult Create()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    string username,
    string password,
    string role)
    {

        // Username Check

        bool exists = await _context.Users
            .AnyAsync(x => x.Username == username);


        if (exists)
        {
            ViewBag.Error = "Username already exists";
            return View();
        }



        var user = new User
        {
            Username = username,

            // Later replace with Password Hash
            PasswordHash = ComputeSha256Hash(password),

            Role = role,

            TechnicianId = null,

            IsActive = true,

            IsDeleted = false,

            CreatedAt = DateTime.Now
        };



        _context.Users.Add(user);


        await _context.SaveChangesAsync();



        return RedirectToAction(nameof(Index));
    }
    private string ComputeSha256Hash(string rawData)
    {
        using var sha256 = SHA256.Create();

        var bytes = sha256.ComputeHash(
            Encoding.UTF8.GetBytes(rawData)
        );

        return Convert.ToBase64String(bytes);
    }
    // ==========================
    // DELETE / DISABLE ACCOUNT
    // ==========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == id);


        if (user == null)
            return NotFound();



        user.IsDeleted = true;
        user.IsActive = false;
        user.DeletedAt = DateTime.Now;



        await _context.SaveChangesAsync();


        return RedirectToAction(nameof(Index));
    }

}