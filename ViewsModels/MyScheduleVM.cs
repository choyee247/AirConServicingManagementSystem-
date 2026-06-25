using AirConServicingManagementSystem.Models;

namespace AirConServicingManagementSystem.ViewsModels
{
    public class MyScheduleVM
    {
        public List<TechnicianSchedulePlan> Plans { get; set; }
        public List<TechnicianSchedulePlan> TodayJobs { get; set; } = new();
        public int TodayCount { get; set; }
        public int UpcomingCount { get; set; }
        public int HighPriorityCount { get; set; }
        public int CompletedCount { get; set; }
    }
}
