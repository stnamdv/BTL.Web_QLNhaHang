using BTL.Web.Models;
using BTL.Web.Repositories;

namespace BTL.Web.Services
{
    public class NguyenLieuService : INguyenLieuService
    {
        private readonly INguyenLieuRepository _nguyenLieuRepository;

        public NguyenLieuService(INguyenLieuRepository nguyenLieuRepository)
        {
            _nguyenLieuRepository = nguyenLieuRepository;
        }

        public async Task<IEnumerable<NguyenLieu>> GetAllAsync()
        {
            return await _nguyenLieuRepository.GetAllAsync();
        }

        public async Task<PagedResult<NguyenLieu>> GetAllPagedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentException("Số trang phải lớn hơn 0", nameof(pageNumber));
            }

            if (pageSize < 1 || pageSize > 100)
            {
                throw new ArgumentException("Kích thước trang phải từ 1 đến 100", nameof(pageSize));
            }

            return await _nguyenLieuRepository.GetAllPagedAsync(pageNumber, pageSize, searchTerm);
        }

        public async Task<NguyenLieu?> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _nguyenLieuRepository.GetByIdAsync(id);
        }

        public async Task<NguyenLieuWithNhaCungCap?> GetWithNhaCungCapAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _nguyenLieuRepository.GetWithNhaCungCapAsync(id);
        }

        public async Task<NguyenLieuWithMon?> GetWithMonAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _nguyenLieuRepository.GetWithMonAsync(id);
        }

        public async Task<NguyenLieu> CreateAsync(NguyenLieu nguyenLieu)
        {
            if (nguyenLieu == null)
            {
                throw new ArgumentNullException(nameof(nguyenLieu));
            }

            // Validation business logic
            await ValidateNguyenLieuAsync(nguyenLieu, null);

            return await _nguyenLieuRepository.CreateAsync(nguyenLieu);
        }

        public async Task<NguyenLieu> UpdateAsync(NguyenLieu nguyenLieu)
        {
            if (nguyenLieu == null)
            {
                throw new ArgumentNullException(nameof(nguyenLieu));
            }

            if (nguyenLieu.nl_id <= 0)
            {
                throw new ArgumentException("ID nguyên liệu phải lớn hơn 0", nameof(nguyenLieu.nl_id));
            }

            // Kiểm tra nguyên liệu có tồn tại không
            if (!await _nguyenLieuRepository.ExistsByIdAsync(nguyenLieu.nl_id))
            {
                throw new InvalidOperationException("Nguyên liệu không tồn tại.");
            }

            // Validation business logic
            await ValidateNguyenLieuAsync(nguyenLieu, nguyenLieu.nl_id);

            return await _nguyenLieuRepository.UpdateAsync(nguyenLieu);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            var (canDelete, message) = await _nguyenLieuRepository.CanDeleteAsync(id);
            if (!canDelete)
            {
                throw new InvalidOperationException(message);
            }

            return await _nguyenLieuRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            if (id <= 0)
            {
                return false;
            }

            return await _nguyenLieuRepository.ExistsByIdAsync(id);
        }

        public async Task<bool> ExistsByTenAsync(string ten, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(ten))
            {
                return false;
            }

            return await _nguyenLieuRepository.ExistsByTenAsync(ten, excludeId);
        }

        public async Task<(bool can_delete, string message)> CanDeleteAsync(int id)
        {
            if (id <= 0)
            {
                return (false, "ID phải lớn hơn 0");
            }

            return await _nguyenLieuRepository.CanDeleteAsync(id);
        }

        public async Task<IEnumerable<NguyenLieu>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await _nguyenLieuRepository.GetAllAsync();
            }

            return await _nguyenLieuRepository.SearchAsync(searchTerm);
        }

        public async Task<PagedResult<NguyenLieu>> SearchPagedAsync(string searchTerm, int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentException("Số trang phải lớn hơn 0", nameof(pageNumber));
            }

            if (pageSize < 1 || pageSize > 100)
            {
                throw new ArgumentException("Kích thước trang phải từ 1 đến 100", nameof(pageSize));
            }

            return await _nguyenLieuRepository.SearchPagedAsync(searchTerm, pageNumber, pageSize);
        }

        public async Task<IEnumerable<NguyenLieu>> SearchByCriteriaAsync(string? searchTerm, string? donVi, string? nguonGoc, int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentException("Số trang phải lớn hơn 0", nameof(pageNumber));
            }

            if (pageSize < 1 || pageSize > 100)
            {
                throw new ArgumentException("Kích thước trang phải từ 1 đến 100", nameof(pageSize));
            }

            return await _nguyenLieuRepository.SearchByCriteriaAsync(searchTerm, donVi, nguonGoc, pageNumber, pageSize);
        }

        public async Task<IEnumerable<dynamic>> GetStatsAsync(int? nlId = null)
        {
            return await _nguyenLieuRepository.GetStatsAsync(nlId);
        }

        public async Task<(bool isValid, string errorMessage)> ValidateNguonGocAsync(string nguonGoc)
        {
            if (string.IsNullOrWhiteSpace(nguonGoc))
            {
                return (false, "Thông tin nguồn gốc không được để trống");
            }

            return await _nguyenLieuRepository.ValidateNguonGocAsync(nguonGoc);
        }

        public async Task<IEnumerable<string>> GetDistinctDonViAsync()
        {
            var allNguyenLieu = await _nguyenLieuRepository.GetAllAsync();
            return allNguyenLieu
                .Select(nl => nl.don_vi)
                .Distinct()
                .OrderBy(dv => dv)
                .ToList();
        }

        public async Task<PagedResult<NguyenLieuWithNhaCungCap>> GetAllWithNhaCungCapPagedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentException("Số trang phải lớn hơn 0", nameof(pageNumber));
            }

            if (pageSize < 1 || pageSize > 100)
            {
                throw new ArgumentException("Kích thước trang phải từ 1 đến 100", nameof(pageSize));
            }

            return await _nguyenLieuRepository.GetAllWithNhaCungCapPagedAsync(pageNumber, pageSize, searchTerm);
        }

        private async Task ValidateNguyenLieuAsync(NguyenLieu nguyenLieu, int? excludeId)
        {
            // Validate tên nguyên liệu
            if (string.IsNullOrWhiteSpace(nguyenLieu.ten))
            {
                throw new ArgumentException("Tên nguyên liệu không được để trống");
            }

            if (nguyenLieu.ten.Length > 160)
            {
                throw new ArgumentException("Tên nguyên liệu không được vượt quá 160 ký tự");
            }

            // Kiểm tra tên trùng lặp
            if (await _nguyenLieuRepository.ExistsByTenAsync(nguyenLieu.ten, excludeId))
            {
                throw new ArgumentException("Tên nguyên liệu đã tồn tại");
            }

            // Validate đơn vị
            if (string.IsNullOrWhiteSpace(nguyenLieu.don_vi))
            {
                throw new ArgumentException("Đơn vị không được để trống");
            }

            if (nguyenLieu.don_vi.Length > 20)
            {
                throw new ArgumentException("Đơn vị không được vượt quá 20 ký tự");
            }

            // Validate nguồn gốc (yêu cầu nghiệp vụ)
            var (isValid, errorMessage) = await ValidateNguonGocAsync(nguyenLieu.nguon_goc);
            if (!isValid)
            {
                throw new ArgumentException(errorMessage);
            }
        }
    }
}
