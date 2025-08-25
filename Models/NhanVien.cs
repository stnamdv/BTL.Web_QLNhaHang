using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public enum TrangThaiNv
    {
        ACTIVE,
        INACTIVE
    }

    public class NhanVien
    {
        [Key]
        public int nv_id { get; set; }

        [Required]
        [StringLength(120)]
        public string ho_ten { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public LoaiNv loai_nv { get; set; }

        public DateTime? ngay_vao_lam { get; set; }

        [Required]
        [StringLength(20)]
        public TrangThaiNv trang_thai { get; set; } = TrangThaiNv.ACTIVE;

        // Navigation properties
        [ForeignKey("loai_nv")]
        public virtual LoaiNhanVien LoaiNhanVien { get; set; } = null!;
    }
}
