using System.ComponentModel.DataAnnotations;

namespace BTL.Web.Models
{
    // Model cho thống kê tần suất món ăn theo tháng
    public class ThongKeMonAnTanSuat
    {
        public int mon_id { get; set; }
        public string ten_mon { get; set; } = string.Empty;
        public string loai_mon { get; set; } = string.Empty;
        public decimal gia { get; set; }
        public int so_lan_duoc_dat { get; set; }
        public int tong_so_luong_dat { get; set; }
        public int so_don_hang_chua_mon { get; set; }
        public int so_ngay_co_dat { get; set; }
        public decimal trung_binh_so_luong_moi_lan { get; set; }
        public decimal tong_doanh_thu_mon { get; set; }
        public decimal ty_le_don_hang_chua_mon { get; set; }
        public decimal ty_le_so_luong_dat { get; set; }
        public decimal tan_suat_trung_binh_ngay { get; set; }
        public int xep_hang_so_lan_dat { get; set; }
        public int xep_hang_so_luong_dat { get; set; }
        public int xep_hang_doanh_thu { get; set; }
    }

    // Model cho thống kê chi tiết theo ngày
    public class ThongKeMonAnChiTiet
    {
        public DateTime ngay { get; set; }
        public int mon_id { get; set; }
        public string ten_mon { get; set; } = string.Empty;
        public string loai_mon { get; set; } = string.Empty;
        public int so_lan_duoc_dat { get; set; }
        public int tong_so_luong_dat { get; set; }
        public int so_don_hang_chua_mon { get; set; }
        public decimal doanh_thu_ngay { get; set; }
    }

    // Model cho top món ăn
    public class ThongKeMonAnTop
    {
        public int mon_id { get; set; }
        public string ten_mon { get; set; } = string.Empty;
        public string loai_mon { get; set; } = string.Empty;
        public decimal gia { get; set; }
        public int so_lan_duoc_dat { get; set; }
        public int tong_so_luong_dat { get; set; }
        public int so_don_hang_chua_mon { get; set; }
        public decimal tong_doanh_thu_mon { get; set; }
        public decimal trung_binh_so_luong_moi_lan { get; set; }
        public int xep_hang_so_lan_dat { get; set; }
        public int xep_hang_so_luong_dat { get; set; }
        public int xep_hang_doanh_thu { get; set; }
    }

    // Model cho so sánh tháng
    public class ThongKeMonAnSoSanh
    {
        public int mon_id { get; set; }
        public string ten_mon { get; set; } = string.Empty;
        public string loai_mon { get; set; } = string.Empty;

        // Tháng 1
        public int thang1_so_lan_duoc_dat { get; set; }
        public int thang1_tong_so_luong_dat { get; set; }
        public int thang1_so_don_hang_chua_mon { get; set; }
        public decimal thang1_tong_doanh_thu_mon { get; set; }

        // Tháng 2
        public int thang2_so_lan_duoc_dat { get; set; }
        public int thang2_tong_so_luong_dat { get; set; }
        public int thang2_so_don_hang_chua_mon { get; set; }
        public decimal thang2_tong_doanh_thu_mon { get; set; }

        // So sánh
        public int chenh_lech_so_lan_dat { get; set; }
        public int chenh_lech_so_luong_dat { get; set; }
        public int chenh_lech_don_hang { get; set; }
        public decimal chenh_lech_doanh_thu { get; set; }
        public decimal ty_le_thay_doi_so_lan_dat { get; set; }
        public decimal ty_le_thay_doi_so_luong_dat { get; set; }
    }

    // Model cho thống kê theo loại món
    public class ThongKeMonAnTheoLoai
    {
        public string loai_mon { get; set; } = string.Empty;
        public int so_mon_trong_loai { get; set; }
        public int so_lan_duoc_dat { get; set; }
        public int tong_so_luong_dat { get; set; }
        public int so_don_hang_chua_loai { get; set; }
        public decimal tong_doanh_thu_loai { get; set; }
        public decimal trung_binh_so_luong_moi_lan { get; set; }
        public decimal ty_le_don_hang_chua_loai { get; set; }
        public decimal ty_le_so_luong_dat { get; set; }
        public decimal ty_le_doanh_thu { get; set; }
        public int xep_hang_so_lan_dat { get; set; }
        public int xep_hang_doanh_thu { get; set; }
    }

    // Model cho request thống kê
    public class ThongKeMonAnRequest
    {
        [Required(ErrorMessage = "Tháng là bắt buộc")]
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12")]
        public int Thang { get; set; }

        [Required(ErrorMessage = "Năm là bắt buộc")]
        [Range(2000, 2100, ErrorMessage = "Năm phải từ 2000 đến 2100")]
        public int Nam { get; set; }
    }

    // Model cho request so sánh
    public class ThongKeMonAnSoSanhRequest
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

    // Model cho request top món ăn
    public class ThongKeMonAnTopRequest
    {
        [Required(ErrorMessage = "Tháng là bắt buộc")]
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12")]
        public int Thang { get; set; }

        [Required(ErrorMessage = "Năm là bắt buộc")]
        [Range(2000, 2100, ErrorMessage = "Năm phải từ 2000 đến 2100")]
        public int Nam { get; set; }

        [Range(1, 100, ErrorMessage = "Số lượng top phải từ 1 đến 100")]
        public int TopN { get; set; } = 10;
    }

    // Model cho dashboard
    public class ThongKeMonAnDashboard
    {
        public List<ThongKeMonAnTanSuat> ThongKeTanSuat { get; set; } = new();
        public List<ThongKeMonAnTop> TopMonAn { get; set; } = new();
        public List<ThongKeMonAnTheoLoai> ThongKeTheoLoai { get; set; } = new();
        public int TongSoMon { get; set; }
        public int TongSoMonCoDat { get; set; }
        public int TongSoLanDat { get; set; }
        public decimal TongDoanhThu { get; set; }
        public decimal TyLeMonCoDat { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
    }
}
