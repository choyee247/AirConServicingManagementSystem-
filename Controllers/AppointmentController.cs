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
        //public async Task<IActionResult> Create()
        //{
        //    ViewBag.Customers = await _context.Customers
        //        .Where(c => c.IsDeleted != true)
        //        .ToListAsync();

        //    ViewBag.Technicians = await _context.Technicians
        //        .Where(t => t.IsDeleted != true)
        //        .ToListAsync();

        //    return View();
        //}

        //// =========================
        //// 📅 CREATE (POST)
        //// =========================
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(Appointment model)
        //{
        //    var technicianId = HttpContext.Session.GetInt32("TechnicianId");


        //    if (technicianId == null)
        //    {
        //        return RedirectToAction("Login", "Login");
        //    }


        //    if (ModelState.IsValid)
        //    {
        //        ViewBag.Customers = await _context.Customers
        //            .Where(c => c.IsDeleted != true)
        //            .ToListAsync();

        //        return View(model);
        //    }



        //    model.Status = "Pending";


        //    // Login Technician Assign
        //    model.TechnicianId = technicianId.Value;



        //    model.ScheduledDate = model.ScheduledDate;



        //    _context.Appointments.Add(model);

        //    await _context.SaveChangesAsync();



        //    TempData["SuccessMessage"] = "Appointment created successfully";


        //    return RedirectToAction("Index");
        //}


        // =========================
        // 📅 CREATE (GET)
        // =========================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // ============================================
            // CHECK LOGIN
            // ============================================

            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction(
                    "Login",
                    "Login"
                );
            }


            // ============================================
            // CHECK ROLE
            // ============================================

            if (userRole != "Admin" &&
                userRole != "Senior" &&
                userRole != "Junior")
            {
                HttpContext.Session.Clear();

                return RedirectToAction(
                    "Login",
                    "Login"
                );
            }


            // ============================================
            // LOAD CUSTOMERS
            // ============================================

            ViewBag.Customers = await _context.Customers
                .Where(c => c.IsDeleted != true)
                .ToListAsync();


            // ============================================
            // LOAD TECHNICIANS
            // ============================================

            ViewBag.Technicians = await _context.Technicians
                .Where(t => t.IsDeleted != true)
                .ToListAsync();


            // ============================================
            // USER ROLE
            // ============================================

            ViewBag.UserRole = userRole;


            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment model)
        {
            // ============================================
            // GET LOGIN USER
            // ============================================

            var userId = HttpContext.Session.GetInt32("UserId");

            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Login");
            }


            // ============================================
            // SAVE CREATED BY
            // ============================================

            model.CreatedByUserId = userId.Value;


            // ============================================
            // ADMIN
            // ============================================

            if (userRole == "Admin")
            {
                // Admin creates appointment
                // Do NOT automatically assign technician

                model.TechnicianId = null;

                model.Status = "Pending";
            }


            // ============================================
            // TECHNICIAN
            // ============================================

            else if (userRole == "Senior" || userRole == "Junior")
            {
                var technicianId =
                    HttpContext.Session.GetInt32("TechnicianId");

                if (technicianId == null)
                {
                    return RedirectToAction("Login", "Login");
                }


                // Technician creates own appointment

                model.TechnicianId = technicianId.Value;

                model.Status = "Pending";
            }


            // ============================================
            // UNKNOWN ROLE
            // ============================================

            else
            {
                HttpContext.Session.Clear();

                return RedirectToAction("Login", "Login");
            }


            // ============================================
            // VALIDATION FAILED
            // ============================================

            if (ModelState.IsValid)
            {
                ViewBag.Customers =
                    await _context.Customers
                        .Where(c => c.IsDeleted != true)
                        .ToListAsync();

                ViewBag.Technicians =
                    await _context.Technicians
                        .Where(t => t.IsDeleted != true)
                        .ToListAsync();

                ViewBag.UserRole = userRole;

                return View(model);
            }


            // ============================================
            // DEFAULT VALUES
            // ============================================

            //model.ScheduledDate = DateTime.Now;

            if (model.ScheduledDate == default)
            {
                model.ScheduledDate = DateTime.Now;
            }


            // ============================================
            // SAVE APPOINTMENT
            // ============================================

            _context.Appointments.Add(model);

            await _context.SaveChangesAsync();


            // ============================================
            // SUCCESS
            // ============================================

            TempData["SuccessMessage"] =
                "Appointment created successfully.";


            // ============================================
            // REDIRECT
            // ============================================

            if (userRole == "Admin")
            {
                return RedirectToAction("AppointmentList");
            }


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
