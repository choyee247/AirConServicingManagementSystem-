using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Controllers
{
    public class CustomerFeedbackController : Controller
    {
        private readonly DBContext _context;

        public CustomerFeedbackController(DBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _context.CustomerFeedbacks
                .Include(x => x.Customer)
                .Include(x => x.Technician)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(data);
        }

        public IActionResult Create()
        {
            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.Technicians = _context.Technicians.ToList();
            ViewBag.Services = _context.ServiceRequests.ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomerFeedback model)
        {
            model.CreatedAt = DateTime.Now;
            model.IsDeleted = false;

            _context.CustomerFeedbacks.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
