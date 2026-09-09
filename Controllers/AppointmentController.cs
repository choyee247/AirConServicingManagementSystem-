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


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Login");
            }

            if (userRole != "Admin" &&
                userRole != "Senior" &&
                userRole != "Junior")
            {
                HttpContext.Session.Clear();

                return RedirectToAction("Login", "Login");
            }

            ViewBag.UserRole = userRole;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Login");
            }

            if (userRole != "Admin" &&
                userRole != "Senior" &&
                userRole != "Junior")
            {
                HttpContext.Session.Clear();

                return RedirectToAction("Login", "Login");
            }


            /*
             * ==========================================
             * Created By
             * ==========================================
             */

            model.CreatedByUserId = userId.Value;


            /*
             * ==========================================
             * Admin
             * ==========================================
             */

            if (userRole == "Admin")
            {
                model.TechnicianId = null;
                model.Status = "Pending";
            }


            /*
             * ==========================================
             * Technician
             * ==========================================
             */

            else if (userRole == "Senior" ||
                     userRole == "Junior")
            {
                var technicianId =
                    HttpContext.Session.GetInt32("TechnicianId");

                if (technicianId == null)
                {
                    return RedirectToAction("Login", "Login");
                }

                //model.TechnicianId = technicianId.Value;
                model.Status = "Pending";
            }


            /*
             * ==========================================
             * Default Date
             * ==========================================
             */

            if (model.ScheduledDate == default)
            {
                model.ScheduledDate = DateTime.Now;
            }


            /*
             * ==========================================
             * Validation
             * ==========================================
             */

            if (ModelState.IsValid)
            {
                ViewBag.UserRole = userRole;

                return View(model);
            }


            /*
             * ==========================================
             * Save Appointment
             * ==========================================
             */

            _context.Appointments.Add(model);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Appointment created successfully.";


            /*
             * ==========================================
             * Redirect
             * ==========================================
             */

            if (userRole == "Admin")
            {
                return RedirectToAction("AppointmentList");
            }

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> SearchCustomers(string? search)
        {
            var query = _context.Customers
                .Where(c => c.IsDeleted != true);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    c.Name.Contains(search) ||
                    c.Phone.Contains(search));
            }

            var customers = await query
                .OrderBy(c => c.Name)
                .Take(20)
                .Select(c => new
                {
                    id = c.Id,
                    text = c.Name + " (" + c.Phone + ")"
                })
                .ToListAsync();

            return Json(customers);
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
            var userId = HttpContext.Session.GetInt32("UserId");
            var technicianId = HttpContext.Session.GetInt32("TechnicianId");
            var role = HttpContext.Session.GetString("UserRole");

            // Login check
            if (userId == null || technicianId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            // Technician only
            if (role != "Senior" && role != "Junior")
            {
                return RedirectToAction("Login", "Login");
            }

            var data = await _context.Appointments
                .Where(a =>
                    a.TechnicianId == technicianId.Value
                )
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
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            // Login check
            if (userId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            // Admin only
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Login");
            }

            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Technician)
                .Include(a => a.ServiceRequests)
                .OrderByDescending(a => a.ScheduledDate)
                .ToListAsync();

            return View(appointments);
        }
    }
}
