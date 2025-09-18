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
    }
}
