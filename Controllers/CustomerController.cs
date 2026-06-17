using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirConServicingManagementSystem.Models;
using AirConServicingManagementSystem.ViewsModels;

namespace AirConServicingManagementSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly DBContext _context;

        public CustomerController(DBContext context)
        {
            _context = context;
        }

        // GET: Customer
        public async Task<IActionResult> Index(
      string search,
      int? stateId,
      int? townshipId,
      DateTime? filterDate)
        {
            // States dropdown
            ViewBag.States = await _context.TbStateDivisions
                .Select(x => new {
                    x.StateDivisionPkid,
                    x.StateDivision,
                    x.StateDivisionEn
                })
                .OrderBy(x => x.StateDivisionEn)
                .ToListAsync();

            // base query
            var customers = _context.Customers
                .Include(x => x.CustomerLocations)
                .Where(x => x.IsDeleted != true)
                .AsQueryable();

            // =========================
            // SEARCH (REAL DATA)
            // =========================
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                customers = customers.Where(c =>
                    c.Name.Contains(search) ||
                    c.Phone.Contains(search) ||
                    c.Address.Contains(search) ||
                    c.CustomerLocations.Any(l =>
                        l.StateDivisionPk.StateDivisionEn.Contains(search) ||
                        l.TownshipPk.TownshipEn.Contains(search))
                );
            }

            // =========================
            // STATE FILTER
            // =========================
            if (stateId.HasValue)
            {
                customers = customers.Where(c =>
                    c.CustomerLocations.Any(l =>
                        l.StateDivisionPkid == stateId.Value));
            }

            // =========================
            // TOWNSHIP FILTER
            // =========================
            if (townshipId.HasValue)
            {
                customers = customers.Where(c =>
                    c.CustomerLocations.Any(l =>
                        l.TownshipPkid == townshipId.Value));
            }

            // =========================
            // DATE FILTER (optional)
            // =========================
            if (filterDate.HasValue)
            {
                customers = customers.Where(c =>
                    c.CreatedAt == filterDate.Value.Date);
            }

            var result = await customers
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return View(result);
        }
        // GET: Customer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers
                .Include(c => c.AirConUnits)
                        .ThenInclude(a => a.Warranty)
                .Include(c => c.CustomerLocations)
                .Include(c => c.ServiceRecords)
                        .ThenInclude(sr => sr.Technician)
                .Include(c => c.ServiceReminders)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null) return NotFound();

            return View(customer);
        }

        // GET: Customer/Create
        public IActionResult Create()
        {
            var vm = new CustomerLocationViewModel();
            return View(vm);
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerLocationViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Save Customer
                var customer = new Customer
                {
                    Name = vm.Name,
                    Phone = vm.Phone,
                    Address = vm.Address,
                    CreatedAt = DateTime.Now
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // Save Location
                var location = new CustomerLocation
                {
                    CustomerId = customer.Id,

                    Latitude = vm.Latitude,
                    Longitude = vm.Longitude,

                    MapAddress = vm.MapAddress,

                    // NEW
                    StateDivisionPkid = vm.StateDivisionPkid,
                    TownshipPkid = vm.TownshipPkid,

                    CreatedAt = DateTime.Now
                };

                _context.CustomerLocations.Add(location);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }
        [HttpGet]
        public JsonResult GetTownshipInfo(string township)
        {
            var data = _context.TbTownships
                .Include(x => x.StateDivisionPk)
                .Where(x => township != null && x.TownshipEn.Contains(township.Trim()))
                .Select(x => new
                {
                    TownshipPkid = x.TownshipPkid,
                    TownshipName = x.TownshipEn,
                    StateDivisionPkid = x.StateDivisionPkid,
                    StateDivisionName = x.StateDivisionPk.StateDivisionEn
                })
                .FirstOrDefault();

            return Json(data);
        }
        public async Task<IActionResult> GetTownshipsByState(int stateId)
        {
            var data = await _context.TbTownships
                .Where(x => x.StateDivisionPkid == stateId)
               .Select(x => new
               {
                   townshipPkid = x.TownshipPkid,
                   townshipEn = x.TownshipEn.Trim()
               })
                .ToListAsync();

            return Json(data);
        }

        // GET: Customer/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Customer/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Name,Phone,Email,Address")] Customer customer)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        customer.CreatedAt = DateTime.Now;
        //        _context.Add(customer);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(customer);
        //}

        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer vm)
        {
            if (id != vm.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null) return NotFound();

                // only update editable fields
                customer.Name = vm.Name;
                customer.Phone = vm.Phone;
                customer.Address = vm.Address;

                // keep original CreatedAt
                customer.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        // GET: Customer/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: Customer/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                customer.IsDeleted = true;
                customer.DeletedAt = DateTime.Now;
                _context.Update(customer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}
