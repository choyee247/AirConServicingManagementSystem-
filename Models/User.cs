using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class User
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Username { get; set; } = null!;

    [StringLength(256)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(20)]
    public string? Role { get; set; }

    public int? TechnicianId { get; set; }

    public bool? IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("TechnicianId")]
    [InverseProperty("Users")]
    public virtual ServiceTechnician? Technician { get; set; }
}
