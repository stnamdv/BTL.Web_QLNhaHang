using BTL.Web.Models;

namespace BTL.Web.Services
{
    public interface IThongKeNguyenLieuService
    {
        // Thống kê theo ngày cụ thể
        Task<(List<ThongKeNguyenLieuChiTiet> chiTiet,
              List<ThongKeNguyenLieuTheoDonVi> theoDonVi,
              List<ThongKeMonAnSuDungNguyenLieu> monAn)>
            GetThongKeTheoNgayAsync(DateTime ngay);

        // Thống kê theo khoảng thời gian
        Task<(ThongKeNguyenLieuKhoangThoiGian tongQuan,
              List<ThongKeNguyenLieuChiTiet> chiTiet,
              List<ThongKeNguyenLieuTheoDonVi> theoDonVi,
              List<ThongKeNguyenLieuTheoNgay> theoNgay)>
            GetThongKeTheoKhoangThoiGianAsync(DateTime tuNgay, DateTime denNgay);

        // Thống kê ngày hiện tại
        Task<(List<ThongKeNguyenLieuChiTiet> chiTiet,
              List<ThongKeNguyenLieuTheoDonVi> theoDonVi,
              List<ThongKeMonAnSuDungNguyenLieu> monAn)>
            GetThongKeNgayHienTaiAsync();
    }
}
