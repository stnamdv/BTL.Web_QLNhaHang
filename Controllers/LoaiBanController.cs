using Microsoft.AspNetCore.Mvc;
using BTL.Web.Models;
using BTL.Web.Services;

namespace BTL.Web.Controllers
{
    public class LoaiBanController : Controller
    {
        private readonly ILoaiBanService _loaiBanService;

        public LoaiBanController(ILoaiBanService loaiBanService)
        {
            _loaiBanService = loaiBanService;
        }

        // GET: LoaiBan
        public async Task<IActionResult> Index()
        {
            var loaiBans = await _loaiBanService.GetAllAsync();
            return View(loaiBans);
        }

        // GET: LoaiBan/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var loaiBanDetails = await _loaiBanService.GetDetailsWithUsageAsync(id);
                if (loaiBanDetails == null)
                {
                    return NotFound();
                }

                // Lấy danh sách bàn thuộc loại này
                var tables = await _loaiBanService.GetTablesAsync(id);

                ViewBag.Tables = tables;
                return View(loaiBanDetails);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: LoaiBan/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LoaiBan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("suc_chua,so_luong")] LoaiBan loaiBan)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem đã tồn tại loại bàn với sức chứa này chưa
                    var exists = await _loaiBanService.ExistsByCapacityAsync(loaiBan.suc_chua);

                    // Tạo loại bàn mới (sẽ merge nếu đã tồn tại)
                    var createdLoaiBan = await _loaiBanService.CreateAsync(loaiBan);

                    if (exists)
                    {
                        TempData["Message"] = $"Đã cộng thêm {loaiBan.so_luong} bàn vào loại bàn có sức chứa {loaiBan.suc_chua} người.";
                    }
                    else
                    {
                        TempData["Message"] = $"Đã tạo mới loại bàn có sức chứa {loaiBan.suc_chua} người với số lượng {loaiBan.so_luong} bàn.";
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                }
            }
            return View(loaiBan);
        }

        // GET: LoaiBan/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var loaiBan = await _loaiBanService.GetByIdAsync(id);
                if (loaiBan == null)
                {
                    return NotFound();
                }

                // Kiểm tra xem có thể edit không
                var updateCheck = await _loaiBanService.CanUpdateAsync(id, loaiBan.suc_chua, loaiBan.so_luong);
                if (!updateCheck.can_update)
                {
                    TempData["Error"] = updateCheck.error_message;
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.UpdateCheck = updateCheck;
                return View(loaiBan);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: LoaiBan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("loai_ban_id,suc_chua,so_luong")] LoaiBan loaiBan)
        {
            if (id != loaiBan.loai_ban_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _loaiBanService.UpdateAsync(loaiBan);
                    TempData["Message"] = "Cập nhật loại bàn thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                }
            }
            return View(loaiBan);
        }

        // GET: LoaiBan/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var loaiBan = await _loaiBanService.GetByIdAsync(id);
                if (loaiBan == null)
                {
                    return NotFound();
                }

                // Kiểm tra xem có thể xóa không
                var deleteCheck = await _loaiBanService.CanDeleteAsync(id);
                if (!deleteCheck.can_delete)
                {
                    TempData["Error"] = deleteCheck.error_message;
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.DeleteCheck = deleteCheck;
                return View(loaiBan);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: LoaiBan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _loaiBanService.DeleteAsync(id);
                TempData["Message"] = "Xóa loại bàn thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
