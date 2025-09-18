using BTL.Web.Models;
using BTL.Web.Repositories;

namespace BTL.Web.Services
{
    public class LichSuThucHienService : ILichSuThucHienService
    {
        private readonly ILichSuThucHienRepository _repository;
        private readonly IBuocXuLyService _buocXuLyService;
        private readonly IPhanCongBuocXuLyService _phanCongService;

        public LichSuThucHienService(
            ILichSuThucHienRepository repository,
            IBuocXuLyService buocXuLyService,
            IPhanCongBuocXuLyService phanCongService)
        {
            _repository = repository;
            _buocXuLyService = buocXuLyService;
            _phanCongService = phanCongService;
        }

        public async Task<PagedResult<LichSuThucHienWithDetails>> GetAllPagedAsync(int pageNumber, int pageSize)
        {
            return await _repository.GetAllPagedAsync(pageNumber, pageSize);
        }

        public async Task<IEnumerable<LichSuThucHienWithDetails>> GetByOrderAsync(int orderId)
        {
            return await _repository.GetByOrderAsync(orderId);
        }

        public async Task<IEnumerable<LichSuThucHienWithDetails>> GetByBuocAsync(int buocId)
        {
            return await _repository.GetByBuocAsync(buocId);
        }

        public async Task<IEnumerable<LichSuThucHienWithDetails>> GetByNhanVienAsync(int nvId)
        {
            return await _repository.GetByNhanVienAsync(nvId);
        }

        public async Task<LichSuThucHien> CreateAsync(LichSuThucHien lichSu)
        {
            return await _repository.CreateAsync(lichSu);
        }

        public async Task<bool> BatDauAsync(int lichSuId)
        {
            return await _repository.BatDauAsync(lichSuId);
        }

        public async Task<bool> HoanThanhAsync(int lichSuId, string? ghiChu = null)
        {
            return await _repository.HoanThanhAsync(lichSuId, ghiChu);
        }

        public async Task<IEnumerable<dynamic>> GetTrangThaiAsync(int orderId)
        {
            return await _repository.GetTrangThaiAsync(orderId);
        }

        public async Task<IEnumerable<ThongKeHieuSuat>> GetThongKeAsync(int? nvId = null, int? buocId = null, DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            return await _repository.GetThongKeAsync(nvId, buocId, tuNgay, denNgay);
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            return await _repository.ExistsByIdAsync(id);
        }

        // Business logic methods
        public async Task<IEnumerable<dynamic>> GetNhanVienChoBuocAsync(int buocId)
        {
            return await _phanCongService.GetNhanVienChoBuocAsync(buocId);
        }

        public async Task<IEnumerable<LichSuThucHienWithDetails>> GetDangXuLyAsync()
        {
            var allLichSu = await _repository.GetAllPagedAsync(1, 1000);
            return allLichSu.Items.Where(x => x.trang_thai == TrangThaiThucHien.DANG_THUC_HIEN);
        }

        public async Task<IEnumerable<LichSuThucHienWithDetails>> GetChoXuLyAsync(int nvId)
        {
            var allLichSu = await _repository.GetAllPagedAsync(1, 1000);
            return allLichSu.Items.Where(x => x.nv_id == nvId && x.trang_thai == TrangThaiThucHien.CHUA_BAT_DAU);
        }

        public async Task<bool> UpdateStepStatusAsync(int orderId, int stepId, int employeeId, string action)
        {
            try
            {
                Console.WriteLine($"=== UpdateStepStatusAsync called ===");
                Console.WriteLine($"OrderId: {orderId}, StepId: {stepId}, EmployeeId: {employeeId}, Action: {action}");

                // Sử dụng stored procedure để có hiệu suất tốt hơn và tính nguyên tử
                var result = await _repository.UpdateStepStatusForOrderAsync(orderId, stepId, employeeId, action);

                Console.WriteLine($"UpdateStepStatusAsync result: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateStepStatusAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> CheckAndUpdateOrderStatusAsync(int orderId)
        {
            try
            {
                Console.WriteLine($"=== CheckAndUpdateOrderStatusAsync called ===");
                Console.WriteLine($"OrderId: {orderId}");

                var result = await _repository.CheckAndUpdateOrderStatusAsync(orderId);

                Console.WriteLine($"CheckAndUpdateOrderStatusAsync result: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckAndUpdateOrderStatusAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
