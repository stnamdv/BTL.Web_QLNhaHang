using BTL.Web.Models;

namespace BTL.Web.Repositories
{
    public interface INhanVienRepository
    {
        Task<IEnumerable<NhanVienWithLoaiNhanVien>> GetAllAsync();
        Task<PagedResult<NhanVienWithLoaiNhanVien>> GetAllPagedAsync(int pageNumber, int pageSize);
        Task<NhanVien?> GetByIdAsync(int id);
        Task<IEnumerable<NhanVienWithLoaiNhanVien>> GetByLoaiNvAsync(string loaiNv);
        Task<PagedResult<NhanVienWithLoaiNhanVien>> GetByLoaiNvPagedAsync(string loaiNv, int pageNumber, int pageSize);
        Task<NhanVien> CreateAsync(NhanVien nhanVien);
        Task<NhanVien> UpdateAsync(NhanVien nhanVien);
        Task<bool> DeleteAsync(int id);

        // Additional methods for enhanced functionality
        Task<NhanVien?> GetDetailsWithLoaiNhanVienAsync(int id);
        Task<bool> ExistsByIdAsync(int id);
        Task<(bool can_delete, string message)> CanDeleteAsync(int id);
        Task<IEnumerable<NhanVienWithLoaiNhanVien>> GetActiveAsync();
        Task<PagedResult<NhanVienWithLoaiNhanVien>> GetActivePagedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<NhanVienWithLoaiNhanVien>> SearchByNameAsync(string searchTerm);
        Task<PagedResult<NhanVienWithLoaiNhanVien>> SearchByNamePagedAsync(string searchTerm, int pageNumber, int pageSize);
        Task<IEnumerable<dynamic>> GetStatsByLoaiAsync();
        Task<NhanVienBulkResult> BulkUpsertAsync(IEnumerable<NhanVienExcelData> nhanVienData);
    }
}
