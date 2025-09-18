using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public static class TrangThaiNv
    {
        public const string ACTIVE = "ACTIVE";
        public const string INACTIVE = "INACTIVE";

        public static readonly string[] All = { ACTIVE, INACTIVE };

        public static string GetDisplayName(string trangThai)
        {
            return trangThai switch
            {
                ACTIVE => "Đang làm việc",
                INACTIVE => "Đã nghỉ",
                _ => trangThai
            };
        }
    }

    public class NhanVien
    {
        [Key]
        public int nv_id { get; set; }

        public int loai_nv_id { get; set; }

        public string? ma_nv { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(120, ErrorMessage = "Họ tên không được vượt quá 120 ký tự")]
        public string ho_ten { get; set; } = string.Empty;


        [Display(Name = "Ngày vào làm")]
        public DateTime? ngay_vao_lam { get; set; }

        [Required]
        [StringLength(20)]
        public string trang_thai { get; set; } = TrangThaiNv.ACTIVE;

        // Navigation properties
        [ForeignKey("loai_nv_id")]
        public virtual LoaiNhanVien? LoaiNhanVien { get; set; }
    }
}
