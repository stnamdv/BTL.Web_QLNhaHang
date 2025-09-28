using System.ComponentModel.DataAnnotations;

namespace BTL.Web.Models
{
    public class TanSuatDatBan
    {
        public int suc_chua { get; set; }
        public double trung_binh_ngay { get; set; }
        public int ngay_trong_thang { get; set; }
    }

    public class TanSuatDatBanSearchModel
    {
        [Display(Name = "Sức chứa")]
        public int? SucChua { get; set; }

        [Display(Name = "Ngày trong tháng")]
        public int? NgayTrongThang { get; set; }
    }

    public class LoaiBanInfo
    {
        public int loai_ban_id { get; set; }
        public int suc_chua { get; set; }
        public int so_luong { get; set; }
    }
}
