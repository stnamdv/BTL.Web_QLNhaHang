using BTL.Web.Models;
using BTL.Web.Repositories;

namespace BTL.Web.Services
{
    public class BuocXuLyService : IBuocXuLyService
    {
        private readonly IBuocXuLyRepository _repository;

        public BuocXuLyService(IBuocXuLyRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<BuocXuLy>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<BuocXuLy?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<BuocXuLy?> GetByThuTuAsync(int thuTu)
        {
            return await _repository.GetByThuTuAsync(thuTu);
        }

        public async Task<BuocXuLy?> GetBuocTiepTheoAsync(int buocId)
        {
            return await _repository.GetBuocTiepTheoAsync(buocId);
        }

        public async Task<BuocXuLy?> GetBuocDauTienAsync()
        {
            return await _repository.GetBuocDauTienAsync();
        }

        public async Task<BuocXuLy> CreateAsync(BuocXuLy buocXuLy)
        {
            return await _repository.CreateAsync(buocXuLy);
        }

        public async Task<BuocXuLy> UpdateAsync(BuocXuLy buocXuLy)
        {
            return await _repository.UpdateAsync(buocXuLy);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _repository.ExistsAsync(id);
        }
    }
}
