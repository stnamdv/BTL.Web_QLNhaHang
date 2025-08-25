using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public class CongThuc
    {
        [Key]
        public int mon_id { get; set; }

        [Key]
        public int nl_id { get; set; }

        [Required]
        [Range(0.001, double.MaxValue, ErrorMessage = "Định lượng phải lớn hơn 0")]
        [Column(TypeName = "decimal(14,3)")]
        public decimal dinh_luong { get; set; }

        // Navigation properties
        [ForeignKey("mon_id")]
        public virtual Mon Mon { get; set; } = null!;

        [ForeignKey("nl_id")]
        public virtual NguyenLieu NguyenLieu { get; set; } = null!;
    }
}
