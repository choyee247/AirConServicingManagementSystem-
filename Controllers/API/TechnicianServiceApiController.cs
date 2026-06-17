using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirConServicingManagementSystem.Models;
using AirConServicingManagementSystem.Services;

namespace AirConServicingManagementSystem.Controllers
{
    [Route("api/technician-services")]
    [ApiController]
    public class TechnicianServiceApiController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly QrService _qrService;

        public TechnicianServiceApiController(DBContext context, QrService qrService)
        {
            _context = context;
            _qrService = qrService;
        }
        [HttpGet("dashboard/{techId}")]
        public async Task<IActionResult> Dashboard(int techId)
        {
            var result = new
            {
                AssignedCount = await _context.ServiceRequests
                    .CountAsync(s => s.TechnicianId == techId && s.Status == ServiceStatus.Assigned),

                PendingCount = await _context.ServiceRequests
                    .CountAsync(s => s.Status == ServiceStatus.Pending),

                AcceptedCount = await _context.ServiceRequests
                    .CountAsync(s => s.TechnicianId == techId && s.Status == ServiceStatus.Accepted),

                CompletedCount = await _context.ServiceRequests
                    .CountAsync(s => s.TechnicianId == techId && s.Status == ServiceStatus.Completed)
            };

            return Ok(result);
        }
        [HttpGet("assigned/{techId}")]
        public async Task<IActionResult> Assigned(int techId)
        {
            var data = await _context.ServiceRequests
                .Include(s => s.Customer)
                .Include(s => s.AirCon)
                .Where(s => s.TechnicianId == techId && s.Status == ServiceStatus.Assigned)
                .Select(s => new
                {
                    s.ServiceId,
                    s.Status,
                    Customer = s.Customer.Name,
                    AirCon = s.AirCon.AirConType
                })
                .ToListAsync();

            return Ok(data);
        }
        [HttpGet("pending")]
        public async Task<IActionResult> Pending()
        {
            var data = await _context.ServiceRequests
                .Include(s => s.Customer)
                .Include(s => s.AirCon)
                .Where(s => s.Status == ServiceStatus.Pending)
                .Select(s => new
                {
                    s.ServiceId,
                    Customer = s.Customer.Name,
                    AirCon = s.AirCon.AirConType,
                    s.Status
                })
                .ToListAsync();

            return Ok(data);
        }
        [HttpPost("accept/{id}/{techId}")]
        public async Task<IActionResult> Accept(int id, int techId)
        {
            var service = await _context.ServiceRequests.FindAsync(id);

            if (service == null)
                return NotFound();

            if (service.Status != ServiceStatus.Pending)
                return BadRequest("Not pending");

            service.Status = ServiceStatus.Accepted;
            service.TechnicianId = techId;

            await _context.SaveChangesAsync();

            return Ok(service);
        }
        [HttpPost("reject/{id}")]
        public async Task<IActionResult> Reject(int id)
        {
            var service = await _context.ServiceRequests.FindAsync(id);

            if (service == null)
                return NotFound();

            service.Status = ServiceStatus.Rejected;
            service.TechnicianId = null;

            await _context.SaveChangesAsync();

            return Ok(service);
        }
        [HttpPost("complete")]
        public async Task<IActionResult> Complete([FromBody] CompleteServiceDto model)
        {
            var service = await _context.ServiceRequests
                .FirstOrDefaultAsync(s => s.ServiceId == model.ServiceId);

            if (service == null)
                return NotFound();

            if (service.Status == ServiceStatus.Completed)
                return BadRequest("Already completed");

            // 1. Update service
            service.Status = ServiceStatus.Completed;
            service.CompletedAt = DateTime.Now;

            // 2. Remarks
            string remarks = model.SummaryOption + " " + model.AdditionalRemarks;

            // 3. Update service record
            var record = await _context.ServiceRecords
                .FirstOrDefaultAsync(r => r.ServiceRequestId == model.ServiceId);

            if (record != null)
            {
                record.Status = "Completed";
                record.Remarks = remarks;
                record.UpdatedAt = DateTime.Now;
                record.NextServiceDue = DateTime.Now.AddMonths(3);
            }

            // 4. Technician available
            var tech = await _context.Technicians
                .FirstOrDefaultAsync(t => t.TechnicianId == service.TechnicianId);

            if (tech != null)
                tech.IsAvailable = true;

            // 5. QR token
            var token = Guid.NewGuid().ToString("N");

            _context.CustomerQrTokens.Add(new CustomerQrToken
            {
                CustomerId = service.CustomerId,
                Token = token,
                CreatedAt = DateTime.Now,
                ExpiredAt = DateTime.Now.AddDays(7),
                IsUsed = false
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Service completed",
                qrToken = token
            });
        }
    }
}