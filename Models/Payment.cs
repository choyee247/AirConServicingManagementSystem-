using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

[Index("ServiceId", Name = "UQ_Payments_ServiceId", IsUnique = true)]
public partial class Payment
{
    [Key]
    public int PaymentId { get; set; }

    public int ServiceId { get; set; }

    public int TechnicianId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime PaidAt { get; set; }

    [StringLength(50)]
    public string PaymentMethod { get; set; } = null!;

    [ForeignKey("ServiceId")]
    [InverseProperty("Payment")]
    public virtual ServiceRequest Service { get; set; } = null!;

    [ForeignKey("TechnicianId")]
    [InverseProperty("Payments")]
    public virtual Technician Technician { get; set; } = null!;
}
