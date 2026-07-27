namespace AirConServicingManagementSystem.ViewModels
{
    public class ServiceUnitVM
    {

        // Group Key
        public List<int> AirConUnitIds { get; set; }
            = new List<int>();


        // AC Information

        public string? BrandName { get; set; }

        public string? ModelName { get; set; }



        // Quantity

        //public int Quantity { get; set; }



        // Installation / Existing

        public string? InstallationType { get; set; }



        // Warranty / Contract

        public int? ContractMonths { get; set; }


        public int? MaintenanceMonths { get; set; }



        public bool HasWarranty { get; set; }


        public DateTime? WarrantyStartDate { get; set; }


        public DateTime? WarrantyEndDate { get; set; }



        public bool IsFreeService { get; set; }



        // Selected Group

        public bool IsSelected { get; set; }



        // Service

        public string? Condition { get; set; }


        public string? ProblemFound { get; set; }


        public string? RepairAction { get; set; }


        public DateTime? NextServiceDue { get; set; }

        public int TotalQuantity { get; set; }


        public int CompletedQuantity { get; set; }


        public int RemainingQuantity
        {
            get
            {
                return TotalQuantity - CompletedQuantity;
            }
        }

        public int ServiceQuantity { get; set; }

        public decimal ServiceFee { get; set; }
        public string? SerialNumber { get; set; }
        public string? CapacityHp { get; set; }
        public DateTime? InstallationDate { get; set; }
    }
}