using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirConServicingManagementSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

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
                .Include(t => t.Payments)
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


        // GET: Technicians/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Technicians/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TechnicianViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var technician = new Technician
                {
                    Name = viewModel.Name,
                    PhoneNumber = viewModel.PhoneNumber,
                    Email = viewModel.Email,
                    Address = viewModel.Address,
                    TechnicianRole = viewModel.TechnicianRole,
                    JoinDate = viewModel.JoinDate ?? DateTime.Now,
                    LeaveDate = viewModel.LeaveDate,
                    IsAvailable = viewModel.IsAvailable,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                };

                _context.Add(technician);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Technician created successfully!";
                return RedirectToAction(nameof(Index));
            }
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
