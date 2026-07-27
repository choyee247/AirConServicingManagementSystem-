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




        public async Task<IActionResult> Create(
        int serviceRecordId)
        {


            var record =
            await _context.ServiceRecords
            .FirstOrDefaultAsync(
            x => x.Id == serviceRecordId);



            if (record == null)
                return NotFound();



            var model = new PaymentViewModel
            {


                ServiceRecordId =
            serviceRecordId,


                Amount =
            record.ServiceCost ?? 0,


                InvoiceNo =
            "INV-"
            +
            DateTime.Now.ToString("yyyyMMddHHmmss")

            };



            return View(model);


        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
        PaymentViewModel model)
        {


            var payment = new Payment
            {


                ServiceRecordId =
            model.ServiceRecordId,


                InvoiceNo =
            model.InvoiceNo,


                PaymentDate =
            DateTime.Now,


                PaymentMethod =
            model.PaymentMethod,


                Amount =
            model.Amount,


                PaidAmount =
            model.PaidAmount,


                ChangeAmount =
            model.ChangeAmount,


                PaymentStatus =
            "Paid",


                CreatedAt =
            DateTime.Now,


                IsDeleted = false


            };



            _context.Payments.Add(payment);



            await _context.SaveChangesAsync();



            return RedirectToAction(
            "Invoice",
            new
            {
                id = payment.PaymentId
            });


        }
        public async Task<IActionResult> Invoice(int id)
        {

            var payment =
             await _context.Payments
             .Include(x => x.ServiceRecord)
                 .ThenInclude(x => x.Customer)

             .Include(x => x.ServiceRecord)
                 .ThenInclude(x => x.ServiceRecordUnits)
                     .ThenInclude(x => x.AirConUnit)
                         .ThenInclude(x => x.Brand)

             .Include(x => x.ServiceRecord)
                 .ThenInclude(x => x.ServiceRecordUnits)
                     .ThenInclude(x => x.AirConUnit)
                         .ThenInclude(x => x.Model)

             .Include(x => x.ServiceRecord)
                 .ThenInclude(x => x.Technician)

             .FirstOrDefaultAsync(
                 x => x.PaymentId == id);


            if (payment == null)
                return NotFound();

            Console.WriteLine(
    "ServiceRecord Id = "
    + payment.ServiceRecord.Id
);


            Console.WriteLine(
                "Unit Count = "
                +
                payment.ServiceRecord.ServiceRecordUnits.Count
            );


            foreach (var unit in payment.ServiceRecord.ServiceRecordUnits)
            {
                Console.WriteLine(
                    "AC ID = "
                    + unit.AirConUnitId
                );
            }


            return View(payment);

        }

    }
}
