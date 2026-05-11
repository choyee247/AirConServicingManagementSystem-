using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Controllers
{
    public class CustomerMaintenanceController : Controller
    {
        private readonly DBContext _context;

        public CustomerMaintenanceController(DBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> MyMaintenance()
        {
            int customerId = HttpContext.Session.GetInt32("CustomerId") ?? 0;

            var data = await _context.MaintenanceSchedules
                .Include(m => m.AirCon)
                .Where(m => m.AirCon.CustomerId == customerId)
                .OrderByDescending(m => m.ScheduledDate)
                .ToListAsync();

            return View(data);
        }

        // Confirm Maintenance
        public async Task<IActionResult> Confirm(int id)
        {
            var data = await _context.MaintenanceSchedules.FindAsync(id);
            if (data == null) return NotFound();

            data.Status = "Pending";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyMaintenance));
        }

        // Reschedule
        [HttpPost]
        public async Task<IActionResult> Reschedule(int id, DateTime newDate)
        {
            var data = await _context.MaintenanceSchedules.FindAsync(id);
            if (data == null) return NotFound();

            data.ScheduledDate = newDate;
            data.Status = "Scheduled";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyMaintenance));
        }
    }

}
