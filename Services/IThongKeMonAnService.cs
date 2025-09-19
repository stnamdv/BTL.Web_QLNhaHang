using BTL.Web.Models;

namespace BTL.Web.Services
{
    public interface IThongKeMonAnService
    {
        Task<List<ThongKeMonAnTanSuat>> GetThongKeTanSuatTheoThangAsync(int thang, int nam);
        Task<List<ThongKeMonAnChiTiet>> GetThongKeChiTietTheoNgayAsync(int thang, int nam);
        Task<List<ThongKeMonAnTop>> GetTopMonAnAsync(int thang, int nam, int topN = 10);
        Task<List<ThongKeMonAnSoSanh>> GetThongKeSoSanhThangAsync(int thang1, int nam1, int thang2, int nam2);
        Task<List<ThongKeMonAnTheoLoai>> GetThongKeTheoLoaiMonAsync(int thang, int nam);
        Task<ThongKeMonAnDashboard> GetDashboardAsync(int thang, int nam);
    }
}
