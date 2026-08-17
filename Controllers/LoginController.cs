using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AirConServicingManagementSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly DBContext _context;

        // ============================================
        // ONE-DAY LOGIN COOKIE
        // ============================================

        private const string LoginCookieName = "AirConServ_Login";


        // ============================================
        // CONSTRUCTOR
        // ============================================

        public LoginController(DBContext context)
        {
            _context = context;
        }


        // ============================================
        // GET: LOGIN
        // ============================================

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            // ============================================
            // CHECK CURRENT SESSION - ADMIN
            // ============================================

            if (HttpContext.Session.GetInt32("AdminId") != null)
            {
                return RedirectToAction(
                    "Dashboard",
                    "Admin"
                );
            }


            // ============================================
            // CHECK CURRENT SESSION - TECHNICIAN
            // ============================================

            if (HttpContext.Session.GetInt32("TechnicianId") != null)
            {
                return RedirectToAction(
                    "Dashboard",
                    "TechnicianService"
                );
            }


            // ============================================
            // CHECK ONE-DAY LOGIN COOKIE
            // ============================================

            if (Request.Cookies.TryGetValue(
                LoginCookieName,
                out string? cookieUserId))
            {
                // ----------------------------------------
                // Convert Cookie UserId
                // ----------------------------------------

                if (int.TryParse(
                    cookieUserId,
                    out int userId))
                {
                    // ------------------------------------
                    // Find Active User
                    // ------------------------------------

                    var user = await _context.Users
                        .FirstOrDefaultAsync(u =>
                            u.Id == userId &&
                            u.IsActive == true &&
                            u.IsDeleted != true
                        );


                    // ------------------------------------
                    // User Still Exists & Active
                    // ------------------------------------

                    if (user != null)
                    {
                        // Restore Session
                        SetUserSession(user);


                        // Redirect According to Role
                        return RedirectUserByRole(user);
                    }
                }


                // ----------------------------------------
                // Invalid / Expired Cookie
                // ----------------------------------------

                Response.Cookies.Delete(
                    LoginCookieName
                );
            }


            // ============================================
            // SHOW LOGIN PAGE
            // ============================================

            return View();
        }


        // ============================================
        // POST: LOGIN
        // ============================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string username,
            string password)
        {
            // ============================================
            // VALIDATION
            // ============================================

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error =
                    "Username and Password are required.";

                return View();
            }


            // ============================================
            // PASSWORD HASH
            // ============================================

            var passwordHash =
                ComputeSha256Hash(password);


            // ============================================
            // FIND USER
            // ============================================

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == username &&
                    u.PasswordHash == passwordHash &&
                    u.IsActive == true &&
                    u.IsDeleted != true
                );


            // ============================================
            // INVALID LOGIN
            // ============================================

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


                ViewBag.Error =
                    "Invalid Username or Password.";

                return View();
            }


            // ============================================
            // CHECK SUPPORTED ROLE
            // ============================================

            if (user.Role != "Admin" &&
                user.Role != "Senior" &&
                user.Role != "Junior")
            {
                await CreateActivityLog(
                    user.Id,
                    user.Username,
                    user.Role,
                    "Login Failed",
                    "User account has an unsupported role.",
                    "Login",
                    "Login"
                );


                ViewBag.Error =
                    "Your account role is not supported.";

                return View();
            }


            // ============================================
            // CREATE SESSION
            // ============================================

            SetUserSession(user);


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
            // CREATE ONE-DAY LOGIN COOKIE
            // ============================================

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,

                Secure = Request.IsHttps,

                SameSite = SameSiteMode.Lax,

                // ----------------------------------------
                // Cookie expires after 24 hours
                // ----------------------------------------

                Expires =
                    DateTimeOffset.Now.AddDays(1),

                IsEssential = true
            };


            Response.Cookies.Append(
                LoginCookieName,
                user.Id.ToString(),
                cookieOptions
            );


            // ============================================
            // REDIRECT USER
            // ============================================

            return RedirectUserByRole(user);
        }


        // ============================================
        // LOGOUT
        // ============================================

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // ============================================
            // GET CURRENT SESSION INFORMATION
            // BEFORE CLEAR
            // ============================================

            var userId =
                HttpContext.Session.GetInt32(
                    "UserId"
                );

            var username =
                HttpContext.Session.GetString(
                    "Username"
                );

            var role =
                HttpContext.Session.GetString(
                    "UserRole"
                );


            // ============================================
            // ACTIVITY LOG - LOGOUT
            // ============================================

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


            // ============================================
            // CLEAR SESSION
            // ============================================

            HttpContext.Session.Clear();


            // ============================================
            // DELETE ONE-DAY LOGIN COOKIE
            // ============================================

            Response.Cookies.Delete(
                LoginCookieName
            );


            // ============================================
            // REDIRECT TO LOGIN
            // ============================================

            return RedirectToAction(
                "Login",
                "Login"
            );
        }


        // ============================================
        // SET USER SESSION
        // ============================================

        private void SetUserSession(User user)
        {
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
            // ADMIN SESSION
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
            }


            // ============================================
            // TECHNICIAN SESSION
            // Senior + Junior
            // ============================================

            if (user.Role == "Senior" ||
                user.Role == "Junior")
            {
                HttpContext.Session.SetString(
                    "TechnicianRole",
                    user.Role
                );


                // ----------------------------------------
                // Existing Technician Profile
                // ----------------------------------------

                if (user.TechnicianId != null)
                {
                    HttpContext.Session.SetInt32(
                        "TechnicianId",
                        user.TechnicianId.Value
                    );

                    HttpContext.Session.SetString(
                        "TechnicianName",
                        user.Username
                    );
                }
            }
        }


        // ============================================
        // REDIRECT USER BY ROLE
        // ============================================

        private IActionResult RedirectUserByRole(
            User user)
        {
            // ============================================
            // ADMIN
            // ============================================

            if (user.Role == "Admin")
            {
                return RedirectToAction(
                    "Dashboard",
                    "Admin"
                );
            }


            // ============================================
            // SENIOR / JUNIOR
            // ============================================

            if (user.Role == "Senior" ||
                user.Role == "Junior")
            {
                // ----------------------------------------
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
                // Technician Dashboard
                // ----------------------------------------

                return RedirectToAction(
                    "Dashboard",
                    "TechnicianService"
                );
            }


            // ============================================
            // UNKNOWN ROLE
            // ============================================

            HttpContext.Session.Clear();

            Response.Cookies.Delete(
                LoginCookieName
            );


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
            // ============================================
            // GET IP ADDRESS
            // ============================================

            var ipAddress =
                HttpContext.Connection
                    .RemoteIpAddress?
                    .ToString();


            // ============================================
            // CREATE ACTIVITY LOG
            // ============================================

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


            // ============================================
            // SAVE
            // ============================================

            _context.ActivityLogs.Add(
                activityLog
            );

            await _context.SaveChangesAsync();
        }


        // ============================================
        // SHA256 PASSWORD HASH
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


            return Convert.ToBase64String(
                bytes
            );
        }
    }
}