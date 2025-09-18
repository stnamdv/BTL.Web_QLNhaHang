using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BTL.Web.Models;
using BTL.Web.Services;

namespace BTL.Web.Controllers
{
    public class NguyenLieuController : Controller
    {
        private readonly INguyenLieuService _nguyenLieuService;
        private readonly INhaCungCapService _nhaCungCapService;

        public NguyenLieuController(INguyenLieuService nguyenLieuService, INhaCungCapService nhaCungCapService)
        {
            _nguyenLieuService = nguyenLieuService;
            _nhaCungCapService = nhaCungCapService;
        }

        private async Task LoadDropdownDataAsync()
        {
            // Lấy danh sách đơn vị để hiển thị trong dropdown
            var donVis = await _nguyenLieuService.GetDistinctDonViAsync();
            ViewBag.DonViOptions = donVis.Select(dv => new SelectListItem { Value = dv, Text = dv }).ToList();

            // Lấy danh sách nhà cung cấp để hiển thị trong dropdown
            var nhaCungCaps = await _nhaCungCapService.GetAllAsync();
            ViewBag.NhaCungCapOptions = nhaCungCaps.Select(ncc => new SelectListItem { Value = ncc.ncc_id.ToString(), Text = ncc.ten }).ToList();
        }

        // GET: NguyenLieu
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                // Validate page parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var pagedResult = await _nguyenLieuService.GetAllWithNhaCungCapPagedAsync(page, pageSize, searchTerm);

                // Pass pagination info to view
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = pagedResult.TotalPages;
                ViewBag.TotalItems = pagedResult.TotalItems;
                ViewBag.SearchTerm = searchTerm;

                return View(pagedResult.Items);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return View(new List<NguyenLieuWithNhaCungCap>());
            }
        }

        // GET: NguyenLieu/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var nguyenLieuWithNhaCungCap = await _nguyenLieuService.GetWithNhaCungCapAsync(id);
                if (nguyenLieuWithNhaCungCap == null)
                {
                    return NotFound();
                }

                return View(nguyenLieuWithNhaCungCap);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: NguyenLieu/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                await LoadDropdownDataAsync();
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: NguyenLieu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NguyenLieu nguyenLieu)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadDropdownDataAsync();
                    return View(nguyenLieu);
                }

                await _nguyenLieuService.CreateAsync(nguyenLieu);
                TempData["Success"] = "Tạo nguyên liệu thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadDropdownDataAsync();
                return View(nguyenLieu);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: NguyenLieu/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var nguyenLieu = await _nguyenLieuService.GetByIdAsync(id);
                if (nguyenLieu == null)
                {
                    return NotFound();
                }

                // Lấy danh sách đơn vị để hiển thị trong dropdown
                await LoadDropdownDataAsync();

                return View(nguyenLieu);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: NguyenLieu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NguyenLieu nguyenLieu)
        {
            try
            {
                if (id != nguyenLieu.nl_id)
                {
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    // Reload data for dropdowns
                    var donVis = await _nguyenLieuService.GetDistinctDonViAsync();
                    ViewBag.DonViOptions = donVis.Select(dv => new SelectListItem { Value = dv, Text = dv }).ToList();
                    return View(nguyenLieu);
                }

                await _nguyenLieuService.UpdateAsync(nguyenLieu);
                TempData["Success"] = "Cập nhật nguyên liệu thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadDropdownDataAsync();
                return View(nguyenLieu);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: NguyenLieu/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var nguyenLieu = await _nguyenLieuService.GetByIdAsync(id);
                if (nguyenLieu == null)
                {
                    return NotFound();
                }

                // Kiểm tra có thể xóa không
                var (canDelete, message) = await _nguyenLieuService.CanDeleteAsync(id);
                if (!canDelete)
                {
                    TempData["Error"] = message;
                    return RedirectToAction(nameof(Index));
                }

                return View(nguyenLieu);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: NguyenLieu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _nguyenLieuService.DeleteAsync(id);
                if (success)
                {
                    TempData["Success"] = "Xóa nguyên liệu thành công!";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa nguyên liệu này.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: NguyenLieu/Stats
        public async Task<IActionResult> Stats(int? id = null)
        {
            try
            {
                var stats = await _nguyenLieuService.GetStatsAsync(id);
                return View(stats);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: NguyenLieu/Search
        public async Task<IActionResult> Search(string? searchTerm, string? donVi, string? nguonGoc, int page = 1, int pageSize = 10)
        {
            try
            {
                // Validate page parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var results = await _nguyenLieuService.SearchByCriteriaAsync(searchTerm, donVi, nguonGoc, page, pageSize);

                // Lấy danh sách đơn vị để hiển thị trong dropdown
                await LoadDropdownDataAsync();
                ViewBag.SearchTerm = searchTerm;
                ViewBag.DonVi = donVi;
                ViewBag.NguonGoc = nguonGoc;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;

                return View("Index", results);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // AJAX: Validate nguồn gốc
        [HttpPost]
        public async Task<IActionResult> ValidateNguonGoc(string nguonGoc)
        {
            try
            {
                var (isValid, errorMessage) = await _nguyenLieuService.ValidateNguonGocAsync(nguonGoc);
                return Json(new { success = true, isValid, errorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
