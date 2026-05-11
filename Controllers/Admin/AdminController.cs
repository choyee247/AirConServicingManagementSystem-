using AirConServicingManagementSystem.Models;
using AirConServicingManagementSystem.ViewsModels;
using Microsoft.AspNetCore.Mvc;

namespace AirConServicingManagementSystem.Controllers.Admin
{
    public class AdminController : Controller
    {
        private readonly DBContext _context;

        public AdminController(DBContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            // 🔐 Session Check
            if (HttpContext.Session.GetInt32("AdminId") == null)
                return RedirectToAction("Login", "AdminLogin");

            // 📊 Dashboard Data
            var model = new AdminDashboardViewModel
            {
                TotalCustomers = _context.Customers
                    .Where(c => c.IsDeleted != true)
                    .Count(),

                TotalTechnicians = _context.ServiceTechnicians
                    .Where(t => t.IsDeleted != true && t.IsActive == true)
                    .Count(),

                ActiveServices = _context.ServiceRecords
                    .Where(s => s.IsDeleted != true)
                    .Count(),

                WarrantyCases = _context.ServiceWarranties.Count()
            };

            return View(model);
        }
    }
}
