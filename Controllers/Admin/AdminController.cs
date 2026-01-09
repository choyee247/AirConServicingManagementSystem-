using Microsoft.AspNetCore.Mvc;

namespace AirConServicingManagementSystem.Controllers.Admin
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
                return RedirectToAction("Login", "AdminLogin");

            return View();
        }
    }
}
