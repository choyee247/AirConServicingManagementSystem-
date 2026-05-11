using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Controllers.Admin
{
    public class AdminMaintenanceController : Controller
    {
        private readonly DBContext _context;

        public AdminMaintenanceController(DBContext context)
        {
            _context = context;
        }

        // List
        public async Task<IActionResult> Index()
        {
            var data = await _context.MaintenanceSchedules
            .Include(m => m.AirCon)
                .ThenInclude(a => a.Brand)
            .Include(m => m.AirCon)
                .ThenInclude(a => a.Model)
            .Include(m => m.AirCon)
                .ThenInclude(a => a.Customer)
            .Include(m => m.Technician)
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync();

            return View(data);
        }

        // Create GET
        public async Task<IActionResult> Create()
        {
            ViewBag.AirCons = await _context.AirConUnits
                .Include(a => a.Brand)
                .Include(a => a.Model)
                .Where(a => a.IsDeleted != true)
                .ToListAsync();

            ViewBag.Technicians = await _context.Technicians
                .Where(t => t.IsDeleted != true)
                .ToListAsync();

            return View();
        }

        // Create POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaintenanceSchedule model)
        {
            model.Status = "Scheduled";
            _context.MaintenanceSchedules.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Edit GET
        public async Task<IActionResult> Edit(int id)
        {
            var data = await _context.MaintenanceSchedules.FindAsync(id);
            if (data == null) return NotFound();

            ViewBag.AirCons = await _context.AirConUnits
                .Where(a => a.IsDeleted != true)
                .Include(a => a.Brand)
                .Include(a => a.Model)
                .ToListAsync();

            ViewBag.Technicians = await _context.Technicians
                .Where(t => t.IsDeleted != true)
                .ToListAsync();

            return View(data);
        }

        // Edit POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MaintenanceSchedule model)
        {
            var data = await _context.MaintenanceSchedules.FindAsync(model.MaintenanceId);
            if (data == null) return NotFound();

            data.AirConId = model.AirConId;
            data.TechnicianId = model.TechnicianId;
            data.ScheduledDate = model.ScheduledDate;
            data.Status = model.Status;
            data.CompletedDate = model.CompletedDate;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Delete
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.MaintenanceSchedules.FindAsync(id);
            if (data == null) return NotFound();

            _context.MaintenanceSchedules.Remove(data);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
