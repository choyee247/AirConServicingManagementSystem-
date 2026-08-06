using AirConServicingManagementSystem.Models;

namespace AirConServicingManagementSystem.ViewModels
{
    public class DashboardViewModel
    {
        // ===========================
        // Dashboard Statistics
        // ===========================

        public int TotalCustomers { get; set; }

        public int TotalTechnicians { get; set; }

        public int TotalAirConUnits { get; set; }

        public int TotalAppointmentsToday { get; set; }

        public int PendingServices { get; set; }

        public int InProgressServices { get; set; }

        public int CompletedServices { get; set; }

        public int UpcomingMaintenance { get; set; }


        // ===========================
        // Latest Records
        // ===========================

        public List<ServiceRequest> RecentServices { get; set; }
            = new();

        public List<MaintenanceSchedule> UpcomingMaintenances { get; set; }
            = new();
    }
}