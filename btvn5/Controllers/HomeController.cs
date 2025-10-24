using Microsoft.AspNetCore.Mvc;

namespace LedControl.Controllers
{
    public class HomeController : Controller
    {
        private static string _ledState = "off";

        public IActionResult Index()
        {
            var user = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(user))
                return RedirectToAction("Login", "Account");

            ViewBag.User = user;
            ViewBag.LedState = _ledState;
            return View();
        }

    }
}
