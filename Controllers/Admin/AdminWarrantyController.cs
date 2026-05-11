using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Controllers.Admin
{
    public class AdminWarrantyController : Controller
    {
        private readonly DBContext _context;

        public AdminWarrantyController(DBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var warranties = await _context.Warranties
                .Include(w => w.AirCon)
                    .ThenInclude(a => a.Brand)
                .Include(w => w.AirCon)
                    .ThenInclude(a => a.Model)
                .Include(w => w.AirCon)
                    .ThenInclude(a => a.Customer)
                .ToListAsync();

            return View(warranties);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.AirCons = await _context.AirConUnits
                .Where(a => a.Warranty == null)
                .Include(a => a.Brand)
                .Include(a => a.Model)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Warranty warranty)
        {
            warranty.IsActive = warranty.EndDate >= DateTime.Now;

            _context.Warranties.Add(warranty);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int id)
        {
            var warranty = await _context.Warranties.FindAsync(id);
            return View(warranty);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Warranty warranty)
        {
            warranty.IsActive = warranty.EndDate >= DateTime.Now;

            _context.Warranties.Update(warranty);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}
