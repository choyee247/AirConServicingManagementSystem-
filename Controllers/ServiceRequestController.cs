using AirConServicingManagementSystem.Models;
using AirConServicingManagementSystem.ViewsModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace AirConServicingManagementSystem.Controllers
{
    public class ServiceRequestController : Controller
    {
        private readonly DBContext _context;

        public ServiceRequestController(DBContext context)
        {
            _context = context;
        }

        //public async Task<IActionResult> Create(int appointmentId)
        //{
        //    var appointment = await _context.Appointments
        //        .Include(a => a.Customer)
        //        .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

        //    if (appointment == null)
        //        return NotFound();

        //    ViewBag.AppointmentId = appointmentId;
        //    ViewBag.CustomerName = appointment.Customer.Name;
        //    ViewBag.CustomerId = appointment.CustomerId;
        //    ViewBag.Location = appointment.Location;

        //    return View();
        //}
        public async Task<IActionResult> Index()
        {
            int technicianId =
                HttpContext.Session.GetInt32("TechnicianId") ?? 0;

            if (technicianId == 0)
            {
                return RedirectToAction("Login", "Login");
            }

            var services = await _context.ServiceRequests

                .Include(x => x.ServiceRecords)
                    .ThenInclude(x => x.Payments)

                .Include(x => x.Customer)

                .Include(x => x.AirConUnits)

                .Where(x =>
                    x.TechnicianId == technicianId
                )

                .Select(x => new ServiceRequestCreateVM
                {
                    ServiceRequest = x,

                    AirCons = _context.AirConUnits
                        .Where(a =>
                            a.CustomerId == x.CustomerId)
                        .ToList()
                })

                .OrderByDescending(x =>
                    x.ServiceRequest.RequestedAt)

                .ToListAsync();

            return View(services);
        }
        public async Task<IActionResult> Create(int? appointmentId)
        {
            int technicianId =
                HttpContext.Session.GetInt32("TechnicianId") ?? 0;

            if (technicianId == 0)
                return RedirectToAction("Login", "Login");

            ViewBag.Customers = await _context.Customers
                .Where(c => c.IsDeleted != true)
                .ToListAsync();

            var model = new ServiceRequest();

            if (appointmentId.HasValue)
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Customer)
                    .Include(a => a.Technician)
                    .FirstOrDefaultAsync(a =>
                        a.AppointmentId == appointmentId.Value);

                if (appointment == null)
                    return NotFound();

               
                if (appointment.TechnicianId != technicianId)
                    return Forbid();

                ViewBag.Appointment = appointment;

                model.CustomerId = appointment.CustomerId;
                model.TechnicianId = appointment.TechnicianId;
                model.Location = appointment.Location;
                model.Notes = appointment.Notes;

                // IMPORTANT
                model.AppointmentId = appointment.AppointmentId;
                model.Status = "Assigned";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
     ServiceRequest model,
     int appointmentId)
        {
            // ==========================================
            // 1. GET APPOINTMENT
            // ==========================================

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == appointmentId);

            if (appointment == null)
                return NotFound();


            // ==========================================
            // 2. GET EXISTING SERVICE REQUEST
            // ==========================================

            var serviceRequest = await _context.ServiceRequests
                .FirstOrDefaultAsync(x =>
                    x.AppointmentId == appointmentId);

            if (serviceRequest == null)
            {
                return NotFound();
            }


            // ==========================================
            // 3. CHECK ASSIGNED
            // ==========================================

            if (serviceRequest.Status != "Assigned")
            {
                TempData["ErrorMessage"] =
                    "This service is not ready to start.";

                return RedirectToAction("Index");
            }


            // ==========================================
            // 4. UPDATE SERVICE REQUEST
            // ==========================================

            serviceRequest.CustomerId =
                appointment.CustomerId;

            serviceRequest.TechnicianId =
                appointment.TechnicianId;

            serviceRequest.Location =
                appointment.Location;

            serviceRequest.Notes =
                appointment.Notes;

            serviceRequest.Status =
                "In Progress";

            serviceRequest.RequestedAt =
                serviceRequest.RequestedAt;

            serviceRequest.AppointmentId =
                appointmentId;

            serviceRequest.PaymentStatus =
                "Unpaid";


            // ==========================================
            // 5. UPDATE APPOINTMENT
            // ==========================================

            appointment.Status =
                "In Progress";


            // ==========================================
            // 6. UPDATE TECHNICIAN SCHEDULE
            // ==========================================

            var schedule = await _context.TechnicianSchedulePlans
                .FirstOrDefaultAsync(x =>
                    x.TechnicianId ==
                        appointment.TechnicianId &&

                    x.CustomerId ==
                        appointment.CustomerId &&

                    x.PlannedDate.Date ==
                        appointment.ScheduledDate.Date &&

                    x.PlanType == "Service");

            if (schedule != null)
            {
                schedule.Status =
                    "In Progress";
            }


            // ==========================================
            // 7. SAVE
            // ==========================================

            await _context.SaveChangesAsync();


            // ==========================================
            // 8. GO TO AIRCON UNIT
            // ==========================================

            return RedirectToAction(
                "Create",
                "AirConUnit",
                new
                {
                    serviceId =
                        serviceRequest.ServiceId
                });
        }
        public async Task<IActionResult> Details(int appointmentId)
        {
            var service = await _context.ServiceRequests
                .Include(x => x.Customer)
                .Include(x => x.Technician)
                .Include(x => x.AirConUnits)
                    .ThenInclude(x => x.Brand)
                .Include(x => x.AirConUnits)
                    .ThenInclude(x => x.Model)
                .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);

            if (service == null)
                return NotFound();


            var record = await _context.ServiceRecords
                .Include(x => x.ServiceRecordUnits)
                    .ThenInclude(x => x.AirConUnit)
                        .ThenInclude(x => x.Brand)
                .Include(x => x.ServiceRecordUnits)
                    .ThenInclude(x => x.AirConUnit)
                        .ThenInclude(x => x.Model)
                .Include(x => x.ServiceParts)
                .Include(x => x.ServiceCharges)
                .Include(x => x.ServiceExpenses)
                .FirstOrDefaultAsync(x =>
                    x.ServiceRequestId == service.ServiceId);


            var payment = record == null
                ? null
                : await _context.Payments
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRecordId == record.Id);


            ViewBag.Record = record;
            ViewBag.Payment = payment;

            return View(service);
        }
        public async Task<IActionResult> GetAirConsByCustomer(int customerId)
        {
            var aircons = await _context.AirConUnits
                .Where(a => a.CustomerId == customerId && a.IsDeleted != true)
                .Include(a => a.Brand)
                .Include(a => a.Model)
                .Select(a => new
                {
                    id = a.Id,
                    brand = a.Brand.BrandName,
                    model = a.Model.ModelName
                })
                .ToListAsync();

            return Json(aircons);
        }
        public async Task<IActionResult> MyRequests()
        {
            int customerId = HttpContext.Session.GetInt32("CustomerId") ?? 0;

            var data = await _context.ServiceRequests
                .Include(s => s.Technician)
                .Include(s => s.AirConUnits)
                    .ThenInclude(a => a.Brand)
                .Include(s => s.AirConUnits)
                    .ThenInclude(a => a.Model)
                .Include(s => s.AirConUnits)
                    .ThenInclude(a => a.Warranty)
                .Where(s => s.CustomerId == customerId)
                .OrderByDescending(s => s.RequestedAt)
                .ToListAsync();

            return View(data);
        }
        public async Task<IActionResult> Warranty()
        {
            int customerId = HttpContext.Session.GetInt32("CustomerId") ?? 0;

            var data = await _context.Warranties
                .Include(w => w.AirCon)
                    .ThenInclude(a => a.Brand)
                .Include(w => w.AirCon)
                    .ThenInclude(a => a.Model)
                .Where(w => w.AirCon.CustomerId == customerId)
                .ToListAsync();

            return View(data);
        }

        public async Task<IActionResult> History()
        {
            // Customer login session
            int customerId = HttpContext.Session.GetInt32("CustomerId") ?? 0;

            if (customerId == 0)
                return RedirectToAction("Login", "Login");

            var records = await _context.ServiceRecords
                .Include(r => r.Technician)
                .Include(s => s.ServiceRecordUnits)
                    .ThenInclude(s => s.AirConUnit)
                .Where(r =>
                    r.CustomerId == customerId &&
                    r.IsDeleted != true)
                .OrderByDescending(r => r.ServiceDate)
                .ToListAsync();

            return View(records);
        }
    }

}
