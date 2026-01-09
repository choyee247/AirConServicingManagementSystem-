using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AirConServicingManagementSystem.Controllers.Admin
{
    public class AdminLoginController : Controller
    {
        private readonly DBContext _context;

        public AdminLoginController(DBContext context)
        {
            _context = context;
        }

        // GET: Admin Login
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("AdminId") != null)
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            return View();
        }

        // POST: Admin Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Username and password are required.";
                return View();
            }

            // Optional: Strong password validation
            if (!IsStrongPassword(password))
            {
                ViewBag.Error = "Password must be at least 8 characters long and include uppercase, lowercase, digit, and special character";
                return View();
            }

            var passwordHash = ComputeSha256Hash(password);

            var admin = _context.Users
                .FirstOrDefault(u =>
                    u.Username == username &&
                    u.PasswordHash == passwordHash &&
                    u.Role == "Admin" &&
                    u.IsActive == true &&
                    u.IsDeleted != true);

            if (admin == null)
            {
                ViewBag.Error = "Invalid username or password";
                return View();
            }

            // Set session
            HttpContext.Session.SetInt32("AdminId", admin.Id);
            HttpContext.Session.SetString("AdminUsername", admin.Username);
            HttpContext.Session.SetString("AdminRole", admin.Role!);
            HttpContext.Session.SetString("SessionStartTime", DateTime.Now.ToString());

            return RedirectToAction("Dashboard", "Admin");
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // -----------------------------
        // Helper Methods
        // -----------------------------

        private string ComputeSha256Hash(string rawData)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return Convert.ToBase64String(bytes);
        }

        private bool IsStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            bool hasUpper = Regex.IsMatch(password, "[A-Z]");
            bool hasLower = Regex.IsMatch(password, "[a-z]");
            bool hasDigit = Regex.IsMatch(password, "[0-9]");
            bool hasSpecial = Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]");

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }
        // GET: Change Password
        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrEmpty(currentPassword) ||
                string.IsNullOrEmpty(newPassword) ||
                string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "New password and confirm password do not match.";
                return View();
            }

            if (!IsStrongPassword(newPassword))
            {
                ViewBag.Error = "Password must contain uppercase, lowercase, number and special character.";
                return View();
            }

            var admin = _context.Users.FirstOrDefault(u => u.Id == adminId);

            if (admin == null)
            {
                return RedirectToAction("Login");
            }

            // Check current password
            var currentHash = ComputeSha256Hash(currentPassword);
            if (admin.PasswordHash != currentHash)
            {
                ViewBag.Error = "Current password is incorrect.";
                return View();
            }

            // Update password
            admin.PasswordHash = ComputeSha256Hash(newPassword);
            _context.SaveChanges();

            ViewBag.Success = "Password changed successfully.";
            return View();
        }
    }
}
