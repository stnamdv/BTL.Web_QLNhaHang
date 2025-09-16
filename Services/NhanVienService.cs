using BTL.Web.Models;
using BTL.Web.Repositories;

namespace BTL.Web.Services
{
    public class NhanVienService : INhanVienService
    {
        private readonly INhanVienRepository _nhanVienRepository;
        private readonly ILoaiNhanVienRepository _loaiNhanVienRepository;

        public NhanVienService(INhanVienRepository nhanVienRepository, ILoaiNhanVienRepository loaiNhanVienRepository)
        {
            _nhanVienRepository = nhanVienRepository;
            _loaiNhanVienRepository = loaiNhanVienRepository;
        }

        public async Task<IEnumerable<NhanVienWithLoaiNhanVien>> GetAllAsync()
        {
            return await _nhanVienRepository.GetAllAsync();
        }

        public async Task<NhanVien?> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _nhanVienRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<NhanVienWithLoaiNhanVien>> GetByLoaiNvAsync(string loaiNv)
        {
            if (string.IsNullOrWhiteSpace(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không được để trống", nameof(loaiNv));
            }

            // Kiểm tra loại nhân viên có tồn tại không
            var loaiNhanVien = await _loaiNhanVienRepository.GetByTypeAsync(loaiNv);
            if (loaiNhanVien == null)
            {
                throw new InvalidOperationException("Loại nhân viên không tồn tại");
            }

            return await _nhanVienRepository.GetByLoaiNvAsync(loaiNv);
        }

        public async Task<NhanVien> CreateAsync(NhanVien nhanVien)
        {
            if (nhanVien == null)
            {
                throw new ArgumentNullException(nameof(nhanVien));
            }

            if (string.IsNullOrWhiteSpace(nhanVien.ho_ten))
            {
                throw new ArgumentException("Họ tên không được để trống");
            }

            if (string.IsNullOrWhiteSpace(nhanVien.loai_nv))
            {
                throw new ArgumentException("Loại nhân viên không được để trống");
            }

            // Kiểm tra loại nhân viên có tồn tại không
            var loaiNhanVien = await _loaiNhanVienRepository.GetByTypeAsync(nhanVien.loai_nv);
            if (loaiNhanVien == null)
            {
                throw new InvalidOperationException("Loại nhân viên không tồn tại");
            }

            // Validation cho ngày vào làm
            if (nhanVien.ngay_vao_lam.HasValue && nhanVien.ngay_vao_lam.Value > DateTime.Today)
            {
                throw new ArgumentException("Ngày vào làm không được là ngày tương lai");
            }

            // Validation cho trạng thái
            if (!TrangThaiNv.All.Contains(nhanVien.trang_thai))
            {
                throw new ArgumentException("Trạng thái không hợp lệ");
            }

            return await _nhanVienRepository.CreateAsync(nhanVien);
        }

        public async Task<NhanVien> UpdateAsync(NhanVien nhanVien)
        {
            if (nhanVien == null)
            {
                throw new ArgumentNullException(nameof(nhanVien));
            }

            if (nhanVien.nv_id <= 0)
            {
                throw new ArgumentException("ID nhân viên không hợp lệ");
            }

            if (string.IsNullOrWhiteSpace(nhanVien.ho_ten))
            {
                throw new ArgumentException("Họ tên không được để trống");
            }

            if (string.IsNullOrWhiteSpace(nhanVien.loai_nv))
            {
                throw new ArgumentException("Loại nhân viên không được để trống");
            }

            // Kiểm tra nhân viên có tồn tại không
            var existingNhanVien = await _nhanVienRepository.GetByIdAsync(nhanVien.nv_id);
            if (existingNhanVien == null)
            {
                throw new InvalidOperationException("Nhân viên không tồn tại");
            }

            // Kiểm tra loại nhân viên có tồn tại không
            var loaiNhanVien = await _loaiNhanVienRepository.GetByTypeAsync(nhanVien.loai_nv);
            if (loaiNhanVien == null)
            {
                throw new InvalidOperationException("Loại nhân viên không tồn tại");
            }

            // Validation cho ngày vào làm
            if (nhanVien.ngay_vao_lam.HasValue && nhanVien.ngay_vao_lam.Value > DateTime.Today)
            {
                throw new ArgumentException("Ngày vào làm không được là ngày tương lai");
            }

            // Validation cho trạng thái
            if (!TrangThaiNv.All.Contains(nhanVien.trang_thai))
            {
                throw new ArgumentException("Trạng thái không hợp lệ");
            }

            return await _nhanVienRepository.UpdateAsync(nhanVien);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            // Kiểm tra nhân viên có tồn tại không
            var existingNhanVien = await _nhanVienRepository.GetByIdAsync(id);
            if (existingNhanVien == null)
            {
                throw new InvalidOperationException("Nhân viên không tồn tại");
            }

            // Kiểm tra có thể xóa không
            var canDelete = await _nhanVienRepository.CanDeleteAsync(id);
            if (!canDelete.can_delete)
            {
                throw new InvalidOperationException(canDelete.message);
            }

            return await _nhanVienRepository.DeleteAsync(id);
        }

        public async Task<NhanVien?> GetDetailsWithLoaiNhanVienAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _nhanVienRepository.GetDetailsWithLoaiNhanVienAsync(id);
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _nhanVienRepository.ExistsByIdAsync(id);
        }

        public async Task<(bool can_delete, string message)> CanDeleteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _nhanVienRepository.CanDeleteAsync(id);
        }

        public async Task<IEnumerable<NhanVienWithLoaiNhanVien>> GetActiveAsync()
        {
            return await _nhanVienRepository.GetActiveAsync();
        }

        public async Task<IEnumerable<NhanVienWithLoaiNhanVien>> SearchByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentException("Từ khóa tìm kiếm không được để trống", nameof(searchTerm));
            }

            return await _nhanVienRepository.SearchByNameAsync(searchTerm);
        }

        public async Task<IEnumerable<dynamic>> GetStatsByLoaiAsync()
        {
            return await _nhanVienRepository.GetStatsByLoaiAsync();
        }
    }
}
