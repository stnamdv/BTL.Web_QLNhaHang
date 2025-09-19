using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public static class TrangThaiThucHien
    {
        public const string CHUA_HOAN_THANH = "CHUA_HOAN_THANH";
        public const string HOAN_THANH = "HOAN_THANH";
        public const string DA_HUY = "DA_HUY";

        public static readonly string[] All = { CHUA_HOAN_THANH, HOAN_THANH, DA_HUY };

        public static string GetDisplayName(string trangThai)
        {
            return trangThai switch
            {
                CHUA_HOAN_THANH => "Chưa bắt đầu",
                HOAN_THANH => "Hoàn thành",
                DA_HUY => "Đã hủy",
                _ => trangThai
            };
        }
    }

    public class LichSuThucHien
    {
        [Key]
        public int lich_su_id { get; set; }

        [Required]
        public int order_id { get; set; }

        [Required]
        public int buoc_id { get; set; }

        [Required]
        public int nv_id { get; set; }

        [Required]
        [StringLength(20)]
        public string trang_thai { get; set; } = TrangThaiThucHien.CHUA_HOAN_THANH;

        public DateTime? thoi_diem_bat_dau { get; set; }

        public DateTime? thoi_diem_ket_thuc { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? ghi_chu { get; set; }

        [Required]
        public DateTime thoi_diem_tao { get; set; } = DateTime.Now;

        // Properties from joins (for stored procedure results)
        public string? ten_buoc { get; set; }
        public string? nhan_vien_ten { get; set; }

        // Navigation properties
        [ForeignKey("order_id")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("buoc_id")]
        public virtual BuocXuLy BuocXuLy { get; set; } = null!;

        [ForeignKey("nv_id")]
        public virtual NhanVien NhanVien { get; set; } = null!;
    }
}
