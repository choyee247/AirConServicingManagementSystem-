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

        var technician = await _context.Technicians
            .FirstOrDefaultAsync(x => x.TechnicianId == techId);

        var appointments = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Technician)
            .Where(a => a.TechnicianId == techId || a.TechnicianId == null)
            .OrderByDescending(a => a.ScheduledDate)
            .Take(3)
            .ToListAsync();

        var reminders = await _context.ServiceReminders
             .Include(r => r.Customer)
             .Include(r => r.AirConUnit)
             .Where(r => r.IsDeleted == false && r.SentStatus == false)
             .OrderBy(r => r.ReminderDate)
             .Take(3)
             .ToListAsync();

        var recentTasks = await _context.ServiceRequests
            .Include(x => x.Customer)
            .OrderByDescending(x => x.CreatedAt)
            .Take(3)
            .ToListAsync();

        var today = DateTime.Today;

        var todayJobs = await _context.TechnicianSchedulePlans
            .Include(x => x.Customer)
            .Where(x =>
                x.TechnicianId == techId &&
                x.PlannedDate.Date == today)
            .OrderBy(x => x.PlannedDate)
            .ToListAsync();

        var complaints = await _context.Complaints
            .Include(x => x.Customer)
            .Where(x => x.TechnicianId == techId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Take(3)
            .ToListAsync();

        var feedbacks = await _context.CustomerFeedbacks
            .Include(x => x.Customer)
            .Where(x => x.TechnicianId == techId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Take(3)
            .ToListAsync();

        var dashboard = new TechnicianDashboardVM
        {
            TechnicianName = technician?.Name ?? "Technician",

            CurrentDateTime = DateTime.Now,

            AssignedCount = await _context.ServiceRequests
                .CountAsync(s => s.TechnicianId == techId && s.Status == "Assigned"),

            PendingCount = await _context.ServiceRequests
                .CountAsync(s => s.TechnicianId == techId && s.Status == "Pending"),

            InProgressCount = await _context.ServiceRequests
                .CountAsync(s => s.TechnicianId == techId && s.Status == "In Progress"),

            CompletedCount = await _context.ServiceRequests
                .CountAsync(s => s.TechnicianId == techId && s.Status == "Completed"),

            RecentAppointments = appointments,

            //RecentServices = await _context.ServiceRequests
            //    .Include(s => s.Customer)
            //    .Where(s => s.TechnicianId == techId)
            //    .OrderByDescending(x => x.CreatedAt)
            //    .Take(5)
            //    .ToListAsync(),

            TodayJobs = todayJobs,

            ReminderCount = reminders.Count,

            ServiceReminders = reminders,

            Complaints = complaints,

            Feedbacks = feedbacks,

            ComplaintCount = complaints.Count,

            FeedbackCount = feedbacks.Count,

            AverageRating = feedbacks.Any()? feedbacks.Average(x => x.Rating): 0,

            RecentTasks = recentTasks ?? new List<ServiceRequest>()
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
             .Include(x => x.AirConUnits)
                 .ThenInclude(a => a.Brand)
             .Include(x => x.AirConUnits)
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
            .Include(s => s.AirConUnits)
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
            .Include(s => s.AirConUnits)
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
            .Include(s => s.AirConUnits)
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
            .Where(x => !x.IsDeleted)
            .Include(x => x.Appointments)
            .ToListAsync();


        foreach (var tech in technicians)
        {
            tech.IsAvailable = !tech.Appointments
                .Any(a =>
                    a.Status != "Completed"
                );
        }


        return View(technicians);
    }

    public async Task<IActionResult> Complete(int id)
    {
        int techId = HttpContext.Session.GetInt32("TechnicianId") ?? 0;


        if (techId == 0)
            return RedirectToAction("Login", "Account");



        // 1. Get Service Request
        var service = await _context.ServiceRequests

            .Include(x => x.Customer)

            .Include(x => x.Technician)

            .FirstOrDefaultAsync(x =>
                x.ServiceId == id &&
                x.TechnicianId == techId);



        if (service == null)
            return NotFound();



        if (service.Status == "Completed")
            return Content("Already Completed");




        // 2. Get Customer AirCon Units

        var aircons = await _context.AirConUnits

            .Include(x => x.Brand)

            .Include(x => x.Model)

            .Include(x => x.Warranty)

            .Include(x => x.MaintenanceSchedules)

            .Include(x => x.ServiceRecordUnits)
                .ThenInclude(x => x.ServiceRecord)

            .Where(x =>
                x.CustomerId == service.CustomerId &&
                x.IsDeleted == false)

            .ToListAsync();




        // 3. Group Brand + Model + Installation Type

        var unitGroups = aircons

            .GroupBy(x => new
            {
                x.BrandId,
                x.ModelId,
                x.InstallationType

            })


            .Select(g =>
            {

                var first = g.First();



                var maintenance =
                    first.MaintenanceSchedules
                    .OrderByDescending(x => x.MaintenanceId)
                    .FirstOrDefault();



                return new ServiceUnitVM
                {


                    // All AC IDs in this group

                    AirConUnitIds =
                        g.Select(x => x.Id)
                        .ToList(),



                    BrandName =
                        first.Brand?.BrandName,



                    ModelName =
                        first.Model?.ModelName,



                    // Total AC Count
                    TotalQuantity =
                        g.Count(),

                    // Already Completed Count
                    //CompletedQuantity =
                    //    g.Count(x =>
                    //        x.ServiceRecordUnits.Any(r =>
                    //            r.ServiceRecord.Status == "Completed"
                    //        )
                    //    ),

                    CompletedQuantity =
                    g.Count(x =>
                        x.ServiceRecordUnits.Any()
                    ),

                    InstallationType =
                        first.InstallationType,




                    // Warranty / Contract

                    HasWarranty =
                        first.Warranty != null,



                    WarrantyStartDate =
                        first.Warranty?.StartDate,



                    WarrantyEndDate =
                        first.Warranty?.EndDate,



                    ContractMonths =
                first.Warranty != null
                    ? (first.Warranty.EndDate - first.Warranty.StartDate).Days / 30
                    : first.WarrantyPeriodMonths,



                    IsFreeService =
                        first.Warranty != null &&
                        first.Warranty.EndDate >= DateTime.Now,




                    // Maintenance
                    MaintenanceMonths =
                first.InstallationType == "New"
                    ? first.NextServiceOption ?? 3
                    : null,



                    // Default

                    IsSelected = false


                };

            })

            .ToList();



        unitGroups = unitGroups
        .Where(x => x.RemainingQuantity > 0)
        .ToList();



        // 4. Prepare ViewModel


        var model = new CompleteServiceViewModel
        {

            ServiceId = service.ServiceId,


            CustomerName =
                service.Customer?.Name,


            PhoneNumber =
                service.Customer?.Phone,


            Address =
                service.Customer?.Address,



            TechnicianName =
                service.Technician?.Name,



            JobNo =
                "JOB-" + service.ServiceId,



            Units = unitGroups,


            CompletedDate =
                DateTime.Now

        };





        return View(model);

    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(
        CompleteServiceViewModel model)
    {

        int techId =
            HttpContext.Session.GetInt32("TechnicianId") ?? 0;


        if (techId == 0)
            return RedirectToAction(
                "Login",
                "Account");


        foreach (var u in model.Units)
        {
            Console.WriteLine(
                $"TYPE={u.InstallationType}, " +
                $"Brand={u.BrandName}, " +
                $"Selected={u.IsSelected}, " +
                $"Qty={u.ServiceQuantity}, " +
                $"IDs={u.AirConUnitIds.Count}, " +
                $"Problem={u.ProblemFound}"
            );
        }

        // ==============================
        // Get Service Request
        // ==============================

        var service =
            await _context.ServiceRequests
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x =>
                x.ServiceId == model.ServiceId &&
                x.TechnicianId == techId);



        if (service == null)
            return NotFound();



        if (service.Status == "Completed")
            return Content("Already Completed");




        // ==============================
        // Create Service Record
        // ==============================


        var record =
            await _context.ServiceRecords
            .FirstOrDefaultAsync(x =>
                x.ServiceRequestId == service.ServiceId);



        if (record == null)
        {

            record = new ServiceRecord
            {

                ServiceRequestId =
                    service.ServiceId,


                CustomerId =
                    service.CustomerId,


                TechnicianId =
                    techId,


                Status =
                    "Completed",


                CreatedAt =
                    DateTime.Now,


                IsDeleted =
                    false

            };


            _context.ServiceRecords.Add(record);



            await _context.SaveChangesAsync();


        }




        record.TechnicianNote =
            model.TechnicianNote;



        record.ServiceCost =
            model.GrandTotal;



        record.UpdatedAt =
            DateTime.Now;

        // ==============================
        // Selected AC Units
        // ==============================



        var selectedUnits = model.Units
     .Where(x =>
         x.IsSelected == true &&
         x.ServiceQuantity > 0 &&
         x.AirConUnitIds.Any())
     .ToList();



        Console.WriteLine("SELECTED COUNT = " + selectedUnits.Count);


        foreach (var item in selectedUnits)
        {
            Console.WriteLine(
                $"INSERT READY => {item.BrandName} Qty={item.ServiceQuantity}"
            );
        }

        Console.WriteLine("======== BEFORE SERVICE RECORD UNIT ========");

        foreach (var unit in model.Units)
        {
            Console.WriteLine(
                $"Brand:{unit.BrandName}, " +
                $"Selected:{unit.IsSelected}, " +
                $"Qty:{unit.ServiceQuantity}, " +
                $"Ids:{unit.AirConUnitIds.Count}"
            );
        }



        foreach (var unit in model.Units
            .Where(x =>
                x.IsSelected ==true &&
                x.ServiceQuantity > 0 &&
                x.AirConUnitIds.Any()))
            {


            // Take Only Today Quantity

            var serviceAircons =
            unit.AirConUnitIds
            .Where(x => x > 0)
            .Take(unit.ServiceQuantity)
            .ToList();

            foreach (var airconId in serviceAircons)
            {
                DateTime nextDate =
                    unit.Condition switch
                    {

                        "Good" =>
                        DateTime.Now.AddMonths(6),


                        "Normal" =>
                        DateTime.Now.AddMonths(3),


                        "Bad" =>
                        DateTime.Now.AddMonths(1),


                        _ =>
                        DateTime.Now.AddMonths(3)

                    };




                // ==========================
                // Service Record Unit
                // ==========================


                var recordUnit =
                    new ServiceRecordUnit
                    {


                        ServiceRecordId =
                            record.Id,


                        AirConUnitId =
                            airconId,


                        Accondition =
                            unit.Condition,


                        ProblemFound =
                            unit.ProblemFound,


                        RepairAction =
                            unit.RepairAction,


                        NextServiceDue =
                            nextDate,


                        CreatedAt =
                            DateTime.Now


                    };



                _context.ServiceRecordUnits.Add(recordUnit);

                await _context.SaveChangesAsync();

                Console.WriteLine(
                 $"INSERTED SRU ID = {recordUnit.Id}"
                
);


                // ==========================
                // Reminder
                // ==========================


                var reminder =
                    new ServiceReminder
                    {

                        CustomerId =
                            service.CustomerId,


                        AirConUnitId =
                            airconId,


                        ServiceRequestId =
                            service.ServiceId,


                        ReminderDate =
                            nextDate,


                        ReminderType =
                            "Next",


                        SentStatus = false,

                        IsDeleted = false,

                        CreatedAt =
                            DateTime.Now

                    };



                _context.ServiceReminders
                    .Add(reminder);




                // ==========================
                // Update AC Next Service
                // ==========================


                var ac =
                    await _context.AirConUnits
                    .FirstOrDefaultAsync(x =>
                        x.Id == airconId);



                if (ac != null)
                {

                    ac.UpdatedAt =
                        DateTime.Now;

                }



            }



        }





        // ==============================
        // Save Service Photos
        // ==============================


        if (model.ServicePhotos != null &&
           model.ServicePhotos.Count > 0)
        {


            string folder =
            Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot/images/service");



            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);




            foreach (var photo in model.ServicePhotos)
            {


                string fileName =
                    Guid.NewGuid()
                    +
                    Path.GetExtension(
                        photo.FileName);



                string filePath =
                    Path.Combine(
                        folder,
                        fileName);



                using (var stream =
                    new FileStream(
                        filePath,
                        FileMode.Create))
                {

                    await photo.CopyToAsync(stream);

                }





                var servicePhoto =
                    new ServicePhoto
                    {

                        ServiceRecordId =
                            record.Id,


                        PhotoPath =
                            "/images/service/"
                            + fileName,


                        PhotoType =
                            "Service",


                        CreatedAt =
                            DateTime.Now,


                        IsDeleted =
                            false

                    };



                _context.ServicePhotos
                    .Add(servicePhoto);


            }

        }





        // ==============================
        // Check Remaining Quantity
        // ==============================


        //bool hasRemaining = false;



        //foreach (var unit in model.Units)
        //{

        //    if (unit.ServiceQuantity <
        //       unit.TotalQuantity)
        //    {

        //        hasRemaining = true;

        //    }

        //}

        //bool hasRemaining = model.Units
        //.Where(x => x.IsSelected)
        //.Any(x =>
        //    x.ServiceQuantity < x.TotalQuantity);
        bool hasRemaining = model.Units
        .Where(x => x.IsSelected)
        .Any(x =>
        {
            int afterComplete =
                x.CompletedQuantity + x.ServiceQuantity;

            return afterComplete < x.TotalQuantity;
        });


        if (hasRemaining)
        {

            service.Status =
                "In Progress";


        }
        else
        {

            service.Status =
                "Completed";


            service.CompletedAt =
                DateTime.Now;

        }






        // ==============================
        // Technician Available
        // ==============================


        var technician =
            await _context.Technicians
            .FirstOrDefaultAsync(x =>
                x.TechnicianId == techId);



        if (technician != null)
        {

            technician.IsAvailable =
                true;

        }







        // ==============================
        // Appointment Update
        // ==============================


        var appointment =
            await _context.Appointments
            .FirstOrDefaultAsync(x =>
                x.AppointmentId ==
                service.AppointmentId);



        if (appointment != null)
        {

            appointment.Status =
                service.Status;

        }






        await _context.SaveChangesAsync();






        TempData["Success"] =
            "Service Saved Successfully";





        // ==============================
        // Payment Only Completed
        // ==============================


        if (service.Status == "Completed")
        {
            return RedirectToAction(
                "Create",
                "Payment",
                new
                {
                    serviceId = service.ServiceId
                });
        }



        return RedirectToAction(
      "Details",
      "ServiceRequest",
      new
      {
          appointmentId = service.AppointmentId
      });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PreviewInvoiceAsync(
     CompleteServiceViewModel model)
    {

        decimal partsTotal = model.Parts
            .Sum(x => x.Total);

        decimal chargeTotal = model.Charges
            .Sum(x => x.Amount);

        decimal expenseTotal = model.Expenses
            .Sum(x => x.Amount);

        foreach (var u in model.Units)
        {
            Console.WriteLine(
                $"POST => {u.BrandName} | Selected:{u.IsSelected} | Qty:{u.ServiceQuantity}"
            );
        }
        model.BeforePhotoPreviews =
            await ConvertToBase64(model.BeforePhotos);

        model.AfterPhotoPreviews =
            await ConvertToBase64(model.AfterPhotos);

        model.ProblemPhotoPreviews =
            await ConvertToBase64(model.ProblemPhotos);

        model.SubTotal =
            partsTotal +
            chargeTotal +
            expenseTotal;


        model.GrandTotal =
            model.SubTotal;


        return View("InvoicePreview", model);
    }
    private async Task<List<string>> ConvertToBase64(List<IFormFile>? files)
    {
        var result = new List<string>();

        if (files == null)
            return result;

        foreach (var file in files)
        {
            using var ms = new MemoryStream();

            await file.CopyToAsync(ms);

            var base64 = Convert.ToBase64String(ms.ToArray());

            result.Add($"data:{file.ContentType};base64,{base64}");
        }

        return result;
    }
    public async Task<IActionResult> Reminders()
    {
        var reminders = await _context.ServiceReminders
            .Include(x => x.Customer)
            .Include(x => x.AirConUnit)
            .Where(x => x.IsDeleted ==false && x.SentStatus == false)
            .OrderBy(x => x.ReminderDate)
            .ToListAsync();

        return View(reminders);
    }
    private void CreateServiceReminder(ServiceRequest service, string acCondition)
    {
        int months = acCondition switch
        {
            "Good" => 6,
            "Normal" => 3,
            "Bad" => 1,
            _ => 3
        };

        var reminder = new ServiceReminder
        {
            CustomerId = service.CustomerId,
            //AirConUnitId = service.AirConId.Value,
            ServiceRequestId = service.ServiceId,

            ReminderType = "Next", // 🔥 shorten (safe for DB)
            ReminderDate = DateTime.Now.AddMonths(months),

            SentStatus = false,
            CreatedAt = DateTime.Now,
            IsDeleted = false
        };

        _context.ServiceReminders.Add(reminder);
    }
    [HttpPost]
    public async Task<IActionResult> HandleReminder(int id, string action)
    {
        var reminder = await _context.ServiceReminders
            .Include(x => x.AirConUnit)
            .Include(x => x.Customer)
            .Include(x => x.ServiceRequest)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (reminder == null)
            return NotFound();

        if (action == "Done")
        {
            reminder.SentStatus = true;
            reminder.IsDeleted = true;
        }
        else if (action == "ReAssign")
        {
            reminder.IsDeleted = true;

            // 1. Create NEW Service Request
            var service = new ServiceRequest
            {
                CustomerId = reminder.CustomerId,
                //AirConId = reminder.AirConUnitId,
                Status = "Assigned",
                CreatedAt = DateTime.Now,
                ServiceType = "Reminder Service",
                Location = reminder.Customer?.Address ?? "N/A"
            };

            _context.ServiceRequests.Add(service);

            // 2. CREATE NEW SCHEDULE (IMPORTANT)
            var schedule = new TechnicianSchedulePlan
            {
                TechnicianId = (int)reminder.ServiceRequest.TechnicianId,

                CustomerId = reminder.CustomerId,
                CustomerName = reminder.Customer?.Name,

                Title = "Reminder Service",
                PlanType = "Reminder",

                PlannedDate = DateTime.Now.AddDays(1),

                Priority = "High",
                Status = "Pending",

                Location = reminder.Customer?.Address ?? "N/A",

                CreatedAt = DateTime.Now
            };

            _context.TechnicianSchedulePlans.Add(schedule);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Dashboard");
    }

    public async Task<IActionResult> MySchedule()
    {
        int techId = HttpContext.Session.GetInt32("TechnicianId") ?? 0;

        if (techId == 0)
            return RedirectToAction("Login", "TechnicianAuth");

        var today = DateTime.Today;

        var plans = await _context.TechnicianSchedulePlans
            .Include(p => p.Customer)
            .Include(p => p.ServiceRequest)
            .Where(p => p.TechnicianId == techId)
            .OrderBy(p => p.PlannedDate)
            .ToListAsync();

        var vm = new MyScheduleVM
        {
            Plans = plans ?? new List<TechnicianSchedulePlan>(),

            TodayCount = plans.Count(x => x.PlannedDate.Date == today),
            UpcomingCount = plans.Count(x => x.PlannedDate > today),
            HighPriorityCount = plans.Count(x => x.Priority == "High"),
            CompletedCount = plans.Count(x => x.Status == "Completed"),
            TodayJobs = plans
            .Where(x => x.PlannedDate.Date == today)
            .ToList()
            };

        return View(vm);
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
