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

        public async Task<IActionResult> Create()
        {
            int customerId = HttpContext.Session.GetInt32("CustomerId") ?? 0;

            ViewBag.AirCons = await _context.AirConUnits
                .Where(a => a.CustomerId == customerId && a.IsDeleted != true)
                
                .Include(a => a.Brand)
                .Include(a => a.Model)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest model)
        {
            int customerId = HttpContext.Session.GetInt32("CustomerId") ?? 0;

            model.CustomerId = customerId;
            model.Status = ServiceStatus.Pending;
            model.RequestedAt = DateTime.Now;
            model.PaymentStatus = "Unpaid";

            // Urgent fee
            if (model.IsUrgent)
                model.Fee = 15000;
            else
                model.Fee = 10000;

            // ===== Warranty Check =====
            var aircon = await _context.AirConUnits
                .Include(a => a.Warranty)
                .FirstOrDefaultAsync(a => a.Id == model.AirConId);

            if (aircon != null && aircon.Warranty != null && aircon.Warranty.IsActive)
            {
                model.IsWarrantyApplied = true;
                model.PaymentStatus = "Free";
                model.Fee = 0;
            }

            _context.ServiceRequests.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyRequests));
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
