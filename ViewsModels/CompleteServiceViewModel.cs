using System.ComponentModel.DataAnnotations;

namespace AirConServicingManagementSystem.ViewModels
{
    public class CompleteServiceViewModel
    {
        public int ServiceId { get; set; }
        public List<string> ServiceTypeList { get; set; } = new List<string>();
        //public string? SummaryOption { get; set; }

        public string? ProblemFound { get; set; }

        public string? RepairAction { get; set; }

        public string? PartsReplaced { get; set; }

        public string? TechnicianNote { get; set; }

        public decimal? ServiceCost { get; set; }
        public string? ACCondition { get; set; }  
        public string? AdditionalRemarks { get; set; }

        public DateTime? NextServiceDue { get; set; }
    }
}