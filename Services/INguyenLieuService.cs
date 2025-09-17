using BTL.Web.Models;

namespace BTL.Web.Services
{
    public interface INguyenLieuService
    {
        Task<IEnumerable<NguyenLieu>> GetAllAsync();
        Task<PagedResult<NguyenLieu>> GetAllPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<NguyenLieu?> GetByIdAsync(int id);
        Task<NguyenLieuWithMon?> GetWithMonAsync(int id);
        Task<NguyenLieu> CreateAsync(NguyenLieu nguyenLieu);
        Task<NguyenLieu> UpdateAsync(NguyenLieu nguyenLieu);
        Task<bool> DeleteAsync(int id);

        // Additional methods for enhanced functionality
        Task<bool> ExistsByIdAsync(int id);
        Task<bool> ExistsByTenAsync(string ten, int? excludeId = null);
        Task<(bool can_delete, string message)> CanDeleteAsync(int id);
        Task<IEnumerable<NguyenLieu>> SearchAsync(string searchTerm);
        Task<PagedResult<NguyenLieu>> SearchPagedAsync(string searchTerm, int pageNumber, int pageSize);
        Task<IEnumerable<NguyenLieu>> SearchByCriteriaAsync(string? searchTerm, string? donVi, string? nguonGoc, int pageNumber, int pageSize);
        Task<IEnumerable<dynamic>> GetStatsAsync(int? nlId = null);
        Task<(bool isValid, string errorMessage)> ValidateNguonGocAsync(string nguonGoc);
        Task<IEnumerable<string>> GetDistinctDonViAsync();
    }
}
