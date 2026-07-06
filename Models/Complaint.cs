using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

[Table("Complaint")]
public partial class Complaint
{
    [Key]
    public int ComplaintId { get; set; }

    public int CustomerId { get; set; }

    public int? TechnicianId { get; set; }

    public int? ServiceRequestId { get; set; }

    [StringLength(150)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [StringLength(30)]
    public string Status { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("Complaints")]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("ServiceRequestId")]
    [InverseProperty("Complaints")]
    public virtual ServiceRequest? ServiceRequest { get; set; }

    [ForeignKey("TechnicianId")]
    [InverseProperty("Complaints")]
    public virtual Technician? Technician { get; set; }
}
