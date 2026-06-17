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
             .AsNoTracking()
             .Include(a => a.Brand)
             .Include(a => a.Model)
             .Include(a => a.Customer)
             .Include(a => a.Warranty)
             .Where(a => a.IsDeleted == false || a.IsDeleted == null)
             .OrderByDescending(a => a.CreatedAt)
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

        public async Task<IActionResult> Create(int serviceId, bool isAnother = false)
        {
            ViewBag.ServiceId = serviceId;
            ViewBag.IsAnother = isAnother;

            var service = await _context.ServiceRequests
                .Include(x => x.Appointment)
                    .ThenInclude(a => a.Customer)
                .FirstOrDefaultAsync(x => x.ServiceId == serviceId);

            if (service == null)
                return NotFound();

            ViewBag.CustomerName = service.Appointment.Customer.Name;
            ViewBag.CustomerPhone = service.Appointment.Customer.Phone;
            ViewBag.AppointmentId = service.AppointmentId;

            ViewBag.Brands = await _context.AirConBrands
                .Where(x => x.IsDeleted != true)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AirConUnit aircon, int serviceId)
        {
            var serviceRequest = await _context.ServiceRequests
                .FirstOrDefaultAsync(x => x.ServiceId == serviceId);

            if (serviceRequest == null)
                return NotFound("ServiceRequest not found");

            // ⭐ SAFE FIX (NO NAVIGATION)
            aircon.CustomerId = serviceRequest.CustomerId;

            aircon.CreatedAt = DateTime.Now;
            aircon.IsDeleted = false;

            _context.AirConUnits.Add(aircon);
            await _context.SaveChangesAsync();

            serviceRequest.AirConId = aircon.Id;
            serviceRequest.Status = "In Progress";

            _context.ServiceRequests.Update(serviceRequest);
            await _context.SaveChangesAsync();

            return RedirectToAction("Assigned", "TechnicianService");
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

            // UPDATE ONLY REQUIRED FIELDS
            aircon.BrandId = model.BrandId;
            aircon.ModelId = model.ModelId;
            aircon.AirConType = model.AirConType;
            aircon.CapacityHp = model.CapacityHp;
            aircon.InstallationDate = model.InstallationDate;

            aircon.UpdatedAt = DateTime.Now;

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
