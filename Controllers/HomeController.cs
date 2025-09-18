using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BTL.Web.Models;
using BTL.Web.Services;

namespace BTL.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IOrderReceptionService _orderReceptionService;
    private readonly IBanAnService _banAnService;
    private readonly IMonService _monService;
    private readonly IOrderService _orderService;

    public HomeController(
        ILogger<HomeController> logger,
        IOrderReceptionService orderReceptionService,
        IBanAnService banAnService,
        IMonService monService,
        IOrderService orderService)
    {
        _logger = logger;
        _orderReceptionService = orderReceptionService;
        _banAnService = banAnService;
        _monService = monService;
        _orderService = orderService;
    }

    public IActionResult Index()
    {
        return View();
    }

    // GET: Home/OrderManagement - Màn hình tạo đơn hàng
    public async Task<IActionResult> OrderManagement()
    {
        Console.WriteLine("=== GET Home/OrderManagement - Bắt đầu tải trang tạo đơn hàng ===");

        try
        {
            Console.WriteLine("Đang lấy danh sách bàn có sẵn...");
            var availableTables = await _orderReceptionService.GetAvailableTablesAsync();
            Console.WriteLine($"Tìm thấy {availableTables?.Count() ?? 0} bàn có sẵn");

            Console.WriteLine("Đang lấy danh sách món ăn...");
            var monAn = await _monService.GetAllAsync();
            Console.WriteLine($"Tìm thấy {monAn?.Count() ?? 0} món ăn");

            ViewBag.AvailableTables = availableTables ?? new List<BanAn>();
            ViewBag.MonAn = monAn ?? new List<Mon>();

            Console.WriteLine("Trang OrderManagement đã được tải thành công");
            return View();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LỖI trong GET Home/OrderManagement: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");

            _logger.LogError(ex, "Lỗi khi tải trang OrderManagement");
            ViewBag.AvailableTables = new List<BanAn>();
            ViewBag.MonAn = new List<Mon>();
            return View();
        }
    }

    // POST: Home/CreateOrder - Tạo đơn hàng mới
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateViewModel model)
    {
        Console.WriteLine("=== POST Home/CreateOrder - Bắt đầu xử lý tạo đơn hàng ===");
        Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
        Console.WriteLine($"Thời gian nhận request: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        if (model != null)
        {
            Console.WriteLine($"Loại đơn hàng: {(model.LaMangVe ? "Mang về" : "Ăn tại chỗ")}");
            Console.WriteLine($"Bàn ID: {model.BanId}");
            Console.WriteLine($"Số khách: {model.SoKhach}");
            Console.WriteLine($"Tên khách hàng: {model.KhachHangTen}");
            Console.WriteLine($"SĐT khách hàng: {model.KhachHangSdt}");
            Console.WriteLine($"Số món ăn: {model.OrderItems?.Count ?? 0}");

            if (model.OrderItems != null && model.OrderItems.Any())
            {
                Console.WriteLine("Chi tiết các món ăn:");
                foreach (var item in model.OrderItems)
                {
                    Console.WriteLine($"- MonId: {item.MonId}, SoLuong: {item.SoLuong}");
                }
            }
        }
        else
        {
            Console.WriteLine("Model là NULL!");
        }

        // Debug: Log raw request data
        Console.WriteLine("=== DEBUG RAW REQUEST ===");
        Console.WriteLine($"Request.ContentType: {Request.ContentType}");
        Console.WriteLine($"Request.Method: {Request.Method}");
        Console.WriteLine($"Request.Headers: {string.Join(", ", Request.Headers.Select(h => $"{h.Key}={h.Value}"))}");

        if (ModelState.IsValid && model != null)
        {
            Console.WriteLine("ModelState hợp lệ, bắt đầu tạo đơn hàng...");
            try
            {
                Console.WriteLine("Gọi OrderService.CreateOrderAsync...");
                var (orderId, success, message) = await _orderService.CreateOrderAsync(model);

                Console.WriteLine($"Kết quả từ OrderService: OrderId={orderId}, Success={success}, Message={message}");

                if (success)
                {
                    Console.WriteLine($"Đơn hàng được tạo thành công với ID: {orderId}");
                    return Json(new { success = true, orderId = orderId, message = message });
                }
                else
                {
                    Console.WriteLine($"Tạo đơn hàng thất bại: {message}");
                    return Json(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LỖI trong POST Home/CreateOrder: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                _logger.LogError(ex, "Lỗi khi tạo đơn hàng");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
        else
        {
            Console.WriteLine("ModelState không hợp lệ:");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"- {error.ErrorMessage}");
            }
        }

        Console.WriteLine("Trả về lỗi validation");
        return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
    }




    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
