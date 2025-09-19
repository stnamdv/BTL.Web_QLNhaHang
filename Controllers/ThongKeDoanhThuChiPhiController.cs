using Microsoft.AspNetCore.Mvc;
using BTL.Web.Models;
using BTL.Web.Services;

namespace BTL.Web.Controllers
{
    public class ThongKeDoanhThuChiPhiController : Controller
    {
        private readonly IThongKeDoanhThuChiPhiService _thongKeDoanhThuChiPhiService;
        private readonly ILogger<ThongKeDoanhThuChiPhiController> _logger;

        public ThongKeDoanhThuChiPhiController(
            IThongKeDoanhThuChiPhiService thongKeDoanhThuChiPhiService,
            ILogger<ThongKeDoanhThuChiPhiController> logger)
        {
            _thongKeDoanhThuChiPhiService = thongKeDoanhThuChiPhiService;
            _logger = logger;
        }

        // GET: ThongKeDoanhThuChiPhi
        public IActionResult Index()
        {
            var searchModel = new ThongKeDoanhThuChiPhiSearchModel
            {
                NgayCuThe = DateTime.Today,
                LoaiThongKe = "ngay_hien_tai"
            };
            return View(searchModel);
        }

        // POST: ThongKeDoanhThuChiPhi/Search
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(ThongKeDoanhThuChiPhiSearchModel model)
        {
            try
            {
                switch (model.LoaiThongKe)
                {
                    case "ngay":
                        if (model.NgayCuThe.HasValue)
                        {
                            var (tongQuan, doanhThuTheoLoaiMon, chiPhiTheoNguyenLieu, chiPhiLuongTheoLoaiNhanVien) =
                                await _thongKeDoanhThuChiPhiService.GetThongKeTheoNgayAsync(model.NgayCuThe.Value);

                            ViewBag.TongQuan = tongQuan;
                            ViewBag.DoanhThuTheoLoaiMon = doanhThuTheoLoaiMon;
                            ViewBag.ChiPhiTheoNguyenLieu = chiPhiTheoNguyenLieu;
                            ViewBag.ChiPhiLuongTheoLoaiNhanVien = chiPhiLuongTheoLoaiNhanVien;
                            ViewBag.LoaiThongKe = "ngay";
                        }
                        break;

                    case "khoang_thoi_gian":
                        if (model.TuNgay.HasValue && model.DenNgay.HasValue)
                        {
                            var (tongQuan, theoNgay) = await _thongKeDoanhThuChiPhiService
                                .GetThongKeTheoKhoangThoiGianAsync(model.TuNgay.Value, model.DenNgay.Value);

                            ViewBag.TongQuan = tongQuan;
                            ViewBag.TheoNgay = theoNgay;
                            ViewBag.LoaiThongKe = "khoang_thoi_gian";
                        }
                        break;

                    case "ngay_hien_tai":
                        var (tongQuanHienTai, doanhThuTheoLoaiMonHienTai, chiPhiTheoNguyenLieuHienTai, chiPhiLuongTheoLoaiNhanVienHienTai) =
                            await _thongKeDoanhThuChiPhiService.GetThongKeNgayHienTaiAsync();

                        ViewBag.TongQuan = tongQuanHienTai;
                        ViewBag.DoanhThuTheoLoaiMon = doanhThuTheoLoaiMonHienTai;
                        ViewBag.ChiPhiTheoNguyenLieu = chiPhiTheoNguyenLieuHienTai;
                        ViewBag.ChiPhiLuongTheoLoaiNhanVien = chiPhiLuongTheoLoaiNhanVienHienTai;
                        ViewBag.LoaiThongKe = "ngay_hien_tai";
                        break;
                }

                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thực hiện thống kê doanh thu chi phí");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thực hiện thống kê: " + ex.Message;
                return View("Index", model);
            }
        }

        // GET: ThongKeDoanhThuChiPhi/NgayHienTai
        public async Task<IActionResult> NgayHienTai()
        {
            try
            {
                var (tongQuan, doanhThuTheoLoaiMon, chiPhiTheoNguyenLieu, chiPhiLuongTheoLoaiNhanVien) =
                    await _thongKeDoanhThuChiPhiService.GetThongKeNgayHienTaiAsync();

                ViewBag.TongQuan = tongQuan;
                ViewBag.DoanhThuTheoLoaiMon = doanhThuTheoLoaiMon;
                ViewBag.ChiPhiTheoNguyenLieu = chiPhiTheoNguyenLieu;
                ViewBag.ChiPhiLuongTheoLoaiNhanVien = chiPhiLuongTheoLoaiNhanVien;
                ViewBag.LoaiThongKe = "ngay_hien_tai";

                var searchModel = new ThongKeDoanhThuChiPhiSearchModel
                {
                    NgayCuThe = DateTime.Today,
                    LoaiThongKe = "ngay_hien_tai"
                };

                return View("Index", searchModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê ngày hiện tại");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lấy thống kê ngày hiện tại: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: ThongKeDoanhThuChiPhi/TheoNgay/{ngay}
        public async Task<IActionResult> TheoNgay(DateTime ngay)
        {
            try
            {
                var (tongQuan, doanhThuTheoLoaiMon, chiPhiTheoNguyenLieu, chiPhiLuongTheoLoaiNhanVien) =
                    await _thongKeDoanhThuChiPhiService.GetThongKeTheoNgayAsync(ngay);

                ViewBag.TongQuan = tongQuan;
                ViewBag.DoanhThuTheoLoaiMon = doanhThuTheoLoaiMon;
                ViewBag.ChiPhiTheoNguyenLieu = chiPhiTheoNguyenLieu;
                ViewBag.ChiPhiLuongTheoLoaiNhanVien = chiPhiLuongTheoLoaiNhanVien;
                ViewBag.LoaiThongKe = "ngay";

                var searchModel = new ThongKeDoanhThuChiPhiSearchModel
                {
                    NgayCuThe = ngay,
                    LoaiThongKe = "ngay"
                };

                return View("Index", searchModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê theo ngày {Ngay}", ngay);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lấy thống kê theo ngày: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: ThongKeDoanhThuChiPhi/TheoKhoangThoiGian
        public async Task<IActionResult> TheoKhoangThoiGian(DateTime tuNgay, DateTime denNgay)
        {
            try
            {
                var (tongQuan, theoNgay) = await _thongKeDoanhThuChiPhiService
                    .GetThongKeTheoKhoangThoiGianAsync(tuNgay, denNgay);

                ViewBag.TongQuan = tongQuan;
                ViewBag.TheoNgay = theoNgay;
                ViewBag.LoaiThongKe = "khoang_thoi_gian";

                var searchModel = new ThongKeDoanhThuChiPhiSearchModel
                {
                    TuNgay = tuNgay,
                    DenNgay = denNgay,
                    LoaiThongKe = "khoang_thoi_gian"
                };

                return View("Index", searchModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê theo khoảng thời gian {TuNgay} - {DenNgay}", tuNgay, denNgay);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lấy thống kê theo khoảng thời gian: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: ThongKeDoanhThuChiPhi/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var dashboard = await _thongKeDoanhThuChiPhiService.GetDashboardThongKeAsync();
                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy dashboard thống kê");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lấy dashboard: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
