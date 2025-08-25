using BTL.Web.Models;
using BTL.Web.Repositories;

namespace BTL.Web.Services
{
    public class MonService : IMonService
    {
        private readonly IMonRepository _monRepository;

        public MonService(IMonRepository monRepository)
        {
            _monRepository = monRepository;
        }

        public async Task<IEnumerable<Mon>> GetAllAsync()
        {
            return await _monRepository.GetAllAsync();
        }

        public async Task<Mon?> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _monRepository.GetByIdAsync(id);
        }

        public async Task<Mon?> GetByMaMonAsync(string maMon)
        {
            if (string.IsNullOrWhiteSpace(maMon))
            {
                throw new ArgumentException("Mã món không được để trống", nameof(maMon));
            }

            return await _monRepository.GetByMaMonAsync(maMon);
        }

        public async Task<IEnumerable<Mon>> GetByLoaiMonAsync(LoaiMon loaiMon)
        {
            return await _monRepository.GetByLoaiMonAsync(loaiMon);
        }

        public async Task<Mon> CreateAsync(Mon mon)
        {
            if (mon == null)
            {
                throw new ArgumentNullException(nameof(mon));
            }

            if (string.IsNullOrWhiteSpace(mon.ma_mon))
            {
                throw new ArgumentException("Mã món không được để trống");
            }

            if (string.IsNullOrWhiteSpace(mon.ten_mon))
            {
                throw new ArgumentException("Tên món không được để trống");
            }

            if (mon.gia < 0)
            {
                throw new ArgumentException("Giá món không được âm");
            }

            // Kiểm tra mã món đã tồn tại chưa
            var existingMon = await _monRepository.GetByMaMonAsync(mon.ma_mon);
            if (existingMon != null)
            {
                throw new InvalidOperationException("Mã món đã tồn tại");
            }

            return await _monRepository.CreateAsync(mon);
        }

        public async Task<Mon> UpdateAsync(Mon mon)
        {
            if (mon == null)
            {
                throw new ArgumentNullException(nameof(mon));
            }

            if (mon.mon_id <= 0)
            {
                throw new ArgumentException("ID món không hợp lệ");
            }

            if (string.IsNullOrWhiteSpace(mon.ma_mon))
            {
                throw new ArgumentException("Mã món không được để trống");
            }

            if (string.IsNullOrWhiteSpace(mon.ten_mon))
            {
                throw new ArgumentException("Tên món không được để trống");
            }

            if (mon.gia < 0)
            {
                throw new ArgumentException("Giá món không được âm");
            }

            // Kiểm tra món có tồn tại không
            var existingMon = await _monRepository.GetByIdAsync(mon.mon_id);
            if (existingMon == null)
            {
                throw new InvalidOperationException("Món không tồn tại");
            }

            // Kiểm tra mã món có bị trùng với món khác không
            var monWithSameMa = await _monRepository.GetByMaMonAsync(mon.ma_mon);
            if (monWithSameMa != null && monWithSameMa.mon_id != mon.mon_id)
            {
                throw new InvalidOperationException("Mã món đã tồn tại");
            }

            return await _monRepository.UpdateAsync(mon);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            // Kiểm tra món có tồn tại không
            var existingMon = await _monRepository.GetByIdAsync(id);
            if (existingMon == null)
            {
                throw new InvalidOperationException("Món không tồn tại");
            }

            // Có thể thêm logic kiểm tra ràng buộc (ví dụ: không cho phép xóa nếu món đang trong thực đơn)
            return await _monRepository.DeleteAsync(id);
        }
    }
}
