using AirConServicingManagementSystem.Models;

namespace AirConServicingManagementSystem.ViewsModels
{
    public class AdminDashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalTechnicians { get; set; }
        public int ActiveServices { get; set; }
        public int WarrantyCases { get; set; }
        public int ActiveTechnicians { get; set; }
        public int AvailableTechnicians { get; set; }
        public int MonthlyNewCustomers { get; set; }
        public List<ServiceRequest> RecentServices { get; set; }
        public int CompletedServices { get;  set; }
        public int TotalServices { get; set; }
        public int ExpiringWarranty { get; set; }
        public string AdminName { get; set; }
        public DateTime CurrentDateTime { get; set; }
        public List<CustomerFeedback> RecentFeedbacks { get; set; } = new();
        public double AverageRating { get; set; }
        public List<Complaint> RecentComplaints { get; set; } = new();
        public int NewComplaints { get; set; }
        public int InProgressComplaints { get; set; }
        public int ResolvedComplaints { get; set; }
        public List<Technician> Technicians { get; set; } = new();

        public int BusyTechnicians { get; set; }

        public int OnLeaveTechnicians { get; set; }
        public List<ServiceRequest> RecentServiceRecords { get; set; } = new();
        public List<Customer> Customers { get; set; } = new();
    }
}
