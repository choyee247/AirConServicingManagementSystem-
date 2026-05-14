using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Controllers.Admin
{
    public class AdminServiceController : Controller
    {
        private readonly DBContext _context;

        public AdminServiceController(DBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var services = await _context.ServiceRequests
                .Include(s => s.Customer)
                .Include(s => s.AirCon)
                .ToListAsync();

            return View(services);
        }

        public async Task<IActionResult> Assign(int id)
        {
            var service = await _context.ServiceRequests
                .Include(s => s.Customer)
                .Include(s => s.AirCon)
                .FirstOrDefaultAsync(s => s.ServiceId == id);

            if (service == null)
                return NotFound();

            var today = DateTime.Today;

            var technicians = await _context.Technicians
                .Where(t => !t.IsDeleted)
                .Where(t =>
                    _context.ServiceRecords.Count(sr =>
                        sr.TechnicianId == t.TechnicianId &&
                        sr.CreatedAt.HasValue &&
                        sr.CreatedAt.Value.Date == today &&
                        sr.Status != "Completed"
                    ) < 3
                )
                .ToListAsync();

            ViewBag.Technicians = technicians;

            return View(service);
        }
        private async Task<bool> CanAssign(int technicianId)
        {
            var today = DateTime.Today;

            int count = await _context.ServiceRecords
                .CountAsync(sr =>
                    sr.TechnicianId == technicianId &&
                    sr.CreatedAt.HasValue &&
                    sr.CreatedAt.Value.Date == today &&
                    sr.Status != "Completed"
                );

            return count < 3;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int id, int technicianId)
        {
            var service = await _context.ServiceRequests.FindAsync(id);

            if (service == null)
                return NotFound();

            // 🔥 HARD VALIDATION (REAL BLOCK)
            if (!await CanAssign(technicianId))
            {
                TempData["ErrorMessage"] =
                    "This technician already has 3 services today.";

                return RedirectToAction(nameof(Assign), new { id });
            }

            service.TechnicianId = technicianId;
            service.Status = "Assigned";

            _context.ServiceRecords.Add(new ServiceRecord
            {
                ServiceRequestId = service.ServiceId,
                CustomerId = service.CustomerId,
                AirConUnitId = (int)service.AirConId!,
                TechnicianId = technicianId,

                ServiceDate = DateTime.Now,
                CreatedAt = DateTime.Now,

                ServiceType = service.ServiceType,
                Remarks = service.Notes,
                Status = "In Progress",
                IsDeleted = false
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Technician assigned successfully.";

            return RedirectToAction("Index");
        }
   
        public async Task<IActionResult> Records(string search)
        {
            ViewData["CurrentFilter"] = search;

            var records = _context.ServiceRecords
                .Include(r => r.Customer)
                .Include(r => r.AirConUnit)
                .Include(r => r.Technician)
                .Where(r => r.IsDeleted != true)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                records = records.Where(r =>
                    r.Customer.Name.Contains(search) ||
                    r.Technician.Name.Contains(search) ||
                   // r.AirConUnit.Model.Contains(search) ||
                    r.ServiceType.Contains(search)
                );
            }

            return View(await records.OrderByDescending(x => x.CreatedAt).ToListAsync());
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.ServiceRecords.FindAsync(id);

            if (record == null)
                return NotFound();

            record.IsDeleted = true;
            record.DeletedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Record deleted successfully.";

            return RedirectToAction(nameof(Records));
        }
        public async Task<IActionResult> Calendar()
        {
            var plans = await _context.TechnicianSchedulePlans
                .Include(p => p.Technician)
                .ToListAsync();

            return View(plans);
        }
        public async Task<IActionResult> GetPlans()
        {
            var plans = await _context.TechnicianSchedulePlans
                .Select(p => new
                {
                    title = p.Technician.Name + " - " + p.CustomerName,
                    start = p.PlannedDate
                })
                .ToListAsync();

            return Json(plans);
        }
        public async Task<IActionResult> CreatePlan(int serviceId)
        {
            var service = await _context.ServiceRequests
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

            if (service == null)
                return NotFound();

            ViewBag.Technicians = await _context.Technicians
                .Where(t => !t.IsDeleted)
                .ToListAsync();

            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePlan(TechnicianSchedulePlan model)
        {
            var service = await _context.ServiceRequests
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.ServiceId == model.ServiceRequestId);

            if (service == null)
                return NotFound();

            // ✅ FIX HERE (IMPORTANT)
            model.PlannedDate = service?.RequestedAt ?? DateTime.Now;

            model.CustomerId = service.CustomerId;
            model.CustomerName = service.Customer.Name;
            model.Location = service.Location;
            model.PlanType = "Service";
            model.Priority = "Normal";
            model.Status = "Planned";
            model.Title = $"Service for {service.Customer.Name}";
            model.CreatedAt = DateTime.Now;

            _context.TechnicianSchedulePlans.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Plan created successfully.";

            return RedirectToAction("Calendar");
        }
    }
}
