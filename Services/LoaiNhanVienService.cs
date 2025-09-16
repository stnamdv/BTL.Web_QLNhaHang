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

        public async Task<LoaiNhanVien?> GetByTypeAsync(string loaiNv)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không được để trống", nameof(loaiNv));
            }

            if (!LoaiNv.All.Contains(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không hợp lệ", nameof(loaiNv));
            }

            return await _loaiNhanVienRepository.GetByTypeAsync(loaiNv);
        }

        public async Task<LoaiNhanVien> CreateAsync(LoaiNhanVien loaiNhanVien)
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

            if (!LoaiNv.All.Contains(loaiNhanVien.loai_nv))
            {
                throw new ArgumentException("Loại nhân viên không hợp lệ");
            }

            if (loaiNhanVien.luong_co_ban < 0)
            {
                throw new ArgumentException("Lương cơ bản không được âm");
            }

            // Kiểm tra xem loại nhân viên đã tồn tại chưa
            var exists = await _loaiNhanVienRepository.ExistsByTypeAsync(loaiNhanVien.loai_nv);
            if (exists)
            {
                throw new InvalidOperationException("Loại nhân viên đã tồn tại");
            }

            // Validate type
            var validation = await _loaiNhanVienRepository.ValidateTypeAsync(loaiNhanVien.loai_nv);
            if (!validation.is_valid)
            {
                throw new ArgumentException(validation.message);
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

            if (!LoaiNv.All.Contains(loaiNhanVien.loai_nv))
            {
                throw new ArgumentException("Loại nhân viên không hợp lệ");
            }

            if (loaiNhanVien.luong_co_ban < 0)
            {
                throw new ArgumentException("Lương cơ bản không được âm");
            }

            // Kiểm tra xem có thể update không
            var updateCheck = await _loaiNhanVienRepository.CanUpdateAsync(loaiNhanVien.loai_nv, loaiNhanVien.luong_co_ban);
            if (!updateCheck.can_update)
            {
                throw new InvalidOperationException(updateCheck.error_message);
            }

            return await _loaiNhanVienRepository.UpdateAsync(loaiNhanVien);
        }

        public async Task<bool> DeleteAsync(string loaiNv)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không được để trống", nameof(loaiNv));
            }

            if (!LoaiNv.All.Contains(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không hợp lệ", nameof(loaiNv));
            }

            // Kiểm tra xem có thể xóa không
            var deleteCheck = await _loaiNhanVienRepository.CanDeleteAsync(loaiNv);
            if (!deleteCheck.can_delete)
            {
                throw new InvalidOperationException(deleteCheck.error_message);
            }

            return await _loaiNhanVienRepository.DeleteAsync(loaiNv);
        }

        public async Task<bool> ExistsByTypeAsync(string loaiNv)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không được để trống", nameof(loaiNv));
            }

            if (!LoaiNv.All.Contains(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không hợp lệ", nameof(loaiNv));
            }

            return await _loaiNhanVienRepository.ExistsByTypeAsync(loaiNv);
        }

        public async Task<LoaiNhanVienDetails?> GetDetailsWithUsageAsync(string loaiNv)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không được để trống", nameof(loaiNv));
            }

            if (!LoaiNv.All.Contains(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không hợp lệ", nameof(loaiNv));
            }

            return await _loaiNhanVienRepository.GetDetailsWithUsageAsync(loaiNv);
        }

        public async Task<IEnumerable<NhanVienInfo>> GetEmployeesAsync(string loaiNv)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không được để trống", nameof(loaiNv));
            }

            if (!LoaiNv.All.Contains(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không hợp lệ", nameof(loaiNv));
            }

            return await _loaiNhanVienRepository.GetEmployeesAsync(loaiNv);
        }

        public async Task<LoaiNhanVienUpdateCheck> CanUpdateAsync(string loaiNv, decimal newLuongCoBan)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không được để trống", nameof(loaiNv));
            }

            if (!LoaiNv.All.Contains(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không hợp lệ", nameof(loaiNv));
            }

            if (newLuongCoBan < 0)
            {
                throw new ArgumentException("Lương cơ bản không được âm", nameof(newLuongCoBan));
            }

            return await _loaiNhanVienRepository.CanUpdateAsync(loaiNv, newLuongCoBan);
        }

        public async Task<LoaiNhanVienDeleteCheck> CanDeleteAsync(string loaiNv)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không được để trống", nameof(loaiNv));
            }

            if (!LoaiNv.All.Contains(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không hợp lệ", nameof(loaiNv));
            }

            return await _loaiNhanVienRepository.CanDeleteAsync(loaiNv);
        }

        public async Task<IEnumerable<LoaiNhanVien>> GetAvailableAsync()
        {
            return await _loaiNhanVienRepository.GetAvailableAsync();
        }

        public async Task<LoaiNhanVienValidation> ValidateTypeAsync(string loaiNv)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không được để trống", nameof(loaiNv));
            }

            if (!LoaiNv.All.Contains(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không hợp lệ", nameof(loaiNv));
            }

            return await _loaiNhanVienRepository.ValidateTypeAsync(loaiNv);
        }
    }
}
