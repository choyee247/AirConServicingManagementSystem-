using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirConServicingManagementSystem.Models;

namespace AirConServicingManagementSystem.Controllers.API
{
    [Route("api/models")]
    [ApiController]
    public class AirConModelApiController : ControllerBase
    {
        private readonly DBContext _context;

        public AirConModelApiController(DBContext context)
        {
            _context = context;
        }

        // =========================
        // GET ALL MODELS
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var models = await _context.AirConModels
                .Include(m => m.Brand)
                .Where(m => m.IsDeleted != true)
                .OrderByDescending(m => m.Id)
                .Select(m => new
                {
                    m.Id,
                    m.ModelName,
                    m.BrandId,
                    BrandName = m.Brand != null ? m.Brand.BrandName : null
                })
                .ToListAsync();

            return Ok(models);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var model = await _context.AirConModels
                .Include(m => m.Brand)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsDeleted != true);

            if (model == null)
                return NotFound();

            return Ok(new
            {
                model.Id,
                model.ModelName,
                model.BrandId,
                BrandName = model.Brand?.BrandName
            });
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AirConModel model)
        {
            if (model == null)
                return BadRequest();

            var newModel = new AirConModel
            {
                BrandId = model.BrandId,
                ModelName = model.ModelName,
                IsDeleted = false
            };

            _context.AirConModels.Add(newModel);
            await _context.SaveChangesAsync();

            return Ok(newModel);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AirConModel model)
        {
            var existing = await _context.AirConModels
                .FirstOrDefaultAsync(m => m.Id == id && m.IsDeleted != true);

            if (existing == null)
                return NotFound();

            existing.BrandId = model.BrandId;
            existing.ModelName = model.ModelName;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _context.AirConModels
                .FirstOrDefaultAsync(m => m.Id == id);

            if (model == null)
                return NotFound();

            model.IsDeleted = true;
            model.DeletedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Model deleted successfully" });
        }
    }
}