using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class ServicePhoto
{
    [Key]
    public int Id { get; set; }

    public int ServiceRecordId { get; set; }

    [StringLength(10)]
    public string? PhotoType { get; set; }

    [StringLength(200)]
    public string? PhotoPath { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    public int? AirConUnitId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [ForeignKey("AirConUnitId")]
    [InverseProperty("ServicePhotos")]
    public virtual AirConUnit? AirConUnit { get; set; }

    [ForeignKey("ServiceRecordId")]
    [InverseProperty("ServicePhotos")]
    public virtual ServiceRecord ServiceRecord { get; set; } = null!;
}
