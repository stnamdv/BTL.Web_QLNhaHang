using BTL.Web.Models;
using BTL.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTL.Web.Controllers
{
    public class ThongKeBanAnController : Controller
    {
        private readonly IThongKeBanAnService _thongKeBanAnService;

        public ThongKeBanAnController(IThongKeBanAnService thongKeBanAnService)
        {
            _thongKeBanAnService = thongKeBanAnService;
        }

        // GET: ThongKeBanAn
        public async Task<IActionResult> Index()
        {
            // Mặc định hiển thị tháng hiện tại
            var currentDate = DateTime.Now;
            var thang = currentDate.Month;
            var nam = currentDate.Year;

            try
            {
                var thongKe = await _thongKeBanAnService.GetThongKeTrungBinhTheoNgayAsync(thang, nam);
                ViewBag.Thang = thang;
                ViewBag.Nam = nam;
                ViewBag.ThongKe = thongKe;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải dữ liệu thống kê: {ex.Message}";
                return View(new List<ThongKeBanAnTrungBinh>());
            }
        }

        // POST: ThongKeBanAn/Index
        [HttpPost]
        public async Task<IActionResult> Index(ThongKeBanAnRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ";
                return View(new List<ThongKeBanAnTrungBinh>());
            }

            try
            {
                var thongKe = await _thongKeBanAnService.GetThongKeTrungBinhTheoNgayAsync(request.Thang, request.Nam);
                ViewBag.Thang = request.Thang;
                ViewBag.Nam = request.Nam;
                ViewBag.ThongKe = thongKe;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải dữ liệu thống kê: {ex.Message}";
                return View(new List<ThongKeBanAnTrungBinh>());
            }
        }

        // GET: ThongKeBanAn/ChiTiet
        public async Task<IActionResult> ChiTiet(int thang, int nam)
        {
            try
            {
                var chiTiet = await _thongKeBanAnService.GetThongKeChiTietTheoNgayAsync(thang, nam);
                ViewBag.Thang = thang;
                ViewBag.Nam = nam;
                return View(chiTiet);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải dữ liệu chi tiết: {ex.Message}";
                return View(new List<ThongKeBanAnChiTiet>());
            }
        }

        // GET: ThongKeBanAn/TongHop
        public async Task<IActionResult> TongHop(int thang, int nam)
        {
            try
            {
                var tongHop = await _thongKeBanAnService.GetThongKeTongHopAsync(thang, nam);
                ViewBag.Thang = thang;
                ViewBag.Nam = nam;
                return View(tongHop);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải dữ liệu tổng hợp: {ex.Message}";
                return View(new List<ThongKeBanAnTongHop>());
            }
        }

        // GET: ThongKeBanAn/SoSanh
        public IActionResult SoSanh()
        {
            return View();
        }

        // POST: ThongKeBanAn/SoSanh
        [HttpPost]
        public async Task<IActionResult> SoSanh(ThongKeBanAnSoSanhRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ";
                return View();
            }

            try
            {
                var soSanh = await _thongKeBanAnService.GetThongKeSoSanhThangAsync(
                    request.Thang1, request.Nam1,
                    request.Thang2, request.Nam2);

                ViewBag.Thang1 = request.Thang1;
                ViewBag.Nam1 = request.Nam1;
                ViewBag.Thang2 = request.Thang2;
                ViewBag.Nam2 = request.Nam2;
                ViewBag.SoSanh = soSanh;

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải dữ liệu so sánh: {ex.Message}";
                return View();
            }
        }

        // GET: ThongKeBanAn/Dashboard
        public async Task<IActionResult> Dashboard(int? thang, int? nam)
        {
            var currentDate = DateTime.Now;
            var thangHienTai = thang ?? currentDate.Month;
            var namHienTai = nam ?? currentDate.Year;

            try
            {
                var dashboard = await _thongKeBanAnService.GetDashboardAsync(thangHienTai, namHienTai);
                return View(dashboard);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải dashboard: {ex.Message}";
                return View(new ThongKeBanAnDashboard());
            }
        }

        // GET: ThongKeBanAn/ExportExcel
        public async Task<IActionResult> ExportExcel(int thang, int nam)
        {
            try
            {
                var thongKe = await _thongKeBanAnService.GetThongKeTrungBinhTheoNgayAsync(thang, nam);

                // Tạo file Excel (có thể sử dụng EPPlus hoặc ClosedXML)
                // Ở đây tôi sẽ trả về JSON để demo
                return Json(new
                {
                    success = true,
                    message = "Xuất Excel thành công",
                    data = thongKe
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Lỗi khi xuất Excel: {ex.Message}"
                });
            }
        }
    }
}
