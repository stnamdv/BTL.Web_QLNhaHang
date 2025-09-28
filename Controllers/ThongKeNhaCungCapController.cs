using BTL.Web.Models;
using BTL.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTL.Web.Controllers
{
    public class ThongKeNhaCungCapController : Controller
    {
        private readonly IThongKeNhaCungCapService _thongKeNhaCungCapService;

        public ThongKeNhaCungCapController(IThongKeNhaCungCapService thongKeNhaCungCapService)
        {
            _thongKeNhaCungCapService = thongKeNhaCungCapService;
        }

        // GET: ThongKeNhaCungCap
        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy thống kê tháng hiện tại
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var request = new ThongKeNhaCungCapRequest
                {
                    Thang = currentMonth,
                    Nam = currentYear
                };

                var thongKe = await _thongKeNhaCungCapService.GetThongKeNhaCungCapNguyenLieuAsync(request);
                var danhSachNhaCungCap = await _thongKeNhaCungCapService.GetDanhSachNhaCungCapAsync();

                ViewBag.DanhSachNhaCungCap = danhSachNhaCungCap;
                ViewBag.ThangHienTai = currentMonth;
                ViewBag.NamHienTai = currentYear;

                return View(thongKe);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Lỗi khi tải dữ liệu: {ex.Message}";
                return View(new List<ThongKeNhaCungCapNguyenLieu>());
            }
        }

        // POST: ThongKeNhaCungCap/Index
        [HttpPost]
        public async Task<IActionResult> Index(ThongKeNhaCungCapRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var danhSachNhaCungCap = await _thongKeNhaCungCapService.GetDanhSachNhaCungCapAsync();
                    ViewBag.DanhSachNhaCungCap = danhSachNhaCungCap;
                    ViewBag.ThangHienTai = request.Thang ?? DateTime.Now.Month;
                    ViewBag.NamHienTai = request.Nam ?? DateTime.Now.Year;
                    ViewBag.ErrorMessage = "Dữ liệu không hợp lệ";
                    return View(new List<ThongKeNhaCungCapNguyenLieu>());
                }

                var thongKe = await _thongKeNhaCungCapService.GetThongKeNhaCungCapNguyenLieuAsync(request);
                var danhSachNhaCungCapFilter = await _thongKeNhaCungCapService.GetDanhSachNhaCungCapAsync();

                ViewBag.DanhSachNhaCungCap = danhSachNhaCungCapFilter;
                ViewBag.ThangHienTai = request.Thang ?? DateTime.Now.Month;
                ViewBag.NamHienTai = request.Nam ?? DateTime.Now.Year;
                ViewBag.NccIdSelected = request.NccId;
                ViewBag.StoredProcedure = "sp_ThongKe_NhaCungCapNguyenLieu";
                ViewBag.StoredProcedureDescription = "Stored procedure thống kê nhà cung cấp theo số lượng nguyên liệu sử dụng";
                ViewBag.ExecutionTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                ViewBag.SuccessMessage = $"Thống kê thành công! Tìm thấy {thongKe?.Count ?? 0} nhà cung cấp.";

                return View(thongKe);
            }
            catch (Exception ex)
            {
                var danhSachNhaCungCap = await _thongKeNhaCungCapService.GetDanhSachNhaCungCapAsync();
                ViewBag.DanhSachNhaCungCap = danhSachNhaCungCap;
                ViewBag.ThangHienTai = request.Thang ?? DateTime.Now.Month;
                ViewBag.NamHienTai = request.Nam ?? DateTime.Now.Year;
                ViewBag.ErrorMessage = $"Lỗi khi lọc dữ liệu: {ex.Message}";
                ViewBag.StoredProcedure = "sp_ThongKe_NhaCungCapNguyenLieu";
                ViewBag.StoredProcedureDescription = "Stored procedure thống kê nhà cung cấp theo số lượng nguyên liệu sử dụng";
                ViewBag.ExecutionTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                return View(new List<ThongKeNhaCungCapNguyenLieu>());
            }
        }

        // GET: ThongKeNhaCungCap/ChiTiet/{id}
        public async Task<IActionResult> ChiTiet(int id, int? thang = null, int? nam = null)
        {
            try
            {
                var request = new ThongKeNhaCungCapRequest
                {
                    NccId = id,
                    Thang = thang ?? DateTime.Now.Month,
                    Nam = nam ?? DateTime.Now.Year
                };

                var thongKe = await _thongKeNhaCungCapService.GetThongKeNhaCungCapNguyenLieuAsync(request);
                var chiTiet = await _thongKeNhaCungCapService.GetChiTietNhaCungCapNguyenLieuAsync(request);
                var tongChi = await _thongKeNhaCungCapService.GetTongChiNhaCungCapAsync(request);
                var danhSachNhaCungCap = await _thongKeNhaCungCapService.GetDanhSachNhaCungCapAsync();

                var nhaCungCap = danhSachNhaCungCap.FirstOrDefault(ncc => ncc.ncc_id == id);
                if (nhaCungCap == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy nhà cung cấp";
                    return RedirectToAction("Index");
                }

                ViewBag.NhaCungCap = nhaCungCap;
                ViewBag.ThangHienTai = request.Thang;
                ViewBag.NamHienTai = request.Nam;

                var model = new
                {
                    ThongKe = thongKe.FirstOrDefault(),
                    ChiTiet = chiTiet,
                    TongChi = tongChi
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải chi tiết: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: ThongKeNhaCungCap/SoSanh
        public async Task<IActionResult> SoSanh()
        {
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;
                var previousYear = currentMonth == 1 ? currentYear - 1 : currentYear;

                var danhSachNhaCungCap = await _thongKeNhaCungCapService.GetDanhSachNhaCungCapAsync();

                ViewBag.DanhSachNhaCungCap = danhSachNhaCungCap;
                ViewBag.Thang1HienTai = previousMonth;
                ViewBag.Nam1HienTai = previousYear;
                ViewBag.Thang2HienTai = currentMonth;
                ViewBag.Nam2HienTai = currentYear;

                return View(new List<SoSanhThongKeNhaCungCap>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu: {ex.Message}";
                return View(new List<SoSanhThongKeNhaCungCap>());
            }
        }

        // POST: ThongKeNhaCungCap/SoSanh
        [HttpPost]
        public async Task<IActionResult> SoSanh(SoSanhThangRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var danhSachNhaCungCap = await _thongKeNhaCungCapService.GetDanhSachNhaCungCapAsync();
                    ViewBag.DanhSachNhaCungCap = danhSachNhaCungCap;
                    ViewBag.Thang1HienTai = request.Thang1;
                    ViewBag.Nam1HienTai = request.Nam1;
                    ViewBag.Thang2HienTai = request.Thang2;
                    ViewBag.Nam2HienTai = request.Nam2;
                    return View(new List<SoSanhThongKeNhaCungCap>());
                }

                var soSanh = await _thongKeNhaCungCapService.SoSanhThongKeNhaCungCapThangAsync(request);
                var danhSachNhaCungCapFilter = await _thongKeNhaCungCapService.GetDanhSachNhaCungCapAsync();

                ViewBag.DanhSachNhaCungCap = danhSachNhaCungCapFilter;
                ViewBag.Thang1HienTai = request.Thang1;
                ViewBag.Nam1HienTai = request.Nam1;
                ViewBag.Thang2HienTai = request.Thang2;
                ViewBag.Nam2HienTai = request.Nam2;
                ViewBag.NccIdSelected = request.NccId;

                return View(soSanh);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi so sánh dữ liệu: {ex.Message}";
                var danhSachNhaCungCap = await _thongKeNhaCungCapService.GetDanhSachNhaCungCapAsync();
                ViewBag.DanhSachNhaCungCap = danhSachNhaCungCap;
                ViewBag.Thang1HienTai = request.Thang1;
                ViewBag.Nam1HienTai = request.Nam1;
                ViewBag.Thang2HienTai = request.Thang2;
                ViewBag.Nam2HienTai = request.Nam2;
                return View(new List<SoSanhThongKeNhaCungCap>());
            }
        }

        // GET: ThongKeNhaCungCap/TopNguyenLieu
        public async Task<IActionResult> TopNguyenLieu()
        {
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var request = new TopNguyenLieuRequest
                {
                    Thang = currentMonth,
                    Nam = currentYear,
                    TopN = 5
                };

                var topNguyenLieu = await _thongKeNhaCungCapService.GetTopNguyenLieuNhaCungCapAsync(request);
                var danhSachNhaCungCap = await _thongKeNhaCungCapService.GetDanhSachNhaCungCapAsync();

                ViewBag.DanhSachNhaCungCap = danhSachNhaCungCap;
                ViewBag.ThangHienTai = currentMonth;
                ViewBag.NamHienTai = currentYear;
                ViewBag.TopNHienTai = 5;

                return View(topNguyenLieu);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu: {ex.Message}";
                return View(new List<TopNguyenLieuNhaCungCap>());
            }
        }

        // POST: ThongKeNhaCungCap/TopNguyenLieu
        [HttpPost]
        public async Task<IActionResult> TopNguyenLieu(TopNguyenLieuRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var danhSachNhaCungCap = await _thongKeNhaCungCapService.GetDanhSachNhaCungCapAsync();
                    ViewBag.DanhSachNhaCungCap = danhSachNhaCungCap;
                    ViewBag.ThangHienTai = request.Thang ?? DateTime.Now.Month;
                    ViewBag.NamHienTai = request.Nam ?? DateTime.Now.Year;
                    ViewBag.TopNHienTai = request.TopN ?? 5;
                    return View(new List<TopNguyenLieuNhaCungCap>());
                }

                var topNguyenLieu = await _thongKeNhaCungCapService.GetTopNguyenLieuNhaCungCapAsync(request);
                var danhSachNhaCungCapFilter = await _thongKeNhaCungCapService.GetDanhSachNhaCungCapAsync();

                ViewBag.DanhSachNhaCungCap = danhSachNhaCungCapFilter;
                ViewBag.ThangHienTai = request.Thang ?? DateTime.Now.Month;
                ViewBag.NamHienTai = request.Nam ?? DateTime.Now.Year;
                ViewBag.TopNHienTai = request.TopN ?? 5;
                ViewBag.NccIdSelected = request.NccId;

                return View(topNguyenLieu);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi lọc dữ liệu: {ex.Message}";
                var danhSachNhaCungCap = await _thongKeNhaCungCapService.GetDanhSachNhaCungCapAsync();
                ViewBag.DanhSachNhaCungCap = danhSachNhaCungCap;
                ViewBag.ThangHienTai = request.Thang ?? DateTime.Now.Month;
                ViewBag.NamHienTai = request.Nam ?? DateTime.Now.Year;
                ViewBag.TopNHienTai = request.TopN ?? 5;
                return View(new List<TopNguyenLieuNhaCungCap>());
            }
        }

        // GET: ThongKeNhaCungCap/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var tongHop = await _thongKeNhaCungCapService.GetThongKeTongHopAsync(currentMonth, currentYear);

                ViewBag.ThangHienTai = currentMonth;
                ViewBag.NamHienTai = currentYear;

                return View(tongHop);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải dashboard: {ex.Message}";
                return View(new ThongKeNhaCungCapTongHop());
            }
        }

        // POST: ThongKeNhaCungCap/Dashboard
        [HttpPost]
        public async Task<IActionResult> Dashboard(int? thang, int? nam)
        {
            try
            {
                var tongHop = await _thongKeNhaCungCapService.GetThongKeTongHopAsync(thang, nam);

                ViewBag.ThangHienTai = thang ?? DateTime.Now.Month;
                ViewBag.NamHienTai = nam ?? DateTime.Now.Year;

                return View(tongHop);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải dashboard: {ex.Message}";
                return View(new ThongKeNhaCungCapTongHop());
            }
        }
    }
}
