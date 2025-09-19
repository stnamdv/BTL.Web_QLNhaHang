using System.ComponentModel.DataAnnotations;

namespace BTL.Web.Models
{
    // Model cho thống kê tổng quan doanh thu và chi phí
    public class ThongKeDoanhThuChiPhiTongQuan
    {
        public DateTime Ngay { get; set; }
        public decimal TongDoanhThu { get; set; }
        public decimal TongChiPhiNguyenLieu { get; set; }
        public decimal TongChiPhiLuong { get; set; }
        public decimal TongChiPhi { get; set; }
        public decimal LoiNhuan { get; set; }
        public decimal TyLeLoiNhuan { get; set; }
        public int SoDonHang { get; set; }
        public int SoDonHoanThanh { get; set; }
        public int SoKhach { get; set; }
    }

    // Model cho thống kê theo khoảng thời gian
    public class ThongKeDoanhThuChiPhiKhoangThoiGian
    {
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public int SoNgay { get; set; }
        public decimal TongDoanhThu { get; set; }
        public decimal TongChiPhiNguyenLieu { get; set; }
        public decimal TongChiPhiLuong { get; set; }
        public decimal TongChiPhi { get; set; }
        public decimal LoiNhuan { get; set; }
        public decimal TyLeLoiNhuan { get; set; }
        public int SoDonHang { get; set; }
        public int SoDonHoanThanh { get; set; }
        public int SoKhach { get; set; }
        public decimal DoanhThuTrungBinhNgay { get; set; }
        public decimal LoiNhuanTrungBinhNgay { get; set; }
    }

    // Model cho thống kê doanh thu theo loại món
    public class ThongKeDoanhThuTheoLoaiMon
    {
        public string LoaiMon { get; set; } = string.Empty;
        public int SoLuongMon { get; set; }
        public int TongSoLuong { get; set; }
        public decimal DoanhThuTheoLoai { get; set; }
        public decimal TyLeDoanhThuPhanTram { get; set; }
    }

    // Model cho thống kê chi phí theo nguyên liệu
    public class ThongKeChiPhiTheoNguyenLieu
    {
        public string TenNguyenLieu { get; set; } = string.Empty;
        public string DonVi { get; set; } = string.Empty;
        public decimal TongLuongSuDung { get; set; }
        public decimal GiaDonVi { get; set; }
        public decimal TongChiPhiNguyenLieu { get; set; }
        public decimal TyLeChiPhiPhanTram { get; set; }
    }

    // Model cho thống kê chi phí lương theo loại nhân viên
    public class ThongKeChiPhiLuongTheoLoaiNhanVien
    {
        public string LoaiNhanVien { get; set; } = string.Empty;
        public int SoNhanVien { get; set; }
        public decimal LuongCoBan { get; set; }
        public decimal LuongNgay { get; set; }
        public decimal TongChiPhiLuongLoai { get; set; }
        public decimal TyLeChiPhiLuongPhanTram { get; set; }
    }

    // Model cho thống kê theo ngày trong khoảng thời gian
    public class ThongKeDoanhThuChiPhiTheoNgay
    {
        public DateTime Ngay { get; set; }
        public int SoDonHang { get; set; }
        public int SoDonHoanThanh { get; set; }
        public decimal DoanhThuNgay { get; set; }
        public decimal ChiPhiNguyenLieuNgay { get; set; }
        public decimal ChiPhiLuongNgay { get; set; }
        public decimal TongChiPhiNgay { get; set; }
        public decimal LoiNhuanNgay { get; set; }
    }

    // Model cho form tìm kiếm thống kê doanh thu chi phí
    public class ThongKeDoanhThuChiPhiSearchModel
    {
        [Display(Name = "Từ ngày")]
        [DataType(DataType.Date)]
        public DateTime? TuNgay { get; set; }

        [Display(Name = "Đến ngày")]
        [DataType(DataType.Date)]
        public DateTime? DenNgay { get; set; }

        [Display(Name = "Ngày cụ thể")]
        [DataType(DataType.Date)]
        public DateTime? NgayCuThe { get; set; }

        public string LoaiThongKe { get; set; } = "ngay"; // "ngay", "khoang_thoi_gian", "ngay_hien_tai"
    }

    // Model tổng hợp cho dashboard
    public class DashboardThongKe
    {
        public ThongKeDoanhThuChiPhiTongQuan? ThongKeNgay { get; set; }
        public List<ThongKeDoanhThuTheoLoaiMon>? DoanhThuTheoLoaiMon { get; set; }
        public List<ThongKeChiPhiTheoNguyenLieu>? ChiPhiTheoNguyenLieu { get; set; }
        public List<ThongKeChiPhiLuongTheoLoaiNhanVien>? ChiPhiLuongTheoLoaiNhanVien { get; set; }
    }
}
