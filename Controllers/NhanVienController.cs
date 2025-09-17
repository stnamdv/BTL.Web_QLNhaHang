using Microsoft.AspNetCore.Mvc;
using BTL.Web.Models;
using BTL.Web.Services;
using System.Text.Json;

namespace BTL.Web.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly INhanVienService _nhanVienService;
        private readonly ILoaiNhanVienService _loaiNhanVienService;

        public NhanVienController(INhanVienService nhanVienService, ILoaiNhanVienService loaiNhanVienService)
        {
            _nhanVienService = nhanVienService;
            _loaiNhanVienService = loaiNhanVienService;
        }

        // GET: NhanVien
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            try
            {
                // Validate page parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var pagedResult = await _nhanVienService.GetAllPagedAsync(page, pageSize);

                // Pass pagination info to view
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = pagedResult.TotalPages;
                ViewBag.TotalItems = pagedResult.TotalItems;

                return View(pagedResult.Items);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return View(new List<NhanVienWithLoaiNhanVien>());
            }
        }

        // GET: NhanVien/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var nhanVienDetails = await _nhanVienService.GetDetailsWithLoaiNhanVienAsync(id);
                if (nhanVienDetails == null)
                {
                    return NotFound();
                }

                return View(nhanVienDetails);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: NhanVien/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Lấy danh sách loại nhân viên để hiển thị trong dropdown
                var loaiNhanViens = await _loaiNhanVienService.GetAllAsync();
                ViewBag.LoaiNhanViens = loaiNhanViens;
                ViewBag.TrangThaiOptions = TrangThaiNv.All;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: NhanVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ho_ten,loai_nv,ngay_vao_lam,trang_thai")] NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Tạo nhân viên mới
                    var createdNhanVien = await _nhanVienService.CreateAsync(nhanVien);
                    TempData["Message"] = $"Đã tạo mới nhân viên {nhanVien.ho_ten}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                }
            }

            // Nếu có lỗi, load lại danh sách loại nhân viên
            try
            {
                var loaiNhanViens = await _loaiNhanVienService.GetAllAsync();
                ViewBag.LoaiNhanViens = loaiNhanViens;
                ViewBag.TrangThaiOptions = TrangThaiNv.All;
            }
            catch
            {
                // Ignore error loading loai nhan viens for display
            }

            return View(nhanVien);
        }

        // GET: NhanVien/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var nhanVien = await _nhanVienService.GetByIdAsync(id);
                if (nhanVien == null)
                {
                    return NotFound();
                }

                // Lấy danh sách loại nhân viên để hiển thị trong dropdown
                var loaiNhanViens = await _loaiNhanVienService.GetAllAsync();
                ViewBag.LoaiNhanViens = loaiNhanViens;
                ViewBag.TrangThaiOptions = TrangThaiNv.All;

                return View(nhanVien);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: NhanVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("nv_id,ho_ten,loai_nv,ngay_vao_lam,trang_thai")] NhanVien nhanVien)
        {
            Console.WriteLine(JsonSerializer.Serialize(nhanVien));
            if (id != nhanVien.nv_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update nhân viên
                    var updatedNhanVien = await _nhanVienService.UpdateAsync(nhanVien);
                    TempData["Message"] = $"Đã cập nhật nhân viên {nhanVien.ho_ten}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                }
            }

            // Nếu có lỗi, load lại danh sách loại nhân viên
            try
            {
                var loaiNhanViens = await _loaiNhanVienService.GetAllAsync();
                ViewBag.LoaiNhanViens = loaiNhanViens;
                ViewBag.TrangThaiOptions = TrangThaiNv.All;
            }
            catch
            {
                // Ignore error loading loai nhan viens for display
            }

            return View(nhanVien);
        }

        // GET: NhanVien/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var nhanVien = await _nhanVienService.GetByIdAsync(id);
                if (nhanVien == null)
                {
                    return NotFound();
                }

                // Kiểm tra xem có thể xóa không
                var deleteCheck = await _nhanVienService.CanDeleteAsync(id);
                if (!deleteCheck.can_delete)
                {
                    TempData["Error"] = deleteCheck.message;
                    return RedirectToAction(nameof(Index));
                }

                return View(nhanVien);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: NhanVien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Kiểm tra xem có thể xóa không
                var deleteCheck = await _nhanVienService.CanDeleteAsync(id);
                if (!deleteCheck.can_delete)
                {
                    TempData["Error"] = deleteCheck.message;
                    return RedirectToAction(nameof(Index));
                }

                var result = await _nhanVienService.DeleteAsync(id);
                if (result)
                {
                    TempData["Message"] = "Đã xóa nhân viên thành công.";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa nhân viên.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: NhanVien/GetByLoaiNv/BEP
        public async Task<IActionResult> GetByLoaiNv(string loaiNv, int page = 1, int pageSize = 10)
        {
            try
            {
                // Validate page parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var pagedResult = await _nhanVienService.GetByLoaiNvPagedAsync(loaiNv, page, pageSize);
                var loaiNhanVien = await _loaiNhanVienService.GetByTypeAsync(loaiNv);

                ViewBag.LoaiNhanVien = loaiNhanVien;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = pagedResult.TotalPages;
                ViewBag.TotalItems = pagedResult.TotalItems;
                ViewBag.LoaiNv = loaiNv;

                return View("Index", pagedResult.Items);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: NhanVien/GetActive
        public async Task<IActionResult> GetActive(int page = 1, int pageSize = 10)
        {
            try
            {
                // Validate page parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var pagedResult = await _nhanVienService.GetActivePagedAsync(page, pageSize);

                ViewBag.Title = "Nhân viên đang hoạt động";
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = pagedResult.TotalPages;
                ViewBag.TotalItems = pagedResult.TotalItems;
                ViewBag.IsActive = true;

                return View("Index", pagedResult.Items);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: NhanVien/Search
        public async Task<IActionResult> Search(string searchTerm, int page = 1, int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return RedirectToAction(nameof(Index));
                }

                // Validate page parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var pagedResult = await _nhanVienService.SearchByNamePagedAsync(searchTerm, page, pageSize);

                ViewBag.SearchTerm = searchTerm;
                ViewBag.Title = $"Kết quả tìm kiếm: {searchTerm}";
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = pagedResult.TotalPages;
                ViewBag.TotalItems = pagedResult.TotalItems;
                ViewBag.IsSearch = true;

                return View("Index", pagedResult.Items);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: NhanVien/Stats
        public async Task<IActionResult> Stats()
        {
            try
            {
                var stats = await _nhanVienService.GetStatsByLoaiAsync();
                return View(stats);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: NhanVien/Upload
        public IActionResult Upload()
        {
            return View();
        }

        // POST: NhanVien/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn file Excel để upload");
                return View();
            }

            try
            {
                var result = await _nhanVienService.ProcessExcelUploadAsync(file);

                if (result.ErrorCount > 0)
                {
                    TempData["Warning"] = $"Upload hoàn thành với {result.ErrorCount} lỗi. " +
                                        $"Đã thêm mới: {result.InsertedCount}, Cập nhật: {result.UpdatedCount}. " +
                                        $"Chi tiết lỗi: {result.ErrorMessages}";
                }
                else
                {
                    TempData["Message"] = $"Upload thành công! Đã thêm mới: {result.InsertedCount}, Cập nhật: {result.UpdatedCount} nhân viên.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi không xác định: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
