using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

[Table("ServiceRecordUnit")]
public partial class ServiceRecordUnit
{
    [Key]
    public int Id { get; set; }

    public int ServiceRecordId { get; set; }

    public int AirConUnitId { get; set; }

    [Column("ACCondition")]
    [StringLength(50)]
    public string? Accondition { get; set; }

    [StringLength(1000)]
    public string? ProblemFound { get; set; }

    [StringLength(1000)]
    public string? RepairAction { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NextServiceDue { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("AirConUnitId")]
    [InverseProperty("ServiceRecordUnits")]
    public virtual AirConUnit AirConUnit { get; set; } = null!;

    [ForeignKey("ServiceRecordId")]
    [InverseProperty("ServiceRecordUnits")]
    public virtual ServiceRecord ServiceRecord { get; set; } = null!;
}
