using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

[Table("ServiceCharge")]
public partial class ServiceCharge
{
    [Key]
    public int Id { get; set; }

    public int? ServiceRecordId { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Amount { get; set; }

    [ForeignKey("ServiceRecordId")]
    [InverseProperty("ServiceCharges")]
    public virtual ServiceRecord? ServiceRecord { get; set; }
}
