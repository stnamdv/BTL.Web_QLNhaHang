using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public class Order
    {
        [Key]
        public int order_id { get; set; }

        public int? kh_id { get; set; }

        public int? ban_id { get; set; }

        [Required]
        public bool la_mang_ve { get; set; } = false;

        [Required]
        public string trang_thai { get; set; } = "pending";

        public int? so_khach { get; set; }

        [Required]
        public DateTime thoi_diem_dat { get; set; }

        public decimal? tong_tien { get; set; }

        // Navigation properties
        [ForeignKey("kh_id")]
        public virtual KhachHang? KhachHang { get; set; }

        [ForeignKey("ban_id")]
        public virtual BanAn? BanAn { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<LichSuThucHien> LichSuThucHien { get; set; } = new List<LichSuThucHien>();
        public virtual HoaDon? HoaDon { get; set; }
    }
}
