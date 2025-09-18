using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public class NguyenLieu
    {
        [Key]
        public int nl_id { get; set; }

        [Required]
        [StringLength(160)]
        public string ten { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string don_vi { get; set; } = string.Empty;

        [Required]
        public string nguon_goc { get; set; } = string.Empty;

        [Required]
        public int ncc_id { get; set; }

        // Navigation properties
        [ForeignKey("ncc_id")]
        public virtual NhaCungCap? NhaCungCap { get; set; } = null!;
        public virtual ICollection<NlNcc> NlNccs { get; set; } = new List<NlNcc>();
        public virtual ICollection<CongThuc> CongThucs { get; set; } = new List<CongThuc>();
    }
}
