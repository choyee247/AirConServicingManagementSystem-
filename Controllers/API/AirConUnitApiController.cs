using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Controllers.API
{
    [Route("api/aircons")]
    [ApiController]
    public class AirConUnitApiController : ControllerBase
    {
        private readonly DBContext _context;

        public AirConUnitApiController(DBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var aircons = await _context.AirConUnits
                .Where(a => a.IsDeleted != true)
                .Select(a => new
                {
                    a.Id,
                    a.AirConType,
                    a.CapacityHp,
                    a.InstallationDate,

                    Brand = a.Brand != null ? a.Brand.BrandName : null,
                    Model = a.Model != null ? a.Model.ModelName : null,
                    Customer = a.Customer != null ? a.Customer.Name : null,
                    WarrantyEnd = a.Warranty != null 
                })
                .ToListAsync();

            return Ok(aircons);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var aircon = await _context.AirConUnits
                .Include(a => a.Brand)
                .Include(a => a.Model)
                .Include(a => a.Customer)
                .Include(a => a.Warranty)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsDeleted != true);

            if (aircon == null)
                return NotFound();

            return Ok(aircon);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AirConUnit aircon)
        {
            aircon.CreatedAt = DateTime.Now;
            aircon.IsDeleted = false;

            _context.AirConUnits.Add(aircon);
            await _context.SaveChangesAsync();

            // WARRANTY CREATE
            if (aircon.WarrantyPeriodMonths != null)
            {
                var warranty = new Warranty
                {
                    AirConId = aircon.Id,
                    StartDate = aircon.InstallationDate.Value,
                    EndDate = aircon.InstallationDate.Value.AddMonths(aircon.WarrantyPeriodMonths.Value),
                    IsActive = true
                };

                _context.Warranties.Add(warranty);
                await _context.SaveChangesAsync();
            }

            return Ok(aircon);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, AirConUnit model)
        {
            var aircon = await _context.AirConUnits
                .FirstOrDefaultAsync(a => a.Id == id && a.IsDeleted != true);

            if (aircon == null)
                return NotFound();

            aircon.BrandId = model.BrandId;
            aircon.ModelId = model.ModelId;
            aircon.AirConType = model.AirConType;
            aircon.CapacityHp = model.CapacityHp;
            aircon.InstallationDate = model.InstallationDate;
            aircon.WarrantyPeriodMonths = model.WarrantyPeriodMonths;
            aircon.UpdatedAt = DateTime.Now;

            // Warranty update
            if (aircon.InstallationDate.HasValue && aircon.WarrantyPeriodMonths.HasValue)
            {
                var start = aircon.InstallationDate.Value;
                var end = start.AddMonths(aircon.WarrantyPeriodMonths.Value);

                var warranty = await _context.Warranties
                    .FirstOrDefaultAsync(w => w.AirConId == aircon.Id);

                if (warranty == null)
                {
                    _context.Warranties.Add(new Warranty
                    {
                        AirConId = aircon.Id,
                        StartDate = start,
                        EndDate = end,
                        IsActive = end >= DateTime.Now
                    });
                }
                else
                {
                    warranty.StartDate = start;
                    warranty.EndDate = end;
                    warranty.IsActive = end >= DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(aircon);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var aircon = await _context.AirConUnits
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aircon == null)
                return NotFound();

            aircon.IsDeleted = true;
            aircon.DeletedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}