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
    }
}
