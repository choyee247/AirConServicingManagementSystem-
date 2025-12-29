using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class ServiceWarranty
{
    [Key]
    public int Id { get; set; }

    public int ServiceRecordId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? WarrantyStartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? WarrantyEndDate { get; set; }

    [StringLength(500)]
    public string? Terms { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("ServiceRecordId")]
    [InverseProperty("ServiceWarranties")]
    public virtual ServiceRecord ServiceRecord { get; set; } = null!;
}
