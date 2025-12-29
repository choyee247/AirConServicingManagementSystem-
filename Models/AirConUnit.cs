using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class AirConUnit
{
    [Key]
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int BrandId { get; set; }

    public int ModelId { get; set; }

    [StringLength(100)]
    public string? SerialNumber { get; set; }

    [Column("CapacityHP")]
    [StringLength(20)]
    public string? CapacityHp { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? InstallationDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("BrandId")]
    [InverseProperty("AirConUnits")]
    public virtual AirConBrand Brand { get; set; } = null!;

    [ForeignKey("CustomerId")]
    [InverseProperty("AirConUnits")]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("ModelId")]
    [InverseProperty("AirConUnits")]
    public virtual AirConModel Model { get; set; } = null!;

    [InverseProperty("AirConUnit")]
    public virtual ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();

    [InverseProperty("AirConUnit")]
    public virtual ICollection<ServiceReminder> ServiceReminders { get; set; } = new List<ServiceReminder>();
}
