using BTL.Web.Models;

namespace BTL.Web.Repositories
{
    public interface ILoaiNhanVienRepository
    {
        Task<IEnumerable<LoaiNhanVien>> GetAllAsync();
        Task<LoaiNhanVien?> GetByTypeAsync(string loaiNv);
        Task<LoaiNhanVien> CreateAsync(LoaiNhanVien loaiNhanVien);
        Task<LoaiNhanVien> UpdateAsync(LoaiNhanVien loaiNhanVien);
        Task<bool> DeleteAsync(string loaiNv);
        Task<bool> ExistsByTypeAsync(string loaiNv);
        Task<LoaiNhanVienDetails?> GetDetailsWithUsageAsync(string loaiNv);
        Task<IEnumerable<NhanVienInfo>> GetEmployeesAsync(string loaiNv);
        Task<LoaiNhanVienUpdateCheck> CanUpdateAsync(string loaiNv, decimal newLuongCoBan);
        Task<LoaiNhanVienDeleteCheck> CanDeleteAsync(string loaiNv);
        Task<IEnumerable<LoaiNhanVien>> GetAvailableAsync();
        Task<LoaiNhanVienValidation> ValidateTypeAsync(string loaiNv);
    }
}
