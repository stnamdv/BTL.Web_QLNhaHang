using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public class OrderItem
    {
        [Key]
        public int order_item_id { get; set; }

        [Required]
        public int order_id { get; set; }

        [Required]
        public int mon_id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int so_luong { get; set; }

        [Required]
        public DateTime t_dat { get; set; }

        public DateTime? t_hoan_thanh { get; set; }

        public DateTime? t_phuc_vu { get; set; }

        // Properties from Mon table join (for stored procedure results)
        public string? ten_mon { get; set; }
        public string? loai_mon { get; set; }
        public decimal? gia { get; set; }

        // Navigation properties
        [ForeignKey("order_id")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("mon_id")]
        public virtual Mon Mon { get; set; } = null!;
    }
}
