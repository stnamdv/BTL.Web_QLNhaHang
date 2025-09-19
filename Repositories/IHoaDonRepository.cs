using BTL.Web.Models;

namespace BTL.Web.Repositories
{
    public interface IHoaDonRepository
    {
        Task<HoaDon?> GetHoaDonByOrderIdAsync(int orderId);
        Task<HoaDon> CreateHoaDonAsync(int orderId, string? phuongThuc = null);
        Task<HoaDon?> GetHoaDonByIdAsync(int hdId);
        Task<IEnumerable<HoaDon>> GetAllHoaDonsAsync();
        Task<(HoaDon? HoaDon, OrderWithDetails? OrderDetails)> GetHoaDonWithOrderDetailsAsync(int orderId);
    }
}
