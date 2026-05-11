using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class MaintenanceSchedule
{
    [Key]
    public int MaintenanceId { get; set; }

    public int AirConId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ScheduledDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CompletedDate { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    public int? TechnicianId { get; set; }

    [ForeignKey("AirConId")]
    [InverseProperty("MaintenanceSchedules")]
    public virtual AirConUnit AirCon { get; set; } = null!;

    [ForeignKey("TechnicianId")]
    [InverseProperty("MaintenanceSchedules")]
    public virtual Technician? Technician { get; set; }
}
