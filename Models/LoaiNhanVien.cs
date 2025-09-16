using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public static class LoaiNv
    {
        public const string BEP = "BEP";
        public const string PHUC_VU = "PHUC_VU";
        public const string DICH_VU = "DICH_VU";
        public const string THU_NGAN = "THU_NGAN";

        public static readonly string[] All = { BEP, PHUC_VU, DICH_VU, THU_NGAN };

        public static string GetDisplayName(string loaiNv)
        {
            return loaiNv switch
            {
                BEP => "Đầu bếp",
                PHUC_VU => "Phục vụ",
                DICH_VU => "Dịch vụ",
                THU_NGAN => "Thu ngân",
                _ => loaiNv
            };
        }
    }

    public class LoaiNhanVien
    {
        [Key]
        [StringLength(20)]
        public string loai_nv { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Lương cơ bản không được âm")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal luong_co_ban { get; set; }

        // Navigation properties
        public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();
    }
}
