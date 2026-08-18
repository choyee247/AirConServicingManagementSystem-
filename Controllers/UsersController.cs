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
        username = username?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username))
        {
            ModelState.AddModelError(
                "username",
                "Username is required."
            );
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(
                "password",
                "Password is required."
            );
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            ModelState.AddModelError(
                "role",
                "Please select a technician role."
            );
        }

        if (!ModelState.IsValid)
        {
            return View();
        }

        bool exists = await _context.Users
            .AnyAsync(x => x.Username.ToLower() == username.ToLower());

        if (exists)
        {
            ModelState.AddModelError(
                "username",
                $"Username '{username}' already exists. Please choose another username."
            );

            return View();
        }

        var user = new User
        {
            Username = username,
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

  
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(
        int id,
        string newPassword)
    {

        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (userId == null || role != "Admin")
        {
            return RedirectToAction("Login", "Login");
        }

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            TempData["ErrorMessage"] =
                "New password is required.";

            return RedirectToAction("Index");
        }

        if (newPassword.Length < 6)
        {
            TempData["ErrorMessage"] =
                "Password must be at least 6 characters.";

            return RedirectToAction("Index");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Id == id &&
                u.IsDeleted != true);

        if (user == null)
        {
            TempData["ErrorMessage"] =
                "User account not found.";

            return RedirectToAction("Index");
        }

        if (user.Role?.Trim() == "Admin")
        {
            TempData["ErrorMessage"] =
                "Admin password cannot be reset from this page.";

            return RedirectToAction("Index");
        }

        user.PasswordHash = ComputeSha256Hash(newPassword);

        user.PasswordResetAt = DateTime.Now;
        user.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        await CreateActivityLog(
            userId.Value,
            user.Username,
            role,
            "Password Reset",
            $"Password reset for technician account: {user.Username}",
            "User",
            "ResetPassword"
        );

        TempData["SuccessMessage"] =
            $"Password reset successfully for {user.Username}.";

        return RedirectToAction("Index");
    }

    private async Task CreateActivityLog(
           int? userId,
           string? username,
           string? role,
           string action,
           string? description,
           string? controller,
           string? actionName)
    {

        var ipAddress =
            HttpContext.Connection
                .RemoteIpAddress?
                .ToString();

        var activityLog = new ActivityLog
        {
            UserId = userId,

            Username = username,

            Role = role,

            Action = action,

            Description = description,

            Controller = controller,

            ActionName = actionName,

            IpAddress = ipAddress,

            CreatedAt = DateTime.Now
        };

        _context.ActivityLogs.Add(
            activityLog
        );

        await _context.SaveChangesAsync();
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
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var admin = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.Role == "Admin" &&
                x.IsDeleted != true);

        if (admin == null)
        {
            return NotFound();
        }

        return View(admin);
    }
}