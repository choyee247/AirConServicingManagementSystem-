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

        public int? VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public DateTime? UpdatedAt { get; set; }


        public string PaymentMethod { get; set; }
            = "Cash";

        // Digital Payment

        public string? TransactionNo { get; set; }


        // Bank Transfer

        public string? BankName { get; set; }

        public string? AccountName { get; set; }

        public string? AccountNo { get; set; }


        // Upload Slip

        public IFormFile? PaymentSlip { get; set; }


        public string? Remark { get; set; }

    }
}
