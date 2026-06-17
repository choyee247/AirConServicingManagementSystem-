using AirConServicingManagementSystem.Models;

namespace AirConServicingManagementSystem.ViewsModels
{
    public class ServiceRequestCreateVM
    {
        public ServiceRequest ServiceRequest { get; set; }
        public List<AirConUnit> AirCons { get; set; } = new();

        // ⭐ NEW FIELDS
        public bool HasAirCon => AirCons.Any();

        public int? FirstAirConId => AirCons.FirstOrDefault()?.Id;
    }

}
