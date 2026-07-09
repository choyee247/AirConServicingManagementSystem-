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

        [HttpGet]
        public async Task<IActionResult> GetCustomerInfo(int id)
        {
            var customer = await _context.Customers

                .Include(x => x.CustomerLocations)
                    .ThenInclude(x => x.StateDivisionPk)

                .Include(x => x.CustomerLocations)
                    .ThenInclude(x => x.TownshipPk)

                .FirstOrDefaultAsync(x => x.Id == id);


            if (customer == null)
            {
                return Json(null);
            }


            var location = customer.CustomerLocations
                .FirstOrDefault();



            string fullLocation = "";



            if (location != null)
            {

                var state = location.StateDivisionPk != null
                    ? location.StateDivisionPk.StateDivisionEn
                    : "";


                var township = location.TownshipPk != null
                    ? location.TownshipPk.TownshipEn
                    : "";



                fullLocation =
                    customer.Address
                    + ", "
                    + state
                    + ", "
                    + township
                    ;

            }
            else
            {

                fullLocation = customer.Address;

            }



            return Json(new
            {

                name = customer.Name,

                phone = customer.Phone,


                location = fullLocation

            });

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
                    .ThenInclude(sr => sr.ServiceRecords)
                .OrderByDescending(a => a.ScheduledDate)
                .ToListAsync();

            return View(data);
        }
        public async Task<IActionResult> AppointmentList()
        {
            var appointments = await _context.Appointments

                .Include(a => a.Customer)

                .Include(a => a.Technician)

                .OrderByDescending(a => a.ScheduledDate)

                .ToListAsync();


            return View(appointments);
        }
    }
}
