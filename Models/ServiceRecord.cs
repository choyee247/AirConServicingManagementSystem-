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

    public int AirConUnitId { get; set; }

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

    [ForeignKey("AirConUnitId")]
    [InverseProperty("ServiceRecords")]
    public virtual AirConUnit AirConUnit { get; set; } = null!;

    [ForeignKey("CustomerId")]
    [InverseProperty("ServiceRecords")]
    public virtual Customer Customer { get; set; } = null!;

    [InverseProperty("ServiceRecord")]
    public virtual ICollection<ServicePhoto> ServicePhotos { get; set; } = new List<ServicePhoto>();

    [InverseProperty("ServiceRecord")]
    public virtual ICollection<ServiceWarranty> ServiceWarranties { get; set; } = new List<ServiceWarranty>();

    [ForeignKey("TechnicianId")]
    [InverseProperty("ServiceRecords")]
    public virtual ServiceTechnician Technician { get; set; } = null!;
}
