using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public class HoaDon
    {
        [Key]
        public int hd_id { get; set; }

        [Required]
        public int order_id { get; set; }

        [Required]
        public DateTime thoi_diem_tt { get; set; }

        [StringLength(20)]
        public string? phuong_thuc { get; set; }

        // Navigation properties
        [ForeignKey("order_id")]
        public virtual Order Order { get; set; } = null!;
    }
}
