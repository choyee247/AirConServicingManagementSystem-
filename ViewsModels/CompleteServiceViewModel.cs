using AirConServicingManagementSystem.ViewsModels;
using Microsoft.AspNetCore.Http;

namespace AirConServicingManagementSystem.ViewModels
{
    public class CompleteServiceViewModel
    {
        public int ServiceId { get; set; }

        public string? CustomerName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? TechnicianName { get; set; }

        public string? JobNo { get; set; }


        public List<string> ServiceTypeList { get; set; }
            = new List<string>();

        public List<ServiceUnitVM> Units { get; set; }
            = new List<ServiceUnitVM>();

        public List<ServicePartVM> Parts { get; set; }
            = new List<ServicePartVM>();

        public List<ServiceChargeVM> Charges { get; set; }
            = new List<ServiceChargeVM>();

        public List<ServiceExpenseVM> Expenses { get; set; }
            = new List<ServiceExpenseVM>();

        public List<IFormFile>? ServicePhotos { get; set; }

        public List<IFormFile>? BeforePhotos { get; set; }

        public List<IFormFile>? AfterPhotos { get; set; }

        public List<IFormFile>? ProblemPhotos { get; set; }


        public List<string> BeforePhotoPreviews { get; set; } = new();

        public List<string> AfterPhotoPreviews { get; set; } = new();

        public List<string> ProblemPhotoPreviews { get; set; } = new();
        public string? TechnicianNote { get; set; }

        public decimal SubTotal { get; set; }

        public decimal GrandTotal { get; set; }


        public DateTime? CompletedDate { get; set; }
    }
}