using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

[Index("AirConId", Name = "UQ_Warranties_AirConId", IsUnique = true)]
public partial class Warranty
{
    [Key]
    public int WarrantyId { get; set; }

    public int AirConId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime StartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("AirConId")]
    [InverseProperty("Warranty")]
    public virtual AirConUnit AirCon { get; set; } = null!;
}
