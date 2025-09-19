using BTL.Web.Models;

namespace BTL.Web.Services
{
    public interface ILuongNhanVienService
    {
        Task<LuongNhanVienViewModel> TinhLuongTheoThangAsync(int thang, int nam);
        Task<LuongNhanVienViewModel> TinhLuongThangHienTaiAsync();
    }
}
