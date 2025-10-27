using btvn5.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;

namespace btvn5.Controllers
{
    public class HomeController : Controller
    {
        private static readonly Dictionary<string, string> validUsers = new()
        {
            { "user1", "123" },
            { "user2", "123" },
            { "user3", "123" },
            { "user4", "123" },
            { "user5", "123" },
            { "user6", "123" },
            { "user7", "123" },
            { "user8", "123" },
            { "admin", "123" }
        };

        private static ConcurrentDictionary<string, string> LedStates = new()
        {
            ["user1"] = "off",
            ["user2"] = "off",
            ["user3"] = "off",
            ["user4"] = "off",
            ["user5"] = "off",
            ["user6"] = "off",
            ["user7"] = "off",
            ["user8"] = "off"
        };


        [HttpGet]
        public IActionResult Index()
        {
            var user = HttpContext.Session.GetString("user");
            if (user == null)
                return RedirectToAction("Login");

            ViewBag.User = user;
            ViewBag.LedState = LedStates.ContainsKey(user) ? LedStates[user] : "off";
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            var user = HttpContext.Session.GetString("user");
            if (user != null)
                return RedirectToAction("Index");

            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (validUsers.ContainsKey(username) && validUsers[username] == password)
            {
                HttpContext.Session.SetString("user", username);
                return RedirectToAction("Index");
            }

            ViewBag.Error = "❌ Sai tài khoản hoặc mật khẩu!";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }


        [HttpGet]
        [Route("api/control/state")]
        public IActionResult GetState()
        {
            var user = HttpContext.Session.GetString("user");
            if (user == null)
                return Unauthorized(new { message = "Chưa đăng nhập" });

            string state = LedStates.ContainsKey(user) ? LedStates[user] : "off";
            return Ok(new { user, state });
        }

        [HttpPost]
        [Route("api/control/set")]
        public IActionResult SetState([FromBody] LedRequest body)
        {
            var user = HttpContext.Session.GetString("user");
            if (user == null)
                return Unauthorized(new { message = "Chưa đăng nhập" });

            if (body == null || string.IsNullOrEmpty(body.State))
                return BadRequest(new { message = "Thiếu trạng thái LED" });

            string state = body.State.ToLower() == "on" ? "on" : "off";
            LedStates[user] = state;

            Console.WriteLine($"[WEB] {user} => {state}");
            return Ok(new { message = "Đã cập nhật", user, state });
        }


        //[HttpGet("api/esp/state")]
        //[AllowAnonymous]
        //public IActionResult GetStateForEsp(string user)
        //{
        //    if (!LedStates.ContainsKey(user))
        //        return NotFound(new { message = $"User '{user}' không tồn tại" });

        //    string state = LedStates[user];
        //    Console.WriteLine($"[ESP] GET {user} => {state}");
        //    return Ok(new { user, state });
        //}

        [HttpGet("api/esp/all")]
        [AllowAnonymous]
        public IActionResult GetAllStatesForEsp()
        {
            Console.WriteLine("[ESP] GET ALL LED STATES");

            var allStates = LedStates.Select(kv => new
            {
                user = kv.Key,
                state = kv.Value
            });

            return Ok(new
            {
                message = "Danh sách trạng thái LED",
                data = allStates
            });
        }
    }
}
