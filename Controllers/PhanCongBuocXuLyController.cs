using BTL.Web.Models;
using BTL.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTL.Web.Controllers
{
    public class PhanCongBuocXuLyController : Controller
    {
        private readonly IPhanCongBuocXuLyService _phanCongService;

        public PhanCongBuocXuLyController(IPhanCongBuocXuLyService phanCongService)
        {
            _phanCongService = phanCongService;
        }

        // GET: PhanCongBuocXuLy
        public async Task<IActionResult> Index()
        {
            var phanCongs = await _phanCongService.GetAllAsync();
            return View(phanCongs);
        }

        // GET: PhanCongBuocXuLy/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var phanCongs = await _phanCongService.GetAllAsync();
            var phanCong = phanCongs.FirstOrDefault(x => x.phan_cong_buoc_id == id);
            if (phanCong == null)
            {
                return NotFound();
            }

            return View(phanCong);
        }

        // GET: PhanCongBuocXuLy/GetByBuoc/1
        public async Task<IActionResult> GetByBuoc(int buocId)
        {
            var phanCongs = await _phanCongService.GetByBuocAsync(buocId);
            return View("Index", phanCongs);
        }

        // GET: PhanCongBuocXuLy/GetByLoaiNhanVien/1
        public async Task<IActionResult> GetByLoaiNhanVien(int loaiNvId)
        {
            var phanCongs = await _phanCongService.GetByLoaiNhanVienAsync(loaiNvId);
            return View("Index", phanCongs);
        }

        // GET: PhanCongBuocXuLy/GetNhanVienChoBuoc/1
        public async Task<IActionResult> GetNhanVienChoBuoc(int buocId)
        {
            var nhanViens = await _phanCongService.GetNhanVienChoBuocAsync(buocId);
            return Json(new { success = true, data = nhanViens });
        }

        // GET: PhanCongBuocXuLy/GetBuocChuaPhanCong
        public async Task<IActionResult> GetBuocChuaPhanCong()
        {
            Console.WriteLine("=== GetBuocChuaPhanCong called ===");
            try
            {
                var buocXuLys = await _phanCongService.GetBuocChuaPhanCongAsync();
                Console.WriteLine($"Found {buocXuLys?.Count()} bước xử lý chưa phân công");
                return Json(buocXuLys);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetBuocChuaPhanCong: {ex.Message}");
                return Json(new { error = ex.Message });
            }
        }

        // GET: PhanCongBuocXuLy/GetLoaiNhanVienChuaPhanCong/1
        public async Task<IActionResult> GetLoaiNhanVienChuaPhanCong(int buocId)
        {
            Console.WriteLine($"=== GetLoaiNhanVienChuaPhanCong called with buocId: {buocId} ===");
            try
            {
                var loaiNhanViens = await _phanCongService.GetLoaiNhanVienChuaPhanCongAsync(buocId);
                Console.WriteLine($"Found {loaiNhanViens?.Count()} loại nhân viên chưa phân công cho bước {buocId}");
                return Json(loaiNhanViens);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLoaiNhanVienChuaPhanCong: {ex.Message}");
                return Json(new { error = ex.Message });
            }
        }

        // GET: PhanCongBuocXuLy/Create
        public IActionResult Create()
        {
            Console.WriteLine("=== PhanCongBuocXuLyController.Create GET ===");
            Console.WriteLine($"Request URL: {Request.Path}");
            Console.WriteLine($"User Agent: {Request.Headers["User-Agent"]}");
            return View();
        }

        // POST: PhanCongBuocXuLy/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhanCongBuocXuLy phanCong)
        {
            Console.WriteLine("=== PhanCongBuocXuLyController.Create POST ===");
            Console.WriteLine($"Received data - BuocId: {phanCong?.buoc_id}, LoaiNvId: {phanCong?.loai_nv_id}, VaiTro: {phanCong?.vai_tro}");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== ModelState Errors ===");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Key: {error.Key}");
                    foreach (var errorMessage in error.Value.Errors)
                    {
                        Console.WriteLine($"  Error: {errorMessage.ErrorMessage}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Console.WriteLine("Calling _phanCongService.CreateAsync...");
                    var result = await _phanCongService.CreateAsync(phanCong);
                    Console.WriteLine($"CreateAsync completed successfully. Result ID: {result?.phan_cong_buoc_id}");
                    TempData["SuccessMessage"] = "Tạo phân công thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in CreateAsync: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    ModelState.AddModelError("", ex.Message);
                }
            }

            Console.WriteLine("Returning View with model...");
            return View(phanCong);
        }

        // GET: PhanCongBuocXuLy/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var phanCongs = await _phanCongService.GetAllAsync();
            var phanCong = phanCongs.FirstOrDefault(x => x.phan_cong_buoc_id == id);
            if (phanCong == null)
            {
                return NotFound();
            }

            var model = new PhanCongBuocXuLy
            {
                phan_cong_buoc_id = phanCong.phan_cong_buoc_id,
                buoc_id = phanCong.buoc_id,
                loai_nv_id = phanCong.loai_nv_id,
                vai_tro = phanCong.vai_tro,
                trang_thai = phanCong.trang_thai
            };

            return View(model);
        }

        // POST: PhanCongBuocXuLy/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PhanCongBuocXuLy phanCong)
        {
            if (id != phanCong.phan_cong_buoc_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _phanCongService.UpdateAsync(phanCong);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(phanCong);
        }

        // GET: PhanCongBuocXuLy/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var phanCongs = await _phanCongService.GetAllAsync();
            var phanCong = phanCongs.FirstOrDefault(x => x.phan_cong_buoc_id == id);
            if (phanCong == null)
            {
                return NotFound();
            }

            return View(phanCong);
        }

        // POST: PhanCongBuocXuLy/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _phanCongService.DeleteAsync(id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "Không thể xóa phân công này. Có thể đang được sử dụng trong lịch sử thực hiện.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Xóa phân công thành công.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
