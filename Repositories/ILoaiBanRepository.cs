using BTL.Web.Models;

namespace BTL.Web.Repositories
{
    public interface ILoaiBanRepository
    {
        Task<IEnumerable<LoaiBan>> GetAllAsync();
        Task<LoaiBan?> GetByIdAsync(int id);
        Task<LoaiBan> CreateAsync(LoaiBan loaiBan);
        Task<LoaiBan> UpdateAsync(LoaiBan loaiBan);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByCapacityAsync(int sucChua);
        Task<LoaiBanDetails?> GetDetailsWithUsageAsync(int id);
        Task<IEnumerable<BanAnInfo>> GetTablesAsync(int loaiBanId);
        Task<LoaiBanUpdateCheck> CanUpdateAsync(int id, int newSucChua, int newSoLuong);
        Task<LoaiBanDeleteCheck> CanDeleteAsync(int id);
    }
}
