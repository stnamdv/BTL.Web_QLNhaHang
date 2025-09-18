using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public static class TrangThaiThucHien
    {
        public const string CHUA_BAT_DAU = "CHUA_BAT_DAU";
        public const string DANG_THUC_HIEN = "DANG_THUC_HIEN";
        public const string HOAN_THANH = "HOAN_THANH";

        public static readonly string[] All = { CHUA_BAT_DAU, DANG_THUC_HIEN, HOAN_THANH };

        public static string GetDisplayName(string trangThai)
        {
            return trangThai switch
            {
                CHUA_BAT_DAU => "Chưa bắt đầu",
                DANG_THUC_HIEN => "Đang thực hiện",
                HOAN_THANH => "Hoàn thành",
                _ => trangThai
            };
        }
    }

    public class LichSuThucHien
    {
        [Key]
        public int lich_su_id { get; set; }

        [Required]
        public int order_item_id { get; set; }

        [Required]
        public int buoc_id { get; set; }

        [Required]
        public int nv_id { get; set; }

        [Required]
        [StringLength(20)]
        public string trang_thai { get; set; } = TrangThaiThucHien.CHUA_BAT_DAU;

        public DateTime? thoi_diem_bat_dau { get; set; }

        public DateTime? thoi_diem_ket_thuc { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? ghi_chu { get; set; }

        [Required]
        public DateTime thoi_diem_tao { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("order_item_id")]
        public virtual OrderItem OrderItem { get; set; } = null!;

        [ForeignKey("buoc_id")]
        public virtual BuocXuLy BuocXuLy { get; set; } = null!;

        [ForeignKey("nv_id")]
        public virtual NhanVien NhanVien { get; set; } = null!;
    }
}
