using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Controllers
{
    public class ComplaintController : Controller
    {
        private readonly DBContext _context;

        public ComplaintController(DBContext context)
        {
            _context = context;
        }

        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> Index()
        {
            var data = await _context.Complaints
                .Include(x => x.Customer)
                .Include(x => x.Technician)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(data);
        }

        // =========================
        // CREATE (GET)
        // =========================
        public IActionResult Create()
        {
            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.Technicians = _context.Technicians.ToList();
            ViewBag.Services = _context.ServiceRequests.ToList();

            return View();
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Complaint model)
        {
            model.CreatedAt = DateTime.Now;
            model.IsDeleted = false;
            model.Status = "New";

            _context.Complaints.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            var data = await _context.Complaints
                .Include(x => x.Customer)
                .Include(x => x.Technician)
                .FirstOrDefaultAsync(x => x.ComplaintId == id);

            return View(data);
        }

        // =========================
        // DELETE (SOFT)
        // =========================
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Complaints.FindAsync(id);

            if (data != null)
            {
                data.IsDeleted = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
