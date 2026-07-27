namespace AirConServicingManagementSystem.ViewModels
{
    public class ServicePartVM
    {

        public int AirConUnitId { get; set; }


        public string? PartName { get; set; }


        public int Quantity { get; set; }


        public decimal UnitPrice { get; set; }


        public decimal Total
        {
            get
            {
                return Quantity * UnitPrice;
            }
        }

    }
}