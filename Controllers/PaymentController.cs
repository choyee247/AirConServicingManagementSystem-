using AirConServicingManagementSystem.Models;
using AirConServicingManagementSystem.ViewsModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Controllers
{
    public class PaymentController : Controller
    {


        private readonly DBContext _context;



        public PaymentController(
        DBContext context)
        {

            _context = context;

        }




        //public async Task<IActionResult> Create(
        //int serviceRecordId)
        //{


        //    var record =
        //    await _context.ServiceRecords
        //    .FirstOrDefaultAsync(
        //    x => x.Id == serviceRecordId);



        //    if (record == null)
        //        return NotFound();



        //    var model = new PaymentViewModel
        //    {


        //        ServiceRecordId =
        //    serviceRecordId,


        //        Amount =
        //    record.ServiceCost ?? 0,


        //        InvoiceNo =
        //    "INV-"
        //    +
        //    DateTime.Now.ToString("yyyyMMddHHmmss")

        //    };



        //    return View(model);


        //}

        public async Task<IActionResult> Create(int serviceId)
        {
            var record = await _context.ServiceRecords
                .FirstOrDefaultAsync(r => r.ServiceRequestId == serviceId);

            if (record == null)
                return NotFound();

            var model = new PaymentViewModel
            {
                ServiceRecordId = record.Id,
                Amount = record.ServiceCost ?? 0,
                InvoiceNo = $"INV-{DateTime.Now:yyyyMMddHHmmss}"
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
      PaymentViewModel model, string fileName)
        {

            Console.WriteLine("STEP 1");


            if (ModelState.IsValid)
            {
                Console.WriteLine("MODEL INVALID");

                foreach (var item in ModelState)
                {
                    foreach (var error in item.Value.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }

                return View(model);
            }


            Console.WriteLine("STEP 2");


            using var transaction =
                await _context.Database.BeginTransactionAsync();


            Console.WriteLine("STEP 3");


            var serviceRecord =
                await _context.ServiceRecords
                .Include(x => x.ServiceRequest)
                .FirstOrDefaultAsync(
                    x => x.Id == model.ServiceRecordId);


            Console.WriteLine("STEP 4");


            if (serviceRecord == null)
            {
                Console.WriteLine("SERVICE RECORD NOT FOUND");
                return NotFound();
            }


            Console.WriteLine("STEP 5");


            var payment = new Payment
            {
                ServiceRecordId = model.ServiceRecordId,

                InvoiceNo = model.InvoiceNo,

                PaymentDate = DateTime.Now,

                PaymentMethod = model.PaymentMethod,

                Amount = model.Amount,

                PaidAmount = model.PaidAmount,

                PaymentStatus = "Paid",

                CreatedAt = DateTime.Now,
                TransactionNo = model.TransactionNo,
                Remark=model.Remark,
                BankName =model.BankName,
                AccountName=model.AccountName,
                AccountNo=model.AccountNo,
               
                PaymentSlip =
                            "/paymentslip/service/"
                            + fileName,

                VerifiedBy =model.VerifiedBy,
                VerifiedDate=DateTime.Now,
                UpdatedAt=DateTime.Now,

                IsDeleted = false
            };


            Console.WriteLine("STEP 6");


            _context.Payments.Add(payment);


            Console.WriteLine("STEP 7");


            var save =
                await _context.SaveChangesAsync();


            Console.WriteLine(
                "SAVE RESULT : " + save
            );


            Console.WriteLine(
                "PAYMENT ID : " + payment.PaymentId
            );


            await transaction.CommitAsync();


            return RedirectToAction(
                "Invoice",
                new
                {
                    id = payment.PaymentId
                });

        }
        public async Task<IActionResult> Invoice(int id)
        {

            var payment = await _context.Payments

                .Include(x => x.ServiceRecord)
                    .ThenInclude(x => x.Customer)


                .Include(x => x.ServiceRecord)
                    .ThenInclude(x => x.Technician)


                .Include(x => x.ServiceRecord)
                    .ThenInclude(x => x.ServiceRecordUnits)
                        .ThenInclude(x => x.AirConUnit)
                            .ThenInclude(x => x.Brand)


                .Include(x => x.ServiceRecord)
                    .ThenInclude(x => x.ServiceRecordUnits)
                        .ThenInclude(x => x.AirConUnit)
                            .ThenInclude(x => x.Model)


                .FirstOrDefaultAsync(
                    x => x.PaymentId == id
                    && x.IsDeleted == false
                );



            if (payment == null)
            {
                return NotFound();
            }



            return View(payment);

        }

    }
}
