using BTL.Web.Models;

namespace BTL.Web.Services
{
    public interface IBanAnService
    {
        Task<IEnumerable<BanAnWithLoaiBan>> GetAllAsync();
        Task<BanAn?> GetByIdAsync(int id);
        Task<IEnumerable<BanAnWithLoaiBan>> GetByLoaiBanIdAsync(int loaiBanId);
        Task<BanAn> CreateAsync(BanAn banAn);
        Task<BanAn> UpdateAsync(BanAn banAn);
        Task<bool> DeleteAsync(int id);

        // Additional methods for enhanced functionality
        Task<BanAn?> GetDetailsWithUsageAsync(int id);
        Task<IEnumerable<Order>> GetOrdersAsync(int banId);
        Task<bool> ExistsBySoHieuAsync(string soHieu, int? excludeId = null);
        Task<(bool can_update, string message)> CanUpdateAsync(int id, int loaiBanId, string soHieu);
        Task<(bool can_delete, string message)> CanDeleteAsync(int id);
        Task<IEnumerable<BanAnWithLoaiBan>> GetAvailableAsync(int? capacity = null);
        Task<int> GetCountByLoaiBanIdAsync(int loaiBanId);
        Task<PagedResult<BanAnWithLoaiBan>> SearchPagedAsync(string? searchTerm, int? loaiBanId, int? capacity, int page, int pageSize);
        Task<IEnumerable<dynamic>> GetTableStatusAsync();
    }
}
