using AirConServicingManagementSystem.Models;

namespace AirConServicingManagementSystem.ViewsModels
{
    public class TechnicianDashboardVM
    {
        public int AssignedCount { get; set; }
        public int PendingCount { get; set; }
        public int AcceptedCount { get; set; }
        public int CompletedCount { get; set; }
        public decimal TotalCashCollected { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public List<ServiceRequest> RecentServices { get; set; } = new();
    }
}
