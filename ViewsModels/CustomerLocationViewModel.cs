using AirConServicingManagementSystem.Models;

namespace AirConServicingManagementSystem.ViewsModels
{
    public class CustomerLocationViewModel
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public Customer Customer { get; set; } = new();
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? MapAddress { get; set; }
    }
}
