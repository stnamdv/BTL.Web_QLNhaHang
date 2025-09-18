using BTL.Web.Models;
using BTL.Web.Repositories;

namespace BTL.Web.Services
{
    public class LoaiNhanVienService : ILoaiNhanVienService
    {
        private readonly ILoaiNhanVienRepository _loaiNhanVienRepository;

        public LoaiNhanVienService(ILoaiNhanVienRepository loaiNhanVienRepository)
        {
            _loaiNhanVienRepository = loaiNhanVienRepository;
        }

        public async Task<IEnumerable<LoaiNhanVien>> GetAllAsync()
        {
            // Có thể thêm business logic ở đây như validation, caching, logging, etc.
            return await _loaiNhanVienRepository.GetAllAsync();
        }

        public async Task<LoaiNhanVien?> GetByIdAsync(int id)
        {
            // Validation
            if (id <= 0)
            {
                throw new ArgumentException("ID không hợp lệ", nameof(id));
            }

            return await _loaiNhanVienRepository.GetByIdAsync(id);
        }
        public async Task<LoaiNhanVien> CreateAsync(LoaiNhanVien loaiNhanVien)
        {
            // Kiểm tra xem loại nhân viên đã tồn tại chưa
            var exists = await _loaiNhanVienRepository.ExistsByTypeAsync(loaiNhanVien.loai_nv_id);
            if (exists)
            {
                throw new InvalidOperationException("Loại nhân viên đã tồn tại");
            }


            return await _loaiNhanVienRepository.CreateAsync(loaiNhanVien);
        }

        public async Task<LoaiNhanVien> UpdateAsync(LoaiNhanVien loaiNhanVien)
        {
            // Business validation
            if (loaiNhanVien == null)
            {
                throw new ArgumentNullException(nameof(loaiNhanVien));
            }

            if (string.IsNullOrWhiteSpace(loaiNhanVien.loai_nv))
            {
                throw new ArgumentException("Loại nhân viên không được để trống");
            }

            if (loaiNhanVien.loai_nv_id <= 0)
            {
                throw new ArgumentException("Loại nhân viên không hợp lệ");
            }

            if (loaiNhanVien.luong_co_ban < 0)
            {
                throw new ArgumentException("Lương cơ bản không được âm");
            }

            // Kiểm tra xem có thể update không
            var updateCheck = await _loaiNhanVienRepository.CanUpdateAsync(loaiNhanVien.loai_nv_id, loaiNhanVien.luong_co_ban);
            if (!updateCheck.can_update)
            {
                throw new InvalidOperationException(updateCheck.error_message);
            }

            return await _loaiNhanVienRepository.UpdateAsync(loaiNhanVien);
        }

        public async Task<bool> DeleteAsync(int loaiNvId)
        {
            // Validation
            if (loaiNvId <= 0)
            {
                throw new ArgumentException("ID loại nhân viên không hợp lệ", nameof(loaiNvId));
            }

            // Kiểm tra xem có thể xóa không
            var deleteCheck = await _loaiNhanVienRepository.CanDeleteAsync(loaiNvId);
            if (!deleteCheck.can_delete)
            {
                throw new InvalidOperationException(deleteCheck.error_message);
            }

            return await _loaiNhanVienRepository.DeleteAsync(loaiNvId);
        }

        public async Task<LoaiNhanVienDetails?> GetDetailsWithUsageAsync(int loaiNvId)
        {
            // Validation
            if (loaiNvId <= 0)
            {
                throw new ArgumentException("ID loại nhân viên không hợp lệ", nameof(loaiNvId));
            }

            return await _loaiNhanVienRepository.GetDetailsWithUsageAsync(loaiNvId);
        }

        public async Task<IEnumerable<NhanVienInfo>> GetEmployeesAsync(int loaiNvId)
        {
            // Validation
            if (loaiNvId <= 0)
            {
                throw new ArgumentException("ID loại nhân viên không hợp lệ", nameof(loaiNvId));
            }

            return await _loaiNhanVienRepository.GetEmployeesAsync(loaiNvId);
        }

        public async Task<LoaiNhanVienUpdateCheck> CanUpdateAsync(int loaiNvId, decimal newLuongCoBan)
        {
            // Validation
            if (loaiNvId <= 0)
            {
                throw new ArgumentException("ID loại nhân viên không hợp lệ", nameof(loaiNvId));
            }

            if (newLuongCoBan < 0)
            {
                throw new ArgumentException("Lương cơ bản không được âm", nameof(newLuongCoBan));
            }

            return await _loaiNhanVienRepository.CanUpdateAsync(loaiNvId, newLuongCoBan);
        }

        public async Task<LoaiNhanVienDeleteCheck> CanDeleteAsync(int loaiNvId)
        {
            // Validation
            if (loaiNvId <= 0)
            {
                throw new ArgumentException("ID loại nhân viên không hợp lệ", nameof(loaiNvId));
            }

            return await _loaiNhanVienRepository.CanDeleteAsync(loaiNvId);
        }

        public async Task<IEnumerable<LoaiNhanVien>> GetAvailableAsync()
        {
            return await _loaiNhanVienRepository.GetAvailableAsync();
        }

    }
}
