using System.ComponentModel.DataAnnotations;

namespace AirConServicingManagementSystem.ViewModels
{
    public class CompleteServiceViewModel
    {
        public int ServiceId { get; set; }

        [Display(Name = "Service Summary")]
        public string? SummaryOption { get; set; }

        [Display(Name = "Additional Remarks")]
        public string? AdditionalRemarks { get; set; }
    }
}