using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirConServicingManagementSystem.Models;

namespace AirConServicingManagementSystem.Controllers.API
{
    [Route("api/technicians")]
    [ApiController]
    public class TechniciansApiController : ControllerBase
    {
        private readonly DBContext _context;

        public TechniciansApiController(DBContext context)
        {
            _context = context;
        }

        // GET: api/technicians
        [HttpGet]
        public async Task<IActionResult> GetAll(string? search, string? status)
        {
            var query = _context.Technicians
                .Where(t => !t.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t =>
                    t.Name.Contains(search) ||
                    t.PhoneNumber.Contains(search) ||
                    t.Email.Contains(search) ||
                    t.Address.Contains(search) ||
                    t.TechnicianRole.Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
            {
                bool isAvailable = status.ToLower() == "available";
                query = query.Where(t => t.IsAvailable == isAvailable);
            }

            var data = await query
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.TechnicianId,
                    t.Name,
                    t.PhoneNumber,
                    t.Email,
                    t.Address,
                    t.TechnicianRole,
                    t.IsAvailable,
                    t.JoinDate,
                    t.LeaveDate,
                    t.CreatedAt
                })
                .ToListAsync();

            return Ok(data);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var technician = await _context.Technicians
                .Include(t => t.Appointments)
                .Include(t => t.Payments)
                .Include(t => t.ServiceRequests)
                .FirstOrDefaultAsync(t => t.TechnicianId == id && !t.IsDeleted);

            if (technician == null)
                return NotFound();

            return Ok(technician);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Technician model)
        {
            if (model == null)
                return BadRequest();

            var technician = new Technician
            {
                Name = model.Name,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Address = model.Address,
                TechnicianRole = model.TechnicianRole,
                JoinDate = model.JoinDate ?? DateTime.Now,
                LeaveDate = model.LeaveDate,
                IsAvailable = model.IsAvailable,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsDeleted = false
            };

            _context.Technicians.Add(technician);
            await _context.SaveChangesAsync();

            return Ok(technician);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Technician model)
        {
            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.TechnicianId == id && !t.IsDeleted);

            if (technician == null)
                return NotFound();

            technician.Name = model.Name;
            technician.PhoneNumber = model.PhoneNumber;
            technician.Email = model.Email;
            technician.Address = model.Address;
            technician.TechnicianRole = model.TechnicianRole;
            technician.JoinDate = model.JoinDate;
            technician.LeaveDate = model.LeaveDate;
            technician.IsAvailable = model.IsAvailable;
            technician.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(technician);
        }

        [HttpPost("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.TechnicianId == id && !t.IsDeleted);

            if (technician == null)
                return NotFound();

            technician.IsAvailable = !technician.IsAvailable;
            technician.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                technician.TechnicianId,
                technician.IsAvailable
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.TechnicianId == id);

            if (technician == null)
                return NotFound();

            technician.IsDeleted = true;
            technician.DeletedAt = DateTime.Now;
            technician.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
    
