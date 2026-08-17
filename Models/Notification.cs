using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class Notification
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [StringLength(200)]
    public string? Title { get; set; }

    public string? Message { get; set; }

    [StringLength(50)]
    public string? Type { get; set; }

    public bool? IsRead { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    public int? ServiceReminderId { get; set; }

    public int? ServiceRequestId { get; set; }

    [ForeignKey("ServiceReminderId")]
    [InverseProperty("Notifications")]
    public virtual ServiceReminder? ServiceReminder { get; set; }

    [ForeignKey("ServiceRequestId")]
    [InverseProperty("Notifications")]
    public virtual ServiceRequest? ServiceRequest { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Notifications")]
    public virtual User User { get; set; } = null!;
}
