using Microsoft.AspNetCore.Mvc;

namespace LedControl.Controllers
{
    public class HomeController : Controller
    {
       
        [HttpGet()]
        public IActionResult Index()
        {
            return View();
        }
    }
}
