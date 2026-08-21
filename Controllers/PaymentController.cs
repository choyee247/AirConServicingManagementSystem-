using AirConServicingManagementSystem.Models;
using AirConServicingManagementSystem.ViewsModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Helpers;
using System.Net.NetworkInformation;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

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
            var records = await _context.ServiceRecords
                .Where(r =>
                    r.ServiceRequestId == serviceId &&
                    r.IsDeleted != true)
                .Include(r => r.ServiceCharges)
                .Include(r => r.ServiceExpenses)
                .Include(r => r.ServiceParts)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            if (!records.Any())
            {
                TempData["Error"] = "No service record found.";
                return RedirectToAction("Index", "ServiceRequest");
            }

            var currentRecord = records.Last();

            decimal currentChargeAmount =
                currentRecord.ServiceCharges?
                    .Sum(x => x.Amount ?? 0m) ?? 0m;

            decimal currentExpenseAmount =
                currentRecord.ServiceExpenses?
                    .Sum(x => x.Amount ?? 0m) ?? 0m;

            decimal currentPartsAmount =
                currentRecord.ServiceParts?
                    .Sum(x => x.UnitPrice ?? 0m) ?? 0m;


            decimal currentTotalAmount =
                currentChargeAmount +
                currentExpenseAmount +
                currentPartsAmount;

            decimal previousRemainingAmount = 0m;

            var previousRecords = records
                .Where(r => r.Id != currentRecord.Id)
                .ToList();


            foreach (var record in previousRecords)
            {
                decimal chargeAmount =
                    record.ServiceCharges?
                        .Sum(x => x.Amount ?? 0m) ?? 0m;

                decimal expenseAmount =
                    record.ServiceExpenses?
                        .Sum(x => x.Amount ?? 0m) ?? 0m;

                decimal partsAmount =
                    record.ServiceParts?
                        .Sum(x => x.UnitPrice ?? 0m) ?? 0m;


                decimal serviceTotal =
                    chargeAmount +
                    expenseAmount +
                    partsAmount;

                decimal paidAmount = await _context.Payments
                    .Where(p =>
                        p.ServiceRecordId == record.Id &&
                        p.IsDeleted == false)
                    .SumAsync(p => p.PaidAmount ?? 0m);


                decimal remaining =
                    Math.Max(serviceTotal - paidAmount, 0m);


                previousRemainingAmount += remaining;
            }

            decimal totalAmount =
                currentTotalAmount +
                previousRemainingAmount;

            var model = new PaymentViewModel
            {
                ServiceRecordId = currentRecord.Id,

                Amount = totalAmount,

                InvoiceNo = $"INV-{DateTime.Now:yyyyMMddHHmmss}"
            };

            ViewBag.CurrentServiceAmount = currentTotalAmount;
            ViewBag.PreviousRemainingAmount = previousRemainingAmount;
            ViewBag.TotalAmount = totalAmount;

            ViewBag.CurrentChargeAmount = currentChargeAmount;
            ViewBag.CurrentExpenseAmount = currentExpenseAmount;
            ViewBag.CurrentPartsAmount = currentPartsAmount;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
      PaymentViewModel model,
      string? fileName)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {

                var currentServiceRecord =
                    await _context.ServiceRecords
                        .Include(x => x.ServiceRequest)
                        .FirstOrDefaultAsync(x =>
                            x.Id == model.ServiceRecordId &&
                            x.IsDeleted != true);

                if (currentServiceRecord == null)
                {
                    return NotFound();
                }


                var serviceRequest =
                    currentServiceRecord.ServiceRequest;

                if (serviceRequest == null)
                {
                    return NotFound();
                }

                var serviceRecords =
                    await _context.ServiceRecords
                        .Include(x => x.Payments)
                        .Where(x =>
                            x.ServiceRequestId ==
                                serviceRequest.ServiceId &&
                            x.IsDeleted != true)
                        .OrderBy(x => x.CreatedAt)
                        .ToListAsync();


                if (!serviceRecords.Any())
                {
                    TempData["Error"] =
                        "No service records found.";

                    return RedirectToAction(
                        "Index",
                        "ServiceRequest"
                    );
                }

                decimal totalRemainingAmount = 0m;


                foreach (var record in serviceRecords)
                {
                    decimal serviceCost =
                        record.ServiceCost ?? 0m;


                    // Sum actual paid money
                    decimal totalPaidForRecord =
                        record.Payments?
                            .Where(p => p.IsDeleted == false)
                            .Sum(p => p.PaidAmount ?? 0m)
                        ?? 0m;


                    // Don't include change as actual payment
                    decimal remaining =
                        Math.Max(
                            serviceCost - totalPaidForRecord,
                            0m
                        );


                    totalRemainingAmount += remaining;
                }

                if (totalRemainingAmount <= 0)
                {
                    TempData["Error"] =
                        "All service fees have already been paid.";

                    return RedirectToAction(
                        "Index",
                        "ServiceRequest"
                    );
                }

                decimal paidAmount =
                    model.PaidAmount;

                if (paidAmount < totalRemainingAmount)
                {
                    ModelState.AddModelError(
                        "PaidAmount",
                        $"Paid amount must be at least {totalRemainingAmount:N0} MMK."
                    );

                    model.Amount =
                        totalRemainingAmount;

                    return View(model);
                }

                decimal changeAmount =
                    Math.Max(
                        paidAmount - totalRemainingAmount,
                        0m
                    );

                var payment = new Payment
                {
                    ServiceRecordId =
                        currentServiceRecord.Id,

                    InvoiceNo =
                        model.InvoiceNo,

                    PaymentDate =
                        DateTime.Now,

                    PaymentMethod =
                        model.PaymentMethod,

                    Amount =
                        totalRemainingAmount,


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

                foreach (var record in serviceRecords)
                {
                    decimal serviceCost =
                        record.ServiceCost ?? 0m;


                    decimal previousPaid =
                        record.Payments?
                            .Where(p => p.IsDeleted == false)
                            .Sum(p => p.PaidAmount ?? 0m)
                        ?? 0m;


                    decimal remaining =
                        Math.Max(
                            serviceCost - previousPaid,
                            0m
                        );


                    if (remaining <= 0)
                    {
                        record.Status = "Paid";

                        record.UpdatedAt =
                            DateTime.Now;
                    }
                }

                serviceRequest.PaymentStatus =
                    "Paid";

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return RedirectToAction(
                    "Invoice",
                    new
                    {
                        id = payment.PaymentId
                    }
                );
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

                .Include(x => x.ServiceRecord)
                    .ThenInclude(x => x.ServiceCharges)

                .Include(x => x.ServiceRecord)
                    .ThenInclude(x => x.ServiceExpenses)

                .Include(x => x.ServiceRecord)
                    .ThenInclude(x => x.ServiceParts)

                .FirstOrDefaultAsync(
                    x => x.PaymentId == id &&
                         x.IsDeleted == false
                );

            if (payment == null)
            {
                return NotFound();
            }

            var record = payment.ServiceRecord;

            // ==========================================
            // PAYMENT CALCULATION
            // ==========================================

            decimal chargeAmount =
                record?.ServiceCharges?.Sum(x => x.Amount ?? 0m) ?? 0m;

            decimal expenseAmount =
                record?.ServiceExpenses?.Sum(x => x.Amount ?? 0m) ?? 0m;

            decimal partsAmount =
                record?.ServiceParts?.Sum(x => x.UnitPrice ?? 0m) ?? 0m;

            // Total Service Cost
            // Charges + Expenses + Parts
            decimal totalServiceCost =
                chargeAmount +
                expenseAmount +
                partsAmount;

            decimal paidAmount =
                payment.PaidAmount ?? 0m;

            decimal remainingAmount =
                Math.Max(totalServiceCost - paidAmount, 0m);

            decimal changeAmount =
                payment.ChangeAmount ?? 0m;


            // ==========================================
            // VIEW BAG
            // ==========================================
            ViewBag.ChargeAmount = chargeAmount;
            ViewBag.ExpenseAmount = expenseAmount;
            ViewBag.PartsAmount = partsAmount;
            ViewBag.TotalServiceCost = totalServiceCost;
            ViewBag.PaidAmount = paidAmount;
            ViewBag.RemainingAmount = remainingAmount;
            ViewBag.ChangeAmount = changeAmount;

            return View(payment);
        }
        public async Task<IActionResult> DownloadInvoicePdf(int id)
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

                .Include(x => x.ServiceRecord)
                    .ThenInclude(x => x.ServiceCharges)

                .Include(x => x.ServiceRecord)
                    .ThenInclude(x => x.ServiceExpenses)

                .Include(x => x.ServiceRecord)
                    .ThenInclude(x => x.ServiceParts)

                .FirstOrDefaultAsync(
                    x => x.PaymentId == id &&
                         x.IsDeleted == false
                );

            if (payment == null)
            {
                return NotFound();
            }

            var record = payment.ServiceRecord;


            // =========================================================
            // CALCULATE PAYMENT
            // =========================================================

            decimal chargeAmount =
                record?.ServiceCharges?
                    .Sum(x => x.Amount ?? 0m) ?? 0m;

            decimal expenseAmount =
                record?.ServiceExpenses?
                    .Sum(x => x.Amount ?? 0m) ?? 0m;

            decimal partsAmount =
                record?.ServiceParts?
                    .Sum(x => x.UnitPrice ?? 0m) ?? 0m;


            // Total Service Cost
            // = Charges + Expenses + Parts

            decimal totalServiceCost =
                chargeAmount +
                expenseAmount +
                partsAmount;


            decimal paidAmount =
                payment.PaidAmount ?? 0m;


            decimal remainingAmount =
                Math.Max(
                    totalServiceCost - paidAmount,
                    0m
                );


            decimal changeAmount =
                payment.ChangeAmount ?? 0m;


            // =========================================================
            // PDF DOCUMENT
            // =========================================================

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // =================================================
                    // PAGE
                    // =================================================

                    page.Size(PageSizes.A4);

                    page.MarginTop(25);
                    page.MarginBottom(25);
                    page.MarginLeft(30);
                    page.MarginRight(30);

                    page.DefaultTextStyle(
                        x => x.FontSize(9)
                    );


                    // =================================================
                    // HEADER
                    // =================================================

                    page.Header()
                        .Background("#2c8c99")
                        .Padding(20)
                        .Row(row =>
                        {
                            // COMPANY
                            row.RelativeItem()
                                .Column(column =>
                                {
                                    column.Item()
                                        .Text("❄  AirCon Servicing")
                                        .FontSize(23)
                                        .Bold()
                                        .FontColor(Colors.White);

                                    column.Item()
                                        .PaddingTop(4)
                                        .Text(
                                            "Professional Air Conditioner Service"
                                        )
                                        .FontSize(10)
                                        .FontColor(Colors.White);
                                });


                            // INVOICE
                            row.ConstantItem(180)
                                .AlignRight()
                                .Column(column =>
                                {
                                    column.Item()
                                        .AlignRight()
                                        .Text("INVOICE")
                                        .FontSize(24)
                                        .Bold()
                                        .FontColor(Colors.White);

                                    column.Item()
                                        .PaddingTop(5)
                                        .AlignRight()
                                        .Text(
                                            $"Invoice No : {payment.InvoiceNo ?? "-"}"
                                        )
                                        .FontSize(9)
                                        .FontColor(Colors.White);
                                });
                        });


                    // =================================================
                    // CONTENT
                    // =================================================

                    page.Content()
                        .PaddingTop(18)
                        .Column(column =>
                        {

                            // =================================================
                            // INVOICE META
                            // =================================================

                            column.Item()
                                .BorderBottom(1)
                                .BorderColor("#e3e8eb")
                                .PaddingBottom(12)
                                .Row(row =>
                                {
                                    // INVOICE DATE
                                    row.RelativeItem()
                                        .Column(meta =>
                                        {
                                            meta.Item()
                                                .Text("INVOICE DATE")
                                                .FontSize(7)
                                                .Bold()
                                                .FontColor("#6c757d");

                                            meta.Item()
                                                .PaddingTop(3)
                                                .Text(
                                                    payment.PaymentDate?
                                                        .ToString("dd MMMM yyyy")
                                                    ?? "-"
                                                )
                                                .FontSize(9)
                                                .Bold();
                                        });


                                    // SERVICE DATE
                                    row.RelativeItem()
                                        .Column(meta =>
                                        {
                                            meta.Item()
                                                .Text("SERVICE DATE")
                                                .FontSize(7)
                                                .Bold()
                                                .FontColor("#6c757d");

                                            meta.Item()
                                                .PaddingTop(3)
                                                .Text(
                                                    record?.CreatedAt?
                                                        .ToString("dd MMMM yyyy")
                                                    ?? "-"
                                                )
                                                .FontSize(9)
                                                .Bold();
                                        });


                                    // PAYMENT STATUS
                                    row.RelativeItem()
                                        .AlignRight()
                                        .Column(meta =>
                                        {
                                            meta.Item()
                                                .AlignRight()
                                                .Text("PAYMENT STATUS")
                                                .FontSize(7)
                                                .Bold()
                                                .FontColor("#6c757d");

                                            meta.Item()
                                                .PaddingTop(3)
                                                .AlignRight()
                                                .Text(
                                                    payment.PaymentStatus ?? "-"
                                                )
                                                .FontSize(9)
                                                .Bold()
                                                .FontColor("#2c8c99");
                                        });
                                });


                            // =================================================
                            // CUSTOMER + TECHNICIAN
                            // =================================================

                            column.Item()
                                .PaddingTop(15)
                                .Row(row =>
                                {

                                    // CUSTOMER
                                    row.RelativeItem()
                                        .Border(1)
                                        .BorderColor("#e3e8eb")
                                        .Padding(12)
                                        .Column(info =>
                                        {
                                            info.Item()
                                                .Text("CUSTOMER INFORMATION")
                                                .FontSize(10)
                                                .Bold()
                                                .FontColor("#2c8c99");

                                            info.Item()
                                                .PaddingTop(8)
                                                .Text(
                                                    $"Name: {record?.Customer?.Name ?? "-"}"
                                                );

                                            info.Item()
                                                .PaddingTop(4)
                                                .Text(
                                                    $"Phone: {record?.Customer?.Phone ?? "-"}"
                                                );

                                            info.Item()
                                                .PaddingTop(4)
                                                .Text(
                                                    $"Address: {record?.Customer?.Address ?? "-"}"
                                                );
                                        });


                                    // GAP
                                    row.ConstantItem(12);


                                    // TECHNICIAN
                                    row.RelativeItem()
                                        .Border(1)
                                        .BorderColor("#e3e8eb")
                                        .Padding(12)
                                        .Column(info =>
                                        {
                                            info.Item()
                                                .Text("TECHNICIAN INFORMATION")
                                                .FontSize(10)
                                                .Bold()
                                                .FontColor("#2c8c99");

                                            info.Item()
                                                .PaddingTop(8)
                                                .Text(
                                                    $"Name: {record?.Technician?.Name ?? "-"}"
                                                );

                                            info.Item()
                                                .PaddingTop(4)
                                                .Text(
                                                    $"Service ID: #{record?.Id}"
                                                );

                                            info.Item()
                                                .PaddingTop(4)
                                                .Text(
                                                    $"Service Date: {record?.CreatedAt?.ToString("dd-MM-yyyy") ?? "-"}"
                                                );
                                        });
                                });


                            // =================================================
                            // AIR CONDITIONER DETAILS
                            // =================================================

                            column.Item()
                                .PaddingTop(18)
                                .Text("Air Conditioner Details")
                                .FontSize(13)
                                .Bold()
                                .FontColor("#2c8c99");


                            column.Item()
                                .PaddingTop(7)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(25);
                                        columns.RelativeColumn(1.6f);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.3f);
                                    });


                                    // HEADER
                                    table.Header(header =>
                                    {
                                        void HeaderCell(
                                            IContainer container,
                                            string text)
                                        {
                                            container
                                                .Background("#2c8c99")
                                                .Padding(6)
                                                .Text(text)
                                                .FontSize(8)
                                                .Bold()
                                                .FontColor(Colors.White);
                                        }

                                        HeaderCell(
                                            header.Cell(),
                                            "#"
                                        );

                                        HeaderCell(
                                            header.Cell(),
                                            "Brand"
                                        );

                                        HeaderCell(
                                            header.Cell(),
                                            "Model"
                                        );

                                        HeaderCell(
                                            header.Cell(),
                                            "Capacity"
                                        );

                                        HeaderCell(
                                            header.Cell(),
                                            "Condition"
                                        );

                                        HeaderCell(
                                            header.Cell(),
                                            "Next Service"
                                        );
                                    });


                                    var unitNo = 1;


                                    foreach (
                                        var unit in record?.ServiceRecordUnits
                                        ?? Enumerable.Empty<ServiceRecordUnit>())
                                    {
                                        table.Cell()
                                            .BorderBottom(1)
                                            .BorderColor("#e3e8eb")
                                            .Padding(6)
                                            .Text(
                                                unitNo.ToString()
                                            );


                                        table.Cell()
                                            .BorderBottom(1)
                                            .BorderColor("#e3e8eb")
                                            .Padding(6)
                                            .Text(
                                                unit.AirConUnit?
                                                    .Brand?
                                                    .BrandName ?? "-"
                                            );


                                        table.Cell()
                                            .BorderBottom(1)
                                            .BorderColor("#e3e8eb")
                                            .Padding(6)
                                            .Text(
                                                unit.AirConUnit?
                                                    .Model?
                                                    .ModelName ?? "-"
                                            );


                                        table.Cell()
                                            .BorderBottom(1)
                                            .BorderColor("#e3e8eb")
                                            .Padding(6)
                                            .Text(
                                                $"{unit.AirConUnit?.CapacityHp} HP"
                                            );


                                        table.Cell()
                                            .BorderBottom(1)
                                            .BorderColor("#e3e8eb")
                                            .Padding(6)
                                            .Text(
                                                unit.Accondition ?? "-"
                                            );


                                        table.Cell()
                                            .BorderBottom(1)
                                            .BorderColor("#e3e8eb")
                                            .Padding(6)
                                            .Text(
                                                unit.NextServiceDue?
                                                    .ToString("dd-MM-yyyy")
                                                ?? "-"
                                            );


                                        unitNo++;
                                    }
                                });


                            // =================================================
                            // PAYMENT SUMMARY
                            // =================================================

                            column.Item()
                                .PaddingTop(20)
                                .Text("Payment Summary")
                                .FontSize(13)
                                .Bold()
                                .FontColor("#2c8c99");


                            column.Item()
                                .PaddingTop(7)
                                .Border(1)
                                .BorderColor("#e3e8eb")
                                .Padding(15)
                                .Column(paymentColumn =>
                                {

                                    // CHARGES
                                    paymentColumn.Item()
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text("Additional Charges");

                                            row.ConstantItem(130)
                                                .AlignRight()
                                                .Text(
                                                    $"{chargeAmount:N0} MMK"
                                                );
                                        });


                                    // EXPENSE
                                    paymentColumn.Item()
                                        .PaddingTop(7)
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text("Service Expenses");

                                            row.ConstantItem(130)
                                                .AlignRight()
                                                .Text(
                                                    $"{expenseAmount:N0} MMK"
                                                );
                                        });


                                    // PARTS
                                    paymentColumn.Item()
                                        .PaddingTop(7)
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text("Parts Cost");

                                            row.ConstantItem(130)
                                                .AlignRight()
                                                .Text(
                                                    $"{partsAmount:N0} MMK"
                                                );
                                        });


                                    // DIVIDER
                                    paymentColumn.Item()
                                        .PaddingVertical(9)
                                        .LineHorizontal(1)
                                        .LineColor("#d9dfe2");


                                    // TOTAL
                                    paymentColumn.Item()
                                        .Background("#e9f7f8")
                                        .Padding(10)
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text("TOTAL SERVICE COST")
                                                .Bold()
                                                .FontSize(11)
                                                .FontColor("#2c8c99");

                                            row.ConstantItem(150)
                                                .AlignRight()
                                                .Text(
                                                    $"{totalServiceCost:N0} MMK"
                                                )
                                                .Bold()
                                                .FontSize(11)
                                                .FontColor("#2c8c99");
                                        });


                                    // PAID
                                    paymentColumn.Item()
                                        .PaddingTop(10)
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text("Paid Amount");

                                            row.ConstantItem(130)
                                                .AlignRight()
                                                .Text(
                                                    $"{paidAmount:N0} MMK"
                                                )
                                                .Bold();
                                        });


                                    // REMAINING
                                    paymentColumn.Item()
                                        .PaddingTop(6)
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text("Remaining Amount");

                                            row.ConstantItem(130)
                                                .AlignRight()
                                                .Text(
                                                    $"{remainingAmount:N0} MMK"
                                                );
                                        });


                                    // CHANGE
                                    paymentColumn.Item()
                                        .PaddingTop(6)
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text("Change");

                                            row.ConstantItem(130)
                                                .AlignRight()
                                                .Text(
                                                    $"{changeAmount:N0} MMK"
                                                );
                                        });


                                    // PAYMENT METHOD
                                    paymentColumn.Item()
                                        .PaddingTop(12)
                                        .Background("#e9f7f8")
                                        .Padding(8)
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text("Payment Method")
                                                .FontSize(9);

                                            row.ConstantItem(130)
                                                .AlignRight()
                                                .Text(
                                                    payment.PaymentMethod ?? "-"
                                                )
                                                .Bold()
                                                .FontColor("#2c8c99");
                                        });


                                    // PAYMENT STATUS
                                    paymentColumn.Item()
                                        .PaddingTop(6)
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text("Payment Status");

                                            row.ConstantItem(130)
                                                .AlignRight()
                                                .Text(
                                                    payment.PaymentStatus ?? "-"
                                                )
                                                .Bold()
                                                .FontColor("#2c8c99");
                                        });


                                    // PAYMENT DATE
                                    paymentColumn.Item()
                                        .PaddingTop(6)
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text("Payment Date");

                                            row.ConstantItem(160)
                                                .AlignRight()
                                                .Text(
                                                    payment.PaymentDate?
                                                        .ToString(
                                                            "dd MMM yyyy HH:mm"
                                                        )
                                                    ?? "-"
                                                );
                                        });
                                });


                            // =================================================
                            // QR / VERIFICATION
                            // =================================================

                            column.Item()
                                .PaddingTop(18)
                                .AlignCenter()
                                .Column(qr =>
                                {
                                    qr.Item()
                                        .Text("Scan to Verify Receipt")
                                        .FontSize(10)
                                        .Bold()
                                        .FontColor("#2c8c99");

                                    qr.Item()
                                        .PaddingTop(3)
                                        .Text(
                                            "Scan the QR code to verify this payment receipt."
                                        )
                                        .FontSize(8)
                                        .FontColor("#6c757d");

                                    // If payment-qr.png exists in wwwroot/images,
                                    // load it here.
                                    var qrPath = Path.Combine(
                                        Directory.GetCurrentDirectory(),
                                        "wwwroot",
                                        "images",
                                        "payment-qr.png"
                                    );

                                    if (System.IO.File.Exists(qrPath))
                                    {
                                        qr.Item()
                                            .PaddingTop(7)
                                            .Width(90)
                                            .Height(90)
                                            .Image(
                                                qrPath
                                            );
                                    }
                                });
                        });


                    // =================================================
                    // FOOTER
                    // =================================================

                    page.Footer()
                        .PaddingTop(12)
                        .AlignCenter()
                        .Column(footer =>
                        {
                            footer.Item()
                                .Text(
                                    "Thank You for Choosing Us!"
                                )
                                .FontSize(11)
                                .Bold()
                                .FontColor("#2c8c99");

                            footer.Item()
                                .PaddingTop(3)
                                .Text(
                                    "Thank you for choosing AirCon Servicing. "
                                    + "We appreciate your trust in our professional service."
                                )
                                .FontSize(8)
                                .FontColor("#6c757d");

                            footer.Item()
                                .PaddingTop(4)
                                .Text(
                                    "❄ AirCon Servicing Management System"
                                    + "  •  Professional AC Service"
                                )
                                .FontSize(7)
                                .FontColor("#7b858b");
                        });
                });
            });


            // =========================================================
            // GENERATE PDF
            // =========================================================

            var pdfBytes = document.GeneratePdf();


            var fileName =
                $"{payment.InvoiceNo ?? "Invoice"}.pdf";


            return File(
                pdfBytes,
                "application/pdf",
                fileName
            );
        }
    }
}
