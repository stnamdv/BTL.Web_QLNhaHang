using BTL.Web.Models;
using BTL.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTL.Web.Controllers
{
    public class TanSuatDatBanController : Controller
    {
        private readonly ITanSuatDatBanService _tanSuatDatBanService;
        private readonly ILogger<TanSuatDatBanController> _logger;

        public TanSuatDatBanController(ITanSuatDatBanService tanSuatDatBanService, ILogger<TanSuatDatBanController> logger)
        {
            _tanSuatDatBanService = tanSuatDatBanService;
            _logger = logger;
        }

        // GET: TanSuatDatBan
        public async Task<IActionResult> Index()
        {
            var searchModel = new TanSuatDatBanSearchModel();
            ViewBag.DanhSachLoaiBan = await _tanSuatDatBanService.GetDanhSachLoaiBanAsync();
            return View(searchModel);
        }

        // POST: TanSuatDatBan/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TanSuatDatBanSearchModel model)
        {
            try
            {
                ViewBag.StoredProcedure = "sp_BanAn_TanSuatBan";
                ViewBag.StoredProcedureDescription = "Stored procedure thống kê tần suất đặt bàn theo sức chứa";
                ViewBag.ExecutionTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                ViewBag.DanhSachLoaiBan = await _tanSuatDatBanService.GetDanhSachLoaiBanAsync();

                var tanSuatDatBan = await _tanSuatDatBanService.GetTanSuatDatBanAsync(model.SucChua, model.NgayTrongThang);
                ViewBag.SuccessMessage = $"Thống kê thành công. Tìm thấy {tanSuatDatBan?.Count ?? 0} bản ghi.";
                ViewBag.TanSuatDatBan = tanSuatDatBan;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thực hiện thống kê tần suất đặt bàn");
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi thực hiện thống kê: " + ex.Message;
                ViewBag.StoredProcedure = "sp_BanAn_TanSuatBan";
                ViewBag.StoredProcedureDescription = "Stored procedure thống kê tần suất đặt bàn theo sức chứa";
                ViewBag.ExecutionTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                ViewBag.DanhSachLoaiBan = await _tanSuatDatBanService.GetDanhSachLoaiBanAsync();
                return View(model);
            }
        }
    }
}
