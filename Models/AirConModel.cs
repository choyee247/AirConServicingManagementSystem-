using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class AirConModel
{
    [Key]
    public int Id { get; set; }

    public int BrandId { get; set; }

    [StringLength(100)]
    public string? ModelName { get; set; }

    [Column("HP")]
    [StringLength(20)]
    public string? Hp { get; set; }

    public bool? IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    [InverseProperty("Model")]
    public virtual ICollection<AirConUnit> AirConUnits { get; set; } = new List<AirConUnit>();

    [ForeignKey("BrandId")]
    [InverseProperty("AirConModels")]
    public virtual AirConBrand Brand { get; set; } = null!;
}
