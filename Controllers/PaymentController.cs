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

        public async Task<IActionResult> Create(int serviceId)
        {
            // Get all active service records
            var records = await _context.ServiceRecords
                .Where(r =>
                    r.ServiceRequestId == serviceId &&
                    r.IsDeleted != true)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            if (!records.Any())
            {
                TempData["Error"] = "No service record found.";
                return RedirectToAction("Index", "ServiceRequest");
            }


            // =====================================
            // CURRENT SERVICE RECORD
            // Latest Record
            // =====================================

            var currentRecord = records.Last();


            // =====================================
            // TOTAL AMOUNT
            // Include previous remaining services
            // =====================================

            decimal totalAmount = records.Sum(r =>
                r.ServiceCost ?? 0);


            // =====================================
            // CREATE PAYMENT VIEW MODEL
            // =====================================

            var model = new PaymentViewModel
            {
                ServiceRecordId = currentRecord.Id,

                Amount = totalAmount,

                InvoiceNo = $"INV-{DateTime.Now:yyyyMMddHHmmss}"
            };


            // ALWAYS GO TO PAYMENT CREATE PAGE
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PaymentViewModel model,
            string? fileName)
        {
            Console.WriteLine("STEP 1");


            // ==========================================
            // VALIDATE MODEL
            // ==========================================

            if (!ModelState.IsValid)
            {
                Console.WriteLine("MODEL INVALID");

                foreach (var item in ModelState)
                {
                    foreach (var error in item.Value.Errors)
                    {
                        Console.WriteLine(
                            $"{item.Key}: {error.ErrorMessage}"
                        );
                    }
                }

                return View(model);
            }


            // ==========================================
            // START TRANSACTION
            // ==========================================

            await using var transaction =
                await _context.Database.BeginTransactionAsync();


            try
            {
                Console.WriteLine("STEP 2");


                // ==========================================
                // GET CURRENT SERVICE RECORD
                // ==========================================

                var currentServiceRecord =
                    await _context.ServiceRecords
                        .Include(x => x.ServiceRequest)
                        .FirstOrDefaultAsync(
                            x =>
                                x.Id == model.ServiceRecordId &&
                                x.IsDeleted != true);


                if (currentServiceRecord == null)
                {
                    Console.WriteLine(
                        "SERVICE RECORD NOT FOUND");

                    return NotFound();
                }


                var serviceRequest =
                    currentServiceRecord.ServiceRequest;


                if (serviceRequest == null)
                {
                    return NotFound();
                }


                Console.WriteLine(
                    $"SERVICE REQUEST ID : " +
                    serviceRequest.ServiceId
                );


                // ==========================================
                // GET ALL UNPAID SERVICE RECORDS
                // SAME SERVICE REQUEST
                // ==========================================

                var unpaidRecords =
                    await _context.ServiceRecords
                        .Where(x =>
                            x.ServiceRequestId ==
                                serviceRequest.ServiceId &&
                            x.IsDeleted != true &&
                            x.Status != "Paid")
                        .ToListAsync();


                if (!unpaidRecords.Any())
                {
                    TempData["Error"] =
                        "All service fees have already been paid.";

                    return RedirectToAction(
                        "Index",
                        "ServiceRequest"
                    );
                }


                // ==========================================
                // CALCULATE ACTUAL UNPAID TOTAL
                // Don't trust the Amount from the browser
                // ==========================================

                decimal totalAmount =
                    unpaidRecords.Sum(x =>
                        x.ServiceCost ?? 0);


                Console.WriteLine(
                    $"TOTAL UNPAID AMOUNT : {totalAmount}"
                );


                // ==========================================
                // CALCULATE CHANGE
                // ==========================================

                decimal paidAmount =
                    model.PaidAmount;


                decimal changeAmount =
                    paidAmount - totalAmount;


                // ==========================================
                // PAYMENT AMOUNT VALIDATION
                // ==========================================

                if (paidAmount < totalAmount)
                {
                    ModelState.AddModelError(
                        "PaidAmount",
                        "Paid amount is less than the total service fee."
                    );

                    return View(model);
                }


                // ==========================================
                // CREATE ONE PAYMENT
                // ==========================================

                var payment = new Payment
                {
                    // Use current/latest record as the
                    // primary record for the invoice

                    ServiceRecordId =
                        currentServiceRecord.Id,

                    InvoiceNo =
                        model.InvoiceNo,

                    PaymentDate =
                        DateTime.Now,

                    PaymentMethod =
                        model.PaymentMethod,

                    // Actual total from all unpaid records
                    Amount =
                        totalAmount,

                    PaidAmount =
                        paidAmount,

                    ChangeAmount =
                        changeAmount,

                    PaymentStatus =
                        "Paid",

                    CreatedAt =
                        DateTime.Now,

                    TransactionNo =
                        model.TransactionNo,

                    Remark =
                        model.Remark,

                    BankName =
                        model.BankName,

                    AccountName =
                        model.AccountName,

                    AccountNo =
                        model.AccountNo,

                    PaymentSlip =
                        !string.IsNullOrEmpty(fileName)
                            ? "/paymentslip/service/" + fileName
                            : null,

                    VerifiedBy =
                        model.VerifiedBy,

                    VerifiedDate =
                        DateTime.Now,

                    UpdatedAt =
                        DateTime.Now,

                    IsDeleted =
                        false
                };


                _context.Payments.Add(payment);


                Console.WriteLine(
                    "PAYMENT CREATED"
                );


                // ==========================================
                // MARK ALL UNPAID SERVICE RECORDS AS PAID
                // ==========================================

                foreach (var record in unpaidRecords)
                {
                    record.Status =
                        "Paid";

                    record.UpdatedAt =
                        DateTime.Now;
                }


                // ==========================================
                // UPDATE SERVICE REQUEST PAYMENT STATUS
                // ==========================================

                serviceRequest.PaymentStatus =
                    "Paid";

                //serviceRequest.UpdatedAt =
                //    DateTime.Now;


                // IMPORTANT:
                // Service Status remains Completed.
                // Don't change ServiceRequest.Status here.


                // ==========================================
                // SAVE ALL
                // ==========================================

                var saveResult =
                    await _context.SaveChangesAsync();


                Console.WriteLine(
                    "SAVE RESULT : " +
                    saveResult
                );


                Console.WriteLine(
                    "PAYMENT ID : " +
                    payment.PaymentId
                );


                await transaction.CommitAsync();


                // ==========================================
                // GO TO INVOICE
                // ==========================================

                return RedirectToAction(
                    "Invoice",
                    new
                    {
                        id = payment.PaymentId
                    });
            }
            catch
            {
                await transaction.RollbackAsync();

                throw;
            }
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
