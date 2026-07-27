using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirConServicingManagementSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Security.Cryptography;
using System.Text;

namespace AirConServicingManagementSystem.Controllers
{
    public class TechniciansController : Controller
    {
        private readonly DBContext _context;

        public TechniciansController(DBContext context)
        {
            _context = context;
        }

        // GET: Technicians
        public async Task<IActionResult> Index(string searchString, string statusFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;

            var technicians = _context.Technicians
                .Where(t => !t.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                technicians = technicians.Where(t =>
                    t.Name.Contains(searchString) ||
                    t.PhoneNumber.Contains(searchString) ||
                    t.Email.Contains(searchString) ||
                    t.Address.Contains(searchString) ||
                    t.TechnicianRole.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                bool isAvailable = statusFilter == "available";
                technicians = technicians.Where(t => t.IsAvailable == isAvailable);
            }

            technicians = technicians.OrderByDescending(t => t.CreatedAt);

            return View(await technicians.ToListAsync());
        }


        // GET: Technicians/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var technician = await _context.Technicians
                .Include(t => t.Appointments)
                //.Include(t => t.Payments)
                .Include(t => t.ServiceRequests)
                .FirstOrDefaultAsync(t => t.TechnicianId == id && !t.IsDeleted);

            if (technician == null) return NotFound();

            var viewModel = new TechnicianViewModel
            {
                Id = technician.TechnicianId,
                Name = technician.Name,
                PhoneNumber = technician.PhoneNumber,
                Email = technician.Email,
                Address = technician.Address,
                TechnicianRole = technician.TechnicianRole,
                JoinDate = technician.JoinDate,
                LeaveDate = technician.LeaveDate,
                IsAvailable = technician.IsAvailable,
                CreatedAt = technician.CreatedAt,
                UpdatedAt = technician.UpdatedAt,
            };

            return View(viewModel);
        }


        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "TechnicianAuth"
                );
            }


            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId);


            if (user == null)
            {
                return NotFound();
            }


            ViewBag.Username = user.Username;
            ViewBag.Role = user.Role;


            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    TechnicianViewModel viewModel)
        {

            var userId = HttpContext.Session.GetInt32("UserId");


            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "TechnicianAuth"
                );
            }



            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId);



            if (user == null)
            {
                return NotFound();
            }



            if (ModelState.IsValid)
            {

                var technician = new Technician
                {
                    Name = viewModel.Name,

                    PhoneNumber = viewModel.PhoneNumber,

                    Address = viewModel.Address,


                    TechnicianRole = user.Role,

                    Email = $"{user.Username}@temp.com",

                    JoinDate = viewModel.JoinDate,


                    IsAvailable = true,

                    CreatedAt = DateTime.Now,

                    UpdatedAt = DateTime.Now,

                    IsDeleted = false
                };



                _context.Technicians.Add(technician);

                await _context.SaveChangesAsync();



                user.TechnicianId = technician.TechnicianId;


                await _context.SaveChangesAsync();



                HttpContext.Session.SetInt32(
                    "TechnicianId",
                    technician.TechnicianId
                );



                return RedirectToAction(
                    "Dashboard",
                    "TechnicianService"
                );

            }


            ViewBag.Username = user.Username;
            ViewBag.Role = user.Role;


            return View(viewModel);
        }
        // GET: Technicians/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.TechnicianId == id && !t.IsDeleted);

            if (technician == null) return NotFound();

            var viewModel = new TechnicianViewModel
            {
                Id = technician.TechnicianId,
                Name = technician.Name,
                PhoneNumber = technician.PhoneNumber,
                Email = technician.Email,
                Address = technician.Address,
                TechnicianRole = technician.TechnicianRole,
                JoinDate = technician.JoinDate,
                LeaveDate = technician.LeaveDate,
                IsAvailable = technician.IsAvailable,
                CreatedAt = technician.CreatedAt,
                UpdatedAt = technician.UpdatedAt
            };

            return View(viewModel);
        }

        // POST: Technicians/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TechnicianViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTechnician = await _context.Technicians
                        .FirstOrDefaultAsync(t => t.TechnicianId == id && !t.IsDeleted);

                    if (existingTechnician == null) return NotFound();

                    existingTechnician.Name = viewModel.Name;
                    existingTechnician.PhoneNumber = viewModel.PhoneNumber;
                    existingTechnician.Email = viewModel.Email;
                    existingTechnician.Address = viewModel.Address;
                    existingTechnician.TechnicianRole = viewModel.TechnicianRole;
                    existingTechnician.JoinDate = viewModel.JoinDate ?? existingTechnician.JoinDate;
                    existingTechnician.LeaveDate = viewModel.LeaveDate;
                    existingTechnician.IsAvailable = viewModel.IsAvailable;
                    existingTechnician.UpdatedAt = DateTime.Now;

                    _context.Update(existingTechnician);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Technician updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TechnicianExists(viewModel.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }
        #region Technician Profile


        // GET: Technician Profile
        public async Task<IActionResult> Profile()
        {
            var technicianId = HttpContext.Session.GetInt32("TechnicianId");

            if (technicianId == null)
            {
                return RedirectToAction("Login", "TechnicianLogin");
            }


            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t =>
                    t.TechnicianId == technicianId &&
                    t.IsDeleted == false);


            if (technician == null)
            {
                return NotFound();
            }


            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.TechnicianId == technicianId &&
                    u.IsDeleted == false);



            var model = new TechnicianProfileVM
            {
                TechnicianId = technician.TechnicianId,

                Username = user?.Username,

                Name = technician.Name,

                PhoneNumber = technician.PhoneNumber,

                Address = technician.Address,

                Email = technician.Email,

                TechnicianRole = technician.TechnicianRole,

                JoinDate = technician.JoinDate,

                IsAvailable = technician.IsAvailable
            };


            return View(model);
        }

        // GET: Edit Profile
        public async Task<IActionResult> EditProfile()
        {
            var technicianId = HttpContext.Session.GetInt32("TechnicianId");

            if (technicianId == null)
            {
                return RedirectToAction("Login", "TechnicianLogin");
            }


            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t =>
                    t.TechnicianId == technicianId &&
                    t.IsDeleted == false);


            if (technician == null)
            {
                return NotFound();
            }


            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.TechnicianId == technicianId &&
                    u.IsDeleted == false);



            var model = new TechnicianProfileVM
            {
                TechnicianId = technician.TechnicianId,

                Username = user?.Username,

                Name = technician.Name,

                PhoneNumber = technician.PhoneNumber,

                Address = technician.Address,

                Email = technician.Email,

                TechnicianRole = technician.TechnicianRole,

                JoinDate = technician.JoinDate,

                IsAvailable = technician.IsAvailable
            };


            return View(model);
        }
        // POST: Edit Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(
            TechnicianProfileVM model)
        {

            var technicianId = HttpContext.Session.GetInt32("TechnicianId");


            if (technicianId == null)
            {
                return RedirectToAction("Login", "TechnicianLogin");
            }



            if (!ModelState.IsValid)
            {
                return View(model);
            }



            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t =>
                    t.TechnicianId == technicianId &&
                    t.IsDeleted == false);



            if (technician == null)
            {
                return NotFound();
            }



            // Update only editable fields

            technician.Name = model.Name;

            technician.PhoneNumber = model.PhoneNumber;

            technician.Address = model.Address;

            technician.Email = model.Email;

            technician.UpdatedAt = DateTime.Now;



            _context.Technicians.Update(technician);

            await _context.SaveChangesAsync();



            TempData["SuccessMessage"] =
                "Profile updated successfully";



            return RedirectToAction(nameof(Profile));
        }
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(
                    Encoding.UTF8.GetBytes(rawData));

                return Convert.ToBase64String(bytes);
            }
        }
        // GET: Change Password
        public IActionResult ChangePassword()
        {
            var technicianId = HttpContext.Session.GetInt32("TechnicianId");


            if (technicianId == null)
            {
                return RedirectToAction(
                    "Login",
                    "TechnicianLogin");
            }


            return View();
        }
        // POST: Change Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            string currentPassword,
            string newPassword,
            string confirmPassword)
        {


            var technicianId = HttpContext.Session.GetInt32("TechnicianId");


            if (technicianId == null)
            {
                return RedirectToAction(
                    "Login",
                    "TechnicianLogin");
            }




            if (string.IsNullOrEmpty(currentPassword) ||
                string.IsNullOrEmpty(newPassword) ||
                string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Error =
                    "All fields are required.";

                return View();
            }





            if (newPassword != confirmPassword)
            {
                ViewBag.Error =
                    "New password and confirm password do not match.";

                return View();
            }






            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.TechnicianId == technicianId &&
                    u.IsDeleted == false);




            if (user == null)
            {
                ViewBag.Error =
                    "User account not found.";

                return View();
            }





            // Check old password

            var currentHash =
                ComputeSha256Hash(currentPassword);




            if (user.PasswordHash != currentHash)
            {
                ViewBag.Error =
                    "Current password is incorrect.";

                return View();
            }






            // Update new password

            user.PasswordHash =
                ComputeSha256Hash(newPassword);



            user.UpdatedAt = DateTime.Now;



            _context.Users.Update(user);


            await _context.SaveChangesAsync();




            ViewBag.Success =
                "Password changed successfully.";

            return View();

        }
        #endregion

        // GET: Technicians/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.TechnicianId == id && !t.IsDeleted);

            if (technician == null) return NotFound();

            return View(technician); // Pass the Technician object to Delete view
        }

        // POST: Technicians/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var technician = await _context.Technicians.FindAsync(id);

            if (technician != null)
            {
                technician.IsDeleted = true;
                technician.DeletedAt = DateTime.Now;
                technician.UpdatedAt = DateTime.Now;

                _context.Update(technician);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Technician deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TechnicianExists(int id)
        {
            return _context.Technicians.Any(t => t.TechnicianId == id && !t.IsDeleted);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var technician = await _context.Technicians.FindAsync(id);
            if (technician == null)
                return Json(new { success = false, message = "Technician not found" });

            technician.IsAvailable = !technician.IsAvailable;
            technician.UpdatedAt = DateTime.Now;

            _context.Update(technician);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Technician is now {(technician.IsAvailable ? "Available" : "Unavailable")}."
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var technician = await _context.Technicians.FindAsync(id);
            if (technician == null)
                return Json(new { success = false, message = "Technician not found" });

            technician.IsDeleted = true;
            technician.DeletedAt = DateTime.Now;
            technician.UpdatedAt = DateTime.Now;

            _context.Update(technician);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Technician deleted successfully!" });
        }
    }
}
