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

    [ForeignKey("CustomerId")]
    [InverseProperty("CustomerLocations")]
    public virtual Customer Customer { get; set; } = null!;
}
