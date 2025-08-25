using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public class NlNcc
    {
        [Key]
        public int phieu_nhap_id { get; set; }

        [Required]
        public int nl_id { get; set; }

        [Required]
        public int ncc_id { get; set; }

        [Required]
        public DateTime ngay_nhap { get; set; }

        [Required]
        [Range(0.001, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Column(TypeName = "decimal(14,3)")]
        public decimal so_luong { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá không được âm")]
        [Column(TypeName = "decimal(14,2)")]
        public decimal don_gia { get; set; }

        // Navigation properties
        [ForeignKey("nl_id")]
        public virtual NguyenLieu NguyenLieu { get; set; } = null!;

        [ForeignKey("ncc_id")]
        public virtual NhaCungCap NhaCungCap { get; set; } = null!;
    }
}
