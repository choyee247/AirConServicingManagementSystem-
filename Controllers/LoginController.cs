using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AirConServicingManagementSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly DBContext _context;

        public LoginController(DBContext context)
        {
            _context = context;
        }


        // ============================================
        // GET: Login
        // ============================================

        [HttpGet]
        public IActionResult Login()
        {
            // Already logged in as Admin
            if (HttpContext.Session.GetInt32("AdminId") != null)
            {
                return RedirectToAction(
                    "Dashboard",
                    "Admin"
                );
            }

            // Already logged in as Technician
            if (HttpContext.Session.GetInt32("TechnicianId") != null)
            {
                return RedirectToAction(
                    "Dashboard",
                    "TechnicianService"
                );
            }

            return View();
        }


        // ============================================
        // POST: Login
        // ============================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string username,
            string password)
        {
            // --------------------------------------------
            // Validation
            // --------------------------------------------

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error =
                    "Username and Password are required.";

                return View();
            }


            // --------------------------------------------
            // Password Hash
            // --------------------------------------------

            var passwordHash =
                ComputeSha256Hash(password);


            // --------------------------------------------
            // Find User
            // --------------------------------------------

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == username &&
                    u.PasswordHash == passwordHash &&
                    u.IsActive == true &&
                    u.IsDeleted != true
                );


            // --------------------------------------------
            // Invalid Login
            // --------------------------------------------

            if (user == null)
            {
                await CreateActivityLog(
                    null,
                    username,
                    null,
                    "Login Failed",
                    "Invalid username or password.",
                    "Login",
                    "Login"
                );

                ViewBag.Error = "Invalid Username or Password.";
                return View();
            }

            // ============================================
            // COMMON SESSION
            // ============================================

            HttpContext.Session.SetInt32(
                "UserId",
                user.Id
            );

            HttpContext.Session.SetString(
                "Username",
                user.Username
            );

            HttpContext.Session.SetString(
                "UserRole",
                user.Role ?? ""
            );

            HttpContext.Session.SetString(
                "SessionStartTime",
                DateTime.Now.ToString()
            );


            // ============================================
            // ACTIVITY LOG - LOGIN
            // ============================================

            await CreateActivityLog(
                user.Id,
                user.Username,
                user.Role,
                "Login",
                $"{user.Role} logged in successfully.",
                "Login",
                "Login"
            );


            // ============================================
            // ADMIN LOGIN
            // ============================================

            if (user.Role == "Admin")
            {
                HttpContext.Session.SetInt32(
                    "AdminId",
                    user.Id
                );

                HttpContext.Session.SetString(
                    "AdminUsername",
                    user.Username
                );

                HttpContext.Session.SetString(
                    "AdminRole",
                    user.Role
                );

                return RedirectToAction(
                    "Dashboard",
                    "Admin"
                );
            }


            // ============================================
            // TECHNICIAN LOGIN
            // Senior + Junior
            // ============================================

            if (user.Role == "Senior" ||
                user.Role == "Junior")
            {
                // ----------------------------------------
                // Technician Role
                // ----------------------------------------

                HttpContext.Session.SetString(
                    "TechnicianRole",
                    user.Role
                );


                // ----------------------------------------
                // First Login
                // Technician Profile Not Created
                // ----------------------------------------

                if (user.TechnicianId == null)
                {
                    return RedirectToAction(
                        "Create",
                        "Technicians"
                    );
                }


                // ----------------------------------------
                // Existing Technician
                // ----------------------------------------

                HttpContext.Session.SetInt32(
                    "TechnicianId",
                    user.TechnicianId.Value
                );

                HttpContext.Session.SetString(
                    "TechnicianName",
                    user.Username
                );


                return RedirectToAction(
                    "Dashboard",
                    "TechnicianService"
                );
            }


            // ============================================
            // UNKNOWN ROLE
            // ============================================

            await CreateActivityLog(
                user.Id,
                user.Username,
                user.Role,
                "Login Failed",
                "User account has an unsupported role.",
                "Login",
                "Login"
            );

            HttpContext.Session.Clear();

            ViewBag.Error =
                "Your account role is not supported.";

            return View();
        }


        // ============================================
        // LOGOUT
        // ============================================

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // --------------------------------------------
            // Get Session Information Before Clear
            // --------------------------------------------

            var userId =
                HttpContext.Session.GetInt32("UserId");

            var username =
                HttpContext.Session.GetString("Username");

            var role =
                HttpContext.Session.GetString("UserRole");


            // --------------------------------------------
            // Activity Log - Logout
            // --------------------------------------------

            if (userId != null)
            {
                await CreateActivityLog(
                    userId.Value,
                    username,
                    role,
                    "Logout",
                    $"{role} logged out successfully.",
                    "Login",
                    "Logout"
                );
            }


            // --------------------------------------------
            // Clear Session
            // --------------------------------------------

            HttpContext.Session.Clear();


            return RedirectToAction(
                "Login",
                "Login"
            );
        }


        // ============================================
        // CREATE ACTIVITY LOG
        // ============================================

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


            _context.ActivityLogs.Add(activityLog);

            await _context.SaveChangesAsync();
        }


        // ============================================
        // SHA256
        // ============================================

        private string ComputeSha256Hash(
            string rawData)
        {
            using var sha256 =
                SHA256.Create();

            var bytes =
                sha256.ComputeHash(
                    Encoding.UTF8.GetBytes(rawData)
                );

            return Convert.ToBase64String(bytes);
        }
    }
}