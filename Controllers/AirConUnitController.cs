using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirConServicingManagementSystem.Models;
using AirConServicingManagementSystem.ViewModels;

namespace AirConServicingManagementSystem.Controllers
{
    public class AirConUnitController : Controller
    {
        private readonly DBContext _context;

        public AirConUnitController(DBContext context)
        {
            _context = context;
        }

        // GET: Index - List all AirConUnits
        public async Task<IActionResult> Index()
        {
            var technicianId = HttpContext.Session.GetInt32("TechnicianId");

            if (technicianId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var aircons = await _context.AirConUnits
                .AsNoTracking()
                .Include(a => a.Brand)
                .Include(a => a.Model)
                .Include(a => a.Customer)
                .Include(a => a.Warranty)
                .Include(a => a.Service)
                .Where(a =>
                    (a.IsDeleted == false || a.IsDeleted == null)
                    &&
                    a.Service != null
                    &&
                    a.Service.TechnicianId == technicianId
                )
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(aircons);
        }

        // GET: Details
        public async Task<IActionResult> Details(int id)
        {
            var aircon = await _context.AirConUnits
                .Include(a => a.Brand)
                .Include(a => a.Model)
                .Include(a => a.Customer)
                .Include(a => a.Warranty)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsDeleted != true);

            if (aircon == null) return NotFound();

            return View(aircon);
        }

        public async Task<IActionResult> Create(int serviceId)
        {
            ViewBag.ServiceId = serviceId;


            var service = await _context.ServiceRequests
                .Include(x => x.Appointment)
                .ThenInclude(x => x.Customer)
                .FirstOrDefaultAsync(x => x.ServiceId == serviceId);



            if (service == null)
                return NotFound();



            ViewBag.CustomerName =
                service.Appointment.Customer.Name;


            ViewBag.CustomerPhone =
                service.Appointment.Customer.Phone;



            ViewBag.ACCount =
                await _context.AirConUnits
                .CountAsync(x => x.CustomerId == service.CustomerId);



            ViewBag.Brands =
                await _context.AirConBrands
                .Where(x => x.IsDeleted != true)
                .ToListAsync();



            return View(new AddAirConUnitViewModel
            {
                ServiceId = serviceId,
                Items = new List<CartAirConItem>()
            });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
           AddAirConUnitViewModel model)
        {

            var service = await _context.ServiceRequests
                .FirstOrDefaultAsync(x =>
                x.ServiceId == model.ServiceId);



            if (service == null)
                return NotFound();



            foreach (var item in model.Items)
            {

                Console.WriteLine(
        $"BrandId={item.BrandId}, ModelId={item.ModelId}"
    );
                for (int i = 0; i < item.Quantity; i++)
                {


                    var aircon = new AirConUnit
                    {

                        CustomerId = service.CustomerId,


                        BrandId = item.BrandId,


                        ModelId = item.ModelId,

                        SerialNumber = item.SerialNumber,

                        CapacityHp = item.CapacityHp,


                        AirConType = item.AirConType,


                        InstallationType =
                            item.InstallationType,


                        InstallationDate =
                            item.InstallationDate,

                        ServiceId = service.ServiceId,

                        CreatedAt = DateTime.Now,


                        IsDeleted = false

                    };

                    var modelExists =
    await _context.AirConModels
    .AnyAsync(x => x.Id == item.ModelId);


                    if (!modelExists)
                    {
                        return BadRequest(
                            "Invalid AirCon Model Id : "
                            + item.ModelId
                        );
                    }

                    _context.AirConUnits.Add(aircon);



                    await _context.SaveChangesAsync();



                    if (item.InstallationType == "New")
                    {


                        if (item.ContractStartDate.HasValue &&
                           item.ContractEndDate.HasValue)
                        {


                            var warranty = new Warranty
                            {

                                AirConId = aircon.Id,


                                StartDate =
                                    item.ContractStartDate.Value,


                                EndDate =
                                    item.ContractEndDate.Value,


                                IsActive = true

                            };



                            _context.Warranties.Add(warranty);

                            await _context.SaveChangesAsync();


                        }

                    }


                }


            }



            service.Status = "In Progress";

            await _context.SaveChangesAsync();

            return RedirectToAction(
      "Complete",
      "TechnicianService",
      new { id = service.ServiceId });

        }

        // POST: Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(AirConUnit aircon)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        aircon.CreatedAt = DateTime.Now;
        //        aircon.IsDeleted = false;

        //        _context.AirConUnits.Add(aircon);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }

        //    // Reload dropdowns if validation fails
        //    ViewBag.Brands = await _context.AirConBrands
        //        .Where(b => b.IsDeleted != true)
        //        .ToListAsync();
        //    ViewBag.Customers = await _context.Customers
        //        .Where(c => c.IsDeleted != true)
        //        .ToListAsync();

        //    return View(aircon);
        //}

        public async Task<IActionResult> Edit(int serviceId)
        {
            var aircon = await _context.AirConUnits
                .Include(x => x.Customer)
                .Include(x => x.Brand)
                .Include(x => x.Model)
                .FirstOrDefaultAsync(x =>
                    x.ServiceId == serviceId &&
                    x.IsDeleted != true);

            if (aircon == null)
                return NotFound();

            ViewBag.ServiceId = serviceId;

            ViewBag.Brands = await _context.AirConBrands
                .Where(x => x.IsDeleted != true)
                .ToListAsync();

            ViewBag.Models = await _context.AirConModels
                .Where(x =>
                    x.BrandId == aircon.BrandId &&
                    x.IsDeleted != true)
                .ToListAsync();

            ViewBag.CustomerName = aircon.Customer?.Name;
            ViewBag.CustomerPhone = aircon.Customer?.Phone;

            var quantity = await _context.AirConUnits
                .CountAsync(x =>
                    x.ServiceId == serviceId &&
                    x.IsDeleted != true);

            var vm = new EditAirConUnitViewModel
            {
                Id = aircon.Id,
                CustomerId = aircon.CustomerId,
                BrandId = aircon.BrandId,
                ModelId = aircon.ModelId,
                BrandName = aircon.Brand?.BrandName,
                ModelName = aircon.Model?.ModelName,
                SerialNumber = aircon.SerialNumber,
                CapacityHp = aircon.CapacityHp,
                AirConType = aircon.AirConType,
                InstallationType = aircon.InstallationType,
                InstallationDate = aircon.InstallationDate,
                Quantity = quantity,
                CustomerName = aircon.Customer?.Name,
                CustomerPhone = aircon.Customer?.Phone,
                ServiceId = serviceId
            };

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditAirConUnitViewModel model,
            DateTime? ContractStartDate,
            DateTime? ContractEndDate,
            int serviceId)

        {
            if (!ModelState.IsValid)
            {
                ViewBag.Brands = await _context.AirConBrands
                    .Where(x => x.IsDeleted != true)
                    .ToListAsync();

                ViewBag.Models = await _context.AirConModels
                    .Where(x => x.BrandId == model.BrandId &&
                                x.IsDeleted != true)
                    .ToListAsync();

                return View(model);
            }

            var aircon = await _context.AirConUnits
                .FirstOrDefaultAsync(x => x.Id == model.Id &&
                                          x.IsDeleted != true);

            if (aircon == null)
                return NotFound();

            aircon.BrandId = model.BrandId;
            aircon.ModelId = model.ModelId;
            aircon.SerialNumber = model.SerialNumber;
            aircon.CapacityHp = model.CapacityHp;
            aircon.AirConType = model.AirConType;
            aircon.InstallationType = model.InstallationType;
            aircon.InstallationDate = model.InstallationDate;
            aircon.UpdatedAt = DateTime.Now;

            // ===============================
            // Quantity Sync
            // ===============================

            var currentUnits = await _context.AirConUnits
                .Where(x =>
                    x.CustomerId == aircon.CustomerId &&
                    x.ServiceId == serviceId &&
                    x.BrandId == aircon.BrandId &&
                    x.ModelId == aircon.ModelId &&
                    x.InstallationType == aircon.InstallationType &&
                    x.IsDeleted != true)
                .ToListAsync();



            int currentQuantity = currentUnits.Count;



            // Add New Units

            if (model.Quantity > currentQuantity)
            {
                int addCount = model.Quantity - currentQuantity;


                for (int i = 0; i < addCount; i++)
                {

                    var newUnit = new AirConUnit
                    {
                        CustomerId = aircon.CustomerId,

                        BrandId = aircon.BrandId,

                        ModelId = aircon.ModelId,

                        CapacityHp = aircon.CapacityHp,

                        AirConType = aircon.AirConType,

                        ServiceId = serviceId,

                        InstallationType = aircon.InstallationType,

                        InstallationDate = aircon.InstallationDate,

                        CreatedAt = DateTime.Now,

                        IsDeleted = false
                    };


                    _context.AirConUnits.Add(newUnit);

                }
            }



            // Remove Units

            else if (model.Quantity < currentQuantity)
            {

                int removeCount =
                    currentQuantity - model.Quantity;


                var removeUnits =
                    currentUnits
                    .OrderByDescending(x => x.Id)
                    .Take(removeCount)
                    .ToList();



                foreach (var item in removeUnits)
                {
                    item.IsDeleted = true;
                    item.DeletedAt = DateTime.Now;
                }

            }

            var warranty = await _context.Warranties
                .FirstOrDefaultAsync(x => x.AirConId == aircon.Id);

            if (aircon.InstallationType == "New")
            {
                if (warranty == null)
                {
                    if (ContractStartDate.HasValue &&
                        ContractEndDate.HasValue)
                    {
                        warranty = new Warranty
                        {
                            AirConId = aircon.Id,
                            StartDate = ContractStartDate.Value,
                            EndDate = ContractEndDate.Value,
                            IsActive = true
                        };

                        _context.Warranties.Add(warranty);
                    }
                }
                else
                {
                    warranty.StartDate = ContractStartDate ?? warranty.StartDate;
                    warranty.EndDate = ContractEndDate ?? warranty.EndDate;
                    warranty.IsActive = true;
                }
            }
            else
            {
                if (warranty != null)
                {
                    warranty.IsActive = false;
                }
            }

            await _context.SaveChangesAsync();


            return RedirectToAction(
                "Index",
                "ServiceRequest",
                new
                {
                    id = serviceId
                });
        }

        public async Task<IActionResult> Delete(
    int id,
    int serviceId)
        {
            var aircon = await _context.AirConUnits
                .Include(x => x.Customer)
                .Include(x => x.Brand)
                .Include(x => x.Model)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsDeleted != true);


            if (aircon == null)
                return NotFound();

            ViewBag.TotalQuantity =
            await _context.AirConUnits
            .CountAsync(x =>
                x.CustomerId == aircon.CustomerId &&
                x.BrandId == aircon.BrandId &&
                x.ModelId == aircon.ModelId &&
                x.InstallationType == aircon.InstallationType &&
                x.IsDeleted == false);

            ViewBag.ServiceId = serviceId;


            return View(aircon);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id,
            int RemoveQuantity,
            int serviceId)
        {

            Console.WriteLine($"Id = {id}");
            Console.WriteLine($"RemoveQuantity = {RemoveQuantity}");
            Console.WriteLine($"ServiceId = {serviceId}");

            var aircon = await _context.AirConUnits
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsDeleted != true);



            if (aircon == null)
                return NotFound();



            // Find all matching AirCon Units

            var units = await _context.AirConUnits
                .Where(x =>
                    x.CustomerId == aircon.CustomerId &&
                    x.BrandId == aircon.BrandId &&
                    x.ModelId == aircon.ModelId &&
                    x.InstallationType == aircon.InstallationType &&
                    x.IsDeleted == false)
                .OrderByDescending(x => x.Id)
                .Take(RemoveQuantity)
                .ToListAsync();

            Console.WriteLine($"Found Units = {units.Count}");
            // Soft Delete Selected Quantity

            foreach (var item in units)
            {
                item.IsDeleted = true;
                item.DeletedAt = DateTime.Now;

                var warranty = await _context.Warranties
                    .FirstOrDefaultAsync(x => x.AirConId == item.Id);

                if (warranty != null)
                {
                    warranty.IsActive = false;
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("Deleted Successfully");


            return RedirectToAction(
                "Complete",
                "TechnicianService",
                new
                {
                    id = serviceId
                });

        }

        // AJAX: Get Models by Brand
        [HttpGet]
        public async Task<JsonResult> GetModelsByBrand(int brandId)
        {
            var models = await _context.AirConModels
                .Where(m => m.BrandId == brandId && m.IsDeleted != true)
                .Select(m => new { id = m.Id, modelName = m.ModelName })
                .ToListAsync();

            return Json(models);
        }
    }
}
