using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public class LoaiNhanVien
    {
        [Key]
        public int loai_nv_id { get; set; }

        [Required]
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
