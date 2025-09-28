using BTL.Web.Models;

namespace BTL.Web.Services
{
    public interface ITanSuatMonAnService
    {
        Task<List<TanSuatMonAn>> GetTanSuatMonAnAsync(int? monId, int thang, int nam);
        Task<List<MonAn>> GetDanhSachMonAnAsync();
    }
}
