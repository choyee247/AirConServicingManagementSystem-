using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class ActivityLog
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [StringLength(50)]
    public string? Username { get; set; }

    [StringLength(20)]
    public string? Role { get; set; }

    [StringLength(50)]
    public string Action { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Controller { get; set; }

    [StringLength(100)]
    public string? ActionName { get; set; }

    [StringLength(50)]
    public string? IpAddress { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("ActivityLogs")]
    public virtual User? User { get; set; }
}
