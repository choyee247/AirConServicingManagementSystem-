using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class ServiceReminder
{
    [Key]
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int AirConUnitId { get; set; }

    [StringLength(10)]
    public string? ReminderType { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ReminderDate { get; set; }

    public bool? SentStatus { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("AirConUnitId")]
    [InverseProperty("ServiceReminders")]
    public virtual AirConUnit AirConUnit { get; set; } = null!;

    [ForeignKey("CustomerId")]
    [InverseProperty("ServiceReminders")]
    public virtual Customer Customer { get; set; } = null!;
}
