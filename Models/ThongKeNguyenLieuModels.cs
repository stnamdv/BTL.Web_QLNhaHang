using System.ComponentModel.DataAnnotations;

namespace BTL.Web.Models
{
    // Model cho thống kê tổng quan nguyên liệu sử dụng
    public class ThongKeNguyenLieuTongQuan
    {
        public DateTime Ngay { get; set; }
        public int SoLoaiNguyenLieuSuDung { get; set; }
        public decimal TongLuongNguyenLieuSuDung { get; set; }
        public decimal TongChiPhiNguyenLieu { get; set; }
    }

    // Model cho thống kê chi tiết theo từng nguyên liệu
    public class ThongKeNguyenLieuChiTiet
    {
        public int NlId { get; set; }
        public string TenNguyenLieu { get; set; } = string.Empty;
        public string DonVi { get; set; } = string.Empty;
        public string NguonGoc { get; set; } = string.Empty;
        public decimal GiaDonVi { get; set; }
        public string NhaCungCap { get; set; } = string.Empty;
        public decimal TongLuongSuDung { get; set; }
        public int SoMonSuDung { get; set; }
        public decimal TongChiPhiNguyenLieu { get; set; }
        public decimal TyLeChiPhiPhanTram { get; set; }
    }

    // Model cho thống kê theo đơn vị
    public class ThongKeNguyenLieuTheoDonVi
    {
        public string DonVi { get; set; } = string.Empty;
        public int SoLoaiNguyenLieu { get; set; }
        public decimal TongLuongSuDung { get; set; }
        public decimal TongChiPhiTheoDonVi { get; set; }
        public decimal TyLeChiPhiPhanTram { get; set; }
        public decimal? ChiPhiTrungBinhNgayTheoDonVi { get; set; }
    }

    // Model cho thống kê chi tiết món ăn sử dụng nguyên liệu
    public class ThongKeMonAnSuDungNguyenLieu
    {
        public int NlId { get; set; }
        public string TenNguyenLieu { get; set; } = string.Empty;
        public string MaMon { get; set; } = string.Empty;
        public string TenMon { get; set; } = string.Empty;
        public string LoaiMon { get; set; } = string.Empty;
        public decimal DinhLuongCho1Mon { get; set; }
        public int TongSoMonDuocDat { get; set; }
        public decimal TongLuongNguyenLieuChoMon { get; set; }
        public decimal ChiPhiNguyenLieuChoMon { get; set; }
    }

    // Model cho thống kê theo khoảng thời gian
    public class ThongKeNguyenLieuKhoangThoiGian
    {
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public int SoNgay { get; set; }
        public int SoLoaiNguyenLieuSuDung { get; set; }
        public decimal TongLuongNguyenLieuSuDung { get; set; }
        public decimal TongChiPhiNguyenLieu { get; set; }
        public decimal ChiPhiNguyenLieuTrungBinhNgay { get; set; }
    }

    // Model cho thống kê theo ngày trong khoảng thời gian
    public class ThongKeNguyenLieuTheoNgay
    {
        public DateTime Ngay { get; set; }
        public int SoLoaiNguyenLieuSuDung { get; set; }
        public decimal TongLuongNguyenLieuSuDung { get; set; }
        public decimal ChiPhiNguyenLieuNgay { get; set; }
    }

    // Model cho form tìm kiếm thống kê
    public class ThongKeNguyenLieuSearchModel
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
}
