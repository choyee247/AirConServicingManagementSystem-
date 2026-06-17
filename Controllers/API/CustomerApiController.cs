using AirConServicingManagementSystem.Models;
using AirConServicingManagementSystem.ViewsModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Controllers.API
{
    [Route("api/customers")]
    [ApiController]
    public class CustomerApiController : ControllerBase
    {
        private readonly DBContext _context;

        public CustomerApiController(DBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _context.Customers
                .Where(c => c.IsDeleted != true)
                .ToListAsync();

            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.AirConUnits)
                    .ThenInclude(a => a.Warranty)
                .Include(c => c.CustomerLocations)
                .Include(c => c.ServiceRecords)
                    .ThenInclude(sr => sr.Technician)
                .Include(c => c.ServiceReminders)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
                return NotFound();

            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomerLocationViewModel vm)
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

            if (vm.Latitude.HasValue && vm.Longitude.HasValue)
            {
                var location = new CustomerLocation
                {
                    CustomerId = customer.Id,
                    Latitude = vm.Latitude,
                    Longitude = vm.Longitude,
                    MapAddress = vm.MapAddress,
                    CreatedAt = DateTime.Now
                };

                _context.CustomerLocations.Add(location);
                await _context.SaveChangesAsync();
            }

            return Ok(customer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Customer model)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
                return NotFound();

            customer.Name = model.Name;
            customer.Phone = model.Phone;
            customer.Address = model.Address;
            customer.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(customer);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
                return NotFound();

            customer.IsDeleted = true;
            customer.DeletedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
