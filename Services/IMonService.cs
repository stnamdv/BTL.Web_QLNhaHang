using BTL.Web.Models;

namespace BTL.Web.Services
{
    public interface IMonService
    {
        Task<IEnumerable<Mon>> GetAllAsync();
        Task<PagedResult<Mon>> GetAllPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, string? loaiMon = null);
        Task<Mon?> GetByIdAsync(int id);
        Task<MonWithCongThuc?> GetWithCongThucAsync(int id);
        Task<Mon> CreateAsync(Mon mon, IEnumerable<CongThuc>? congThucs = null);
        Task<Mon> UpdateAsync(Mon mon, IEnumerable<CongThuc>? congThucs = null);
        Task<bool> DeleteAsync(int id);

        // Additional methods for enhanced functionality
        Task<bool> ExistsByIdAsync(int id);
        Task<bool> ExistsByMaMonAsync(string maMon, int? excludeId = null);
        Task<(bool can_delete, string message)> CanDeleteAsync(int id);
        Task<IEnumerable<Mon>> GetByLoaiMonAsync(string loaiMon);
        Task<PagedResult<Mon>> GetByLoaiMonPagedAsync(string loaiMon, int pageNumber, int pageSize);
        Task<IEnumerable<Mon>> SearchAsync(string searchTerm);
        Task<PagedResult<Mon>> SearchPagedAsync(string searchTerm, int pageNumber, int pageSize);
        Task<IEnumerable<dynamic>> GetStatsByLoaiAsync();
        Task<IEnumerable<NguyenLieu>> GetAllNguyenLieuAsync();
        Task<string> GenerateMaMonAsync(string loaiMon);
        Task<bool> ValidateMaMonAsync(string maMon, string loaiMon, int? excludeId = null);
    }
}