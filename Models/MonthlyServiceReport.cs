using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class MonthlyServiceReport
{
    [Key]
    public int Id { get; set; }

    public int? Month { get; set; }

    public int? Year { get; set; }

    public int? TotalServices { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? TotalRevenue { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }
}
