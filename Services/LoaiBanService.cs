using BTL.Web.Models;
using BTL.Web.Repositories;

namespace BTL.Web.Services
{
    public class LoaiBanService : ILoaiBanService
    {
        private readonly ILoaiBanRepository _loaiBanRepository;

        public LoaiBanService(ILoaiBanRepository loaiBanRepository)
        {
            _loaiBanRepository = loaiBanRepository;
        }

        public async Task<IEnumerable<LoaiBan>> GetAllAsync()
        {
            // Có thể thêm business logic ở đây như validation, caching, logging, etc.
            return await _loaiBanRepository.GetAllAsync();
        }

        public async Task<LoaiBan?> GetByIdAsync(int id)
        {
            // Validation
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _loaiBanRepository.GetByIdAsync(id);
        }

        public async Task<LoaiBan> CreateAsync(LoaiBan loaiBan)
        {
            // Business validation
            if (loaiBan == null)
            {
                throw new ArgumentNullException(nameof(loaiBan));
            }

            if (loaiBan.suc_chua <= 0)
            {
                throw new ArgumentException("Sức chứa phải lớn hơn 0");
            }

            if (loaiBan.so_luong < 0)
            {
                throw new ArgumentException("Số lượng không được âm");
            }

            // Có thể thêm logic kiểm tra trùng lặp, validation phức tạp, etc.
            return await _loaiBanRepository.CreateAsync(loaiBan);
        }

        public async Task<LoaiBan> UpdateAsync(LoaiBan loaiBan)
        {
            // Business validation
            if (loaiBan == null)
            {
                throw new ArgumentNullException(nameof(loaiBan));
            }

            if (loaiBan.loai_ban_id <= 0)
            {
                throw new ArgumentException("ID loại bàn không hợp lệ");
            }

            if (loaiBan.suc_chua <= 0)
            {
                throw new ArgumentException("Sức chứa phải lớn hơn 0");
            }

            if (loaiBan.so_luong < 0)
            {
                throw new ArgumentException("Số lượng không được âm");
            }

            // Kiểm tra xem có thể update không
            var updateCheck = await _loaiBanRepository.CanUpdateAsync(loaiBan.loai_ban_id, loaiBan.suc_chua, loaiBan.so_luong);
            if (!updateCheck.can_update)
            {
                throw new InvalidOperationException(updateCheck.error_message);
            }

            return await _loaiBanRepository.UpdateAsync(loaiBan);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // Validation
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            // Kiểm tra xem có thể xóa không
            var deleteCheck = await _loaiBanRepository.CanDeleteAsync(id);
            if (!deleteCheck.can_delete)
            {
                throw new InvalidOperationException(deleteCheck.error_message);
            }

            return await _loaiBanRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsByCapacityAsync(int sucChua)
        {
            // Validation
            if (sucChua <= 0)
            {
                throw new ArgumentException("Sức chứa phải lớn hơn 0", nameof(sucChua));
            }

            return await _loaiBanRepository.ExistsByCapacityAsync(sucChua);
        }

        public async Task<LoaiBanDetails?> GetDetailsWithUsageAsync(int id)
        {
            // Validation
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _loaiBanRepository.GetDetailsWithUsageAsync(id);
        }

        public async Task<IEnumerable<BanAnInfo>> GetTablesAsync(int loaiBanId)
        {
            // Validation
            if (loaiBanId <= 0)
            {
                throw new ArgumentException("ID loại bàn phải lớn hơn 0", nameof(loaiBanId));
            }

            return await _loaiBanRepository.GetTablesAsync(loaiBanId);
        }

        public async Task<LoaiBanUpdateCheck> CanUpdateAsync(int id, int newSucChua, int newSoLuong)
        {
            // Validation
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            if (newSucChua <= 0)
            {
                throw new ArgumentException("Sức chứa phải lớn hơn 0", nameof(newSucChua));
            }

            if (newSoLuong < 0)
            {
                throw new ArgumentException("Số lượng không được âm", nameof(newSoLuong));
            }

            return await _loaiBanRepository.CanUpdateAsync(id, newSucChua, newSoLuong);
        }

        public async Task<LoaiBanDeleteCheck> CanDeleteAsync(int id)
        {
            // Validation
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _loaiBanRepository.CanDeleteAsync(id);
        }
    }
}
