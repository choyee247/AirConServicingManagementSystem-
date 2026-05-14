using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class TechnicianSchedulePlan
{
    [Key]
    public int PlanId { get; set; }

    public int TechnicianId { get; set; }

    public int? ServiceRequestId { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = null!;

    [StringLength(50)]
    public string PlanType { get; set; } = null!;

    [StringLength(100)]
    public string? CustomerName { get; set; }

    [StringLength(255)]
    public string? Location { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime PlannedDate { get; set; }

    [StringLength(20)]
    public string Priority { get; set; } = null!;

    [StringLength(20)]
    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    public int? CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("TechnicianSchedulePlans")]
    public virtual Customer? Customer { get; set; }

    [ForeignKey("ServiceRequestId")]
    [InverseProperty("TechnicianSchedulePlans")]
    public virtual ServiceRequest? ServiceRequest { get; set; }

    [ForeignKey("TechnicianId")]
    [InverseProperty("TechnicianSchedulePlans")]
    public virtual Technician Technician { get; set; } = null!;
}
