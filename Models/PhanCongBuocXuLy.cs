using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public class PhanCongBuocXuLy
    {
        [Key]
        public int phan_cong_buoc_id { get; set; }

        [Required]
        public int buoc_id { get; set; }

        [Required]
        public int loai_nv_id { get; set; }

        [StringLength(50, ErrorMessage = "Vai trò không được vượt quá 50 ký tự")]
        public string? vai_tro { get; set; }

        [Required]
        [StringLength(20)]
        public string trang_thai { get; set; } = "ACTIVE";

        [Required]
        public DateTime thoi_diem_tao { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("buoc_id")]
        public virtual BuocXuLy BuocXuLy { get; set; } = null!;

        [ForeignKey("loai_nv_id")]
        public virtual LoaiNhanVien LoaiNhanVien { get; set; } = null!;
    }
}
