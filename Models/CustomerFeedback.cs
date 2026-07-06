using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

[Table("CustomerFeedback")]
public partial class CustomerFeedback
{
    [Key]
    public int FeedbackId { get; set; }

    public int CustomerId { get; set; }

    public int? TechnicianId { get; set; }

    public int? ServiceRequestId { get; set; }

    public int Rating { get; set; }

    [StringLength(500)]
    public string? Comment { get; set; }

    [StringLength(30)]
    public string? FeedbackType { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("CustomerFeedbacks")]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("ServiceRequestId")]
    [InverseProperty("CustomerFeedbacks")]
    public virtual ServiceRequest? ServiceRequest { get; set; }

    [ForeignKey("TechnicianId")]
    [InverseProperty("CustomerFeedbacks")]
    public virtual Technician? Technician { get; set; }
}
