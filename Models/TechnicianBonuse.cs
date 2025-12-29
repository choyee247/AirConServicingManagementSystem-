using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class TechnicianBonuse
{
    [Key]
    public int Id { get; set; }

    public int TechnicianId { get; set; }

    public int? Month { get; set; }

    public int? Year { get; set; }

    public int? TotalServices { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? BonusAmount { get; set; }

    [StringLength(200)]
    public string? Remarks { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("TechnicianId")]
    [InverseProperty("TechnicianBonuses")]
    public virtual ServiceTechnician Technician { get; set; } = null!;
}
