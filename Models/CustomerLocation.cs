using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class CustomerLocation
{
    [Key]
    public int Id { get; set; }

    public int CustomerId { get; set; }

    [Column(TypeName = "decimal(10, 7)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(10, 7)")]
    public decimal? Longitude { get; set; }

    [StringLength(200)]
    public string? MapAddress { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    public int? StateDivisionPkid { get; set; }

    public int? TownshipPkid { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("CustomerLocations")]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("StateDivisionPkid")]
    [InverseProperty("CustomerLocations")]
    public virtual TbStateDivision? StateDivisionPk { get; set; }

    [ForeignKey("TownshipPkid")]
    [InverseProperty("CustomerLocations")]
    public virtual TbTownship? TownshipPk { get; set; }
}
