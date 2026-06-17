using AirConServicingManagementSystem.Models;
using AirConServicingManagementSystem.Services;
using AirConServicingManagementSystem.ViewModels;
using AirConServicingManagementSystem.ViewsModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

public class TechnicianServiceController : Controller
{
    private readonly DBContext _context;
    private readonly QrService _qrService;

    public TechnicianServiceController(DBContext context, QrService qrService)
    {
        _context = context;
        _qrService = qrService;
    }
    public async Task<IActionResult> Dashboard()
    {
        int techId = HttpContext.Session.GetInt32("TechnicianId") ?? 0;
        if (techId == 0)
            return RedirectToAction("Login", "TechnicianAuth");

        var dashboard = new TechnicianDashboardVM
        {
            AssignedCount = await _context.ServiceRequests
                .CountAsync(s => s.TechnicianId == techId && s.Status == ServiceStatus.Assigned),

            PendingCount = await _context.ServiceRequests
                .CountAsync(s => s.Status == ServiceStatus.Pending),

            AcceptedCount = await _context.ServiceRequests
                .CountAsync(s => s.TechnicianId == techId && s.Status == ServiceStatus.Accepted),

            CompletedCount = await _context.ServiceRequests
                .CountAsync(s => s.TechnicianId == techId && s.Status == ServiceStatus.Completed),

            RecentServices = await _context.ServiceRequests
                .Include(s => s.Customer)
                .Where(s =>
                    s.Status == ServiceStatus.Pending ||        // Pending (TechnicianId = NULL)
                    s.TechnicianId == techId)                  // Technician jobs
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .ToListAsync()
        };

        return View(dashboard);
    }
    public async Task<IActionResult> Assigned()
    {
        int techId = HttpContext.Session.GetInt32("TechnicianId") ?? 0;

        if (techId == 0)
            return Content("Technician session missing ❌");

        var list = await _context.ServiceRequests
             .Include(x => x.Customer)
             .Include(x => x.AirCon)
                 .ThenInclude(a => a.Brand)
             .Include(x => x.AirCon)
                 .ThenInclude(a => a.Model)
             .Where(x =>
                 x.TechnicianId == techId &&
                 x.Status == "In Progress")
             .OrderByDescending(x => x.RequestedAt)
             .ToListAsync();

        return View(list);
    }


    // =========================
    // Pending Jobs (Not Assigned)
    // =========================
    public async Task<IActionResult> Pending()
    {
        int techId = HttpContext.Session.GetInt32("TechnicianId") ?? 0;

        var data = await _context.ServiceRequests
            .Include(s => s.Customer)
            .Include(s => s.AirCon)
            .Where(s => s.Status == ServiceStatus.Pending)
            .ToListAsync();

        return View(data);
    }

    // =========================
    // Accepted Jobs
    // =========================
    public async Task<IActionResult> Accepted()
    {
        int techId = HttpContext.Session.GetInt32("TechnicianId") ?? 0;

        var data = await _context.ServiceRequests
            .Include(s => s.Customer)
            .Include(s => s.AirCon)
            .Where(s => s.TechnicianId == techId && s.Status == ServiceStatus.Accepted)
            .ToListAsync();

        return View(data);
    }

    // =========================
    // Completed Jobs
    // =========================
    public async Task<IActionResult> Completed()
    {
        int techId = HttpContext.Session.GetInt32("TechnicianId") ?? 0;

        var data = await _context.ServiceRequests
            .Include(s => s.Customer)
            .Include(s => s.AirCon)
            .Where(s => s.TechnicianId == techId && s.Status == ServiceStatus.Completed)
            .ToListAsync();

        return View(data);
    }


    // =========================
    // Accept Service
    // =========================
    public async Task<IActionResult> Accept(int id)
    {
        int techId = HttpContext.Session.GetInt32("TechnicianId") ?? 0;
        if (techId == 0)
            return RedirectToAction("Login", "TechnicianAuth");

        var service = await _context.ServiceRequests.FindAsync(id);

        if (service == null)
            return NotFound();

        if (service.Status != ServiceStatus.Pending)
            return BadRequest("This request is not pending.");

        service.Status = ServiceStatus.Accepted;
        service.TechnicianId = techId;

        await _context.SaveChangesAsync();

        return RedirectToAction("Assigned");
    }



    // =========================
    // Reject Service
    // =========================
    public async Task<IActionResult> Reject(int id)
    {
        var service = await _context.ServiceRequests.FindAsync(id);
        if (service == null)
            return NotFound();

        service.Status = ServiceStatus.Rejected;
        service.TechnicianId = null;

        await _context.SaveChangesAsync();
        return RedirectToAction("Dashboard");
    }

    public async Task<IActionResult> Availability()
    {
        var technicians = await _context.Technicians
            .ToListAsync();

        return View(technicians);
    }

    public async Task<IActionResult> Complete(int id)
    {
        int techId = HttpContext.Session.GetInt32("TechnicianId") ?? 0;

        if (techId == 0)
            return RedirectToAction("Login", "Account");

        var service = await _context.ServiceRequests
            .FirstOrDefaultAsync(s =>
                s.ServiceId == id &&
                s.TechnicianId == techId);

        if (service == null)
            return NotFound();

        if (service.Status == "Completed")
            return Content("Already Completed");

        var model = new AirConServicingManagementSystem.ViewModels.CompleteServiceViewModel
        {
            ServiceId = id
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(CompleteServiceViewModel model)
    {
        int techId = HttpContext.Session.GetInt32("TechnicianId") ?? 0;

        if (techId == 0)
            return RedirectToAction("Login", "Account");

        var service = await _context.ServiceRequests
            .FirstOrDefaultAsync(x =>
                x.ServiceId == model.ServiceId &&
                x.TechnicianId == techId);

        if (service == null)
            return NotFound();

        if (service.Status == "Completed")
            return Content("Already Completed");

        // ❗ VALIDATION FIX (IMPORTANT)
        if (!service.AirConId.HasValue)
            return Content("AirCon not assigned yet ❌");

        // =========================
        // 1. COMPLETE SERVICE REQUEST
        // =========================
        service.Status = "Completed";
        service.CompletedAt = DateTime.Now;

        // =========================
        // 2. NEXT SERVICE CALC
        // =========================
        DateTime nextServiceDate = model.ACCondition switch
        {
            "Good" => DateTime.Now.AddMonths(6),
            "Normal" => DateTime.Now.AddMonths(3),
            "Bad" => DateTime.Now.AddMonths(1),
            _ => DateTime.Now.AddMonths(3)
        };

        // =========================
        // 3. SERVICE RECORD (FIXED)
        // =========================
        var record = await _context.ServiceRecords
            .FirstOrDefaultAsync(r => r.ServiceRequestId == service.ServiceId);

        if (record == null)
        {
            record = new ServiceRecord
            {
                ServiceRequestId = service.ServiceId,
                CustomerId = service.CustomerId,
                AirConUnitId = service.AirConId.Value,   // ✅ SAFE NOW
                TechnicianId = techId,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            _context.ServiceRecords.Add(record);
        }

        record.Status = "Completed";
        record.TechnicianNote = model.TechnicianNote;
        record.ProblemFound = model.ProblemFound;
        record.RepairAction = model.RepairAction;
        record.PartsReplaced = model.PartsReplaced;
        record.ServiceCost = model.ServiceCost;

        record.ServiceType = (model.ServiceTypeList != null && model.ServiceTypeList.Any())
            ? string.Join(", ", model.ServiceTypeList)
            : "";

        record.NextServiceDue = model.NextServiceDue ?? nextServiceDate;
        record.UpdatedAt = DateTime.Now;

        // =========================
        // 4. TECH STATUS RESET
        // =========================
        var technician = await _context.Technicians
            .FirstOrDefaultAsync(t => t.TechnicianId == techId);

        if (technician != null)
            technician.IsAvailable = true;

        var appointment = await _context.Appointments
        .FirstOrDefaultAsync(a => a.AppointmentId == service.AppointmentId);

        if (appointment != null)
        {
            appointment.Status = "Completed";
        }   
        // =========================
        // 5. SAVE ALL
        // =========================
        await _context.SaveChangesAsync();

        TempData["Success"] = "Service Completed Successfully";

        return RedirectToAction("Records", "AdminService");
    }

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Complete(
    //AirConServicingManagementSystem.ViewModels.CompleteServiceViewModel model)
    //{
    //    int techId = HttpContext.Session.GetInt32("TechnicianId") ?? 0;

    //    if (techId == 0)
    //        return Content("Invalid Technician Session");

    //    var service = await _context.ServiceRequests
    //        .FirstOrDefaultAsync(s => s.ServiceId == model.ServiceId);

    //    if (service == null || service.TechnicianId != techId)
    //        return NotFound();

    //    if (service.Status == "Completed")
    //        return Content("Already Completed");

    //    // 1. Update ServiceRequest
    //    service.Status = "Completed";
    //    service.CompletedAt = DateTime.Now;

    //    // 2. Build remarks text
    //    string remarks = "";

    //    if (!string.IsNullOrWhiteSpace(model.SummaryOption))
    //        remarks += model.SummaryOption;

    //    if (!string.IsNullOrWhiteSpace(model.AdditionalRemarks))
    //    {
    //        if (!string.IsNullOrWhiteSpace(remarks))
    //            remarks += Environment.NewLine + Environment.NewLine;

    //        remarks += model.AdditionalRemarks;
    //    }

    //    // 3. Update ServiceRecord
    //    var record = await _context.ServiceRecords
    //        .FirstOrDefaultAsync(r =>
    //            r.ServiceRequestId == model.ServiceId &&
    //            r.TechnicianId == techId &&
    //            r.CustomerId == service.CustomerId &&
    //            r.AirConUnitId == service.AirConId &&
    //            r.Status != "Completed");

    //    if (record != null)
    //    {
    //        record.Status = "Completed";
    //        record.Remarks = remarks;
    //        record.UpdatedAt = DateTime.Now;
    //        record.NextServiceDue = DateTime.Now.AddMonths(3);
    //    }

    //    // 4. Technician available again
    //    var technician = await _context.Technicians
    //        .FirstOrDefaultAsync(t => t.TechnicianId == techId);

    //    if (technician != null)
    //        technician.IsAvailable = true;

    //    // 5. Generate QR token
    //    var token = Guid.NewGuid().ToString("N");

    //    _context.CustomerQrTokens.Add(new CustomerQrToken
    //    {
    //        CustomerId = service.CustomerId,
    //        Token = token,
    //        CreatedAt = DateTime.Now,
    //        ExpiredAt = DateTime.Now.AddDays(7),
    //        IsUsed = false
    //    });

    //    // 6. Save all changes
    //    await _context.SaveChangesAsync();

    //    // 7. Generate QR image
    //    var url = Url.Action("Verify", "Qr",
    //        new { token }, Request.Scheme);

    //    ViewBag.QrImage = "data:image/png;base64," +
    //        Convert.ToBase64String(_qrService.GenerateQr(url));

    //    return View("QrResult");
    //}
    public async Task<IActionResult> GenerateQR(int customerId)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x => x.Id == customerId);

        if (customer == null)
            return Content("Invalid Customer");

        var token = Guid.NewGuid().ToString("N");

        var qr = new CustomerQrToken
        {
            CustomerId = customer.Id, // ✔ valid FK
            Token = token,
            CreatedAt = DateTime.Now,
            ExpiredAt = DateTime.Now.AddDays(7),
            IsUsed = false
        };

        _context.CustomerQrTokens.Add(qr);
        await _context.SaveChangesAsync();

        var url = Url.Action("Verify", "Qr", new { token }, Request.Scheme);

        ViewBag.QrImage = "data:image/png;base64," +
            Convert.ToBase64String(_qrService.GenerateQr(url));

        return View("QrResult");
    }

  

}
