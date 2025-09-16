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

        // GET: LoaiNhanVien/Details/BEP
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id) || !LoaiNv.All.Contains(id))
                {
                    return NotFound();
                }

                var loaiNhanVienDetails = await _loaiNhanVienService.GetDetailsWithUsageAsync(id);
                if (loaiNhanVienDetails == null)
                {
                    return NotFound();
                }

                // Lấy danh sách nhân viên thuộc loại này
                var employees = await _loaiNhanVienService.GetEmployeesAsync(id);

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
            // Tạo danh sách các loại nhân viên có thể chọn
            ViewBag.LoaiNvOptions = LoaiNv.All
                .Select(e => new { Value = e, Text = LoaiNv.GetDisplayName(e) })
                .ToList();

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
                    var exists = await _loaiNhanVienService.ExistsByTypeAsync(loaiNhanVien.loai_nv);

                    if (exists)
                    {
                        ModelState.AddModelError("loai_nv", "Loại nhân viên đã tồn tại.");
                    }
                    else
                    {
                        // Tạo loại nhân viên mới
                        var createdLoaiNhanVien = await _loaiNhanVienService.CreateAsync(loaiNhanVien);
                        TempData["Message"] = $"Đã tạo mới loại nhân viên {LoaiNv.GetDisplayName(loaiNhanVien.loai_nv)} với lương cơ bản {loaiNhanVien.luong_co_ban:N0} VNĐ.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                }
            }

            // Tạo lại danh sách các loại nhân viên có thể chọn
            ViewBag.LoaiNvOptions = LoaiNv.All
                .Select(e => new { Value = e, Text = LoaiNv.GetDisplayName(e) })
                .ToList();

            return View(loaiNhanVien);
        }

        // GET: LoaiNhanVien/Edit/BEP
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id) || !LoaiNv.All.Contains(id))
                {
                    return NotFound();
                }

                var loaiNhanVien = await _loaiNhanVienService.GetByTypeAsync(id);
                if (loaiNhanVien == null)
                {
                    return NotFound();
                }

                // Kiểm tra xem có thể edit không
                var updateCheck = await _loaiNhanVienService.CanUpdateAsync(id, loaiNhanVien.luong_co_ban);
                if (!updateCheck.can_update)
                {
                    TempData["Error"] = updateCheck.error_message;
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.UpdateCheck = updateCheck;
                ViewBag.LoaiNvDisplayName = LoaiNv.GetDisplayName(id);
                return View(loaiNhanVien);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: LoaiNhanVien/Edit/BEP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("loai_nv,luong_co_ban")] LoaiNhanVien loaiNhanVien)
        {
            if (string.IsNullOrWhiteSpace(id) || id != loaiNhanVien.loai_nv)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _loaiNhanVienService.UpdateAsync(loaiNhanVien);
                    TempData["Message"] = $"Cập nhật loại nhân viên {LoaiNv.GetDisplayName(loaiNhanVien.loai_nv)} thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                }
            }

            ViewBag.LoaiNvDisplayName = LoaiNv.GetDisplayName(id);
            return View(loaiNhanVien);
        }

        // GET: LoaiNhanVien/Delete/BEP
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id) || !LoaiNv.All.Contains(id))
                {
                    return NotFound();
                }

                var loaiNhanVien = await _loaiNhanVienService.GetByTypeAsync(id);
                if (loaiNhanVien == null)
                {
                    return NotFound();
                }

                // Kiểm tra xem có thể xóa không
                Console.WriteLine("id: " + id);
                var deleteCheck = await _loaiNhanVienService.CanDeleteAsync(id);
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(deleteCheck, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                if (!deleteCheck.can_delete)
                {
                    TempData["Error"] = deleteCheck.error_message;
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.DeleteCheck = deleteCheck;
                ViewBag.LoaiNvDisplayName = LoaiNv.GetDisplayName(id);
                return View(loaiNhanVien);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: LoaiNhanVien/Delete/BEP
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id) || !LoaiNv.All.Contains(id))
                {
                    return NotFound();
                }

                await _loaiNhanVienService.DeleteAsync(id);
                TempData["Message"] = $"Xóa loại nhân viên {LoaiNv.GetDisplayName(id)} thành công.";
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
