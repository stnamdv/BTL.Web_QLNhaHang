using BTL.Web.Models;

namespace BTL.Web.Services
{
    public interface IThongKeDoanhThuChiPhiService
    {
        // Thống kê theo ngày cụ thể
        Task<(ThongKeDoanhThuChiPhiTongQuan tongQuan,
              List<ThongKeDoanhThuTheoLoaiMon> doanhThuTheoLoaiMon,
              List<ThongKeChiPhiTheoNguyenLieu> chiPhiTheoNguyenLieu,
              List<ThongKeChiPhiLuongTheoLoaiNhanVien> chiPhiLuongTheoLoaiNhanVien)>
            GetThongKeTheoNgayAsync(DateTime ngay);

        // Thống kê theo khoảng thời gian
        Task<(ThongKeDoanhThuChiPhiKhoangThoiGian tongQuan,
              List<ThongKeDoanhThuChiPhiTheoNgay> theoNgay)>
            GetThongKeTheoKhoangThoiGianAsync(DateTime tuNgay, DateTime denNgay);

        // Thống kê ngày hiện tại
        Task<(ThongKeDoanhThuChiPhiTongQuan tongQuan,
              List<ThongKeDoanhThuTheoLoaiMon> doanhThuTheoLoaiMon,
              List<ThongKeChiPhiTheoNguyenLieu> chiPhiTheoNguyenLieu,
              List<ThongKeChiPhiLuongTheoLoaiNhanVien> chiPhiLuongTheoLoaiNhanVien)>
            GetThongKeNgayHienTaiAsync();

        // Lấy dashboard thống kê
        Task<DashboardThongKe> GetDashboardThongKeAsync();
    }
}
