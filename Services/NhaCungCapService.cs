using BTL.Web.Models;
using BTL.Web.Repositories;

namespace BTL.Web.Services
{
    public class NhaCungCapService : INhaCungCapService
    {
        private readonly INhaCungCapRepository _repository;

        public NhaCungCapService(INhaCungCapRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<NhaCungCap>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<PagedResult<NhaCungCap>> GetAllPagedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            return await _repository.GetAllPagedAsync(pageNumber, pageSize, searchTerm);
        }

        public async Task<NhaCungCap?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<NhaCungCap> CreateAsync(NhaCungCap nhaCungCap)
        {
            // Validation business logic
            if (string.IsNullOrWhiteSpace(nhaCungCap.ten))
            {
                throw new ArgumentException("Tên nhà cung cấp không được để trống.");
            }

            if (await _repository.ExistsByTenAsync(nhaCungCap.ten))
            {
                throw new ArgumentException("Tên nhà cung cấp đã tồn tại.");
            }

            return await _repository.CreateAsync(nhaCungCap);
        }

        public async Task<NhaCungCap> UpdateAsync(NhaCungCap nhaCungCap)
        {
            // Validation business logic
            if (string.IsNullOrWhiteSpace(nhaCungCap.ten))
            {
                throw new ArgumentException("Tên nhà cung cấp không được để trống.");
            }

            if (await _repository.ExistsByTenAsync(nhaCungCap.ten, nhaCungCap.ncc_id))
            {
                throw new ArgumentException("Tên nhà cung cấp đã tồn tại ở nhà cung cấp khác.");
            }

            return await _repository.UpdateAsync(nhaCungCap);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var (canDelete, message) = await _repository.CanDeleteAsync(id);
            if (!canDelete)
            {
                throw new InvalidOperationException(message);
            }

            return await _repository.DeleteAsync(id);
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            return await _repository.ExistsByIdAsync(id);
        }

        public async Task<bool> ExistsByTenAsync(string ten, int? excludeId = null)
        {
            return await _repository.ExistsByTenAsync(ten, excludeId);
        }

        public async Task<(bool can_delete, string message)> CanDeleteAsync(int id)
        {
            return await _repository.CanDeleteAsync(id);
        }

        public async Task<IEnumerable<NhaCungCap>> SearchAsync(string searchTerm)
        {
            return await _repository.SearchAsync(searchTerm);
        }

        public async Task<PagedResult<NhaCungCap>> SearchPagedAsync(string searchTerm, int pageNumber, int pageSize)
        {
            return await _repository.SearchPagedAsync(searchTerm, pageNumber, pageSize);
        }

        public async Task<IEnumerable<dynamic>> GetStatsAsync(int? nccId = null)
        {
            return await _repository.GetStatsAsync(nccId);
        }

        public async Task<IEnumerable<NguyenLieuWithNhaCungCap>> GetNguyenLieuByNhaCungCapAsync(int nccId)
        {
            return await _repository.GetNguyenLieuByNhaCungCapAsync(nccId);
        }
    }
}
