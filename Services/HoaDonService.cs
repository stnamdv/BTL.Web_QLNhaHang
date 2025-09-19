using BTL.Web.Models;
using BTL.Web.Repositories;

namespace BTL.Web.Services
{
    public class HoaDonService : IHoaDonService
    {
        private readonly IHoaDonRepository _hoaDonRepository;

        public HoaDonService(IHoaDonRepository hoaDonRepository)
        {
            _hoaDonRepository = hoaDonRepository;
        }

        public async Task<HoaDon?> GetHoaDonByOrderIdAsync(int orderId)
        {
            return await _hoaDonRepository.GetHoaDonByOrderIdAsync(orderId);
        }

        public async Task<HoaDon> CreateHoaDonAsync(int orderId, string? phuongThuc = null)
        {
            return await _hoaDonRepository.CreateHoaDonAsync(orderId, phuongThuc);
        }

        public async Task<HoaDon?> GetHoaDonByIdAsync(int hdId)
        {
            return await _hoaDonRepository.GetHoaDonByIdAsync(hdId);
        }

        public async Task<IEnumerable<HoaDon>> GetAllHoaDonsAsync()
        {
            return await _hoaDonRepository.GetAllHoaDonsAsync();
        }

        public async Task<(HoaDon? HoaDon, OrderWithDetails? OrderDetails)> GetHoaDonWithOrderDetailsAsync(int orderId)
        {
            return await _hoaDonRepository.GetHoaDonWithOrderDetailsAsync(orderId);
        }
    }
}
