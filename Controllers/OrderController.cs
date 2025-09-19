using BTL.Web.Models;
using BTL.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace BTL.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderReceptionService _orderReceptionService;
        private readonly IBanAnService _banAnService;
        private readonly IMonService _monService;
        private readonly IOrderService _orderService;
        private readonly IHoaDonService _hoaDonService;

        public OrderController(
            IOrderReceptionService orderReceptionService,
            IBanAnService banAnService,
            IMonService monService,
            IOrderService orderService,
            IHoaDonService hoaDonService)
        {
            _orderReceptionService = orderReceptionService;
            _banAnService = banAnService;
            _monService = monService;
            _orderService = orderService;
            _hoaDonService = hoaDonService;
        }

        // GET: Order
        public IActionResult Index()
        {
            // Redirect to PendingOrders since Index view doesn't exist
            return RedirectToAction("PendingOrders");
        }

        // GET: Order/Create
        public async Task<IActionResult> Create()
        {
            Console.WriteLine("=== GET Order/Create - Bắt đầu tải trang tạo đơn hàng ===");

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

                Console.WriteLine("Trang tạo đơn hàng đã được tải thành công");
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LỖI trong GET Order/Create: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Log error và trả về view với dữ liệu rỗng
                ViewBag.AvailableTables = new List<BanAn>();
                ViewBag.MonAn = new List<Mon>();
                return View();
            }
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel model)
        {
            Console.WriteLine("=== POST Order/Create - Bắt đầu xử lý tạo đơn hàng ===");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            if (model != null)
            {
                Console.WriteLine($"Loại đơn hàng: {(model.LaMangVe ? "Mang về" : "Ăn tại chỗ")}");
                Console.WriteLine($"Bàn ID: {model.BanId}");
                Console.WriteLine($"Số khách: {model.SoKhach}");
                Console.WriteLine($"Tên khách hàng: {model.KhachHangTen}");
                Console.WriteLine($"SĐT khách hàng: {model.KhachHangSdt}");
                Console.WriteLine($"Số món ăn: {model.OrderItems?.Count ?? 0}");
            }
            else
            {
                Console.WriteLine("Model là NULL!");
            }

            if (ModelState.IsValid && model != null)
            {
                Console.WriteLine("ModelState hợp lệ, bắt đầu tạo đơn hàng...");
                try
                {
                    // Tạo đơn hàng mới
                    var orderId = await CreateNewOrderAsync(model);
                    Console.WriteLine($"Kết quả tạo đơn hàng: OrderId = {orderId}");

                    if (orderId > 0)
                    {
                        Console.WriteLine("Đơn hàng được tạo thành công");
                        TempData["SuccessMessage"] = "Đơn hàng đã được tạo thành công!";
                        return RedirectToAction("PendingOrders", "Order");
                    }
                    else
                    {
                        Console.WriteLine("Tạo đơn hàng thất bại - OrderId <= 0");
                        ModelState.AddModelError("", "Có lỗi xảy ra khi tạo đơn hàng.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"LỖI trong POST Order/Create: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    ModelState.AddModelError("", $"Lỗi: {ex.Message}");
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

            Console.WriteLine("Reload dữ liệu cho View...");
            // Reload data nếu có lỗi
            try
            {
                var availableTables = await _orderReceptionService.GetAvailableTablesAsync();
                var monAn = await _monService.GetAllAsync();

                ViewBag.AvailableTables = availableTables ?? new List<BanAn>();
                ViewBag.MonAn = monAn ?? new List<Mon>();
                Console.WriteLine("Dữ liệu đã được reload thành công");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi reload dữ liệu: {ex.Message}");
                ViewBag.AvailableTables = new List<BanAn>();
                ViewBag.MonAn = new List<Mon>();
            }

            Console.WriteLine("Trả về View với model");
            return View(model);
        }

        // GET: Order/Details/5 - Redirect to new Details view
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var orderDetails = await _orderService.GetOrderDetailsAsync(id);
                if (orderDetails == null)
                {
                    return NotFound();
                }
                return View(orderDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Details: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải chi tiết đơn hàng.";
                return RedirectToAction("AllOrders");
            }
        }

        // GET: Order/Workflow/5
        public async Task<IActionResult> Workflow(int id)
        {
            // Get order with details for workflow view
            var pendingOrders = await _orderReceptionService.GetAllPendingOrdersAsync();
            var order = pendingOrders.FirstOrDefault(o => o.order_id == id);

            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // GET: Order/PendingOrders - Danh sách tất cả đơn hàng chưa hoàn thành
        public async Task<IActionResult> PendingOrders()
        {
            var pendingOrders = await _orderReceptionService.GetAllPendingOrdersAsync();
            return View(pendingOrders);
        }

        // GET: Order/AllOrders - Danh sách tất cả đơn hàng với phân trang và tìm kiếm
        public async Task<IActionResult> AllOrders(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchKeyword = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? filterType = null,
            string? status = null)
        {
            try
            {
                var result = await _orderService.GetOrdersWithPaginationAsync(
                    pageNumber, pageSize, searchKeyword, fromDate, toDate, filterType, status);

                ViewBag.SearchKeyword = searchKeyword;
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;
                ViewBag.FilterType = filterType;
                ViewBag.Status = status;

                return View(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AllOrders: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách đơn hàng.";
                return View(new PagedResult<OrderWithDetails>());
            }
        }


        // POST: Order/Cancel/5 - Hủy đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason = null)
        {
            try
            {
                // Validate order ID
                if (id <= 0)
                {
                    TempData["ErrorMessage"] = "ID đơn hàng không hợp lệ.";
                    return RedirectToAction("AllOrders");
                }

                // TODO: Get current employee ID from session/authentication
                // For now, using a default employee ID
                int employeeId = 1; // This should be replaced with actual employee ID from session

                var (success, message) = await _orderService.CancelOrderAsync(id, employeeId, reason);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Cancel: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi hủy đơn hàng.";
            }

            return RedirectToAction("AllOrders");
        }

        // POST: Order/CancelAjax/5 - Hủy đơn hàng qua AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAjax(int id, string? reason = null)
        {
            try
            {
                // Validate order ID
                if (id <= 0)
                {
                    return Json(new { success = false, message = "ID đơn hàng không hợp lệ." });
                }

                // TODO: Get current employee ID from session/authentication
                int employeeId = 1; // This should be replaced with actual employee ID from session

                var (success, message) = await _orderService.CancelOrderAsync(id, employeeId, reason);

                return Json(new { success = success, message = message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CancelAjax: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi hủy đơn hàng." });
            }
        }

        // GET: Order/Statistics - Thống kê đơn hàng
        public async Task<IActionResult> Statistics(DateTime? fromDate = null, DateTime? toDate = null, string? filterType = null)
        {
            try
            {
                var statistics = await _orderService.GetOrderStatisticsAsync(fromDate, toDate, filterType);
                return Json(statistics);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Statistics: {ex.Message}");
                return Json(new { error = "Có lỗi xảy ra khi tải thống kê." });
            }
        }

        private async Task<int> CreateNewOrderAsync(OrderCreateViewModel model)
        {
            Console.WriteLine("=== CreateNewOrderAsync - Bắt đầu tạo đơn hàng ===");

            try
            {
                Console.WriteLine("Đang chuẩn bị dữ liệu cho stored procedure...");

                // Log chi tiết model
                if (model.OrderItems != null && model.OrderItems.Any())
                {
                    Console.WriteLine("Chi tiết các món ăn:");
                    foreach (var item in model.OrderItems)
                    {
                        Console.WriteLine($"- MonId: {item.MonId}, SoLuong: {item.SoLuong}");
                    }
                }

                // Tạo đơn hàng và OrderItems bằng stored procedure duy nhất
                Console.WriteLine("Gọi stored procedure sp_Order_CreateWithItems (tạo đơn hàng + tất cả OrderItems)...");
                var orderId = await CreateOrderWithStoredProcedureAsync(model);
                Console.WriteLine($"Stored procedure hoàn thành, trả về OrderId: {orderId}");

                return orderId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LỖI trong CreateNewOrderAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task<int> CreateOrderWithStoredProcedureAsync(OrderCreateViewModel model)
        {
            Console.WriteLine("=== CreateOrderWithStoredProcedureAsync ===");

            try
            {
                Console.WriteLine("Gọi OrderService.CreateOrderAsync (tạo đơn hàng + tất cả OrderItems trong 1 stored procedure)...");
                var (orderId, success, message) = await _orderService.CreateOrderAsync(model);

                Console.WriteLine($"Kết quả: OrderId={orderId}, Success={success}, Message={message}");

                if (!success)
                {
                    throw new Exception($"Tạo đơn hàng thất bại: {message}");
                }

                return orderId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong CreateOrderWithStoredProcedureAsync: {ex.Message}");
                throw;
            }
        }

        // POST: Order/XuatHoaDonAjax/5 - Xuất hoá đơn cho đơn hàng qua AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XuatHoaDonAjax(int id, string? phuongThuc = "Tiền mặt")
        {
            try
            {
                // Kiểm tra đơn hàng có tồn tại không
                var orderDetails = await _orderService.GetOrderDetailsAsync(id);
                if (orderDetails == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                // Kiểm tra xem đã có hoá đơn chưa
                var existingHoaDon = await _hoaDonService.GetHoaDonByOrderIdAsync(id);
                if (existingHoaDon != null)
                {
                    // Nếu đã có hoá đơn, lấy thông tin chi tiết
                    var (hoaDon, orderDetailsWithItems) = await _hoaDonService.GetHoaDonWithOrderDetailsAsync(id);
                    return Json(new
                    {
                        success = true,
                        message = "Hoá đơn đã tồn tại.",
                        hoaDon = hoaDon,
                        orderDetails = orderDetailsWithItems,
                        isExisting = true
                    });
                }

                // Tạo hoá đơn mới
                var newHoaDon = await _hoaDonService.CreateHoaDonAsync(id, phuongThuc);

                // Lấy thông tin chi tiết hoá đơn vừa tạo
                var (createdHoaDon, orderDetailsWithItemsNew) = await _hoaDonService.GetHoaDonWithOrderDetailsAsync(id);

                return Json(new
                {
                    success = true,
                    message = "Hoá đơn đã được tạo thành công.",
                    hoaDon = createdHoaDon,
                    orderDetails = orderDetailsWithItemsNew,
                    isExisting = false
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in XuatHoaDonAjax: {ex.Message}");
                return Json(new { success = false, message = $"Có lỗi xảy ra khi xuất hoá đơn: {ex.Message}" });
            }
        }
    }
}
