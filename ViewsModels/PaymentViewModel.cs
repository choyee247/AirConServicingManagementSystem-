namespace AirConServicingManagementSystem.ViewsModels
{
    public class PaymentViewModel
    {

        public int ServiceRecordId { get; set; }


        public string? InvoiceNo { get; set; }



        public decimal Amount { get; set; }



        public decimal PaidAmount { get; set; }



        public decimal ChangeAmount
        {
            get
            {
                return PaidAmount - Amount;
            }
        }



        public string PaymentMethod { get; set; }
            = "Cash";



    }
}
