using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirConServicingManagementSystem.Models;
using System;

public class ReAppointmentController : Controller
{
    private readonly DBContext _context;

    public ReAppointmentController(DBContext context)
    {
        _context = context;
    }
    public async Task<IActionResult> Create(int serviceRecordId)
    {
        var serviceRecord = await _context.ServiceRecords
            .Include(x => x.Customer)
            .Include(x => x.ServiceRequest)
                .ThenInclude(x => x.Appointment)
            .FirstOrDefaultAsync(x => x.Id == serviceRecordId);

        if (serviceRecord == null)
            return NotFound();

        ViewBag.ServiceRecord = serviceRecord;

        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int serviceRecordId, DateTime scheduledDate, string location, string notes)
    {
        var serviceRecord = await _context.ServiceRecords
            .Include(x => x.ServiceRequest)
                .ThenInclude(x => x.Appointment)
            .FirstOrDefaultAsync(x => x.Id == serviceRecordId);

        if (serviceRecord == null)
            return NotFound();

        var oldAppointment = serviceRecord.ServiceRequest?.Appointment;

        if (oldAppointment == null)
            return NotFound();

        var newAppointment = new Appointment
        {
            CustomerId = oldAppointment.CustomerId,
            TechnicianId = null,
            ScheduledDate = scheduledDate,
            Location = location,
            Notes = notes,
            Status = "Pending",
            IsReAppointment = true,
            ParentAppointmentId = oldAppointment.AppointmentId
        };

        _context.Appointments.Add(newAppointment);

        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Appointment");
    }
}