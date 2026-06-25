using AirConServicingManagementSystem.Models;

namespace AirConServicingManagementSystem.ViewsModels
{
    public class TechnicianDashboardVM
    {
        // =========================
        // TASK COUNTS
        // =========================
        public int AssignedCount { get; set; }
        public int PendingCount { get; set; }
        public int AcceptedCount { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public int RejectedCount { get; set; }
        public string TechnicianName { get; set; }
        public DateTime CurrentDateTime { get; set; }
        public int TotalCount => CompletedCount + PendingCount + InProgressCount;
        public double CompletionRate => TotalCount == 0 ? 0 : (double)CompletedCount / TotalCount * 100;
        public List<Appointment> RecentAppointments { get; set; }
        public List<ServiceReminder> ServiceReminders { get; set; }= new List<ServiceReminder>();
        public int ReminderCount { get; set; }
        public List<ServiceRequest> RecentTasks { get; set; } = new();
        public List<TechnicianSchedulePlan> TodayJobs { get; set; } = new();
        // =========================
        // FINANCIAL
        // =========================
        public decimal TotalCashCollected { get; set; }

        // =========================
        // LOCATION (optional GPS)
        // =========================
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // =========================
        // SERVICE DATA
        // =========================
        public List<ServiceRequest> RecentServices { get; set; } = new();

        public List<ServiceRequest> UpcomingJobs { get; set; } = new();

        // =========================
        // APPOINTMENTS
        // =========================
        public List<AppointmentVM> Appointments { get; set; } = new();

        // =========================
        // SERVICE REMINDERS
        // =========================
        //public List<ServiceReminderVM> ServiceReminders { get; set; } = new();

        public List<ServiceReminderVM> NextServiceAlerts { get; set; } = new();

        // =========================
        // COMPLAINTS
        // =========================
        public List<ComplaintVM> NewComplaints { get; set; } = new();

        public List<ComplaintVM> InProgressComplaints { get; set; } = new();

        public List<ComplaintVM> ResolvedComplaints { get; set; } = new();

        // =========================
        // FEEDBACK
        // =========================
        public List<FeedbackVM> Feedbacks { get; set; } = new();

        public decimal AverageRating { get; set; }

        public int TotalFeedbackCount { get; set; }

        // =========================
        // DAILY SUMMARY
        // =========================
        public int TodayTaskCount { get; set; }
        public int TodayCompletedCount { get; set; }
        public int TodayPendingCount { get; set; }

        // =========================
        // PERFORMANCE
        // =========================
        //public decimal CompletionRate { get; set; }

        public decimal EfficiencyRate { get; set; }

        // =========================
        // NOTIFICATIONS
        // =========================
        public List<NotificationVM> Notifications { get; set; } = new();
    }

    // =========================
    // SUPPORT VMs
    // =========================

    public class AppointmentVM
    {
        public int AppointmentId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ServiceReminderVM
    {
        public int ServiceId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime NextServiceDate { get; set; }
        public string Priority { get; set; } = "Normal"; // Low / Normal / High
    }

    public class ComplaintVM
    {
        public int ComplaintId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // New / InProgress / Resolved
        public DateTime CreatedAt { get; set; }
    }

    public class FeedbackVM
    {
        public int FeedbackId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class NotificationVM
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}