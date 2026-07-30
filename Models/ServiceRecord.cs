using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class ServiceRecord
{
    [Key]
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int TechnicianId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ServiceDate { get; set; }

    [StringLength(50)]
    public string? ServiceType { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NextServiceDue { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    public int? ServiceRequestId { get; set; }

    [StringLength(1000)]
    public string? ProblemFound { get; set; }

    [StringLength(1000)]
    public string? RepairAction { get; set; }

    [StringLength(1000)]
    public string? PartsReplaced { get; set; }

    [StringLength(1000)]
    public string? TechnicianNote { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? ServiceCost { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("ServiceRecords")]
    public virtual Customer Customer { get; set; } = null!;

    [InverseProperty("ServiceRecord")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [InverseProperty("ServiceRecord")]
    public virtual ICollection<ServiceCharge> ServiceCharges { get; set; } = new List<ServiceCharge>();

    [InverseProperty("ServiceRecord")]
    public virtual ICollection<ServiceExpense> ServiceExpenses { get; set; } = new List<ServiceExpense>();

    [InverseProperty("ServiceRecord")]
    public virtual ICollection<ServicePart> ServiceParts { get; set; } = new List<ServicePart>();

    [InverseProperty("ServiceRecord")]
    public virtual ICollection<ServicePhoto> ServicePhotos { get; set; } = new List<ServicePhoto>();

    [InverseProperty("ServiceRecord")]
    public virtual ICollection<ServiceRecordUnit> ServiceRecordUnits { get; set; } = new List<ServiceRecordUnit>();

    [ForeignKey("ServiceRequestId")]
    [InverseProperty("ServiceRecords")]
    public virtual ServiceRequest? ServiceRequest { get; set; }

    [ForeignKey("TechnicianId")]
    [InverseProperty("ServiceRecords")]
    public virtual Technician Technician { get; set; } = null!;
}
