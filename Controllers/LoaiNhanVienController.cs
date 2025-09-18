using Microsoft.AspNetCore.Mvc;
using BTL.Web.Models;
using BTL.Web.Services;

namespace BTL.Web.Controllers
{
    public class LoaiNhanVienController : Controller
    {
        private readonly ILoaiNhanVienService _loaiNhanVienService;

        public LoaiNhanVienController(ILoaiNhanVienService loaiNhanVienService)
        {
            _loaiNhanVienService = loaiNhanVienService;
        }

        // GET: LoaiNhanVien
        public async Task<IActionResult> Index()
        {
            try
            {
                var loaiNhanViens = await _loaiNhanVienService.GetAllAsync();
                return View(loaiNhanViens);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return View(new List<LoaiNhanVien>());
            }
        }

        // GET: LoaiNhanVien/Details/1
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound();
                }

                var loaiNhanVien = await _loaiNhanVienService.GetByIdAsync(id);
                if (loaiNhanVien == null)
                {
                    return NotFound();
                }

                var loaiNhanVienDetails = await _loaiNhanVienService.GetDetailsWithUsageAsync(loaiNhanVien.loai_nv_id);
                if (loaiNhanVienDetails == null)
                {
                    return NotFound();
                }

                // Lấy danh sách nhân viên thuộc loại này
                var employees = await _loaiNhanVienService.GetEmployeesAsync(loaiNhanVien.loai_nv_id);

                ViewBag.Employees = employees;
                return View(loaiNhanVienDetails);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: LoaiNhanVien/Create
        public IActionResult Create()
        {

            return View();
        }

        // POST: LoaiNhanVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("loai_nv,luong_co_ban")] LoaiNhanVien loaiNhanVien)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem đã tồn tại loại nhân viên này chưa                                        
                    // Tạo loại nhân viên mới
                    var createdLoaiNhanVien = await _loaiNhanVienService.CreateAsync(loaiNhanVien);
                    TempData["Message"] = $"Đã tạo mới loại nhân viên {loaiNhanVien.loai_nv} với lương cơ bản {loaiNhanVien.luong_co_ban:N0} VNĐ.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                }
            }

            return View(loaiNhanVien);
        }

        // GET: LoaiNhanVien/Edit/1
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound();
                }

                var loaiNhanVien = await _loaiNhanVienService.GetByIdAsync(id);
                if (loaiNhanVien == null)
                {
                    return NotFound();
                }

                // Kiểm tra xem có thể edit không
                var updateCheck = await _loaiNhanVienService.CanUpdateAsync(loaiNhanVien.loai_nv_id, loaiNhanVien.luong_co_ban);
                if (!updateCheck.can_update)
                {
                    TempData["Error"] = updateCheck.error_message;
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.UpdateCheck = updateCheck;
                ViewBag.LoaiNvDisplayName = loaiNhanVien.loai_nv;
                return View(loaiNhanVien);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: LoaiNhanVien/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("loai_nv_id,loai_nv,luong_co_ban")] LoaiNhanVien loaiNhanVien)
        {
            if (id != loaiNhanVien.loai_nv_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _loaiNhanVienService.UpdateAsync(loaiNhanVien);
                    TempData["Message"] = $"Cập nhật loại nhân viên {loaiNhanVien.loai_nv} thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                }
            }

            ViewBag.LoaiNvDisplayName = loaiNhanVien.loai_nv;
            return View(loaiNhanVien);
        }

        // GET: LoaiNhanVien/Delete/1
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound();
                }

                var loaiNhanVien = await _loaiNhanVienService.GetByIdAsync(id);
                if (loaiNhanVien == null)
                {
                    return NotFound();
                }

                // Kiểm tra xem có thể xóa không
                var deleteCheck = await _loaiNhanVienService.CanDeleteAsync(loaiNhanVien.loai_nv_id);
                if (!deleteCheck.can_delete)
                {
                    TempData["Error"] = deleteCheck.error_message;
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.DeleteCheck = deleteCheck;
                ViewBag.LoaiNvDisplayName = loaiNhanVien.loai_nv;
                return View(loaiNhanVien);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: LoaiNhanVien/Delete/1
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound();
                }

                var loaiNhanVien = await _loaiNhanVienService.GetByIdAsync(id);
                if (loaiNhanVien == null)
                {
                    return NotFound();
                }

                await _loaiNhanVienService.DeleteAsync(loaiNhanVien.loai_nv_id);
                TempData["Message"] = $"Xóa loại nhân viên {loaiNhanVien.loai_nv} thành công.";
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
