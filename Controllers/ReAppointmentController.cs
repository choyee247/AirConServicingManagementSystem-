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
                .ThenInclude(c => c.CustomerLocations)
                    .ThenInclude(l => l.StateDivisionPk)
            .Include(x => x.Customer)
                .ThenInclude(c => c.CustomerLocations)
                    .ThenInclude(l => l.TownshipPk)
            .Include(x => x.ServiceRequest)
                .ThenInclude(x => x.Appointment)
            .FirstOrDefaultAsync(x => x.Id == serviceRecordId);

        if (serviceRecord == null)
            return NotFound();

        var location = serviceRecord.Customer.CustomerLocations
            .FirstOrDefault();

        if (location != null)
        {
            ViewBag.LocationText =
                $"{serviceRecord.Customer.Address}," + $"{location.TownshipPk?.TownshipEn}, " + $"{location.StateDivisionPk?.StateDivisionEn}" ;
        }

        else
        {
            ViewBag.LocationText = serviceRecord.Customer.Address;
        }

        ViewBag.ServiceRecord = serviceRecord;

        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
      int serviceRecordId,
      DateTime scheduledDate,
      string location,
      string notes)
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

        if (scheduledDate < DateTime.Now)
        {
            TempData["Error"] = "Please choose a future date.";

            return RedirectToAction(nameof(Create),
                new { serviceRecordId });
        }

        Appointment appointment = new Appointment
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

        _context.Appointments.Add(appointment);

        await _context.SaveChangesAsync();

        TempData["Success"] = "ReAppointment created successfully.";

        return RedirectToAction("Index", "Appointment");
    }
}