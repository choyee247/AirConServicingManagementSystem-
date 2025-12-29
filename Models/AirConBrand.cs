using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class AirConBrand
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string BrandName { get; set; } = null!;

    [StringLength(200)]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    [InverseProperty("Brand")]
    public virtual ICollection<AirConModel> AirConModels { get; set; } = new List<AirConModel>();

    [InverseProperty("Brand")]
    public virtual ICollection<AirConUnit> AirConUnits { get; set; } = new List<AirConUnit>();
}
