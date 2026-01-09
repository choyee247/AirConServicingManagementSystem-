using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirConServicingManagementSystem.Models;

namespace AirConServicingManagementSystem.Controllers
{

    public class AirConModelController : Controller
    {
        private readonly DBContext _context;

        public AirConModelController(DBContext context)
        {
            _context = context;
        }

        // ======== MODEL CRUD ========

        // GET: Models
        public async Task<IActionResult> Models()
        {
            var models = await _context.AirConModels
                .Include(m => m.Brand)
                .Where(m => m.IsDeleted != true)
                .ToListAsync();
            return View(models);
        }

        // GET: Create Model
        public async Task<IActionResult> CreateModel()
        {
            ViewBag.Brands = await _context.AirConBrands
                .Where(b => b.IsDeleted != true)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModel([Bind("BrandId,ModelName,Hp")] AirConModel model)
        {
            if (!ModelState.IsValid)
            {
                model.IsDeleted = false; // Add default
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Models));
            }

            ViewBag.Brands = await _context.AirConBrands
                .Where(b => b.IsDeleted != true)
                .ToListAsync();

            return View(model);
        }


        // GET: Edit Model
        public async Task<IActionResult> EditModel(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.AirConModels.FindAsync(id);
            if (model == null) return NotFound();

            ViewBag.Brands = await _context.AirConBrands.Where(b => b.IsDeleted != true).ToListAsync();
            return View(model);
        }

        // POST: Edit Model
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditModel(int id, [Bind("Id,BrandId,ModelName,Hp")] AirConModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModelExists(model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Models));
            }
            ViewBag.Brands = await _context.AirConBrands.ToListAsync();
            return View(model);
        }

        // GET: Delete Model
        public async Task<IActionResult> DeleteModel(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.AirConModels
                .Include(m => m.Brand)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: Delete Model
        [HttpPost, ActionName("DeleteModel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteModelConfirmed(int id)
        {
            var model = await _context.AirConModels.FindAsync(id);
            if (model != null)
            {
                model.IsDeleted = true;
                model.DeletedAt = DateTime.Now;
                _context.Update(model);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Models));
        }

        private bool ModelExists(int id) => _context.AirConModels.Any(e => e.Id == id);
    }
}
