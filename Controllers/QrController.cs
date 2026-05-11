using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace AirConServicingManagementSystem.Controllers
{
    public class QrController : Controller
    {
        private readonly DBContext _context;

        public QrController(DBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Verify(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Invalid");

            var qr = await _context.CustomerQrTokens
                .Include(x => x.AirConId)
                .FirstOrDefaultAsync(x => x.Token == token);

            // invalid / expired / used
            if (qr == null || qr.IsUsed || qr.ExpiredAt < DateTime.Now)
                return RedirectToAction("Invalid");

            // ✅ Auto login by QR
            HttpContext.Session.SetInt32("CustomerId", qr.CustomerId);

            // optional useful session
            HttpContext.Session.SetInt32("AirConId", qr.AirConId);

            // mark token consumed
            qr.IsUsed = true;

            await _context.SaveChangesAsync();

            // no login -> dashboard direct
            return RedirectToAction("Index", "CustomerDashboard");
        }

        public IActionResult Invalid()
        {
            return View();
        }
    }
}
