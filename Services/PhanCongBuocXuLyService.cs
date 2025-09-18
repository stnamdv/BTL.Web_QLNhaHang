using BTL.Web.Models;
using BTL.Web.Repositories;

namespace BTL.Web.Services
{
    public class PhanCongBuocXuLyService : IPhanCongBuocXuLyService
    {
        private readonly IPhanCongBuocXuLyRepository _repository;

        public PhanCongBuocXuLyService(IPhanCongBuocXuLyRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<BuocXuLyWithPhanCong>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<BuocXuLyWithPhanCong>> GetByBuocAsync(int buocId)
        {
            return await _repository.GetByBuocAsync(buocId);
        }

        public async Task<IEnumerable<BuocXuLyWithPhanCong>> GetByLoaiNhanVienAsync(int loaiNvId)
        {
            return await _repository.GetByLoaiNhanVienAsync(loaiNvId);
        }

        public async Task<IEnumerable<dynamic>> GetNhanVienChoBuocAsync(int buocId)
        {
            return await _repository.GetNhanVienChoBuocAsync(buocId);
        }

        public async Task<PhanCongBuocXuLy> CreateAsync(PhanCongBuocXuLy phanCong)
        {
            return await _repository.CreateAsync(phanCong);
        }

        public async Task<PhanCongBuocXuLy> UpdateAsync(PhanCongBuocXuLy phanCong)
        {
            return await _repository.UpdateAsync(phanCong);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            return await _repository.ExistsByIdAsync(id);
        }

        public async Task<IEnumerable<BuocXuLy>> GetBuocChuaPhanCongAsync()
        {
            return await _repository.GetBuocChuaPhanCongAsync();
        }

        public async Task<IEnumerable<LoaiNhanVien>> GetLoaiNhanVienChuaPhanCongAsync(int buocId)
        {
            return await _repository.GetLoaiNhanVienChuaPhanCongAsync(buocId);
        }
    }
}
