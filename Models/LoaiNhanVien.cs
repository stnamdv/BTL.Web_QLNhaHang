using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public enum LoaiNv
    {
        DAU_BEP,
        PHUC_VU,
        DICH_VU,
        THU_NGAN
    }

    public class LoaiNhanVien
    {
        [Key]
        [StringLength(20)]
        public LoaiNv loai_nv { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Lương cơ bản không được âm")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal luong_co_ban { get; set; }

        // Navigation properties
        public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();
    }
}
