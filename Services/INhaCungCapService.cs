using BTL.Web.Models;

namespace BTL.Web.Services
{
    public interface INhaCungCapService
    {
        Task<IEnumerable<NhaCungCap>> GetAllAsync();
        Task<PagedResult<NhaCungCap>> GetAllPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<NhaCungCap?> GetByIdAsync(int id);
        Task<NhaCungCap> CreateAsync(NhaCungCap nhaCungCap);
        Task<NhaCungCap> UpdateAsync(NhaCungCap nhaCungCap);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByIdAsync(int id);
        Task<bool> ExistsByTenAsync(string ten, int? excludeId = null);
        Task<(bool can_delete, string message)> CanDeleteAsync(int id);
        Task<IEnumerable<NhaCungCap>> SearchAsync(string searchTerm);
        Task<PagedResult<NhaCungCap>> SearchPagedAsync(string searchTerm, int pageNumber, int pageSize);
        Task<IEnumerable<dynamic>> GetStatsAsync(int? nccId = null);
        Task<IEnumerable<NguyenLieuWithNhaCungCap>> GetNguyenLieuByNhaCungCapAsync(int nccId);
    }
}
