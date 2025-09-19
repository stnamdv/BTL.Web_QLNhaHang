using Microsoft.AspNetCore.Mvc;
using BTL.Web.Models;
using BTL.Web.Services;

namespace BTL.Web.Controllers
{
    public class LuongNhanVienController : Controller
    {
        private readonly ILuongNhanVienService _luongNhanVienService;
        private readonly ILogger<LuongNhanVienController> _logger;

        public LuongNhanVienController(
            ILuongNhanVienService luongNhanVienService,
            ILogger<LuongNhanVienController> logger)
        {
            _luongNhanVienService = luongNhanVienService;
            _logger = logger;
        }

        // GET: LuongNhanVien
        public async Task<IActionResult> Index()
        {
            try
            {
                var luongThangHienTai = await _luongNhanVienService.TinhLuongThangHienTaiAsync();
                return View(luongThangHienTai);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang lương nhân viên");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dữ liệu lương nhân viên.";
                return View(new LuongNhanVienViewModel());
            }
        }

        // GET: LuongNhanVien/TinhLuongTheoThang
        public async Task<IActionResult> TinhLuongTheoThang(int? thang, int? nam)
        {
            try
            {
                // Nếu không có tham số, sử dụng tháng hiện tại
                var thangTinh = thang ?? DateTime.Now.Month;
                var namTinh = nam ?? DateTime.Now.Year;

                // Validation
                if (thangTinh < 1 || thangTinh > 12)
                {
                    TempData["ErrorMessage"] = "Tháng phải từ 1 đến 12.";
                    return RedirectToAction(nameof(Index));
                }

                if (namTinh < 2000 || namTinh > 2100)
                {
                    TempData["ErrorMessage"] = "Năm không hợp lệ.";
                    return RedirectToAction(nameof(Index));
                }

                var luongTheoThang = await _luongNhanVienService.TinhLuongTheoThangAsync(thangTinh, namTinh);
                return View("Index", luongTheoThang);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tính lương theo tháng {Thang}/{Nam}", thang, nam);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tính lương nhân viên.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: LuongNhanVien/TinhLuongTheoThang
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TinhLuongTheoThang(int thang, int nam)
        {
            try
            {
                // Validation
                if (thang < 1 || thang > 12)
                {
                    TempData["ErrorMessage"] = "Tháng phải từ 1 đến 12.";
                    return RedirectToAction(nameof(Index));
                }

                if (nam < 2000 || nam > 2100)
                {
                    TempData["ErrorMessage"] = "Năm không hợp lệ.";
                    return RedirectToAction(nameof(Index));
                }

                var luongTheoThang = await _luongNhanVienService.TinhLuongTheoThangAsync(thang, nam);
                TempData["SuccessMessage"] = $"Đã tính lương thành công cho tháng {thang}/{nam}.";
                return View("Index", luongTheoThang);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tính lương theo tháng {Thang}/{Nam}", thang, nam);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tính lương nhân viên.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: LuongNhanVien/ThangHienTai
        public async Task<IActionResult> ThangHienTai()
        {
            try
            {
                var luongThangHienTai = await _luongNhanVienService.TinhLuongThangHienTaiAsync();
                TempData["SuccessMessage"] = "Đã tải lương tháng hiện tại.";
                return View("Index", luongThangHienTai);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải lương tháng hiện tại");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải lương tháng hiện tại.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
