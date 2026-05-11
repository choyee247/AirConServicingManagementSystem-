using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

[Table("CustomerQrToken")]
public partial class CustomerQrToken
{
    [Key]
    public int Id { get; set; }

    public int CustomerId { get; set; }

    [StringLength(100)]
    public string Token { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ExpiredAt { get; set; }

    public bool IsUsed { get; set; }

    public int AirConId { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("CustomerQrTokens")]
    public virtual Customer Customer { get; set; } = null!;
}
