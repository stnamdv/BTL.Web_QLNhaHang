using BTL.Web.Models;
using BTL.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTL.Web.Controllers
{
    public class LichSuThucHienController : Controller
    {
        private readonly ILichSuThucHienService _lichSuService;

        public LichSuThucHienController(ILichSuThucHienService lichSuService)
        {
            _lichSuService = lichSuService;
        }

        // GET: LichSuThucHien
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var result = await _lichSuService.GetAllPagedAsync(pageNumber, pageSize);
            return View(result);
        }

        // GET: LichSuThucHien/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var result = await _lichSuService.GetAllPagedAsync(1, 1000);
            var lichSu = result.Items.FirstOrDefault(x => x.lich_su_id == id);
            if (lichSu == null)
            {
                return NotFound();
            }

            return View(lichSu);
        }

        // GET: LichSuThucHien/GetByOrder/1
        [HttpGet("LichSuThucHien/GetByOrder/{orderId}")]
        public async Task<IActionResult> GetByOrder(int orderId)
        {
            var lichSus = await _lichSuService.GetByOrderAsync(orderId);
            return Json(new { success = true, data = lichSus });
        }

        // GET: LichSuThucHien/GetByBuoc/1
        public async Task<IActionResult> GetByBuoc(int buocId)
        {
            var lichSus = await _lichSuService.GetByBuocAsync(buocId);
            return Json(new { success = true, data = lichSus });
        }

        // GET: LichSuThucHien/GetByNhanVien/1
        public async Task<IActionResult> GetByNhanVien(int nvId)
        {
            var lichSus = await _lichSuService.GetByNhanVienAsync(nvId);
            return Json(new { success = true, data = lichSus });
        }

        // GET: LichSuThucHien/GetDangXuLy
        public async Task<IActionResult> GetDangXuLy()
        {
            var lichSus = await _lichSuService.GetDangXuLyAsync();
            return Json(new { success = true, data = lichSus });
        }

        // GET: LichSuThucHien/GetChoXuLy/1
        public async Task<IActionResult> GetChoXuLy(int nvId)
        {
            var lichSus = await _lichSuService.GetChoXuLyAsync(nvId);
            return Json(new { success = true, data = lichSus });
        }

        // GET: LichSuThucHien/GetTrangThai/1
        public async Task<IActionResult> GetTrangThai(int orderId)
        {
            var trangThai = await _lichSuService.GetTrangThaiAsync(orderId);
            return Json(new { success = true, data = trangThai });
        }

        // GET: LichSuThucHien/ThongKe
        public async Task<IActionResult> ThongKe(int? nvId = null, int? buocId = null, DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            var thongKe = await _lichSuService.GetThongKeAsync(nvId, buocId, tuNgay, denNgay);
            return View(thongKe);
        }

        // GET: LichSuThucHien/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LichSuThucHien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LichSuThucHien lichSu)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _lichSuService.CreateAsync(lichSu);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(lichSu);
        }

        // POST: LichSuThucHien/BatDau/5
        [HttpPost]
        public async Task<IActionResult> BatDau(int lichSuId)
        {
            try
            {
                var result = await _lichSuService.BatDauAsync(lichSuId);
                if (result)
                {
                    return Json(new { success = true, message = "Bắt đầu thực hiện thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể bắt đầu thực hiện" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: LichSuThucHien/HoanThanh/5
        [HttpPost]
        public async Task<IActionResult> HoanThanh(int lichSuId, string? ghiChu = null)
        {
            try
            {
                var result = await _lichSuService.HoanThanhAsync(lichSuId, ghiChu);
                if (result)
                {
                    return Json(new { success = true, message = "Hoàn thành thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể hoàn thành" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: LichSuThucHien/GetNhanVienChoBuoc/1
        public async Task<IActionResult> GetNhanVienChoBuoc(int buocId)
        {
            try
            {
                var nhanViens = await _lichSuService.GetNhanVienChoBuocAsync(buocId);
                return Json(new { success = true, data = nhanViens });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        // POST: LichSuThucHien/UpdateStepStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStepStatus([FromBody] UpdateStepStatusRequest request)
        {
            try
            {
                Console.WriteLine($"=== UpdateStepStatus called ===");
                Console.WriteLine($"OrderId: {request.OrderId}, StepId: {request.StepId}, EmployeeId: {request.EmployeeId}, Action: {request.Action}");

                var result = await _lichSuService.UpdateStepStatusAsync(request.OrderId, request.StepId, request.EmployeeId, request.Action);

                if (result)
                {
                    return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể cập nhật trạng thái" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateStepStatus: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: LichSuThucHien/CheckOrderStatus
        [HttpPost]
        public async Task<IActionResult> CheckOrderStatus(int orderId)
        {
            try
            {
                Console.WriteLine($"=== CheckOrderStatus called ===");
                Console.WriteLine($"OrderId: {orderId}");

                var result = await _lichSuService.CheckAndUpdateOrderStatusAsync(orderId);

                if (result)
                {
                    return Json(new { success = true, message = "Trạng thái đơn hàng đã được kiểm tra và cập nhật" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể kiểm tra trạng thái đơn hàng" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckOrderStatus: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: LichSuThucHien/Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
