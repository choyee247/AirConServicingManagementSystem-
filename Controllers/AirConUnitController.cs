using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirConServicingManagementSystem.Models;

namespace AirConServicingManagementSystem.Controllers
{
    public class AirConUnitController : Controller
    {
        private readonly DBContext _context;

        public AirConUnitController(DBContext context)
        {
            _context = context;
        }

        // GET: Index - List all AirConUnits
        public async Task<IActionResult> Index()
        {
            var aircons = await _context.AirConUnits
                .Include(a => a.Brand)
                .Include(a => a.Model)
                .Include(a => a.Customer)
                .Include(a => a.Warranty)
                .Where(a => a.IsDeleted != true)
                .ToListAsync();

            return View(aircons);
        }

        // GET: Details
        public async Task<IActionResult> Details(int id)
        {
            var aircon = await _context.AirConUnits
                .Include(a => a.Brand)
                .Include(a => a.Model)
                .Include(a => a.Customer)
                .Include(a => a.Warranty)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsDeleted != true);

            if (aircon == null) return NotFound();

            return View(aircon);
        }

        // GET: Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Brands = await _context.AirConBrands
                .Where(b => b.IsDeleted != true)
                .ToListAsync();

            ViewBag.Customers = await _context.Customers
                .Where(c => c.IsDeleted != true)
                .ToListAsync();

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AirConUnit aircon)
        {
            if (!ModelState.IsValid)
            {
                aircon.CreatedAt = DateTime.Now;
                aircon.IsDeleted = false;

                _context.AirConUnits.Add(aircon);
                await _context.SaveChangesAsync();

                // ⭐⭐⭐ WARRANTY CREATE ⭐⭐⭐
                if (aircon.InstallationDate != null && aircon.WarrantyPeriodMonths != null)
                {
                    Warranty warranty = new Warranty
                    {
                        AirConId = aircon.Id,
                        StartDate = aircon.InstallationDate.Value,
                        EndDate = aircon.InstallationDate.Value
                            .AddMonths(aircon.WarrantyPeriodMonths.Value),
                        IsActive = true
                    };

                    _context.Warranties.Add(warranty);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            // Reload dropdowns
            ViewBag.Brands = await _context.AirConBrands
                .Where(b => b.IsDeleted != true)
                .ToListAsync();

            ViewBag.Customers = await _context.Customers
                .Where(c => c.IsDeleted != true)
                .ToListAsync();

            return View(aircon);
        }


        // POST: Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(AirConUnit aircon)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        aircon.CreatedAt = DateTime.Now;
        //        aircon.IsDeleted = false;

        //        _context.AirConUnits.Add(aircon);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }

        //    // Reload dropdowns if validation fails
        //    ViewBag.Brands = await _context.AirConBrands
        //        .Where(b => b.IsDeleted != true)
        //        .ToListAsync();
        //    ViewBag.Customers = await _context.Customers
        //        .Where(c => c.IsDeleted != true)
        //        .ToListAsync();

        //    return View(aircon);
        //}

        public async Task<IActionResult> Edit(int id)
        {
            var aircon = await _context.AirConUnits
                .FirstOrDefaultAsync(a => a.Id == id && a.IsDeleted != true);

            if (aircon == null) return NotFound();

            ViewBag.Brands = await _context.AirConBrands
                .Where(b => b.IsDeleted != true)
                .ToListAsync();

            ViewBag.Models = await _context.AirConModels
                .Where(m => m.BrandId == aircon.BrandId && m.IsDeleted != true)
                .ToListAsync();

            ViewBag.Customers = await _context.Customers
                .Where(c => c.IsDeleted != true)
                .ToListAsync();

            return View(aircon);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AirConUnit model)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Brands = await _context.AirConBrands
                    .Where(b => b.IsDeleted != true)
                    .ToListAsync();

                ViewBag.Models = await _context.AirConModels
                    .Where(m => m.BrandId == model.BrandId && m.IsDeleted != true)
                    .ToListAsync();

                ViewBag.Customers = await _context.Customers
                    .Where(c => c.IsDeleted != true)
                    .ToListAsync();

                return View(model);
            }

            var aircon = await _context.AirConUnits
                .FirstOrDefaultAsync(a => a.Id == model.Id && a.IsDeleted != true);

            if (aircon == null) return NotFound();

            aircon.BrandId = model.BrandId;
            aircon.ModelId = model.ModelId;
            aircon.SerialNumber = model.SerialNumber;
            aircon.CapacityHp = model.CapacityHp;
            aircon.InstallationDate = model.InstallationDate;
            aircon.WarrantyPeriodMonths = model.WarrantyPeriodMonths;
            aircon.UpdatedAt = DateTime.Now;

            // ⭐ Warranty Auto Update (Important)
            if (aircon.InstallationDate.HasValue && aircon.WarrantyPeriodMonths.HasValue)
            {
                var startDate = aircon.InstallationDate.Value;
                var endDate = startDate.AddMonths(aircon.WarrantyPeriodMonths.Value);

                var warranty = await _context.Warranties
                    .FirstOrDefaultAsync(w => w.AirConId == aircon.Id);

                if (warranty == null)
                {
                    // Create new warranty record
                    warranty = new Warranty
                    {
                        AirConId = aircon.Id,
                        StartDate = startDate,
                        EndDate = endDate,
                        IsActive = endDate >= DateTime.Now
                    };
                    _context.Warranties.Add(warranty);
                }
                else
                {
                    // Update existing warranty record
                    warranty.StartDate = startDate;
                    warranty.EndDate = endDate;
                    warranty.IsActive = endDate >= DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Delete (Soft Delete)
        public async Task<IActionResult> Delete(int id)
        {
            var aircon = await _context.AirConUnits
                .FirstOrDefaultAsync(a => a.Id == id && a.IsDeleted != true);

            if (aircon == null) return NotFound();

            // Soft delete
            aircon.IsDeleted = true;
            aircon.DeletedAt = DateTime.Now;

            _context.Update(aircon);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // AJAX: Get Models by Brand
        [HttpGet]
        public async Task<JsonResult> GetModelsByBrand(int brandId)
        {
            var models = await _context.AirConModels
                .Where(m => m.BrandId == brandId && m.IsDeleted != true)
                .Select(m => new { id = m.Id, modelName = m.ModelName })
                .ToListAsync();

            return Json(models);
        }
    }
}
