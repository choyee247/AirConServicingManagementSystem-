using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

[Table("Payment")]
public partial class Payment
{
    [Key]
    public int PaymentId { get; set; }

    public int ServiceRecordId { get; set; }

    [StringLength(50)]
    public string? InvoiceNo { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PaymentDate { get; set; }

    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Amount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? PaidAmount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? ChangeAmount { get; set; }

    [StringLength(50)]
    public string? PaymentStatus { get; set; }

    [StringLength(500)]
    public string? Remark { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("ServiceRecordId")]
    [InverseProperty("Payments")]
    public virtual ServiceRecord ServiceRecord { get; set; } = null!;
}
