using BTL.Web.Models;

namespace BTL.Web.Services
{
    public interface IBuocXuLyService
    {
        Task<IEnumerable<BuocXuLy>> GetAllAsync();
        Task<BuocXuLy?> GetByIdAsync(int id);
        Task<BuocXuLy?> GetByThuTuAsync(int thuTu);
        Task<BuocXuLy?> GetBuocTiepTheoAsync(int buocId);
        Task<BuocXuLy?> GetBuocDauTienAsync();
    }
}
