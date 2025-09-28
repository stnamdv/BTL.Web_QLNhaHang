using BTL.Web.Models;

namespace BTL.Web.Services
{
    public interface ITanSuatDatBanService
    {
        Task<List<TanSuatDatBan>> GetTanSuatDatBanAsync(int? sucChua = null, int? ngayTrongThang = null);
        Task<List<LoaiBanInfo>> GetDanhSachLoaiBanAsync();
    }
}
