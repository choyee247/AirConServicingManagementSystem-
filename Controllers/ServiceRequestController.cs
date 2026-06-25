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

            var services = await _context.ServiceRequests
             .Include(x => x.Customer)
             .Include(x => x.AirCon)
             .Where(x => x.TechnicianId == technicianId)
             .Select(x => new ServiceRequestCreateVM
             {
                 ServiceRequest = x,
                 AirCons = _context.AirConUnits
                     .Where(a => a.CustomerId == x.CustomerId)
                     .ToList()
             })
             .ToListAsync();

            return View(services);
        }
        public async Task<IActionResult> Create(int? appointmentId)
        {
            ViewBag.Customers = await _context.Customers
                .Where(c => c.IsDeleted != true)
                .ToListAsync();

            var model = new ServiceRequest();

            if (appointmentId.HasValue)
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Customer)
                    .Include(a => a.Technician)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId.Value);

                if (appointment != null)
                {
                    ViewBag.Appointment = appointment;

                    // Auto Fill
                    model.CustomerId = appointment.CustomerId;
                    model.TechnicianId = appointment.TechnicianId;
                    model.Location = appointment.Location;
                    model.Notes = appointment.Notes;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest model, int appointmentId)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
                return NotFound();

            model.CustomerId = appointment.CustomerId;
            model.TechnicianId = appointment.TechnicianId;
            model.Location = appointment.Location;
            model.Notes = appointment.Notes;
            model.Status = "In Progress";
            model.RequestedAt = DateTime.Now;
            model.CreatedAt = DateTime.Now;
            model.PaymentStatus = "Unpaid";
            model.AirConId = null;
            model.AppointmentId = appointmentId;

            _context.ServiceRequests.Add(model);

            appointment.Status = "In Progress";

            var schedule = await _context.TechnicianSchedulePlans
                .FirstOrDefaultAsync(x =>
                    x.TechnicianId == appointment.TechnicianId &&
                    x.CustomerId == appointment.CustomerId &&
                    x.Status == "Pending");

            if (schedule != null)
            {
                schedule.Status = "In Progress";
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int appointmentId)
        {
            var service = await _context.ServiceRequests
                .Include(x => x.Customer)
                .Include(x => x.Technician)
                .Include(x => x.AirCon)
                    .ThenInclude(x => x.Brand)
                .Include(x => x.AirCon)
                    .ThenInclude(x => x.Model)
                .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);

            if (service == null)
                return NotFound();

            var record = await _context.ServiceRecords
                .FirstOrDefaultAsync(x => x.ServiceRequestId == service.ServiceId);

            ViewBag.Record = record;

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
                .Include(s => s.AirCon)
                    .ThenInclude(a => a.Brand)
                .Include(s => s.AirCon)
                    .ThenInclude(a => a.Model)
                .Include(s => s.AirCon)
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
                return RedirectToAction("Login", "Account");

            var records = await _context.ServiceRecords
                .Include(r => r.Technician)
                .Include(r => r.AirConUnit)
                .Where(r =>
                    r.CustomerId == customerId &&
                    r.IsDeleted != true)
                .OrderByDescending(r => r.ServiceDate)
                .ToListAsync();

            return View(records);
        }
    }

}
