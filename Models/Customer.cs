using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class Customer
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    [InverseProperty("Customer")]
    public virtual ICollection<AirConUnit> AirConUnits { get; set; } = new List<AirConUnit>();

    [InverseProperty("Customer")]
    public virtual ICollection<CustomerLocation> CustomerLocations { get; set; } = new List<CustomerLocation>();

    [InverseProperty("Customer")]
    public virtual ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();

    [InverseProperty("Customer")]
    public virtual ICollection<ServiceReminder> ServiceReminders { get; set; } = new List<ServiceReminder>();
}
