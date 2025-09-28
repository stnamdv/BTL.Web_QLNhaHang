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
                TuNgay = DateTime.Today,
                DenNgay = DateTime.Today,
                LoaiThongKe = "khoang_thoi_gian"
            };
            return View(searchModel);
        }

        // POST: ThongKeDoanhThuChiPhi/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ThongKeDoanhThuChiPhiSearchModel model)
        {
            try
            {
                ViewBag.StoredProcedure = "sp_ThongKe_DoanhThuChiPhiTheoKhoangThoiGian";
                ViewBag.StoredProcedureDescription = "Stored procedure thống kê doanh thu, chi phí và lợi nhuận theo khoảng thời gian";
                ViewBag.ExecutionTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                if (model.TuNgay.HasValue && model.DenNgay.HasValue)
                {
                    var (tongQuan, theoNgay) = await _thongKeDoanhThuChiPhiService
                        .GetThongKeTheoKhoangThoiGianAsync(model.TuNgay.Value, model.DenNgay.Value);

                    ViewBag.TongQuan = tongQuan;
                    ViewBag.TheoNgay = theoNgay;
                    ViewBag.LoaiThongKe = "khoang_thoi_gian";
                    ViewBag.SuccessMessage = $"Thống kê thành công từ {model.TuNgay.Value:dd/MM/yyyy} đến {model.DenNgay.Value:dd/MM/yyyy}";
                }
                else
                {
                    ViewBag.ErrorMessage = "Vui lòng chọn từ ngày và đến ngày";
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thực hiện thống kê doanh thu chi phí");
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi thực hiện thống kê: " + ex.Message;
                ViewBag.StoredProcedure = "sp_ThongKe_DoanhThuChiPhiTheoKhoangThoiGian";
                ViewBag.StoredProcedureDescription = "Stored procedure thống kê doanh thu, chi phí và lợi nhuận theo khoảng thời gian";
                ViewBag.ExecutionTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                return View(model);
            }
        }

        // GET: ThongKeDoanhThuChiPhi/NgayHienTai
        public async Task<IActionResult> NgayHienTai()
        {
            try
            {
                var (tongQuan, doanhThuTheoLoaiMon, chiPhiTheoNguyenLieu) =
                    await _thongKeDoanhThuChiPhiService.GetThongKeNgayHienTaiAsync();

                ViewBag.TongQuan = tongQuan;
                ViewBag.DoanhThuTheoLoaiMon = doanhThuTheoLoaiMon;
                ViewBag.ChiPhiTheoNguyenLieu = chiPhiTheoNguyenLieu;
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
                var (tongQuan, doanhThuTheoLoaiMon, chiPhiTheoNguyenLieu) =
                    await _thongKeDoanhThuChiPhiService.GetThongKeTheoNgayAsync(ngay);

                ViewBag.TongQuan = tongQuan;
                ViewBag.DoanhThuTheoLoaiMon = doanhThuTheoLoaiMon;
                ViewBag.ChiPhiTheoNguyenLieu = chiPhiTheoNguyenLieu;
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
