using BTL.Web.Models;
using BTL.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTL.Web.Controllers
{
    public class ThongKeMonAnController : Controller
    {
        private readonly IThongKeMonAnService _thongKeMonAnService;

        public ThongKeMonAnController(IThongKeMonAnService thongKeMonAnService)
        {
            _thongKeMonAnService = thongKeMonAnService;
        }

        // GET: ThongKeMonAn
        public async Task<IActionResult> Index()
        {
            // Mặc định hiển thị tháng hiện tại
            var currentDate = DateTime.Now;
            var thang = currentDate.Month;
            var nam = currentDate.Year;

            try
            {
                var thongKe = await _thongKeMonAnService.GetThongKeTanSuatTheoThangAsync(thang, nam);
                ViewBag.Thang = thang;
                ViewBag.Nam = nam;
                ViewBag.ThongKe = thongKe;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải dữ liệu thống kê: {ex.Message}";
                return View(new List<ThongKeMonAnTanSuat>());
            }
        }

        // POST: ThongKeMonAn/Index
        [HttpPost]
        public async Task<IActionResult> Index(ThongKeMonAnRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ";
                return View(new List<ThongKeMonAnTanSuat>());
            }

            try
            {
                var thongKe = await _thongKeMonAnService.GetThongKeTanSuatTheoThangAsync(request.Thang, request.Nam);
                ViewBag.Thang = request.Thang;
                ViewBag.Nam = request.Nam;
                ViewBag.ThongKe = thongKe;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải dữ liệu thống kê: {ex.Message}";
                return View(new List<ThongKeMonAnTanSuat>());
            }
        }

        // GET: ThongKeMonAn/ChiTiet
        public async Task<IActionResult> ChiTiet(int thang, int nam)
        {
            try
            {
                var chiTiet = await _thongKeMonAnService.GetThongKeChiTietTheoNgayAsync(thang, nam);
                ViewBag.Thang = thang;
                ViewBag.Nam = nam;
                return View(chiTiet);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải dữ liệu chi tiết: {ex.Message}";
                return View(new List<ThongKeMonAnChiTiet>());
            }
        }

        // GET: ThongKeMonAn/TopMonAn
        public async Task<IActionResult> TopMonAn(int thang, int nam, int topN = 10)
        {
            try
            {
                var topMonAn = await _thongKeMonAnService.GetTopMonAnAsync(thang, nam, topN);
                ViewBag.Thang = thang;
                ViewBag.Nam = nam;
                ViewBag.TopN = topN;
                return View(topMonAn);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải dữ liệu top món ăn: {ex.Message}";
                return View(new List<ThongKeMonAnTop>());
            }
        }

        // POST: ThongKeMonAn/TopMonAn
        [HttpPost]
        public async Task<IActionResult> TopMonAn(ThongKeMonAnTopRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ";
                return View(new List<ThongKeMonAnTop>());
            }

            try
            {
                var topMonAn = await _thongKeMonAnService.GetTopMonAnAsync(request.Thang, request.Nam, request.TopN);
                ViewBag.Thang = request.Thang;
                ViewBag.Nam = request.Nam;
                ViewBag.TopN = request.TopN;
                return View(topMonAn);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải dữ liệu top món ăn: {ex.Message}";
                return View(new List<ThongKeMonAnTop>());
            }
        }

        // GET: ThongKeMonAn/TheoLoai
        public async Task<IActionResult> TheoLoai(int thang, int nam)
        {
            try
            {
                var theoLoai = await _thongKeMonAnService.GetThongKeTheoLoaiMonAsync(thang, nam);
                ViewBag.Thang = thang;
                ViewBag.Nam = nam;
                return View(theoLoai);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải dữ liệu theo loại: {ex.Message}";
                return View(new List<ThongKeMonAnTheoLoai>());
            }
        }

        // GET: ThongKeMonAn/SoSanh
        public IActionResult SoSanh()
        {
            return View();
        }

        // POST: ThongKeMonAn/SoSanh
        [HttpPost]
        public async Task<IActionResult> SoSanh(ThongKeMonAnSoSanhRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ";
                return View();
            }

            try
            {
                var soSanh = await _thongKeMonAnService.GetThongKeSoSanhThangAsync(
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

        // GET: ThongKeMonAn/Dashboard
        public async Task<IActionResult> Dashboard(int? thang, int? nam)
        {
            var currentDate = DateTime.Now;
            var thangHienTai = thang ?? currentDate.Month;
            var namHienTai = nam ?? currentDate.Year;

            try
            {
                var dashboard = await _thongKeMonAnService.GetDashboardAsync(thangHienTai, namHienTai);
                return View(dashboard);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải dashboard: {ex.Message}";
                return View(new ThongKeMonAnDashboard());
            }
        }

        // GET: ThongKeMonAn/ExportExcel
        public async Task<IActionResult> ExportExcel(int thang, int nam)
        {
            try
            {
                var thongKe = await _thongKeMonAnService.GetThongKeTanSuatTheoThangAsync(thang, nam);

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
