using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class ServiceRequest
{
    [Key]
    public int ServiceId { get; set; }

    public int CustomerId { get; set; }

    public int? AirConId { get; set; }

    public int? TechnicianId { get; set; }

    [StringLength(100)]
    public string ServiceType { get; set; } = null!;

    public bool IsUrgent { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime RequestedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CompletedAt { get; set; }

    [StringLength(255)]
    public string Location { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Fee { get; set; }

    [StringLength(50)]
    public string PaymentStatus { get; set; } = null!;

    [StringLength(500)]
    public string? Notes { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    public bool IsWarrantyApplied { get; set; }

    public bool IsFreeService { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DiscountAmount { get; set; }

    [ForeignKey("AirConId")]
    [InverseProperty("ServiceRequests")]
    public virtual AirConUnit? AirCon { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("ServiceRequests")]
    public virtual Customer Customer { get; set; } = null!;

    [InverseProperty("Service")]
    public virtual Payment? Payment { get; set; }

    [InverseProperty("ServiceRequest")]
    public virtual ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();

    [ForeignKey("TechnicianId")]
    [InverseProperty("ServiceRequests")]
    public virtual Technician? Technician { get; set; }
}
