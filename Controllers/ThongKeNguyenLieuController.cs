using Microsoft.AspNetCore.Mvc;
using BTL.Web.Models;
using BTL.Web.Services;

namespace BTL.Web.Controllers
{
    public class ThongKeNguyenLieuController : Controller
    {
        private readonly IThongKeNguyenLieuService _thongKeNguyenLieuService;
        private readonly ILogger<ThongKeNguyenLieuController> _logger;

        public ThongKeNguyenLieuController(
            IThongKeNguyenLieuService thongKeNguyenLieuService,
            ILogger<ThongKeNguyenLieuController> logger)
        {
            _thongKeNguyenLieuService = thongKeNguyenLieuService;
            _logger = logger;
        }

        // GET: ThongKeNguyenLieu
        public IActionResult Index()
        {
            var searchModel = new ThongKeNguyenLieuSearchModel
            {
                NgayCuThe = DateTime.Today,
                LoaiThongKe = "ngay_hien_tai"
            };
            return View(searchModel);
        }

        // POST: ThongKeNguyenLieu/Search
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(ThongKeNguyenLieuSearchModel model)
        {
            try
            {
                switch (model.LoaiThongKe)
                {
                    case "ngay":
                        if (model.NgayCuThe.HasValue)
                        {
                            var (chiTiet, theoDonVi, monAn) = await _thongKeNguyenLieuService
                                .GetThongKeTheoNgayAsync(model.NgayCuThe.Value);

                            ViewBag.ChiTiet = chiTiet;
                            ViewBag.TheoDonVi = theoDonVi;
                            ViewBag.MonAn = monAn;
                            ViewBag.Ngay = model.NgayCuThe.Value;
                            ViewBag.LoaiThongKe = "ngay";
                        }
                        break;

                    case "khoang_thoi_gian":
                        if (model.TuNgay.HasValue && model.DenNgay.HasValue)
                        {
                            var (tongQuan, chiTiet, theoDonVi, theoNgay) = await _thongKeNguyenLieuService
                                .GetThongKeTheoKhoangThoiGianAsync(model.TuNgay.Value, model.DenNgay.Value);

                            ViewBag.TongQuan = tongQuan;
                            ViewBag.ChiTiet = chiTiet;
                            ViewBag.TheoDonVi = theoDonVi;
                            ViewBag.TheoNgay = theoNgay;
                            ViewBag.LoaiThongKe = "khoang_thoi_gian";
                        }
                        break;

                    case "ngay_hien_tai":
                        var (chiTietHienTai, theoDonViHienTai, monAnHienTai) = await _thongKeNguyenLieuService
                            .GetThongKeNgayHienTaiAsync();

                        ViewBag.ChiTiet = chiTietHienTai;
                        ViewBag.TheoDonVi = theoDonViHienTai;
                        ViewBag.MonAn = monAnHienTai;
                        ViewBag.Ngay = DateTime.Today;
                        ViewBag.LoaiThongKe = "ngay_hien_tai";
                        break;
                }

                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thực hiện thống kê nguyên liệu");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thực hiện thống kê: " + ex.Message;
                return View("Index", model);
            }
        }

        // GET: ThongKeNguyenLieu/NgayHienTai
        public async Task<IActionResult> NgayHienTai()
        {
            try
            {
                var (chiTiet, theoDonVi, monAn) = await _thongKeNguyenLieuService
                    .GetThongKeNgayHienTaiAsync();

                ViewBag.ChiTiet = chiTiet;
                ViewBag.TheoDonVi = theoDonVi;
                ViewBag.MonAn = monAn;
                ViewBag.Ngay = DateTime.Today;
                ViewBag.LoaiThongKe = "ngay_hien_tai";

                var searchModel = new ThongKeNguyenLieuSearchModel
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

        // GET: ThongKeNguyenLieu/TheoNgay/{ngay}
        public async Task<IActionResult> TheoNgay(DateTime ngay)
        {
            try
            {
                var (chiTiet, theoDonVi, monAn) = await _thongKeNguyenLieuService
                    .GetThongKeTheoNgayAsync(ngay);

                ViewBag.ChiTiet = chiTiet;
                ViewBag.TheoDonVi = theoDonVi;
                ViewBag.MonAn = monAn;
                ViewBag.Ngay = ngay;
                ViewBag.LoaiThongKe = "ngay";

                var searchModel = new ThongKeNguyenLieuSearchModel
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

        // GET: ThongKeNguyenLieu/TheoKhoangThoiGian
        public async Task<IActionResult> TheoKhoangThoiGian(DateTime tuNgay, DateTime denNgay)
        {
            try
            {
                var (tongQuan, chiTiet, theoDonVi, theoNgay) = await _thongKeNguyenLieuService
                    .GetThongKeTheoKhoangThoiGianAsync(tuNgay, denNgay);

                ViewBag.TongQuan = tongQuan;
                ViewBag.ChiTiet = chiTiet;
                ViewBag.TheoDonVi = theoDonVi;
                ViewBag.TheoNgay = theoNgay;
                ViewBag.LoaiThongKe = "khoang_thoi_gian";

                var searchModel = new ThongKeNguyenLieuSearchModel
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
    }
}
