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
            return RedirectToAction("Login", "Login");

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
            return RedirectToAction("Login", "Login");

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
            return RedirectToAction("Login", "Login");



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
                x.IsDeleted == false &&
                x.ServiceId == service.ServiceId)

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

                    //CompletedQuantity =
                    //g.Count(x =>
                    //    x.ServiceRecordUnits.Any()
                    //),
                    //CompletedQuantity =
                    //g.Count(x =>
                    //    x.ServiceRecordUnits.Any(r =>
                    //        r.ServiceRecordId == service.ServiceId
                    //    )
                    //),

                    CompletedQuantity =
                    g.Count(x =>
                        x.ServiceRecordUnits.Any(sru =>
                            sru.ServiceRecord.ServiceRequestId == service.ServiceId
                        )
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
        CompleteServiceViewModel model,
        string submitAction = "details")
    {
        

        int techId =
            HttpContext.Session.GetInt32("TechnicianId") ?? 0;

        if (techId == 0)
        {
            return RedirectToAction("Login", "Login");
        }
 
        var service = await _context.ServiceRequests
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x =>
                x.ServiceId == model.ServiceId &&
                x.TechnicianId == techId);

        if (service == null)
        {
            return NotFound();
        }


        if (service.Status == "Completed")
        {
            return Content("Already Completed");
        }

        if (model.Units == null || !model.Units.Any())
        {
            TempData["Error"] = "No AC units found.";

            return RedirectToAction(
                "Complete",
                new
                {
                    id = service.ServiceId
                });
        }


        var record = await _context.ServiceRecords
            .FirstOrDefaultAsync(x =>
                x.ServiceRequestId == service.ServiceId);

        if (record == null)
        {
            record = new ServiceRecord
            {
                ServiceRequestId = service.ServiceId,

                CustomerId = service.CustomerId,

                TechnicianId = techId,

                CreatedAt = DateTime.Now,

                IsDeleted = false
            };

            _context.ServiceRecords.Add(record);

            await _context.SaveChangesAsync();
        }


        record.TechnicianNote = model.TechnicianNote;

        record.PartsReplaced = model.PartsReplaced;

        record.ServiceCost = model.GrandTotal;

        record.ServiceType = model.ServiceType;

        record.UpdatedAt = DateTime.Now;


        var completedAirConIds = await _context.ServiceRecordUnits
            .Where(x =>
                x.ServiceRecordId == record.Id)
            .Select(x => x.AirConUnitId)
            .ToListAsync();

        if (model.Parts != null && model.Parts.Any())
        {
            foreach (var part in model.Parts)
            {
                if (string.IsNullOrWhiteSpace(part.PartName))
                {
                    continue;
                }

                if (part.Quantity <= 0)
                {
                    continue;
                }

                if (part.UnitPrice < 0)
                {
                    continue;
                }


                var servicePart = new ServicePart
                {
                    ServiceRecordId = record.Id,

                    AirConUnitId =
                        part.AirConUnitId > 0
                            ? part.AirConUnitId
                            : null,

                    PartName =
                        part.PartName.Trim(),

                    Qty =
                        part.Quantity,

                    UnitPrice =
                        part.UnitPrice,

                    Total =
                        part.Total
                };

                Console.WriteLine(
    $"Parts: {model.Parts?.Count ?? 0}"
);
                _context.ServiceParts.Add(servicePart);
            }
        }

        // ==========================================
        // SAVE SERVICE CHARGES
        // ==========================================

        if (model.Charges != null && model.Charges.Any())
        {
            foreach (var charge in model.Charges)
            {
                // Skip empty description
                if (string.IsNullOrWhiteSpace(charge.Description))
                {
                    continue;
                }

                // Skip zero / negative amount
                if (charge.Amount <= 0)
                {
                    continue;
                }

                var serviceCharge = new ServiceCharge
                {
                    ServiceRecordId = record.Id,

                    Description =
                        charge.Description.Trim(),

                    Amount =
                        charge.Amount
                };

                Console.WriteLine(
                    $"Charges: {model.Charges?.Count ?? 0}"
                );

                _context.ServiceCharges.Add(serviceCharge);
            }
        }

        // ==========================================
        // SAVE SERVICE EXPENSES
        // ==========================================

        if (model.Expenses != null && model.Expenses.Any())
        {
            foreach (var expense in model.Expenses)
            {
                if (string.IsNullOrWhiteSpace(expense.Description))
                {
                    continue;
                }

                if (expense.Amount <= 0)
                {
                    continue;
                }

                var serviceExpense = new ServiceExpense
                {
                    ServiceRecordId = record.Id,

                    Description =
                        expense.Description.Trim(),

                    Amount =
                        expense.Amount
                };
                Console.WriteLine(
    $"Expenses: {model.Expenses?.Count ?? 0}"
);
                _context.ServiceExpenses.Add(serviceExpense);
            }
        }

        foreach (var unit in model.Units
            .Where(x =>
                x.IsSelected &&
                x.ServiceQuantity > 0 &&
                x.AirConUnitIds != null &&
                x.AirConUnitIds.Any()))
        {
           
            var availableIds = unit.AirConUnitIds
                .Where(x => x > 0)
                .ToList();

            var remainingIds = availableIds
                .Where(x => !completedAirConIds.Contains(x))
                .Take(unit.ServiceQuantity)
                .ToList();

            foreach (var airconId in remainingIds)
            {
                var nextDate = unit.Condition switch
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


                var recordUnit = new ServiceRecordUnit
                {
                    ServiceRecordId = record.Id,

                    AirConUnitId = airconId,

                    Accondition = unit.Condition,

                    ProblemFound = unit.ProblemFound,

                    RepairAction = unit.RepairAction,

                    NextServiceDue = nextDate,

                    CreatedAt = DateTime.Now
                };


                _context.ServiceRecordUnits.Add(recordUnit);


                var reminder = new ServiceReminder
                {
                    CustomerId = service.CustomerId,

                    AirConUnitId = airconId,

                    ServiceRequestId = service.ServiceId,

                    ReminderDate = nextDate,

                    ReminderType = "Next",

                    SentStatus = false,

                    IsDeleted = false,

                    CreatedAt = DateTime.Now
                };


                _context.ServiceReminders.Add(reminder);

                var aircon = await _context.AirConUnits
                    .FirstOrDefaultAsync(x =>
                        x.Id == airconId &&
                        x.IsDeleted != true);

                if (aircon != null)
                {
                    aircon.UpdatedAt = DateTime.Now;
                }
            }
        }


        string folder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "images",
            "service"
        );

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }


        // ==========================================
        // BEFORE PHOTOS
        // ==========================================

        if (model.BeforePhotos != null &&
            model.BeforePhotos.Any())
        {
            foreach (var photo in model.BeforePhotos)
            {
                Console.WriteLine(
                $"Before Photos: {model.BeforePhotos?.Count ?? 0}"
            );

                await SaveServicePhoto(
                    photo,
                    record.Id,
                    "Before",
                    folder
                );
            }
        }


        // ==========================================
        // AFTER PHOTOS
        // ==========================================

        if (model.AfterPhotos != null &&
            model.AfterPhotos.Any())
        {
            foreach (var photo in model.AfterPhotos)
            {
                Console.WriteLine(
    $"After Photos: {model.AfterPhotos?.Count ?? 0}"
);

                await SaveServicePhoto(
                    photo,
                    record.Id,
                    "After",
                    folder
                );
            }
        }


        // ==========================================
        // PROBLEM PHOTOS
        // ==========================================

        if (model.ProblemPhotos != null &&
            model.ProblemPhotos.Any())
        {
            foreach (var photo in model.ProblemPhotos)
            {

                Console.WriteLine(
                    $"Problem Photos: {model.ProblemPhotos?.Count ?? 0}"
                );

                await SaveServicePhoto(
                    photo,
                    record.Id,
                    "Problem",
                    folder
                );
            }
        }

        bool hasRemaining = model.Units.Any(x =>
        {
            int completedQuantity =
                x.CompletedQuantity;

            int serviceQuantity =
                x.IsSelected
                    ? x.ServiceQuantity
                    : 0;

            int afterComplete =
                completedQuantity +
                serviceQuantity;

            return afterComplete < x.TotalQuantity;
        });

        if (hasRemaining)
        {
            service.Status = "Remaining";

            service.CompletedAt = null;

            record.Status = "Remaining";
        }
        else
        {

            service.Status = "Completed";

            service.CompletedAt = DateTime.Now;

            record.Status = "Completed";
        }

        var technician = await _context.Technicians
            .FirstOrDefaultAsync(x =>
                x.TechnicianId == techId);

        if (technician != null)
        {
            technician.IsAvailable = true;
        }

        var appointment = await _context.Appointments
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

        if (service.Status == "Completed")
        {
            return RedirectToAction(
                "Create",
                "Payment",
                new
                {
                    serviceId =
                        service.ServiceId
                });
        }
        if (service.Status == "Remaining")
        {
            return RedirectToAction(
                "RemainingOptions",
                "TechnicianService",
                new
                {
                    serviceId = service.ServiceId
                });
        }
        if (submitAction == "payment")
        {
            return RedirectToAction(
                "Create",
                "Payment",
                new
                {
                    serviceId =
                        service.ServiceId
                });
        }

        return RedirectToAction(
            "Details",
            "ServiceRequest",
            new
            {
                appointmentId =
                    service.AppointmentId
            });
    }

    private async Task SaveServicePhoto(
    IFormFile photo,
    int serviceRecordId,
    string photoType,
    string folder)
    {
        if (photo == null || photo.Length == 0)
        {
            return;
        }


        string extension =
            Path.GetExtension(photo.FileName);


        string fileName =
            Guid.NewGuid().ToString()
            + extension;


        string filePath =
            Path.Combine(
                folder,
                fileName
            );


        using (var stream =
               new FileStream(
                   filePath,
                   FileMode.Create))
        {
            await photo.CopyToAsync(stream);
        }


        var servicePhoto = new ServicePhoto
        {
            ServiceRecordId =
                serviceRecordId,

            PhotoPath =
                "/images/service/"
                + fileName,

            PhotoType =
                photoType,

            CreatedAt =
                DateTime.Now,

            IsDeleted =
                false
        };


        _context.ServicePhotos.Add(
            servicePhoto
        );
    }
    [HttpGet]
    public async Task<IActionResult> RemainingOptions(int serviceId)
    {
        var service = await _context.ServiceRequests
            .FirstOrDefaultAsync(x =>
                x.ServiceId == serviceId);

        if (service == null)
            return NotFound();

        return View(service);
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
        var technicianId = HttpContext.Session.GetInt32("TechnicianId");

        if (technicianId == null)
        {
            return RedirectToAction("Login", "Login");
        }

        var reminders = await _context.ServiceReminders
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.AirConUnit)
            .Include(x => x.ServiceRequest)
            .Where(x =>
                x.IsDeleted == false &&
                x.SentStatus == false &&
                x.ServiceRequest != null &&
                x.ServiceRequest.TechnicianId == technicianId
            )
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
            return RedirectToAction("Login", "Login");

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
