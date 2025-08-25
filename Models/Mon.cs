using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public enum LoaiMon
    {
        KHAI_VI,
        MON_CHINH,
        TRANG_MIENG
    }

    public class Mon
    {
        [Key]
        public int mon_id { get; set; }

        [Required]
        [StringLength(20)]
        public string ma_mon { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string ten_mon { get; set; } = string.Empty;

        [Required]
        public LoaiMon loai_mon { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá không được âm")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal gia { get; set; }

        // Navigation properties
        public virtual ICollection<ThucDonMon> ThucDonMons { get; set; } = new List<ThucDonMon>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<CongThuc> CongThucs { get; set; } = new List<CongThuc>();
    }
}
