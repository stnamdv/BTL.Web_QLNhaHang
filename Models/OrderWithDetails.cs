using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public class OrderWithDetails
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

        // Thông tin bàn
        public string? ban_so_hieu { get; set; }
        public int? ban_suc_chua { get; set; }

        // Thông tin khách hàng
        public string? khach_hang_ten { get; set; }
        public string? khach_hang_sdt { get; set; }

        // Thống kê
        public int so_mon { get; set; }
        public int tong_so_luong { get; set; }

        // Navigation properties
        [ForeignKey("kh_id")]
        public virtual KhachHang? KhachHang { get; set; }

        [ForeignKey("ban_id")]
        public virtual BanAn? BanAn { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<LichSuThucHien> LichSuThucHiens { get; set; } = new List<LichSuThucHien>();
    }
}
