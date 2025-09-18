using BTL.Web.Models;

namespace BTL.Web.Services
{
    public interface ILoaiNhanVienService
    {
        Task<IEnumerable<LoaiNhanVien>> GetAllAsync();
        Task<LoaiNhanVien?> GetByIdAsync(int id);
        Task<LoaiNhanVien> CreateAsync(LoaiNhanVien loaiNhanVien);
        Task<LoaiNhanVien> UpdateAsync(LoaiNhanVien loaiNhanVien);
        Task<bool> DeleteAsync(int loaiNvId);
        Task<LoaiNhanVienDetails?> GetDetailsWithUsageAsync(int loaiNvId);
        Task<IEnumerable<NhanVienInfo>> GetEmployeesAsync(int loaiNvId);
        Task<LoaiNhanVienUpdateCheck> CanUpdateAsync(int loaiNvId, decimal newLuongCoBan);
        Task<LoaiNhanVienDeleteCheck> CanDeleteAsync(int loaiNvId);
        Task<IEnumerable<LoaiNhanVien>> GetAvailableAsync();
    }
}
