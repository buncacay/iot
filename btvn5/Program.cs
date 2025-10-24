using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using btvn5.Models;
using System.Text.Json;

// ✅ Biến toàn cục giữ trạng thái hiện tại
string ledState = "off";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSession();



// ✅ Dùng MVC
builder.Services.AddControllersWithViews();

// ✅ CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseAuthorization();

// ✅ Route mặc định MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ✅ API: Lấy trạng thái LED hiện tại
app.MapGet("/control", () =>
{
    Console.WriteLine($"Gửi trạng thái hiện tại: {ledState}");
    return Results.Json(new
    {
        status = "ok",
        state = ledState
    });
});

// ✅ API: Cập nhật trạng thái LED từ FE
app.MapPost("/control", async (HttpContext context) =>
{
    var data = await context.Request.ReadFromJsonAsync<ControlRequest>();
    if (data?.State != null)
    {
        ledState = data.State.ToLower() == "on" ? "on" : "off";
        Console.WriteLine($"Nhận từ FE: {JsonSerializer.Serialize(data)}");
    }

    return Results.Json(new
    {
        status = "ok",
        state = ledState
    });
});
app.UseSession();
app.Run();
