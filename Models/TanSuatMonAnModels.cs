using System.ComponentModel.DataAnnotations;

namespace BTL.Web.Models
{
    public class TanSuatMonAn
    {
        public int mon_id { get; set; }
        public string ten_mon { get; set; } = string.Empty;
        public int tong_so_luong { get; set; }
        public DateTime thoi_diem_dat { get; set; }
    }

    public class TanSuatMonAnSearchModel
    {
        [Display(Name = "Món ăn")]
        public int? MonId { get; set; }

        [Required(ErrorMessage = "Tháng là bắt buộc")]
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12")]
        [Display(Name = "Tháng")]
        public int Thang { get; set; } = DateTime.Now.Month;

        [Required(ErrorMessage = "Năm là bắt buộc")]
        [Range(2000, 2100, ErrorMessage = "Năm không hợp lệ")]
        [Display(Name = "Năm")]
        public int Nam { get; set; } = DateTime.Now.Year;
    }

    public class MonAn
    {
        public int mon_id { get; set; }
        public string ten_mon { get; set; } = string.Empty;
    }
}
