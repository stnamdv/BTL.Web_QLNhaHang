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

        // GET: BuocXuLy/GetAll
        public async Task<IActionResult> GetAll()
        {
            var buocXuLys = await _buocXuLyService.GetAllAsync();
            return Json(buocXuLys);
        }

        // GET: BuocXuLy/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BuocXuLy/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BuocXuLy buocXuLy)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _buocXuLyService.CreateAsync(buocXuLy);
                    TempData["SuccessMessage"] = "Tạo bước xử lý thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi tạo bước xử lý: {ex.Message}";
                }
            }
            return View(buocXuLy);
        }

        // GET: BuocXuLy/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var buocXuLy = await _buocXuLyService.GetByIdAsync(id);
            if (buocXuLy == null)
            {
                return NotFound();
            }
            return View(buocXuLy);
        }

        // POST: BuocXuLy/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BuocXuLy buocXuLy)
        {
            if (id != buocXuLy.buoc_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _buocXuLyService.UpdateAsync(buocXuLy);
                    TempData["SuccessMessage"] = "Cập nhật bước xử lý thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi cập nhật bước xử lý: {ex.Message}";
                }
            }
            return View(buocXuLy);
        }

        // GET: BuocXuLy/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var buocXuLy = await _buocXuLyService.GetByIdAsync(id);
            if (buocXuLy == null)
            {
                return NotFound();
            }
            return View(buocXuLy);
        }

        // POST: BuocXuLy/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _buocXuLyService.DeleteAsync(id);

                if (result)
                {
                    TempData["SuccessMessage"] = "Xóa bước xử lý thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa bước xử lý này.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa bước xử lý: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
