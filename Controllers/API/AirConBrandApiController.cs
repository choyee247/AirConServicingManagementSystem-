using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirConServicingManagementSystem.Models;

namespace AirConServicingManagementSystem.Controllers.API
{
    [Route("api/brands")]
    [ApiController]
    public class AirConBrandApiController : ControllerBase
    {
        private readonly DBContext _context;

        public AirConBrandApiController(DBContext context)
        {
            _context = context;
        }

        // =======================
        // GET ALL BRANDS
        // =======================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var brands = await _context.AirConBrands
                .Where(b => b.IsDeleted != true)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    b.Id,
                    b.BrandName,
                    b.CreatedAt
                })
                .ToListAsync();

            return Ok(brands);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var brand = await _context.AirConBrands
                .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted != true);

            if (brand == null)
                return NotFound();

            return Ok(brand);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AirConBrand model)
        {
            if (model == null)
                return BadRequest();

            var brand = new AirConBrand
            {
                BrandName = model.BrandName,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            _context.AirConBrands.Add(brand);
            await _context.SaveChangesAsync();

            return Ok(brand);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AirConBrand model)
        {
            var brand = await _context.AirConBrands
                .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted != true);

            if (brand == null)
                return NotFound();

            brand.BrandName = model.BrandName;
            brand.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(brand);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _context.AirConBrands
                .FirstOrDefaultAsync(b => b.Id == id);

            if (brand == null)
                return NotFound();

            brand.IsDeleted = true;
            brand.DeletedAt = DateTime.Now;
            brand.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Brand deleted successfully" });
        }
    }
}