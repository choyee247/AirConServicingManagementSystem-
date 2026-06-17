using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace AirConServicingManagementSystem.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly DBContext _context;

        public AppointmentController(DBContext context)
        {
            _context = context;
        }

        // =========================
        // 📅 CREATE (GET)
        // =========================
        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = await _context.Customers
                .Where(c => c.IsDeleted != true)
                .ToListAsync();

            ViewBag.Technicians = await _context.Technicians
                .Where(t => t.IsDeleted != true)
                .ToListAsync();

            return View();
        }

        // =========================
        // 📅 CREATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment model)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Customers = await _context.Customers.ToListAsync();
                return View(model);
            }

            model.Status = "Pending";

            _context.Appointments.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment created successfully";

            return RedirectToAction("Index");
        }

        // =========================
        // 📋 INDEX
        // =========================
        public async Task<IActionResult> Index()
        {
            var data = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Technician)
                .Include(a => a.ServiceRequests)
                .OrderByDescending(a => a.ScheduledDate)
                .ToListAsync();

            return View(data);
        }
    }
}
