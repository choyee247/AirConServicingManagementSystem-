using System.ComponentModel.DataAnnotations;

namespace AirConServicingManagementSystem.ViewModels
{
    public class AddAirConUnitViewModel
    {
        public int ServiceId { get; set; }

        public int CustomerId { get; set; }

        public int BrandId { get; set; }

        public int ModelId { get; set; }

        public string? CapacityHp { get; set; }

        public string? AirConType { get; set; }

        public string? InstallationType { get; set; }

        public DateTime? InstallationDate { get; set; }

        public string? SerialNumber { get; set; }

        public int Quantity { get; set; }

        public List<CartAirConItem> Items { get; set; }
            = new();

    }



    public class CartAirConItem
    {

        public int BrandId { get; set; }

        public string BrandName { get; set; }

        public int ModelId { get; set; }

        public string ModelName { get; set; }
        public string CapacityHp { get; set; }
        public string? SerialNumber { get; set; }

        public int Quantity { get; set; }
        public string? AirConType { get;  set; }
        public string InstallationType { get; set; }


        public DateTime? InstallationDate { get; set; }


        public DateTime? ContractStartDate { get; set; }


        public DateTime? ContractEndDate { get; set; }
    }
}