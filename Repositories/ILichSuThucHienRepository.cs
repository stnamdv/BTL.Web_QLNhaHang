using BTL.Web.Models;

namespace BTL.Web.Repositories
{
    public interface ILichSuThucHienRepository
    {
        Task<PagedResult<LichSuThucHienWithDetails>> GetAllPagedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<LichSuThucHienWithDetails>> GetByOrderItemAsync(int orderItemId);
        Task<IEnumerable<LichSuThucHienWithDetails>> GetByBuocAsync(int buocId);
        Task<IEnumerable<LichSuThucHienWithDetails>> GetByNhanVienAsync(int nvId);
        Task<LichSuThucHien> CreateAsync(LichSuThucHien lichSu);
        Task<bool> BatDauAsync(int lichSuId);
        Task<bool> HoanThanhAsync(int lichSuId, string? ghiChu = null);
        Task<IEnumerable<dynamic>> GetTrangThaiAsync(int orderItemId);
        Task<IEnumerable<ThongKeHieuSuat>> GetThongKeAsync(int? nvId = null, int? buocId = null, DateTime? tuNgay = null, DateTime? denNgay = null);
        Task<bool> ExistsByIdAsync(int id);
    }
}
