using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public class BuocXuLy
    {
        [Key]
        public int buoc_id { get; set; }

        [Required(ErrorMessage = "Tên bước không được để trống")]
        [StringLength(100, ErrorMessage = "Tên bước không được vượt quá 100 ký tự")]
        public string ten_buoc { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? mo_ta { get; set; }

        [Required(ErrorMessage = "Thứ tự không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Thứ tự phải lớn hơn 0")]
        public int thu_tu { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Thời gian dự kiến không được âm")]
        public int? thoi_gian_du_kien { get; set; }

        // Navigation properties
        public virtual ICollection<PhanCongBuocXuLy> PhanCongBuocXuLys { get; set; } = new List<PhanCongBuocXuLy>();
        public virtual ICollection<LichSuThucHien> LichSuThucHiens { get; set; } = new List<LichSuThucHien>();
    }
}
