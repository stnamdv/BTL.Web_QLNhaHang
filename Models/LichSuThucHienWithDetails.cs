using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public class LichSuThucHienWithDetails
    {
        public int lich_su_id { get; set; }
        public int order_item_id { get; set; }
        public int order_id { get; set; }
        public int? ban_id { get; set; }
        public string? ban_so_hieu { get; set; }
        public int mon_id { get; set; }
        public string ten_mon { get; set; } = string.Empty;
        public int buoc_id { get; set; }
        public string ten_buoc { get; set; } = string.Empty;
        public int thu_tu { get; set; }
        public int nv_id { get; set; }
        public string nv_ho_ten { get; set; } = string.Empty;
        public string loai_nv { get; set; } = string.Empty;
        public string trang_thai { get; set; } = string.Empty;
        public DateTime? thoi_diem_bat_dau { get; set; }
        public DateTime? thoi_diem_ket_thuc { get; set; }
        public string? ghi_chu { get; set; }
        public DateTime thoi_diem_tao { get; set; }

        // Thời gian thực hiện (phút)
        public int? thoi_gian_thuc_hien_phut { get; set; }
    }
}
