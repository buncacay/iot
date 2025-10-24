using Microsoft.AspNetCore.Mvc;
using LedControl.Models;
using System.Collections.Generic;

namespace LedControl.Controllers
{
    public class AccountController : Controller
    {
        private readonly Dictionary<string, string> accounts = new()
        {
            {"user1", "123"},
            {"user2", "123"},
            {"user3", "123"},
            {"user4", "123"},
            {"user5", "123"},
            {"user6", "123"},
            {"user7", "123"},
            {"user8", "123"}
        };

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(UserModel model)
        {
            if (model.Username != null && accounts.ContainsKey(model.Username)
                && accounts[model.Username] == model.Password)
            {
                HttpContext.Session.SetString("user", model.Username);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
