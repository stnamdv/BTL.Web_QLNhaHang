using BTL.Web.Models;

namespace BTL.Web.Repositories
{
    public interface ILichSuThucHienRepository
    {
        Task<PagedResult<LichSuThucHienWithDetails>> GetAllPagedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<LichSuThucHienWithDetails>> GetByOrderAsync(int orderId);
        Task<IEnumerable<LichSuThucHienWithDetails>> GetByBuocAsync(int buocId);
        Task<IEnumerable<LichSuThucHienWithDetails>> GetByNhanVienAsync(int nvId);
        Task<LichSuThucHien> CreateAsync(LichSuThucHien lichSu);
        Task<bool> BatDauAsync(int lichSuId);
        Task<bool> HoanThanhAsync(int lichSuId, string? ghiChu = null);
        Task<IEnumerable<dynamic>> GetTrangThaiAsync(int orderId);
        Task<IEnumerable<ThongKeHieuSuat>> GetThongKeAsync(int? nvId = null, int? buocId = null, DateTime? tuNgay = null, DateTime? denNgay = null);
        Task<bool> ExistsByIdAsync(int id);
        Task<IEnumerable<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId);
        Task<bool> StartStepForOrderAsync(int orderId, int stepId, int employeeId);
        Task<bool> CompleteStepForOrderAsync(int orderId, int stepId, int employeeId);
        Task<bool> UpdateStepStatusForOrderAsync(int orderId, int stepId, int employeeId, string action);
        Task<bool> CheckAndUpdateOrderStatusAsync(int orderId);
    }
}
