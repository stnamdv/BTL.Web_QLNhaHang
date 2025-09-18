using BTL.Web.Models;
using System.Linq;
using BTL.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BTL.Web.Controllers
{
    public class NhaCungCapController : Controller
    {
        private readonly INhaCungCapService _nhaCungCapService;

        public NhaCungCapController(INhaCungCapService nhaCungCapService)
        {
            _nhaCungCapService = nhaCungCapService;
        }

        // GET: NhaCungCap
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null)
        {
            try
            {
                var result = await _nhaCungCapService.GetAllPagedAsync(page, pageSize, search);
                ViewBag.SearchTerm = search;
                return View(result);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải danh sách nhà cung cấp: {ex.Message}";
                return View(new PagedResult<NhaCungCap>(new List<NhaCungCap>(), 0, page, pageSize));
            }
        }

        // GET: NhaCungCap/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var nhaCungCap = await _nhaCungCapService.GetByIdAsync(id.Value);
                if (nhaCungCap == null)
                {
                    return NotFound();
                }

                // Lấy danh sách nguyên liệu cung cấp
                var nguyenLieuList = await _nhaCungCapService.GetNguyenLieuByNhaCungCapAsync(id.Value);
                ViewBag.NguyenLieuList = nguyenLieuList;

                return View(nhaCungCap);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải thông tin nhà cung cấp: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: NhaCungCap/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NhaCungCap/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ten,dia_chi,sdt")] NhaCungCap nhaCungCap)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _nhaCungCapService.CreateAsync(nhaCungCap);
                    TempData["Message"] = "Thêm nhà cung cấp thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Lỗi khi thêm nhà cung cấp: {ex.Message}";
                }
            }

            return View(nhaCungCap);
        }

        // GET: NhaCungCap/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var nhaCungCap = await _nhaCungCapService.GetByIdAsync(id.Value);
                if (nhaCungCap == null)
                {
                    return NotFound();
                }

                return View(nhaCungCap);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải thông tin nhà cung cấp: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: NhaCungCap/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ncc_id,ten,dia_chi,sdt")] NhaCungCap nhaCungCap)
        {
            if (id != nhaCungCap.ncc_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _nhaCungCapService.UpdateAsync(nhaCungCap);
                    TempData["Message"] = "Cập nhật nhà cung cấp thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Lỗi khi cập nhật nhà cung cấp: {ex.Message}";
                }
            }

            return View(nhaCungCap);
        }

        // GET: NhaCungCap/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var nhaCungCap = await _nhaCungCapService.GetByIdAsync(id.Value);
                if (nhaCungCap == null)
                {
                    return NotFound();
                }

                var (canDelete, message) = await _nhaCungCapService.CanDeleteAsync(id.Value);
                ViewBag.CanDelete = canDelete;
                ViewBag.DeleteMessage = message;

                return View(nhaCungCap);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải thông tin nhà cung cấp: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: NhaCungCap/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _nhaCungCapService.DeleteAsync(id);
                if (success)
                {
                    TempData["Message"] = "Xóa nhà cung cấp thành công!";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa nhà cung cấp này.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi xóa nhà cung cấp: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: NhaCungCap/Stats
        public async Task<IActionResult> Stats(int? id)
        {
            try
            {
                var stats = await _nhaCungCapService.GetStatsAsync(id);
                return View(stats);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải thống kê: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
