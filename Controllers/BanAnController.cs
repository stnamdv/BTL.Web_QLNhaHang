using Microsoft.AspNetCore.Mvc;
using BTL.Web.Models;
using BTL.Web.Services;

namespace BTL.Web.Controllers
{
    public class BanAnController : Controller
    {
        private readonly IBanAnService _banAnService;
        private readonly ILoaiBanService _loaiBanService;
        private readonly ILayoutService _layoutService;

        public BanAnController(IBanAnService banAnService, ILoaiBanService loaiBanService, ILayoutService layoutService)
        {
            _banAnService = banAnService;
            _loaiBanService = loaiBanService;
            _layoutService = layoutService;
        }

        // GET: BanAn
        public async Task<IActionResult> Index()
        {
            try
            {
                var banAns = await _banAnService.GetAllAsync();
                return View(banAns);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return View(new List<BanAnWithLoaiBan>());
            }
        }

        // GET: BanAn/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var banAnDetails = await _banAnService.GetDetailsWithUsageAsync(id);
                if (banAnDetails == null)
                {
                    return NotFound();
                }

                // Lấy danh sách đơn hàng của bàn này
                var orders = await _banAnService.GetOrdersAsync(id);

                ViewBag.Orders = orders;
                return View(banAnDetails);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: BanAn/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Lấy danh sách loại bàn để hiển thị trong dropdown
                var loaiBans = await _loaiBanService.GetAllAsync();

                // Lấy số lượng bàn đã được tạo cho mỗi loại bàn
                var banCounts = new Dictionary<int, int>();
                foreach (var loaiBan in loaiBans)
                {
                    var count = await _banAnService.GetCountByLoaiBanIdAsync(loaiBan.loai_ban_id);
                    banCounts[loaiBan.loai_ban_id] = count;
                }

                ViewBag.LoaiBans = loaiBans;
                ViewBag.BanCounts = banCounts;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: BanAn/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("loai_ban_id,so_hieu")] BanAn banAn)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem số hiệu bàn đã tồn tại chưa
                    var exists = await _banAnService.ExistsBySoHieuAsync(banAn.so_hieu);

                    if (exists)
                    {
                        ModelState.AddModelError("so_hieu", "Số hiệu bàn đã tồn tại.");
                    }
                    else
                    {
                        // Tạo bàn mới
                        var createdBanAn = await _banAnService.CreateAsync(banAn);
                        TempData["Message"] = $"Đã tạo mới bàn với số hiệu {banAn.so_hieu}.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                }
            }

            // Nếu có lỗi, load lại danh sách loại bàn
            try
            {
                var loaiBans = await _loaiBanService.GetAllAsync();

                // Lấy số lượng bàn đã được tạo cho mỗi loại bàn
                var banCounts = new Dictionary<int, int>();
                foreach (var loaiBan in loaiBans)
                {
                    var count = await _banAnService.GetCountByLoaiBanIdAsync(loaiBan.loai_ban_id);
                    banCounts[loaiBan.loai_ban_id] = count;
                }

                ViewBag.LoaiBans = loaiBans;
                ViewBag.BanCounts = banCounts;
            }
            catch
            {
                // Ignore error loading loai bans for display
            }

            return View(banAn);
        }

        // GET: BanAn/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var banAn = await _banAnService.GetByIdAsync(id);
                if (banAn == null)
                {
                    return NotFound();
                }

                // Kiểm tra xem có thể edit không
                var updateCheck = await _banAnService.CanUpdateAsync(id, banAn.loai_ban_id, banAn.so_hieu);
                if (!updateCheck.can_update)
                {
                    TempData["Error"] = updateCheck.message;
                    return RedirectToAction(nameof(Index));
                }

                // Lấy danh sách loại bàn để hiển thị trong dropdown
                var loaiBans = await _loaiBanService.GetAllAsync();
                ViewBag.LoaiBans = loaiBans;

                return View(banAn);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: BanAn/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ban_id,loai_ban_id,so_hieu")] BanAn banAn)
        {
            if (id != banAn.ban_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem có thể update không
                    var updateCheck = await _banAnService.CanUpdateAsync(id, banAn.loai_ban_id, banAn.so_hieu);
                    if (!updateCheck.can_update)
                    {
                        ModelState.AddModelError("", updateCheck.message);
                    }
                    else
                    {
                        // Kiểm tra xem số hiệu bàn đã tồn tại chưa (trừ bàn hiện tại)
                        var exists = await _banAnService.ExistsBySoHieuAsync(banAn.so_hieu, id);

                        if (exists)
                        {
                            ModelState.AddModelError("so_hieu", "Số hiệu bàn đã tồn tại.");
                        }
                        else
                        {
                            // Update bàn
                            var updatedBanAn = await _banAnService.UpdateAsync(banAn);
                            TempData["Message"] = $"Đã cập nhật bàn với số hiệu {banAn.so_hieu}.";
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                }
            }

            // Nếu có lỗi, load lại danh sách loại bàn
            try
            {
                var loaiBans = await _loaiBanService.GetAllAsync();
                ViewBag.LoaiBans = loaiBans;
            }
            catch
            {
                // Ignore error loading loai bans for display
            }

            return View(banAn);
        }

        // GET: BanAn/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var banAn = await _banAnService.GetByIdAsync(id);
                if (banAn == null)
                {
                    return NotFound();
                }

                // Kiểm tra xem có thể xóa không
                var deleteCheck = await _banAnService.CanDeleteAsync(id);
                if (!deleteCheck.can_delete)
                {
                    TempData["Error"] = deleteCheck.message;
                    return RedirectToAction(nameof(Index));
                }

                return View(banAn);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: BanAn/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Kiểm tra xem có thể xóa không
                var deleteCheck = await _banAnService.CanDeleteAsync(id);
                if (!deleteCheck.can_delete)
                {
                    TempData["Error"] = deleteCheck.message;
                    return RedirectToAction(nameof(Index));
                }

                var result = await _banAnService.DeleteAsync(id);
                if (result)
                {
                    TempData["Message"] = "Đã xóa bàn thành công.";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa bàn.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: BanAn/GetByLoaiBan/5
        public async Task<IActionResult> GetByLoaiBan(int loaiBanId)
        {
            try
            {
                var banAns = await _banAnService.GetByLoaiBanIdAsync(loaiBanId);
                var loaiBan = await _loaiBanService.GetByIdAsync(loaiBanId);

                ViewBag.LoaiBan = loaiBan;
                return View("Index", banAns);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: BanAn/GetAvailable
        public async Task<IActionResult> GetAvailable(int? capacity = null)
        {
            try
            {
                var banAns = await _banAnService.GetAvailableAsync(capacity);
                ViewBag.Capacity = capacity;
                return View("Available", banAns);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: BanAn/Layout
        public async Task<IActionResult> Layout(string? layoutName = null)
        {
            try
            {
                var banAns = await _banAnService.GetAllAsync();
                var loaiBans = await _loaiBanService.GetAllAsync();
                var layouts = await _layoutService.GetLayoutListAsync();

                // Load layout hiện tại
                LayoutResponse? currentLayout = null;
                if (!string.IsNullOrEmpty(layoutName))
                {
                    currentLayout = await _layoutService.GetLayoutAsync(layoutName);
                }
                else if (layouts.Any())
                {
                    // Load layout đầu tiên nếu không có layoutName
                    currentLayout = await _layoutService.GetLayoutAsync();
                }

                ViewBag.LoaiBans = loaiBans;
                ViewBag.Layouts = layouts;
                ViewBag.CurrentLayout = currentLayout;
                return View(banAns);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return View(new List<BanAnWithLoaiBan>());
            }
        }

        // POST: BanAn/SaveLayout
        [HttpPost]
        public async Task<IActionResult> SaveLayout([FromBody] LayoutSaveRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var layoutId = await _layoutService.SaveLayoutAsync(request);
                return Json(new { success = true, message = "Layout đã được lưu thành công!", layoutId = layoutId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: BanAn/LoadLayout
        [HttpGet]
        public async Task<IActionResult> LoadLayout(string layoutName)
        {
            try
            {
                var layout = await _layoutService.GetLayoutAsync(layoutName);
                if (layout == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy layout" });
                }

                return Json(new { success = true, layout = layout });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: BanAn/GetLayoutList
        [HttpGet]
        public async Task<IActionResult> GetLayoutList()
        {
            try
            {
                var layouts = await _layoutService.GetLayoutListAsync();
                return Json(new { success = true, layouts = layouts });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: BanAn/DeleteLayout
        [HttpPost]
        public async Task<IActionResult> DeleteLayout(int layoutId)
        {
            try
            {
                var result = await _layoutService.DeleteLayoutAsync(layoutId);
                if (result)
                {
                    return Json(new { success = true, message = "Layout đã được xóa thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể xóa layout" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}
