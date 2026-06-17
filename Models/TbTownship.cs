using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

[Table("TB_Township")]
public partial class TbTownship
{
    [Key]
    public int TownshipPkid { get; set; }

    public int? StateDivisionPkid { get; set; }

    [StringLength(100)]
    public string? Township { get; set; }

    [Column("TownshipEN")]
    [StringLength(200)]
    public string? TownshipEn { get; set; }

    [InverseProperty("TownshipPk")]
    public virtual ICollection<CustomerLocation> CustomerLocations { get; set; } = new List<CustomerLocation>();

    [ForeignKey("StateDivisionPkid")]
    [InverseProperty("TbTownships")]
    public virtual TbStateDivision? StateDivisionPk { get; set; }
}
