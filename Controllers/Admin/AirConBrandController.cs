using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirConServicingManagementSystem.Models;

namespace AirConServicingManagementSystem.Controllers
{

    public class AirConBrandController : Controller
    {
        private readonly DBContext _context;

        public AirConBrandController(DBContext context)
        {
            _context = context;
        }

        // ======== BRAND CRUD ========

        // GET: Brands
        public async Task<IActionResult> Brands()
        {
            var brands = await _context.AirConBrands
                .Where(b => b.IsDeleted != true)
                .ToListAsync();
            return View(brands);
        }

        // GET: Create Brand
        public IActionResult CreateBrand()
        {
            return View();
        }

        // POST: Create Brand
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBrand([Bind("BrandName,Description,IsActive")] AirConBrand brand)
        {
            if (ModelState.IsValid)
            {
                brand.CreatedAt = DateTime.Now;
                _context.Add(brand);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Brands));
            }
            return View(brand);
        }

        // GET: Edit Brand
        public async Task<IActionResult> EditBrand(int? id)
        {
            if (id == null) return NotFound();

            var brand = await _context.AirConBrands.FindAsync(id);
            if (brand == null) return NotFound();

            return View(brand);
        }

        // POST: Edit Brand
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBrand(int id, [Bind("Id,BrandName,Description,IsActive")] AirConBrand brand)
        {
            if (id != brand.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    brand.UpdatedAt = DateTime.Now;
                    _context.Update(brand);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BrandExists(brand.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Brands));
            }
            return View(brand);
        }

        // GET: Delete Brand
        public async Task<IActionResult> DeleteBrand(int? id)
        {
            if (id == null) return NotFound();

            var brand = await _context.AirConBrands.FirstOrDefaultAsync(b => b.Id == id);
            if (brand == null) return NotFound();

            return View(brand);
        }

        // POST: Delete Brand
        [HttpPost, ActionName("DeleteBrand")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBrandConfirmed(int id)
        {
            var brand = await _context.AirConBrands.FindAsync(id);
            if (brand != null)
            {
                brand.IsDeleted = true;
                brand.DeletedAt = DateTime.Now;
                _context.Update(brand);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Brands));
        }

        private bool BrandExists(int id) => _context.AirConBrands.Any(e => e.Id == id);
    }
}
