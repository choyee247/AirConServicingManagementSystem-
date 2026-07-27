using AirConServicingManagementSystem.Models;
using AirConServicingManagementSystem.ViewsModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            if (HttpContext.Session.GetInt32("AdminId") == null)
                return RedirectToAction("Login", "AdminLogin");

            var today = DateTime.Today;

            var feedbacks = _context.CustomerFeedbacks
                .Include(x => x.Customer)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Take(3)
                .ToList();

            var complaints = _context.Complaints
                .Include(x => x.Customer)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Take(3)
                .ToList();

            var customers = _context.Customers
                .Where(c => c.IsDeleted != true)
                .OrderBy(c => c.Name)
                .ToList();

            var technicians = _context.Technicians
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .ToList();

            var model = new AdminDashboardViewModel
            {
                AdminName = "Admin",

                TotalCustomers = _context.Customers
                    .Count(c => (bool)!c.IsDeleted),

                CurrentDateTime = DateTime.Now,

                TotalTechnicians = _context.Technicians
                    .Count(t => !t.IsDeleted),

                ActiveTechnicians = _context.Technicians
                    .Count(t => !t.IsDeleted && t.IsAvailable),

                AvailableTechnicians = _context.Technicians
                    .Count(t => !t.IsDeleted && t.IsAvailable),

                TotalServices = _context.ServiceRequests
                    .Count(),

                ActiveServices = _context.ServiceRequests
                    .Count(s => s.Status != "Completed"),

                CompletedServices = _context.ServiceRequests
                    .Count(s => s.Status == "Completed"),

                WarrantyCases = _context.Warranties
                    .Count(),
                RecentFeedbacks = feedbacks,

                RecentComplaints = complaints,

                Customers = customers,

                AverageRating = _context.CustomerFeedbacks.Any()
                ? _context.CustomerFeedbacks.Average(x => x.Rating)
                : 0,

                NewComplaints = _context.Complaints.Count(x => x.Status == "New"),

                InProgressComplaints = _context.Complaints.Count(x => x.Status == "In Progress"),

                ResolvedComplaints = _context.Complaints.Count(x => x.Status == "Resolved"),

                ExpiringWarranty = _context.Warranties
                    .Count(w => w.EndDate <= today.AddDays(30)),

                Technicians = technicians,

                //ActiveTechnicians = technicians.Count(x => x.IsAvailable),

                //AvailableTechnicians = technicians.Count(x => x.IsAvailable),

                BusyTechnicians = _context.ServiceRequests
                .Count(x => x.Status == "In Progress"),

                OnLeaveTechnicians = technicians.Count(x => x.LeaveDate != null),

                RecentServiceRecords = _context.ServiceRequests
                .Include(x => x.Customer)
                .Include(x => x.Technician)
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .ToList()
            };

            return View(model);
        }
    }
}
