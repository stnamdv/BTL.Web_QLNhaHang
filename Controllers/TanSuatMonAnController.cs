using Microsoft.AspNetCore.Mvc;
using BTL.Web.Models;
using BTL.Web.Services;

namespace BTL.Web.Controllers
{
    public class TanSuatMonAnController : Controller
    {
        private readonly ITanSuatMonAnService _tanSuatMonAnService;
        private readonly ILogger<TanSuatMonAnController> _logger;

        public TanSuatMonAnController(
            ITanSuatMonAnService tanSuatMonAnService,
            ILogger<TanSuatMonAnController> logger)
        {
            _tanSuatMonAnService = tanSuatMonAnService;
            _logger = logger;
        }

        // GET: TanSuatMonAn
        public async Task<IActionResult> Index()
        {
            try
            {
                var searchModel = new TanSuatMonAnSearchModel
                {
                    Thang = DateTime.Now.Month,
                    Nam = DateTime.Now.Year
                };

                var danhSachMonAn = await _tanSuatMonAnService.GetDanhSachMonAnAsync();
                ViewBag.DanhSachMonAn = danhSachMonAn;

                return View(searchModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang tần suất món ăn");
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi tải dữ liệu: " + ex.Message;
                return View(new TanSuatMonAnSearchModel());
            }
        }

        // POST: TanSuatMonAn/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TanSuatMonAnSearchModel model)
        {
            try
            {
                ViewBag.StoredProcedure = "sp_Mon_TanSuatTheoThang";
                ViewBag.StoredProcedureDescription = "Stored procedure thống kê tần suất món ăn theo tháng";
                ViewBag.ExecutionTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                if (ModelState.IsValid)
                {
                    var tanSuatMonAn = await _tanSuatMonAnService.GetTanSuatMonAnAsync(model.MonId, model.Thang, model.Nam);
                    var danhSachMonAn = await _tanSuatMonAnService.GetDanhSachMonAnAsync();

                    ViewBag.DanhSachMonAn = danhSachMonAn;
                    ViewBag.TanSuatMonAn = tanSuatMonAn;
                    ViewBag.SuccessMessage = $"Thống kê thành công! Tìm thấy {tanSuatMonAn.Count} bản ghi tần suất món ăn.";
                }
                else
                {
                    var danhSachMonAn = await _tanSuatMonAnService.GetDanhSachMonAnAsync();
                    ViewBag.DanhSachMonAn = danhSachMonAn;
                    ViewBag.ErrorMessage = "Dữ liệu không hợp lệ";
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thực hiện thống kê tần suất món ăn");
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi thực hiện thống kê: " + ex.Message;
                ViewBag.StoredProcedure = "sp_Mon_TanSuatTheoThang";
                ViewBag.StoredProcedureDescription = "Stored procedure thống kê tần suất món ăn theo tháng";
                ViewBag.ExecutionTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                var danhSachMonAn = await _tanSuatMonAnService.GetDanhSachMonAnAsync();
                ViewBag.DanhSachMonAn = danhSachMonAn;

                return View(model);
            }
        }
    }
}
