using BTL.Web.Models;

namespace BTL.Web.Repositories
{
    public interface IBuocXuLyRepository
    {
        Task<IEnumerable<BuocXuLy>> GetAllAsync();
        Task<BuocXuLy?> GetByIdAsync(int id);
        Task<BuocXuLy?> GetByThuTuAsync(int thuTu);
        Task<BuocXuLy?> GetBuocTiepTheoAsync(int buocId);
        Task<BuocXuLy?> GetBuocDauTienAsync();
        Task<BuocXuLy> CreateAsync(BuocXuLy buocXuLy);
        Task<BuocXuLy> UpdateAsync(BuocXuLy buocXuLy);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
