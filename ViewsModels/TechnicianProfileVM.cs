using System;

namespace AirConServicingManagementSystem.Models
{
    public class TechnicianProfileVM
    {

        public int TechnicianId { get; set; }


        public string? Username { get; set; }


        public string? Name { get; set; }


        public string? PhoneNumber { get; set; }


        public string? Address { get; set; }


        public string? Email { get; set; }


        public string? TechnicianRole { get; set; }


        public DateTime? JoinDate { get; set; }


        public bool? IsAvailable { get; set; }

    }
}