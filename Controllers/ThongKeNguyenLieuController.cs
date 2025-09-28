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
                TuNgay = DateTime.Today,
                DenNgay = DateTime.Today
            };
            return View(searchModel);
        }

        // POST: ThongKeNguyenLieu/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ThongKeNguyenLieuSearchModel model)
        {
            try
            {
                if (model.TuNgay.HasValue && model.DenNgay.HasValue)
                {
                    // Sử dụng stored procedure duy nhất cho thống kê nguyên liệu
                    var nguyenLieuTheoNgay = await _thongKeNguyenLieuService
                        .GetThongKeNguyenLieuTheoNgayAsync(model.TuNgay.Value, model.DenNgay.Value);

                    ViewBag.NguyenLieuTheoNgay = nguyenLieuTheoNgay;
                    ViewBag.TuNgay = model.TuNgay.Value;
                    ViewBag.DenNgay = model.DenNgay.Value;
                    ViewBag.StoredProcedure = "sp_ThongKe_NhaCungCapNguyenLieu_TongChi_TheoNgay";
                    ViewBag.StoredProcedureDescription = "Stored procedure thống kê nguyên liệu theo nhà cung cấp và tổng chi phí theo ngày";
                    ViewBag.ExecutionTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    ViewBag.SuccessMessage = $"Thống kê thành công! Tìm thấy {nguyenLieuTheoNgay?.Count ?? 0} loại nguyên liệu.";
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thực hiện thống kê nguyên liệu");
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi thực hiện thống kê: " + ex.Message;
                ViewBag.StoredProcedure = "sp_ThongKe_NhaCungCapNguyenLieu_TongChi_TheoNgay";
                ViewBag.StoredProcedureDescription = "Stored procedure thống kê nguyên liệu theo nhà cung cấp và tổng chi phí theo ngày";
                ViewBag.ExecutionTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                return View(model);
            }
        }
    }
}