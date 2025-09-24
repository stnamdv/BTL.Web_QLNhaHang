using System.ComponentModel.DataAnnotations;

namespace BTL.Web.Models
{
    // Model cho kết quả thống kê nhà cung cấp chính
    public class ThongKeNhaCungCap
    {
        public int ncc_id { get; set; }
        public string ten_nha_cung_cap { get; set; } = string.Empty;
        public string? dia_chi { get; set; }
        public string? sdt { get; set; }
        public int tong_so_nguyen_lieu { get; set; }
        public int tong_so_mon_su_dung { get; set; }
        public decimal tong_luong_su_dung { get; set; }
        public decimal tong_gia_tri_su_dung { get; set; }
        public int tong_so_don_hang { get; set; }
        public int tong_so_mon_duoc_dat { get; set; }
        public decimal trung_binh_luong_su_dung_per_nguyen_lieu { get; set; }
        public decimal trung_binh_luong_su_dung_per_mon { get; set; }

        // Danh sách lượng theo đơn vị (được populate từ result set thứ 2)
        public List<LuuongTheoDonVi> LuongTheoDonVi { get; set; } = new List<LuuongTheoDonVi>();
    }

    // Model cho lượng theo đơn vị
    public class LuuongTheoDonVi
    {
        public int ncc_id { get; set; }
        public string don_vi { get; set; } = string.Empty;
        public decimal tong_luong_theo_don_vi { get; set; }
    }

    // Model cho chi tiết nguyên liệu của nhà cung cấp
    public class ChiTietNguyenLieuNhaCungCap
    {
        public int nl_id { get; set; }
        public string ten_nguyen_lieu { get; set; } = string.Empty;
        public string don_vi { get; set; } = string.Empty;
        public decimal gia_nhap { get; set; }
        public int so_mon_su_dung { get; set; }
        public decimal luong_su_dung { get; set; }
        public decimal gia_tri_su_dung { get; set; }
        public int so_don_hang { get; set; }
        public int so_lan_dat { get; set; }
    }

    // Model cho so sánh thống kê giữa 2 tháng
    public class SoSanhThongKeNhaCungCap
    {
        public int ncc_id { get; set; }
        public string ten_nha_cung_cap { get; set; } = string.Empty;
        public decimal luong_su_dung_thang1 { get; set; }
        public decimal gia_tri_su_dung_thang1 { get; set; }
        public int so_don_hang_thang1 { get; set; }
        public decimal luong_su_dung_thang2 { get; set; }
        public decimal gia_tri_su_dung_thang2 { get; set; }
        public int so_don_hang_thang2 { get; set; }
        public decimal chenh_lech_luong { get; set; }
        public decimal chenh_lech_gia_tri { get; set; }
        public int chenh_lech_don_hang { get; set; }
        public decimal phan_tram_tang_luong { get; set; }
        public decimal phan_tram_tang_gia_tri { get; set; }
    }

    // Model cho top nguyên liệu của nhà cung cấp
    public class TopNguyenLieuNhaCungCap
    {
        public int ncc_id { get; set; }
        public string ten_nha_cung_cap { get; set; } = string.Empty;
        public int nl_id { get; set; }
        public string ten_nguyen_lieu { get; set; } = string.Empty;
        public string don_vi { get; set; } = string.Empty;
        public decimal gia_nhap { get; set; }
        public decimal luong_su_dung { get; set; }
        public decimal gia_tri_su_dung { get; set; }
        public int so_mon_su_dung { get; set; }
        public int so_don_hang { get; set; }
        public int thu_hang { get; set; }
    }

    // Model cho thống kê tổng hợp
    public class ThongKeTongHopNhaCungCap
    {
        public int tong_so_nha_cung_cap { get; set; }
        public int tong_so_nguyen_lieu { get; set; }
        public int tong_so_mon { get; set; }
        public int tong_so_don_hang { get; set; }
        public int tong_so_mon_duoc_dat { get; set; }
        public decimal tong_luong_nguyen_lieu_su_dung { get; set; }
        public decimal tong_gia_tri_nguyen_lieu_su_dung { get; set; }
        public decimal trung_binh_luong_su_dung_per_mon { get; set; }
        public decimal trung_binh_gia_tri_su_dung_per_mon { get; set; }
    }

    // Model cho top nhà cung cấp
    public class TopNhaCungCap
    {
        public int ncc_id { get; set; }
        public string ten_nha_cung_cap { get; set; } = string.Empty;
        public decimal luong_su_dung { get; set; }
        public decimal gia_tri_su_dung { get; set; }
        public int so_don_hang { get; set; }
    }

    // Model cho top nguyên liệu
    public class TopNguyenLieu
    {
        public int nl_id { get; set; }
        public string ten_nguyen_lieu { get; set; } = string.Empty;
        public string ten_nha_cung_cap { get; set; } = string.Empty;
        public decimal luong_su_dung { get; set; }
        public decimal gia_tri_su_dung { get; set; }
        public int so_mon_su_dung { get; set; }
    }

    // Model cho request thống kê
    public class ThongKeNhaCungCapRequest
    {
        public int? Thang { get; set; }
        public int? Nam { get; set; }
        public int? NccId { get; set; }
    }

    // Model cho request so sánh tháng
    public class SoSanhThangRequest
    {
        [Required(ErrorMessage = "Tháng 1 là bắt buộc")]
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12")]
        public int Thang1 { get; set; }

        [Required(ErrorMessage = "Năm 1 là bắt buộc")]
        [Range(2020, 2030, ErrorMessage = "Năm phải từ 2020 đến 2030")]
        public int Nam1 { get; set; }

        [Required(ErrorMessage = "Tháng 2 là bắt buộc")]
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12")]
        public int Thang2 { get; set; }

        [Required(ErrorMessage = "Năm 2 là bắt buộc")]
        [Range(2020, 2030, ErrorMessage = "Năm phải từ 2020 đến 2030")]
        public int Nam2 { get; set; }

        public int? NccId { get; set; }
    }

    // Model cho request top nguyên liệu
    public class TopNguyenLieuRequest
    {
        public int? Thang { get; set; }
        public int? Nam { get; set; }
        public int? TopN { get; set; } = 5;
        public int? NccId { get; set; }
    }

    // Model cho kết quả tổng hợp đầy đủ
    public class ThongKeNhaCungCapTongHop
    {
        public ThongKeTongHopNhaCungCap TongHop { get; set; } = new();
        public List<TopNhaCungCap> TopNhaCungCap { get; set; } = new();
        public List<TopNguyenLieu> TopNguyenLieu { get; set; } = new();
    }

    // Model cho kết quả từ stored procedure sp_ThongKe_NhaCungCapNguyenLieu
    public class ThongKeNhaCungCapNguyenLieu
    {
        public int ncc_id { get; set; }
        public string ten { get; set; } = string.Empty;
        public string don_vi { get; set; } = string.Empty;
        public decimal tong_su_dung { get; set; }
    }

    // Model cho kết quả từ stored procedure sp_ThongKe_NhaCungCapNguyenLieu_ChiTiet
    public class ChiTietNhaCungCapNguyenLieu
    {
        public int ncc_id { get; set; }
        public string ten { get; set; } = string.Empty;
        public string don_vi { get; set; } = string.Empty;
        public string ten_nguyen_lieu { get; set; } = string.Empty;
        public int nl_id { get; set; }
        public decimal gia_nhap { get; set; }
        public decimal tong_su_dung { get; set; }
        public decimal tong_gia_tri { get; set; }
    }

    // Model cho kết quả từ stored procedure sp_ThongKe_NhaCungCapNguyenLieu_TongChi
    public class TongChiNhaCungCap
    {
        public int ncc_id { get; set; }
        public string ten { get; set; } = string.Empty;
        public string? dia_chi { get; set; }
        public string? sdt { get; set; }
        public decimal tong_gia_tri { get; set; }
    }
}
