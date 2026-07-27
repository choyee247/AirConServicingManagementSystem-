namespace AirConServicingManagementSystem.ViewModels
{
        public class EditAirConUnitViewModel
        {

            public int Id { get; set; }


            public int CustomerId { get; set; }


            public int BrandId { get; set; }


            public int ModelId { get; set; }



            public string? BrandName { get; set; }


            public string? ModelName { get; set; }



            public string? SerialNumber { get; set; }



            public string? CapacityHp { get; set; }



            public string? AirConType { get; set; }



            public string? InstallationType { get; set; }



            public DateTime? InstallationDate { get; set; }


            public int Quantity { get; set; }

            public DateTime? CreatedAt { get; set; }

            public DateTime? ContractStartDate { get; set; }


            public DateTime? ContractEndDate { get; set; }


            public int ServiceId { get; set; }
            public bool? IsDeleted { get; set; }

            public string? CustomerName { get; set; }


            public string? CustomerPhone { get; set; }

        }
    }

