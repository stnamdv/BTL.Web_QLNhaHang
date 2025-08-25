using BTL.Web.Models;
using BTL.Web.Repositories;

namespace BTL.Web.Services
{
    public class BanAnService : IBanAnService
    {
        private readonly IBanAnRepository _banAnRepository;
        private readonly ILoaiBanRepository _loaiBanRepository;

        public BanAnService(IBanAnRepository banAnRepository, ILoaiBanRepository loaiBanRepository)
        {
            _banAnRepository = banAnRepository;
            _loaiBanRepository = loaiBanRepository;
        }

        public async Task<IEnumerable<BanAnWithLoaiBan>> GetAllAsync()
        {
            return await _banAnRepository.GetAllAsync();
        }

        public async Task<BanAn?> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _banAnRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<BanAnWithLoaiBan>> GetByLoaiBanIdAsync(int loaiBanId)
        {
            if (loaiBanId <= 0)
            {
                throw new ArgumentException("ID loại bàn phải lớn hơn 0", nameof(loaiBanId));
            }

            // Kiểm tra loại bàn có tồn tại không
            var loaiBan = await _loaiBanRepository.GetByIdAsync(loaiBanId);
            if (loaiBan == null)
            {
                throw new InvalidOperationException("Loại bàn không tồn tại");
            }

            return await _banAnRepository.GetByLoaiBanIdAsync(loaiBanId);
        }

        public async Task<BanAn> CreateAsync(BanAn banAn)
        {
            if (banAn == null)
            {
                throw new ArgumentNullException(nameof(banAn));
            }

            if (string.IsNullOrWhiteSpace(banAn.so_hieu))
            {
                throw new ArgumentException("Số hiệu bàn không được để trống");
            }

            if (banAn.loai_ban_id <= 0)
            {
                throw new ArgumentException("ID loại bàn không hợp lệ");
            }

            // Kiểm tra loại bàn có tồn tại không
            var loaiBan = await _loaiBanRepository.GetByIdAsync(banAn.loai_ban_id);
            if (loaiBan == null)
            {
                throw new InvalidOperationException("Loại bàn không tồn tại");
            }

            return await _banAnRepository.CreateAsync(banAn);
        }

        public async Task<BanAn> UpdateAsync(BanAn banAn)
        {
            if (banAn == null)
            {
                throw new ArgumentNullException(nameof(banAn));
            }

            if (banAn.ban_id <= 0)
            {
                throw new ArgumentException("ID bàn không hợp lệ");
            }

            if (string.IsNullOrWhiteSpace(banAn.so_hieu))
            {
                throw new ArgumentException("Số hiệu bàn không được để trống");
            }

            if (banAn.loai_ban_id <= 0)
            {
                throw new ArgumentException("ID loại bàn không hợp lệ");
            }

            // Kiểm tra bàn có tồn tại không
            var existingBan = await _banAnRepository.GetByIdAsync(banAn.ban_id);
            if (existingBan == null)
            {
                throw new InvalidOperationException("Bàn không tồn tại");
            }

            // Kiểm tra loại bàn có tồn tại không
            var loaiBan = await _loaiBanRepository.GetByIdAsync(banAn.loai_ban_id);
            if (loaiBan == null)
            {
                throw new InvalidOperationException("Loại bàn không tồn tại");
            }

            return await _banAnRepository.UpdateAsync(banAn);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            // Kiểm tra bàn có tồn tại không
            var existingBan = await _banAnRepository.GetByIdAsync(id);
            if (existingBan == null)
            {
                throw new InvalidOperationException("Bàn không tồn tại");
            }

            // Có thể thêm logic kiểm tra ràng buộc (ví dụ: không cho phép xóa nếu bàn đang có khách)
            return await _banAnRepository.DeleteAsync(id);
        }

        public async Task<BanAn?> GetDetailsWithUsageAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _banAnRepository.GetDetailsWithUsageAsync(id);
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync(int banId)
        {
            if (banId <= 0)
            {
                throw new ArgumentException("ID bàn phải lớn hơn 0", nameof(banId));
            }

            return await _banAnRepository.GetOrdersAsync(banId);
        }

        public async Task<bool> ExistsBySoHieuAsync(string soHieu, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(soHieu))
            {
                throw new ArgumentException("Số hiệu bàn không được để trống", nameof(soHieu));
            }

            return await _banAnRepository.ExistsBySoHieuAsync(soHieu, excludeId);
        }

        public async Task<(bool can_update, string message)> CanUpdateAsync(int id, int loaiBanId, string soHieu)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            if (loaiBanId <= 0)
            {
                throw new ArgumentException("ID loại bàn phải lớn hơn 0", nameof(loaiBanId));
            }

            if (string.IsNullOrWhiteSpace(soHieu))
            {
                throw new ArgumentException("Số hiệu bàn không được để trống", nameof(soHieu));
            }

            return await _banAnRepository.CanUpdateAsync(id, loaiBanId, soHieu);
        }

        public async Task<(bool can_delete, string message)> CanDeleteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _banAnRepository.CanDeleteAsync(id);
        }

        public async Task<IEnumerable<BanAnWithLoaiBan>> GetAvailableAsync(int? capacity = null)
        {
            if (capacity.HasValue && capacity <= 0)
            {
                throw new ArgumentException("Sức chứa phải lớn hơn 0", nameof(capacity));
            }

            return await _banAnRepository.GetAvailableAsync(capacity);
        }

        public async Task<int> GetCountByLoaiBanIdAsync(int loaiBanId)
        {
            if (loaiBanId <= 0)
            {
                throw new ArgumentException("ID loại bàn phải lớn hơn 0", nameof(loaiBanId));
            }

            return await _banAnRepository.GetCountByLoaiBanIdAsync(loaiBanId);
        }
    }
}
