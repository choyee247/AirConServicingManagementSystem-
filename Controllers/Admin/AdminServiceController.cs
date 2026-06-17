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
                .Include(s => s.Technician)
                .Include(s => s.ServiceRecords)
                .Include(s => s.Appointment)
                .Where(s => s.TechnicianId == HttpContext.Session.GetInt32("TechnicianId"))
                .ToListAsync();

            return View(services);
        }

        //public async Task<IActionResult> Assign(int id)
        //{
        //    var service = await _context.ServiceRequests
        //        .Include(s => s.Customer)
        //        .Include(s => s.AirCon)
        //        .FirstOrDefaultAsync(s => s.ServiceId == id);

        //    if (service == null)
        //        return NotFound();

        //    var today = DateTime.Today;

        //    var technicians = await _context.Technicians
        //        .Where(t => !t.IsDeleted)
        //        .Where(t =>
        //            _context.ServiceRecords.Count(sr =>
        //                sr.TechnicianId == t.TechnicianId &&
        //                sr.CreatedAt.HasValue &&
        //                sr.CreatedAt.Value.Date == today &&
        //                sr.Status != "Completed"
        //            ) < 3
        //        )
        //        .ToListAsync();

        //    ViewBag.Technicians = technicians;

        //    return View(service);
        //}
        //private async Task<bool> CanAssign(int technicianId)
        //{
        //    var today = DateTime.Today;

        //    int count = await _context.ServiceRecords
        //        .CountAsync(sr =>
        //            sr.TechnicianId == technicianId &&
        //            sr.CreatedAt.HasValue &&
        //            sr.CreatedAt.Value.Date == today &&
        //            sr.Status != "Completed"
        //        );

        //    return count < 3;
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Assign(int id, int technicianId)
        //{
        //    var service = await _context.ServiceRequests.FindAsync(id);

        //    if (service == null)
        //        return NotFound();

        //    // Technician workload validation
        //    if (!await CanAssign(technicianId))
        //    {
        //        TempData["ErrorMessage"] =
        //            "This technician already has 3 services today.";

        //        return RedirectToAction(nameof(Assign), new { id });
        //    }

        //    // Assign Technician
        //    service.TechnicianId = technicianId;
        //    service.Status = "Assigned";

        //    // AirCon ရှိမှ ServiceRecord Create
        //    if (service.AirConId.HasValue)
        //    {
        //        _context.ServiceRecords.Add(new ServiceRecord
        //        {
        //            ServiceRequestId = service.ServiceId,
        //            CustomerId = service.CustomerId,
        //            AirConUnitId = service.AirConId.Value,
        //            TechnicianId = technicianId,

        //            ServiceDate = DateTime.Now,
        //            CreatedAt = DateTime.Now,

        //            ServiceType = service.ServiceType,
        //            Remarks = service.Notes,
        //            Status = "In Progress",
        //            IsDeleted = false
        //        });
        //    }

        //    await _context.SaveChangesAsync();

        //    TempData["SuccessMessage"] =
        //        "Technician assigned successfully.";

        //    return RedirectToAction(nameof(Index));
        //}
        public async Task<IActionResult> Assign(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
                return NotFound();

            ViewBag.Technicians = await _context.Technicians
                .Where(t => !t.IsDeleted)
                .ToListAsync();

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int id, int technicianId)
        {
            var appointment = await _context.Appointments
                .FindAsync(id);

            if (appointment == null)
                return NotFound();

            appointment.TechnicianId = technicianId;
            appointment.Status = "Assigned";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Technician assigned successfully.";

            return RedirectToAction("Index", "Appointment");
        }
        public async Task<IActionResult> Records(string search)
        {
            ViewData["CurrentFilter"] = search;

            var technicianId = HttpContext.Session.GetInt32("TechnicianId");

            if (!technicianId.HasValue)
                return RedirectToAction("Login", "TechnicianAuth");

            var records = _context.ServiceRecords
                .Include(r => r.Customer)
                .Include(r => r.Technician)
                .Include(r => r.ServiceRequest)
                    .ThenInclude(a => a.Appointment)
                .Include(r => r.AirConUnit)
                    .ThenInclude(a => a.Brand)
                .Include(r => r.AirConUnit)
                    .ThenInclude(a => a.Model)
                .Where(r => r.IsDeleted != true
                         && r.TechnicianId == technicianId.Value);

            if (!string.IsNullOrEmpty(search))
            {
                records = records.Where(r =>
                    (r.Customer != null && r.Customer.Name.Contains(search)) ||
                    (r.Technician != null && r.Technician.Name.Contains(search)) ||
                    (r.ServiceType != null && r.ServiceType.Contains(search))
                );
            }

            return View(await records
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync());
        }
        public async Task<IActionResult> Detail(int id)
        {
            int techId = HttpContext.Session.GetInt32("TechnicianId") ?? 0;

            if (techId == 0)
                return RedirectToAction("Login", "Account");

            var record = await _context.ServiceRecords
                .Include(x => x.Customer)
                .Include(x => x.AirConUnit)
                    .ThenInclude(x =>x.Brand)
                    .ThenInclude(x =>x.AirConModels)
                .Include(x => x.Technician)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.TechnicianId == techId);

            if (record == null)
                return NotFound();

            return View(record);
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
