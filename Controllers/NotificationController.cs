using AirConServicingManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Controllers
{
    public class NotificationController : Controller
    {

        private readonly DBContext _context;


        public NotificationController(DBContext context)
        {
            _context = context;
        }


        // ===========================
        // All Notifications Page
        // ===========================

        public async Task<IActionResult> Index()
        {

            var userId =
                HttpContext.Session.GetInt32("UserId");


            if (userId == null)
            {
                return RedirectToAction("Login");
            }



            var notifications =
                await _context.Notifications
                .Where(x =>
                    x.UserId == userId)

                .OrderByDescending(x =>
                    x.CreatedAt)

                .ToListAsync();



            return View(notifications);

        }

        // ===========================
        // Unread Count
        // ===========================

        [HttpGet]
        public async Task<IActionResult> Count()
        {

            var userId =
                HttpContext.Session.GetInt32("UserId");


            if (userId == null)
            {
                return Json(0);
            }



            var count =
                await _context.Notifications
                .CountAsync(x =>
                    x.UserId == userId &&
                    x.IsRead == false);



            return Json(count);

        }





        // ===========================
        // Latest 5 Notifications
        // ===========================

        [HttpGet]
        public async Task<IActionResult> Latest()
        {

            var userId =
                HttpContext.Session.GetInt32("UserId");



            if (userId == null)
            {
                return Json(new List<object>());
            }




            var data =
                await _context.Notifications
                .Where(x =>
                    x.UserId == userId)

                .OrderByDescending(x =>
                    x.CreatedAt)

                .Take(5)

                .Select(x => new
                {

                    id = x.Id,

                    title = x.Title,

                    message = x.Message,

                    isRead = x.IsRead,

                    created =
                    x.CreatedAt.Value.ToString(
                        "dd MMM yyyy hh:mm tt"
                    )

                })

                .ToListAsync();



            return Json(data);

        }





        // ===========================
        // Read Notification
        // ===========================

        [HttpGet]
        public async Task<IActionResult> Read(int id)
        {

            var notification =
                await _context.Notifications
                .FirstOrDefaultAsync(x =>
                    x.Id == id);



            if (notification != null)
            {

                notification.IsRead = true;

                await _context.SaveChangesAsync();

            }



            return RedirectToAction(
                "Index"
            );

        }


    }
}