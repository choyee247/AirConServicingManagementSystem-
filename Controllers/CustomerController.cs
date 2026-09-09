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

        public async Task<IActionResult> Index(
    string search,
    int? stateId,
    int? townshipId,
    DateTime? filterDate)
        {

            var technicianId = HttpContext.Session.GetInt32("TechnicianId");

            if (technicianId == null)
            {
                return Unauthorized();
            }

            ViewBag.States = await _context.TbStateDivisions
                .Select(x => new
                {
                    x.StateDivisionPkid,
                    x.StateDivision,
                    x.StateDivisionEn
                })
                .OrderBy(x => x.StateDivisionEn)
                .ToListAsync();

            var customers = _context.Customers

                .Include(x => x.CustomerLocations)
                    .ThenInclude(x => x.StateDivisionPk)

                .Include(x => x.CustomerLocations)
                    .ThenInclude(x => x.TownshipPk)

                .Where(x =>
                    x.IsDeleted != true &&

                   x.TechnicianId == technicianId
                )

                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                customers = customers.Where(c =>
                    c.Name.Contains(search) ||
                    c.Phone.Contains(search) ||
                    c.Address.Contains(search) ||

                    c.CustomerLocations.Any(l =>
                        l.StateDivisionPk.StateDivisionEn.Contains(search) ||
                        l.TownshipPk.TownshipEn.Contains(search)
                    )
                );
            }

            if (stateId.HasValue)
            {
                customers = customers.Where(c =>
                    c.CustomerLocations.Any(l =>
                        l.StateDivisionPkid == stateId.Value
                    )
                );
            }

            if (townshipId.HasValue)
            {
                customers = customers.Where(c =>
                    c.CustomerLocations.Any(l =>
                        l.TownshipPkid == townshipId.Value
                    )
                );
            }

            if (filterDate.HasValue)
            {
                var date = filterDate.Value.Date;
                var nextDate = date.AddDays(1);

                customers = customers.Where(c =>
                    c.CreatedAt >= date &&
                    c.CreatedAt < nextDate
                );
            }

            var result = await customers
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return View(result);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var customer = await _context.Customers

                .Include(c => c.AirConUnits)
                    .ThenInclude(a => a.Brand)

                .Include(c => c.AirConUnits)
                    .ThenInclude(a => a.Model)

                .Include(c => c.AirConUnits)
                    .ThenInclude(a => a.Warranty)

                .Include(c => c.AirConUnits)
                    .ThenInclude(a => a.ServiceRecordUnits)
                        .ThenInclude(sru => sru.ServiceRecord)
                            .ThenInclude(sr => sr.Technician)

                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
                return NotFound();

            return View(customer);
        }
        public IActionResult Create()
        {
            var vm = new CustomerLocationViewModel();

            vm.StateDivisions = _context.TbStateDivisions
                            .OrderBy(x => x.StateDivisionEn)
                            .ToList();

            vm.Townships = new List<TbTownship>();

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerLocationViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var customer = new Customer
                {
                    Name = vm.Name,
                    Phone = vm.Phone,
                    Address = vm.Address,
                    CreatedAt = DateTime.Now
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                var location = new CustomerLocation
                {
                    CustomerId = customer.Id,

                    StateDivisionPkid = vm.StateDivisionPkid,
                    TownshipPkid = vm.TownshipPkid,

                    CreatedAt = DateTime.Now
                };

                _context.CustomerLocations.Add(location);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            vm.StateDivisions = _context.TbStateDivisions.ToList();
            vm.Townships = new List<TbTownship>();

            return View(vm);
        }
        public async Task<IActionResult> GetTownshipsByState(int stateId)
        {
            var data = await _context.TbTownships
                .Where(x => x.StateDivisionPkid == stateId)
                .OrderBy(x => x.TownshipEn)
                .Select(x => new
                {
                    townshipPkid = x.TownshipPkid,
                    townshipEn = x.TownshipEn
                })
                .ToListAsync();

            return Json(data);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var customer = await _context.Customers
                .Include(c => c.CustomerLocations)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
                return NotFound();

            var location = customer.CustomerLocations?.FirstOrDefault();

            var vm = new CustomerLocationViewModel
            {
                Id = customer.Id,
                Name = customer.Name,
                Phone = customer.Phone,
                Address = customer.Address,
                StateDivisionPkid = (location?.StateDivisionPkid),
                TownshipPkid = (location?.TownshipPkid),

                StateDivisions = await _context.TbStateDivisions
                    .OrderBy(x => x.StateDivisionEn)
                    .ToListAsync(),

                Townships = location != null
                    ? await _context.TbTownships
                        .Where(x => x.StateDivisionPkid == location.StateDivisionPkid)
                        .OrderBy(x => x.TownshipEn)
                        .ToListAsync()
                    : new List<TbTownship>()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
      int id,
      CustomerLocationViewModel vm)
        {
            if (id != vm.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var customer = await _context.Customers
                    .Include(c => c.CustomerLocations)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (customer == null)
                    return NotFound();

                // =========================
                // CUSTOMER
                // =========================

                customer.Name = vm.Name;
                customer.Phone = vm.Phone;
                customer.Address = vm.Address;
                customer.UpdatedAt = DateTime.Now;


                // =========================
                // CUSTOMER LOCATION
                // =========================

                var location = customer.CustomerLocations?.FirstOrDefault();

                if (location != null)
                {
                    location.StateDivisionPkid = vm.StateDivisionPkid;
                    location.TownshipPkid = vm.TownshipPkid;
                }
                else
                {
                    location = new CustomerLocation
                    {
                        CustomerId = customer.Id,
                        StateDivisionPkid = vm.StateDivisionPkid,
                        TownshipPkid = vm.TownshipPkid,
                        CreatedAt = DateTime.Now
                    };

                    _context.CustomerLocations.Add(location);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }


            // =========================
            // VALIDATION FAILED
            // =========================

            vm.StateDivisions = await _context.TbStateDivisions
                .OrderBy(x => x.StateDivisionEn)
                .ToListAsync();

            if (vm.StateDivisionPkid.HasValue)
            {
                vm.Townships = await _context.TbTownships
                    .Where(x => x.StateDivisionPkid == vm.StateDivisionPkid.Value)
                    .OrderBy(x => x.TownshipEn)
                    .ToListAsync();
            }
            else
            {
                vm.Townships = new List<TbTownship>();
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
