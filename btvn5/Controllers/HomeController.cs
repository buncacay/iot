using btvn5.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace btvn5.Controllers
{
    public class HomeController : Controller
    {
        // ✅ Biến lưu dữ liệu cảm biến hiện tại
        private static SensorData CurrentSensorData = new SensorData
        {
            Temperature = 0,
            Humidity = 0,
            Light = 0
        };

        // ✅ Tài khoản hợp lệ
        private static readonly Dictionary<string, string> validUsers = new()
        {
            { "user1", "123" }, { "user2", "123" }, { "user3", "123" },
            { "user4", "123" }, { "user5", "123" }, { "user6", "123" },
            { "user7", "123" }, { "user8", "123" }
        };

        // ✅ Lưu trạng thái LED theo từng user
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

        // ✅ Khóa tránh race condition
        private static readonly ConcurrentDictionary<string, object> UserLocks = new();

        // ✅ Hiển thị trang chính
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

        // ✅ Trang đăng nhập
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("user") != null)
                return RedirectToAction("Index");
            return View();
        }

        // ✅ Xử lý đăng nhập
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (validUsers.ContainsKey(username) && validUsers[username] == password)
            {
                HttpContext.Session.SetString("user", username);
                return RedirectToAction("Index");
            }
            ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
            return View();
        }

        // ✅ Đăng xuất
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ✅ Lấy trạng thái LED cho FE (KHÔNG có message)
        [HttpGet("api/control/state")]
        public IActionResult GetState()
        {
            var user = HttpContext.Session.GetString("user");
            if (user == null)
                return Unauthorized(new { message = "Chưa đăng nhập" });

            string state = LedStates.ContainsKey(user) ? LedStates[user] : "off";

            return Ok(new
            {
                user,
                state,
                light = CurrentSensorData.Light
            });
        }

        // ✅ Cập nhật trạng thái LED (CÓ message)
        [HttpPost("api/control/set")]
        public async Task<IActionResult> SetState([FromBody] JsonElement json)
        {
            var user = HttpContext.Session.GetString("user");
            if (user == null)
                return Unauthorized(new { message = "Chưa đăng nhập" });

            if (!json.TryGetProperty("state", out JsonElement stateElement))
                return BadRequest(new { message = "Thiếu dữ liệu 'state'" });

            string newState = stateElement.GetString() ?? "off";
            var userLock = UserLocks.GetOrAdd(user, new object());

            lock (userLock)
            {
                LedStates[user] = newState;
            }

            // ✅ Ghi nhận giá trị ánh sáng ban đầu
            int oldLight = CurrentSensorData.Light;

            // ⏳ Chờ tối đa 5 giây để ESP gửi dữ liệu cảm biến mới
            await Task.Delay(5000);

            string message;
            int newLight = CurrentSensorData.Light;

            // ✅ Logic bật/tắt sau khi đợi 5 giây
            if (newState == "on" && newLight == 1 && oldLight != newLight)
                message = "💡 Bật đèn thành công!";
            else if (newState == "off" && newLight == 0 && oldLight != newLight)
                message = "💤 Tắt đèn thành công!";
            else
                message = "⚠️ Thao tác thất bại, cảm biến không phản hồi sau 5 giây!";

            Console.WriteLine($"[SERVER] User: {user}, LED: {newState}, LightSensor: {newLight}, Message: {message}");

            return Ok(new { user, state = newState, message });
        }


        // ✅ ESP lấy tất cả trạng thái LED
        [HttpGet("api/esp/all")]
        [AllowAnonymous]
        public IActionResult GetAllStatesForEsp()
        {
            var allStates = LedStates.Select(kv => new
            {
                user = kv.Key,
                state = kv.Value
            });
            return Ok(new { message = "Danh sách trạng thái LED", data = allStates });
        }

        // ✅ ESP cập nhật dữ liệu cảm biến
        [HttpPut("api/esp/update-sensor")]
        [AllowAnonymous]
        public IActionResult UpdateSensor([FromBody] JsonElement json)
        {
            if (!json.ValueKind.Equals(JsonValueKind.Object))
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });

            try
            {
                if (json.TryGetProperty("temperature", out var tempProp))
                    CurrentSensorData.Temperature = (float)tempProp.GetDouble();

                if (json.TryGetProperty("humidity", out var humProp))
                    CurrentSensorData.Humidity = (float)humProp.GetDouble();

                if (json.TryGetProperty("light", out var lightProp))
                    CurrentSensorData.Light = lightProp.GetInt32();

                Console.WriteLine("=====================================");
                Console.WriteLine($"[ESP] 📡 CẬP NHẬT CẢM BIẾN:");
                Console.WriteLine($"🌡️ Nhiệt độ: {CurrentSensorData.Temperature} °C");
                Console.WriteLine($"💧 Độ ẩm: {CurrentSensorData.Humidity} %");
                Console.WriteLine($"💡 Ánh sáng: {CurrentSensorData.Light}");
                Console.WriteLine("=====================================");

                return Ok(new
                {
                    message = "Đã cập nhật dữ liệu cảm biến",
                    data = CurrentSensorData
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi xử lý dữ liệu", error = ex.Message });
            }
        }

        // ✅ FE lấy dữ liệu cảm biến
        [HttpGet("api/sensor/get")]
        public IActionResult GetSensor()
        {
            return Ok(new { message = "Dữ liệu cảm biến hiện tại", data = CurrentSensorData });
        }
    }
}
