using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

[Table("TB_StateDivision")]
public partial class TbStateDivision
{
    [Key]
    public int StateDivisionPkid { get; set; }

    [StringLength(255)]
    public string StateDivision { get; set; } = null!;

    [Column("StateDivisionEN")]
    [StringLength(255)]
    public string? StateDivisionEn { get; set; }

    [InverseProperty("StateDivisionPk")]
    public virtual ICollection<CustomerLocation> CustomerLocations { get; set; } = new List<CustomerLocation>();

    [InverseProperty("StateDivisionPk")]
    public virtual ICollection<TbTownship> TbTownships { get; set; } = new List<TbTownship>();
}
