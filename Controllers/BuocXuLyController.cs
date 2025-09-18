using BTL.Web.Models;
using BTL.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTL.Web.Controllers
{
    public class BuocXuLyController : Controller
    {
        private readonly IBuocXuLyService _buocXuLyService;

        public BuocXuLyController(IBuocXuLyService buocXuLyService)
        {
            _buocXuLyService = buocXuLyService;
        }

        // GET: BuocXuLy
        public async Task<IActionResult> Index()
        {
            var buocXuLys = await _buocXuLyService.GetAllAsync();
            return View(buocXuLys);
        }

        // GET: BuocXuLy/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var buocXuLy = await _buocXuLyService.GetByIdAsync(id);
            if (buocXuLy == null)
            {
                return NotFound();
            }

            return View(buocXuLy);
        }

        // GET: BuocXuLy/GetByThuTu/1
        public async Task<IActionResult> GetByThuTu(int thuTu)
        {
            var buocXuLy = await _buocXuLyService.GetByThuTuAsync(thuTu);
            if (buocXuLy == null)
            {
                return NotFound();
            }

            return View("Details", buocXuLy);
        }

        // GET: BuocXuLy/GetBuocTiepTheo/1
        public async Task<IActionResult> GetBuocTiepTheo(int buocId)
        {
            var buocTiepTheo = await _buocXuLyService.GetBuocTiepTheoAsync(buocId);
            if (buocTiepTheo == null)
            {
                return Json(new { success = false, message = "Không có bước tiếp theo" });
            }

            return Json(new { success = true, data = buocTiepTheo });
        }

        // GET: BuocXuLy/GetBuocDauTien
        public async Task<IActionResult> GetBuocDauTien()
        {
            var buocDauTien = await _buocXuLyService.GetBuocDauTienAsync();
            if (buocDauTien == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bước đầu tiên" });
            }

            return Json(new { success = true, data = buocDauTien });
        }
    }
}
