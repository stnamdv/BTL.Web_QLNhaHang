using BTL.Web.Models;

namespace BTL.Web.Repositories
{
    public interface IPhanCongBuocXuLyRepository
    {
        Task<IEnumerable<BuocXuLyWithPhanCong>> GetAllAsync();
        Task<IEnumerable<BuocXuLyWithPhanCong>> GetByBuocAsync(int buocId);
        Task<IEnumerable<BuocXuLyWithPhanCong>> GetByLoaiNhanVienAsync(int loaiNvId);
        Task<IEnumerable<dynamic>> GetNhanVienChoBuocAsync(int buocId);
        Task<IEnumerable<dynamic>> GetAllActiveEmployeesAsync();
        Task<IEnumerable<dynamic>> GetNhanVienTheoLoaiBuocAsync(int buocId);
        Task<IEnumerable<BuocXuLy>> GetBuocChuaPhanCongAsync();
        Task<IEnumerable<LoaiNhanVien>> GetLoaiNhanVienChuaPhanCongAsync(int buocId);
        Task<PhanCongBuocXuLy> CreateAsync(PhanCongBuocXuLy phanCong);
        Task<PhanCongBuocXuLy> UpdateAsync(PhanCongBuocXuLy phanCong);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByIdAsync(int id);
    }
}
