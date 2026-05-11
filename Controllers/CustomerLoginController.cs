using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Controllers
{
    public class CustomerLoginController : Controller
    {
        private readonly DBContext _context;

        public CustomerLoginController(DBContext context)
        {
            _context = context;
        }

        // GET: /CustomerLogin/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /CustomerLogin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string phone)
        {
            phone = phone.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phone))
            {
                ViewBag.Error = "Email and Phone are required.";
                return View();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => /*c.Email == email &&*/ c.Phone == phone && c.IsDeleted != true);

            if (customer == null)
            {
                ViewBag.Error = "Invalid credentials.";
                return View();
            }

            // 🔐 Set Session
            HttpContext.Session.SetInt32("CustomerId", customer.Id);
            HttpContext.Session.SetString("CustomerName", customer.Name);

            return RedirectToAction("Index", "CustomerDashboard");
        }

        // GET: /CustomerLogin/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /CustomerLogin/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Customer model)
        {
            model.Phone = model.Phone.Trim();

            if (!ModelState.IsValid)
                return View(model);

            // Check email already exists
            //bool exists = await _context.Customers.AnyAsync(c => c.Email == model.Email);
            //if (exists)
            //{
            //    ViewBag.Error = "Email already registered.";
            //    return View(model);
            //}

            model.CreatedAt = DateTime.Now;
            _context.Customers.Add(model);
            await _context.SaveChangesAsync();

            // Auto login after register
            HttpContext.Session.SetInt32("CustomerId", model.Id);
            HttpContext.Session.SetString("CustomerName", model.Name);

            return RedirectToAction("Login", "CustomerLogin");
        }

        // GET: /CustomerLogin/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "CustomerLogin");
        }
    }
}
