using System;
using System.ComponentModel.DataAnnotations;

namespace AirConServicingManagementSystem.Models
{
    public class TechnicianViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = null!;

        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        [StringLength(200)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Join Date")]
        [DataType(DataType.Date)]
        public DateTime? JoinDate { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created At")]
        public DateTime? CreatedAt { get; set; }

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }
    }
}