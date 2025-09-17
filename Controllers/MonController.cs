using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BTL.Web.Models;
using BTL.Web.Services;
using System.Text.Json;

namespace BTL.Web.Controllers
{
    public class MonController : Controller
    {
        private readonly IMonService _monService;
        private readonly INguyenLieuService _nguyenLieuService;

        public MonController(IMonService monService, INguyenLieuService nguyenLieuService)
        {
            _monService = monService;
            _nguyenLieuService = nguyenLieuService;
        }

        // GET: Mon
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? searchTerm = null, string? loaiMon = null)
        {
            try
            {
                // Validate page parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var pagedResult = await _monService.GetAllPagedAsync(page, pageSize, searchTerm, loaiMon);

                // Pass pagination info to view
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = pagedResult.TotalPages;
                ViewBag.TotalItems = pagedResult.TotalItems;
                ViewBag.SearchTerm = searchTerm;
                ViewBag.LoaiMon = loaiMon;
                ViewBag.LoaiMonOptions = GetLoaiMonOptions();

                return View(pagedResult.Items);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return View(new List<Mon>());
            }
        }

        // GET: Mon/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var monWithCongThuc = await _monService.GetWithCongThucAsync(id);
                if (monWithCongThuc == null)
                {
                    return NotFound();
                }

                return View(monWithCongThuc);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Mon/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Lấy danh sách nguyên liệu để hiển thị trong dropdown
                var nguyenLieus = await _nguyenLieuService.GetAllAsync();
                ViewBag.NguyenLieus = nguyenLieus;
                ViewBag.LoaiMonOptions = GetLoaiMonOptions();

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Mon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Mon mon, string? congThucJson)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Reload data for dropdowns
                    var nguyenLieus = await _nguyenLieuService.GetAllAsync();
                    ViewBag.NguyenLieus = nguyenLieus;
                    ViewBag.LoaiMonOptions = GetLoaiMonOptions();
                    return View(mon);
                }

                // Parse công thức từ JSON
                IEnumerable<CongThuc>? congThucs = null;
                if (!string.IsNullOrWhiteSpace(congThucJson))
                {
                    try
                    {
                        var congThucData = JsonSerializer.Deserialize<List<CongThucInput>>(congThucJson);
                        if (congThucData != null && congThucData.Any())
                        {
                            congThucs = congThucData.Select(ct => new CongThuc
                            {
                                nl_id = ct.nl_id,
                                dinh_luong = ct.dinh_luong
                            });
                        }
                    }
                    catch (JsonException)
                    {
                        ModelState.AddModelError("CongThuc", "Dữ liệu công thức không hợp lệ");
                        var nguyenLieus = await _nguyenLieuService.GetAllAsync();
                        ViewBag.NguyenLieus = nguyenLieus;
                        ViewBag.LoaiMonOptions = GetLoaiMonOptions();
                        return View(mon);
                    }
                }

                await _monService.CreateAsync(mon, congThucs);
                TempData["Success"] = "Tạo món ăn thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var nguyenLieus = await _nguyenLieuService.GetAllAsync();
                ViewBag.NguyenLieus = nguyenLieus;
                ViewBag.LoaiMonOptions = GetLoaiMonOptions();
                return View(mon);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Mon/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var monWithCongThuc = await _monService.GetWithCongThucAsync(id);
                if (monWithCongThuc == null)
                {
                    return NotFound();
                }

                // Lấy danh sách nguyên liệu để hiển thị trong dropdown
                var nguyenLieus = await _nguyenLieuService.GetAllAsync();
                ViewBag.NguyenLieus = nguyenLieus;
                ViewBag.LoaiMonOptions = GetLoaiMonOptions();

                return View(monWithCongThuc);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Mon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Mon mon, string? congThucJson)
        {
            try
            {
                if (id != mon.mon_id)
                {
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    // Reload data for dropdowns
                    var nguyenLieus = await _nguyenLieuService.GetAllAsync();
                    ViewBag.NguyenLieus = nguyenLieus;
                    ViewBag.LoaiMonOptions = GetLoaiMonOptions();

                    // Reload công thức
                    var monWithCongThuc = await _monService.GetWithCongThucAsync(id);
                    return View(monWithCongThuc);
                }

                // Parse công thức từ JSON
                IEnumerable<CongThuc>? congThucs = null;
                if (!string.IsNullOrWhiteSpace(congThucJson))
                {
                    try
                    {
                        var congThucData = JsonSerializer.Deserialize<List<CongThucInput>>(congThucJson);
                        if (congThucData != null && congThucData.Any())
                        {
                            congThucs = congThucData.Select(ct => new CongThuc
                            {
                                mon_id = mon.mon_id,
                                nl_id = ct.nl_id,
                                dinh_luong = ct.dinh_luong
                            });
                        }
                    }
                    catch (JsonException)
                    {
                        ModelState.AddModelError("CongThuc", "Dữ liệu công thức không hợp lệ");
                        var nguyenLieus = await _nguyenLieuService.GetAllAsync();
                        ViewBag.NguyenLieus = nguyenLieus;
                        ViewBag.LoaiMonOptions = GetLoaiMonOptions();

                        var monWithCongThuc = await _monService.GetWithCongThucAsync(id);
                        return View(monWithCongThuc);
                    }
                }

                await _monService.UpdateAsync(mon, congThucs);
                TempData["Success"] = "Cập nhật món ăn thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var nguyenLieus = await _nguyenLieuService.GetAllAsync();
                ViewBag.NguyenLieus = nguyenLieus;
                ViewBag.LoaiMonOptions = GetLoaiMonOptions();

                var monWithCongThuc = await _monService.GetWithCongThucAsync(id);
                return View(monWithCongThuc);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Mon/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var mon = await _monService.GetByIdAsync(id);
                if (mon == null)
                {
                    return NotFound();
                }

                // Kiểm tra có thể xóa không
                var (canDelete, message) = await _monService.CanDeleteAsync(id);
                if (!canDelete)
                {
                    TempData["Error"] = message;
                    return RedirectToAction(nameof(Index));
                }

                return View(mon);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Mon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _monService.DeleteAsync(id);
                if (success)
                {
                    TempData["Success"] = "Xóa món ăn thành công!";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa món ăn này.";
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

        // GET: Mon/Stats
        public async Task<IActionResult> Stats()
        {
            try
            {
                var stats = await _monService.GetStatsByLoaiAsync();
                return View(stats);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // AJAX: Generate mã món
        [HttpPost]
        public async Task<IActionResult> GenerateMaMon(string loaiMon)
        {
            try
            {
                var maMon = await _monService.GenerateMaMonAsync(loaiMon);
                return Json(new { success = true, maMon });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX: Validate mã món
        [HttpPost]
        public async Task<IActionResult> ValidateMaMon(string maMon, string loaiMon, int? excludeId = null)
        {
            try
            {
                var isValid = await _monService.ValidateMaMonAsync(maMon, loaiMon, excludeId);
                return Json(new { success = true, isValid });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private static List<SelectListItem> GetLoaiMonOptions()
        {
            return new List<SelectListItem>
            {
                new() { Value = "KHAI_VI", Text = "Khai vị" },
                new() { Value = "MON_CHINH", Text = "Món chính" },
                new() { Value = "TRANG_MIENG", Text = "Tráng miệng" }
            };
        }
    }

    // Helper class for JSON deserialization
    public class CongThucInput
    {
        public int nl_id { get; set; }
        public decimal dinh_luong { get; set; }
    }
}
