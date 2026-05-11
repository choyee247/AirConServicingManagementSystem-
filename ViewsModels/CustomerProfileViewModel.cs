namespace AirConServicingManagementSystem.ViewsModels
{
    public class CustomerProfileViewModel
    {
        // Customer fields
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        // Map / Location fields
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? MapAddress { get; set; }
    }
}
