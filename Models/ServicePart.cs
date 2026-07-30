using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

[Table("ServicePart")]
public partial class ServicePart
{
    [Key]
    public int Id { get; set; }

    public int? ServiceRecordId { get; set; }

    public int? AirConUnitId { get; set; }

    [StringLength(100)]
    public string? PartName { get; set; }

    public int? Qty { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? UnitPrice { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Total { get; set; }

    [ForeignKey("AirConUnitId")]
    [InverseProperty("ServiceParts")]
    public virtual AirConUnit? AirConUnit { get; set; }

    [ForeignKey("ServiceRecordId")]
    [InverseProperty("ServiceParts")]
    public virtual ServiceRecord? ServiceRecord { get; set; }
}
