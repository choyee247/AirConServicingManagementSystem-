using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace AirConServicingManagementSystem.Controllers
{
    public class ReAssignController : Controller
    {
        private readonly DBContext _context;

        public ReAssignController(DBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Create(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(x => x.Technician)
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);

            if (appointment == null) return NotFound();
            ViewBag.Technicians = await _context.Technicians
            .Where(x => !x.IsDeleted
                     && x.IsAvailable
                     && x.TechnicianId != appointment.TechnicianId) 
            .ToListAsync();

            return View(appointment);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int appointmentId, int newTechnicianId, string? reason)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            // Current Technician Id
            int? oldTechId = appointment.TechnicianId;

            var newTechnician = await _context.Technicians
                .FirstOrDefaultAsync(x => x.TechnicianId == newTechnicianId);

            if (newTechnician == null)
            {
                ModelState.AddModelError("", "Selected technician does not exist.");
                return RedirectToAction(nameof(Create), new { appointmentId });
            }

            // Transaction (Recommended)
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. ReAssign History Save
                var reassign = new AppointmentReassign
                {
                    AppointmentId = appointment.AppointmentId,
                    OldTechnicianId = oldTechId,
                    NewTechnicianId = newTechnicianId,
                    Reason = reason,
                    ChangedAt = DateTime.Now
                };

                _context.AppointmentReassigns.Add(reassign);

                if (oldTechId.HasValue)
                {
                    var oldTechnician = await _context.Technicians
                        .FirstOrDefaultAsync(x => x.TechnicianId == oldTechId.Value);

                    if (oldTechnician != null)
                    {
                        oldTechnician.IsAvailable = true;
                        oldTechnician.UpdatedAt = DateTime.Now;
                    }
                }

                // 3. New Technician Assign
                newTechnician.IsAvailable = false;
                newTechnician.UpdatedAt = DateTime.Now;

                // 4. Appointment Update
                appointment.TechnicianId = newTechnicianId;

                appointment.Status = "Assigned";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Technician reassigned successfully.";

                return RedirectToAction("Index", "Appointment");
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Error"] = "ReAssign failed.";

                return RedirectToAction(nameof(Create), new { appointmentId });
            }
        }
    }
}
