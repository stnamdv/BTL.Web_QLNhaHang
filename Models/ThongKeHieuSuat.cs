namespace BTL.Web.Models
{
    public class ThongKeHieuSuat
    {
        public int nv_id { get; set; }
        public string nv_ho_ten { get; set; } = string.Empty;
        public string loai_nv { get; set; } = string.Empty;
        public int buoc_id { get; set; }
        public string ten_buoc { get; set; } = string.Empty;
        public int so_luong_thuc_hien { get; set; }
        public double? thoi_gian_trung_binh_phut { get; set; }
        public int? thoi_gian_nhanh_nhat_phut { get; set; }
        public int? thoi_gian_cham_nhat_phut { get; set; }
    }
}
