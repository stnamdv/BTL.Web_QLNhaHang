using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public class BuocXuLyWithPhanCong
    {
        public int buoc_id { get; set; }
        public string ten_buoc { get; set; } = string.Empty;
        public string? mo_ta { get; set; }
        public int thu_tu { get; set; }
        public int? thoi_gian_du_kien { get; set; }

        // Thông tin phân công
        public int phan_cong_buoc_id { get; set; }
        public int loai_nv_id { get; set; }
        public string loai_nv { get; set; } = string.Empty;
        public string? vai_tro { get; set; }
        public string trang_thai { get; set; } = string.Empty;
        public DateTime thoi_diem_tao { get; set; }
    }
}
