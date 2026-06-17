using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

[Table("AppointmentReassign")]
public partial class AppointmentReassign
{
    [Key]
    public int Id { get; set; }

    public int AppointmentId { get; set; }

    public int? OldTechnicianId { get; set; }

    public int NewTechnicianId { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ChangedAt { get; set; }

    public int? ChangedByUserId { get; set; }

    [ForeignKey("AppointmentId")]
    [InverseProperty("AppointmentReassigns")]
    public virtual Appointment Appointment { get; set; } = null!;

    [ForeignKey("NewTechnicianId")]
    [InverseProperty("AppointmentReassignNewTechnicians")]
    public virtual Technician NewTechnician { get; set; } = null!;

    [ForeignKey("OldTechnicianId")]
    [InverseProperty("AppointmentReassignOldTechnicians")]
    public virtual Technician? OldTechnician { get; set; }
}
