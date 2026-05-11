using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirConServicingManagementSystem.Models;
using AirConServicingManagementSystem.ViewsModels;

public class CustomerDashboardController : Controller
{
    private readonly DBContext _context;

    public CustomerDashboardController(DBContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        int customerId = 1; // 🔴 later replace with Session

        var customer = await _context.Customers
            .Include(c => c.AirConUnits.Where(a => a.IsDeleted != true))
            .ThenInclude(a => a.Brand)
            .Include(c => c.AirConUnits.Where(a => a.IsDeleted != true))
            .ThenInclude(a => a.Model)
            .Include(c => c.ServiceRecords.Where(s => s.IsDeleted != true))
            .Include(c => c.ServiceReminders.Where(r => r.IsDeleted != true))
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
            return NotFound();

        return View(customer);
    }
    //public async Task<IActionResult> Index()
    //{
    //    int? customerId = HttpContext.Session.GetInt32("CustomerId");

    //    if (customerId == null)
    //    {
    //        return RedirectToAction("Invalid", "Qr"); 
    //    }

    //    var customer = await _context.Customers
    //        .Include(c => c.AirConUnits.Where(a => a.IsDeleted != true))
    //            .ThenInclude(a => a.Brand)
    //        .Include(c => c.AirConUnits.Where(a => a.IsDeleted != true))
    //            .ThenInclude(a => a.Model)
    //        .Include(c => c.ServiceRecords.Where(s => s.IsDeleted != true))
    //        .Include(c => c.ServiceReminders.Where(r => r.IsDeleted != true))
    //        .FirstOrDefaultAsync(c => c.Id == customerId);

    //    if (customer == null)
    //    {
    //        HttpContext.Session.Clear();
    //        return RedirectToAction("Invalid", "Qr");
    //    }

    //    return View(customer);
    //}

    ///this?//
    //public async Task<IActionResult> Index(int customerId)
    //{
    //    var customer = await _context.Customers
    //        .Include(c => c.AirConUnits.Where(a => a.IsDeleted != true))
    //            .ThenInclude(a => a.Brand)
    //        .Include(c => c.AirConUnits.Where(a => a.IsDeleted != true))
    //            .ThenInclude(a => a.Model)
    //        .Include(c => c.ServiceRecords.Where(s => s.IsDeleted != true))
    //        .Include(c => c.ServiceReminders.Where(r => r.IsDeleted != true))
    //        .FirstOrDefaultAsync(c => c.Id == customerId);

    //    if (customer == null)
    //        return Content("Invalid Customer");

    //    return View(customer);
    //}
    // GET: CustomerDashboard/Aircons
    public async Task<IActionResult> Aircons()
    {
        int customerId = HttpContext.Session.GetInt32("CustomerId") ?? 0;

        var aircons = await _context.AirConUnits
            .Include(a => a.Brand)
            .Include(a => a.Model)
            .Include(a => a.Warranty)
            .Where(a => a.CustomerId == customerId && a.IsDeleted != true)
            .ToListAsync();

        return View(aircons);
    }


    // GET: Add Aircon
    public async Task<IActionResult> AddAircon()
    {
        int customerId = 9; // 🔴 later replace with Session

        ViewBag.Brands = await _context.AirConBrands
            .Where(b => b.IsDeleted != true)
            .ToListAsync();

        ViewBag.CustomerId = customerId;

        return View();
    }

    // POST: Add Aircon
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAircon(AirConUnit aircon)
    {
        if (!ModelState.IsValid)
        {
            aircon.CreatedAt = DateTime.Now;
            _context.AirConUnits.Add(aircon);
            await _context.SaveChangesAsync();

            return RedirectToAction("Aircons");
        }

        ViewBag.Brands = await _context.AirConBrands.ToListAsync();
        return View(aircon);
    }

    // AJAX: Get Models by Brand
    [HttpGet]
    public JsonResult GetModelsByBrand(int brandId)
    {
        var models = _context.AirConModels
            .Where(m => m.BrandId == brandId && m.IsDeleted != true)
            .Select(m => new { id = m.Id, modelName = m.ModelName })
            .ToList();

        return Json(models);
    }

    // GET: Edit Aircon
    public IActionResult EditAircon(int id)
    {
        var aircon = _context.AirConUnits
            .Include(a => a.Brand)
            .Include(a => a.Model)
            .FirstOrDefault(a => a.Id == id && a.IsDeleted != true);

        if (aircon == null)
            return NotFound();

        // Brands for dropdown
        ViewBag.Brands = _context.AirConBrands.Where(b => b.IsDeleted != true).ToList();

        // Models of selected brand
        ViewBag.Models = _context.AirConModels
            .Where(m => m.BrandId == aircon.BrandId && m.IsDeleted != true)
            .ToList();

        // Customer Id from session
        ViewBag.CustomerId = HttpContext.Session.GetInt32("CustomerId") ?? aircon.CustomerId;

        return View(aircon);
    }

    // POST: Edit Aircon
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditAircon(AirConUnit model)
    {
        if (!ModelState.IsValid)
        {
            var aircon = _context.AirConUnits.FirstOrDefault(a => a.Id == model.Id && a.IsDeleted != true);
            if (aircon == null) return NotFound();

            aircon.BrandId = model.BrandId;
            aircon.ModelId = model.ModelId;
            aircon.SerialNumber = model.SerialNumber;
            aircon.CapacityHp = model.CapacityHp;
            //aircon.Location = model.Location;
            aircon.InstallationDate = model.InstallationDate;
            aircon.UpdatedAt = DateTime.Now;

            _context.SaveChanges();
            return RedirectToAction("Aircons");
        }

        // Reload dropdowns if validation fails
        ViewBag.Brands = _context.AirConBrands.Where(b => b.IsDeleted != true).ToList();
        ViewBag.Models = _context.AirConModels
            .Where(m => m.BrandId == model.BrandId && m.IsDeleted != true)
            .ToList();
        ViewBag.CustomerId = model.CustomerId;

        return View(model);
    }
    public IActionResult DeleteAircon(int id)
    {
        var aircon = _context.AirConUnits.FirstOrDefault(a => a.Id == id && a.IsDeleted != true);
        if (aircon == null) return NotFound();

        // Soft delete
        aircon.IsDeleted = true;
        aircon.DeletedAt = DateTime.Now;

        _context.SaveChanges();
        return RedirectToAction("Aircons");
    }


    // 🛠 Service History
    public async Task<IActionResult> ServiceHistory()
    {
        int customerId = 1;

        var services = await _context.ServiceRecords
            .Include(s => s.AirConUnit)
                .ThenInclude(a => a.Brand)
            .Include(s => s.Technician)
            .Include(s => s.ServiceWarranties)
            .Where(s => s.CustomerId == customerId && s.IsDeleted != true)
            .OrderByDescending(s => s.ServiceDate)
            .ToListAsync();

        return View(services);
    }

    // 🔔 Service Reminders
    public async Task<IActionResult> Reminders()
    {
        int customerId = 1;

        var reminders = await _context.ServiceReminders
            .Include(r => r.AirConUnit)
                .ThenInclude(a => a.Brand)
            .Where(r => r.CustomerId == customerId && r.IsDeleted != true)
            .OrderBy(r => r.ReminderDate)
            .ToListAsync();

        return View(reminders);
    }

    // 📍 My Location
    public async Task<IActionResult> Location()
    {
        int customerId = 1;

        var locations = await _context.CustomerLocations
            .Where(l => l.CustomerId == customerId)
            .ToListAsync();

        return View(locations);
    }
    // GET: Customer/Profile
    public async Task<IActionResult> Profile()
    {
        int? customerId = HttpContext.Session.GetInt32("CustomerId");
        if (customerId == null)
            return RedirectToAction("Login", "CustomerLogin");

        var customer = await _context.Customers
            .Include(c => c.CustomerLocations)
            .FirstOrDefaultAsync(c => c.Id == customerId && c.IsDeleted != true);

        if (customer == null)
            return NotFound();

        var location = customer.CustomerLocations.FirstOrDefault();

        var vm = new CustomerLocationViewModel
        {
            Name = customer.Name,
            Phone = customer.Phone,
            //Email = customer.Email,
            Address = customer.Address,
            Latitude = location?.Latitude,   // default Yangon
            Longitude = location?.Longitude,
            MapAddress = location?.MapAddress
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(CustomerLocationViewModel vm)
    {
        int? customerId = HttpContext.Session.GetInt32("CustomerId");
        if (customerId == null)
            return RedirectToAction("Login", "CustomerLogin");

        var customer = await _context.Customers
            .Include(c => c.CustomerLocations)
            .FirstOrDefaultAsync(c => c.Id == customerId && c.IsDeleted != true);

        if (customer == null)
            return NotFound();

        if (ModelState.IsValid)
        {
            return View(vm);
        }

        // Update customer info
        customer.Name = vm.Name;
        customer.Phone = vm.Phone?.Trim();
        //customer.Email = vm.Email;
        customer.Address = vm.Address;
        customer.UpdatedAt = DateTime.Now;

        // Update or Insert location
        var location = customer.CustomerLocations.FirstOrDefault();
        if (location != null)
        {
            location.Latitude = vm.Latitude;
            location.Longitude = vm.Longitude;
            location.MapAddress = vm.MapAddress;
        }
        else if (vm.Latitude.HasValue && vm.Longitude.HasValue)
        {
            location = new CustomerLocation
            {
                CustomerId = customer.Id,
                Latitude = vm.Latitude,
                Longitude = vm.Longitude,
                MapAddress = vm.MapAddress,
                CreatedAt = DateTime.Now
            };
            _context.Add(location);
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Profile updated successfully.";

        // Redirect to GET Profile to refresh View
        return RedirectToAction("Profile");
    }


    // 👤 Profile
    //public async Task<IActionResult> Profile()
    //{
    //    int? customerId = HttpContext.Session.GetInt32("CustomerId");

    //    if (customerId == null)
    //        return RedirectToAction("Login", "CustomerLogin");

    //    var customer = await _context.Customers
    //        .FirstOrDefaultAsync(c => c.Id == customerId && c.IsDeleted != true);

    //    if (customer == null)
    //        return NotFound();

    //    return View(customer);
    //}

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Profile(Customer model)
    //{
    //    int? customerId = HttpContext.Session.GetInt32("CustomerId");

    //    if (customerId == null)
    //        return RedirectToAction("Login", "CustomerLogin");

    //    var customer = await _context.Customers
    //        .FirstOrDefaultAsync(c => c.Id == customerId && c.IsDeleted != true);

    //    if (customer == null)
    //        return NotFound();

    //    // ✅ Only update allowed fields
    //    customer.Name = model.Name;
    //    customer.Phone = model.Phone?.Trim();
    //    customer.Email = model.Email;
    //    customer.Address = model.Address;
    //    customer.UpdatedAt = DateTime.Now;

    //    await _context.SaveChangesAsync();

    //    TempData["Success"] = "Profile updated successfully.";
    //    return RedirectToAction("Profile");
    //}
}
