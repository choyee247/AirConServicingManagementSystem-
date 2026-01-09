using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirConServicingManagementSystem.Models;
using System.Linq;
using System.Threading.Tasks;

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

            var technicians = _context.ServiceTechnicians
                .Where(t => t.IsDeleted == null || t.IsDeleted == false)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                technicians = technicians.Where(t =>
                    t.Name.Contains(searchString) ||
                    t.Phone.Contains(searchString) ||
                    t.Email.Contains(searchString) ||
                    t.Address.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                bool isActive = statusFilter == "active";
                technicians = technicians.Where(t => t.IsActive == isActive);
            }

            technicians = technicians.OrderByDescending(t => t.CreatedAt);

            return View(await technicians.ToListAsync());
        }

        // GET: Technicians/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var technician = await _context.ServiceTechnicians
                .Include(t => t.ServiceRecords)
                .Include(t => t.TechnicianBonuses)
                .FirstOrDefaultAsync(m => m.Id == id && (m.IsDeleted == null || m.IsDeleted == false));

            if (technician == null)
            {
                return NotFound();
            }

            return View(technician);
        }

        // GET: Technicians/Create
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
                var technician = new ServiceTechnician
                {
                    Name = viewModel.Name,
                    Phone = viewModel.Phone,
                    Email = viewModel.Email,
                    Address = viewModel.Address,
                    JoinDate = viewModel.JoinDate,
                    IsActive = viewModel.IsActive,
                    CreatedAt = DateTime.Now,
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
            if (id == null)
            {
                return NotFound();
            }

            var technician = await _context.ServiceTechnicians
                .FirstOrDefaultAsync(m => m.Id == id && (m.IsDeleted == null || m.IsDeleted == false));

            if (technician == null)
            {
                return NotFound();
            }

            var viewModel = new TechnicianViewModel
            {
                Id = technician.Id,
                Name = technician.Name,
                Phone = technician.Phone,
                Email = technician.Email,
                Address = technician.Address,
                JoinDate = technician.JoinDate,
                IsActive = technician.IsActive ?? true,
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
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTechnician = await _context.ServiceTechnicians
                        .FirstOrDefaultAsync(t => t.Id == id && (t.IsDeleted == null || t.IsDeleted == false));

                    if (existingTechnician == null)
                    {
                        return NotFound();
                    }

                    existingTechnician.Name = viewModel.Name;
                    existingTechnician.Phone = viewModel.Phone;
                    existingTechnician.Email = viewModel.Email;
                    existingTechnician.Address = viewModel.Address;
                    existingTechnician.JoinDate = viewModel.JoinDate;
                    existingTechnician.IsActive = viewModel.IsActive;
                    existingTechnician.UpdatedAt = DateTime.Now;

                    _context.Update(existingTechnician);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Technician updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TechnicianExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Technicians/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var technician = await _context.ServiceTechnicians
                .FirstOrDefaultAsync(m => m.Id == id && (m.IsDeleted == null || m.IsDeleted == false));

            if (technician == null)
            {
                return NotFound();
            }

            return View(technician);
        }

        // POST: Technicians/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var technician = await _context.ServiceTechnicians.FindAsync(id);

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

        // POST: Technicians/SoftDelete/5 (AJAX)
        [HttpPost]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var technician = await _context.ServiceTechnicians.FindAsync(id);

            if (technician != null)
            {
                technician.IsDeleted = true;
                technician.DeletedAt = DateTime.Now;
                technician.UpdatedAt = DateTime.Now;

                _context.Update(technician);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Technician deleted successfully!" });
            }

            return Json(new { success = false, message = "Technician not found!" });
        }

        // POST: Technicians/ToggleStatus/5 (AJAX)
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var technician = await _context.ServiceTechnicians
                .FirstOrDefaultAsync(t => t.Id == id && (t.IsDeleted == null || t.IsDeleted == false));

            if (technician != null)
            {
                technician.IsActive = !technician.IsActive;
                technician.UpdatedAt = DateTime.Now;

                _context.Update(technician);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Technician {(technician.IsActive == true ? "activated" : "deactivated")} successfully!",
                    isActive = technician.IsActive
                });
            }

            return Json(new { success = false, message = "Technician not found!" });
        }

        private bool TechnicianExists(int id)
        {
            return _context.ServiceTechnicians.Any(e => e.Id == id && (e.IsDeleted == null || e.IsDeleted == false));
        }

        // GET: Technicians/Statistics
        public async Task<IActionResult> Statistics()
        {
            var totalTechnicians = await _context.ServiceTechnicians
                .Where(t => t.IsDeleted == null || t.IsDeleted == false)
                .CountAsync();

            var activeTechnicians = await _context.ServiceTechnicians
                .Where(t => (t.IsDeleted == null || t.IsDeleted == false) && t.IsActive == true)
                .CountAsync();

            var statistics = new
            {
                Total = totalTechnicians,
                Active = activeTechnicians,
                Inactive = totalTechnicians - activeTechnicians,
                JoinThisMonth = await _context.ServiceTechnicians
                    .Where(t => (t.IsDeleted == null || t.IsDeleted == false) &&
                                t.JoinDate != null &&
                                t.JoinDate.Value.Month == DateTime.Now.Month &&
                                t.JoinDate.Value.Year == DateTime.Now.Year)
                    .CountAsync()
            };

            return View(statistics);
        }
    }
}