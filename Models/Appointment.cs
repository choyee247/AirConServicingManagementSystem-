using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class Appointment
{
    [Key]
    public int AppointmentId { get; set; }

    public int CustomerId { get; set; }

    public int? TechnicianId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ScheduledDate { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [StringLength(255)]
    public string Location { get; set; } = null!;

    [StringLength(500)]
    public string? Notes { get; set; }

    public bool IsReAppointment { get; set; }

    public int? ParentAppointmentId { get; set; }

    [InverseProperty("Appointment")]
    public virtual ICollection<AppointmentReassign> AppointmentReassigns { get; set; } = new List<AppointmentReassign>();

    [ForeignKey("CustomerId")]
    [InverseProperty("Appointments")]
    public virtual Customer Customer { get; set; } = null!;

    [InverseProperty("ParentAppointment")]
    public virtual ICollection<Appointment> InverseParentAppointment { get; set; } = new List<Appointment>();

    [ForeignKey("ParentAppointmentId")]
    [InverseProperty("InverseParentAppointment")]
    public virtual Appointment? ParentAppointment { get; set; }

    [InverseProperty("Appointment")]
    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

    [ForeignKey("TechnicianId")]
    [InverseProperty("Appointments")]
    public virtual Technician? Technician { get; set; }
}
