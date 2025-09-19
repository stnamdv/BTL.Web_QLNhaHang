using System.ComponentModel.DataAnnotations;

namespace BTL.Web.Models
{
    // Model cho thống kê trung bình theo ngày
    public class ThongKeBanAnTrungBinh
    {
        public int suc_chua_ban { get; set; }
        public string ten_loai_ban { get; set; } = string.Empty;
        public int so_ngay_co_don_trong_thang { get; set; }
        public decimal trung_binh_ban_su_dung_ngay { get; set; }
        public decimal trung_binh_lan_dat_ban_ngay { get; set; }
        public decimal trung_binh_so_khach_ngay { get; set; }
        public int tong_ban_su_dung_thang { get; set; }
        public int tong_lan_dat_ban_thang { get; set; }
        public int tong_so_khach_thang { get; set; }
        public decimal ty_le_su_dung_trung_binh_ngay { get; set; }
    }

    // Model cho thống kê chi tiết theo ngày
    public class ThongKeBanAnChiTiet
    {
        public DateTime ngay { get; set; }
        public int suc_chua_ban { get; set; }
        public string ten_loai_ban { get; set; } = string.Empty;
        public int so_ban_su_dung { get; set; }
        public int so_lan_dat_ban { get; set; }
        public int tong_so_khach { get; set; }
        public decimal ty_le_su_dung_ngay { get; set; }
    }

    // Model cho thống kê tổng hợp
    public class ThongKeBanAnTongHop
    {
        public int suc_chua_ban { get; set; }
        public string ten_loai_ban { get; set; } = string.Empty;
        public int tong_so_ban_co_san { get; set; }
        public int so_ban_da_su_dung { get; set; }
        public int tong_so_don_hang { get; set; }
        public int tong_so_khach { get; set; }
        public decimal trung_binh_khach_moi_don { get; set; }
        public int so_ngay_hoat_dong { get; set; }
        public decimal ty_le_ban_da_su_dung { get; set; }
        public decimal trung_binh_don_hang_ngay { get; set; }
        public decimal trung_binh_khach_ngay { get; set; }
    }

    // Model cho so sánh tháng
    public class ThongKeBanAnSoSanh
    {
        public int suc_chua_ban { get; set; }
        public string ten_loai_ban { get; set; } = string.Empty;

        // Tháng 1
        public int thang1_so_ban_su_dung { get; set; }
        public int thang1_so_don_hang { get; set; }
        public int thang1_tong_khach { get; set; }
        public decimal thang1_trung_binh_khach_don { get; set; }

        // Tháng 2
        public int thang2_so_ban_su_dung { get; set; }
        public int thang2_so_don_hang { get; set; }
        public int thang2_tong_khach { get; set; }
        public decimal thang2_trung_binh_khach_don { get; set; }

        // So sánh
        public int chenh_lech_ban_su_dung { get; set; }
        public int chenh_lech_don_hang { get; set; }
        public int chenh_lech_tong_khach { get; set; }
        public decimal ty_le_thay_doi_ban_su_dung { get; set; }
        public decimal ty_le_thay_doi_don_hang { get; set; }
    }

    // Model cho request thống kê
    public class ThongKeBanAnRequest
    {
        [Required(ErrorMessage = "Tháng là bắt buộc")]
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12")]
        public int Thang { get; set; }

        [Required(ErrorMessage = "Năm là bắt buộc")]
        [Range(2000, 2100, ErrorMessage = "Năm phải từ 2000 đến 2100")]
        public int Nam { get; set; }
    }

    // Model cho request so sánh
    public class ThongKeBanAnSoSanhRequest
    {
        [Required(ErrorMessage = "Tháng 1 là bắt buộc")]
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12")]
        public int Thang1 { get; set; }

        [Required(ErrorMessage = "Năm 1 là bắt buộc")]
        [Range(2000, 2100, ErrorMessage = "Năm phải từ 2000 đến 2100")]
        public int Nam1 { get; set; }

        [Required(ErrorMessage = "Tháng 2 là bắt buộc")]
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12")]
        public int Thang2 { get; set; }

        [Required(ErrorMessage = "Năm 2 là bắt buộc")]
        [Range(2000, 2100, ErrorMessage = "Năm phải từ 2000 đến 2100")]
        public int Nam2 { get; set; }
    }

    // Model cho dashboard
    public class ThongKeBanAnDashboard
    {
        public List<ThongKeBanAnTrungBinh> ThongKeTrungBinh { get; set; } = new();
        public List<ThongKeBanAnTongHop> ThongKeTongHop { get; set; } = new();
        public int TongSoLoaiBan { get; set; }
        public int TongSoBanCoSan { get; set; }
        public int TongSoBanDaSuDung { get; set; }
        public decimal TyLeSuDungChung { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
    }
}
