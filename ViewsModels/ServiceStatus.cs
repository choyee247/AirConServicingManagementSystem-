using AirConServicingManagementSystem.Models;
namespace AirConServicingManagementSystem.Models
{
    public static class ServiceStatus
    {
        public const string Pending = "Pending";        // Customer requested
        public const string Assigned = "Assigned";      // Admin assigned
        public const string Accepted = "Accepted";      // Technician accepted
        public const string Rejected = "Rejected";      // Technician rejected
        public const string InProgress = "InProgress";  // Technician on the way
        public const string Completed = "Completed";    // Service done
    }
}
