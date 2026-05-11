using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Controllers
{
    public class TechnicianMaintenanceController : Controller
    {
        private readonly DBContext _context;

        public TechnicianMaintenanceController(DBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> MyMaintenance()
        {
            int techId = HttpContext.Session.GetInt32("TechnicianId") ?? 0;

            var data = await _context.MaintenanceSchedules
                .Include(m => m.AirCon)
                .Where(m => m.TechnicianId == techId && m.Status != "Completed")
                .OrderBy(m => m.ScheduledDate)
                .ToListAsync();

            return View(data);
        }

        public async Task<IActionResult> Complete(int id)
        {
            int techId = HttpContext.Session.GetInt32("TechnicianId") ?? 0;
            var data = await _context.MaintenanceSchedules.FindAsync(id);

            if (data == null || data.TechnicianId != techId) return NotFound();

            data.Status = "Completed";
            data.CompletedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyMaintenance));
        }
    }

}
