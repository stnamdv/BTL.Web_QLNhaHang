using BTL.Web.Models;

namespace BTL.Web.Services
{
    public interface IThongKeBanAnService
    {
        Task<List<ThongKeBanAnTrungBinh>> GetThongKeTrungBinhTheoNgayAsync(int thang, int nam);
        Task<List<ThongKeBanAnChiTiet>> GetThongKeChiTietTheoNgayAsync(int thang, int nam);
        Task<List<ThongKeBanAnTongHop>> GetThongKeTongHopAsync(int thang, int nam);
        Task<List<ThongKeBanAnSoSanh>> GetThongKeSoSanhThangAsync(int thang1, int nam1, int thang2, int nam2);
        Task<ThongKeBanAnDashboard> GetDashboardAsync(int thang, int nam);
    }
}
