using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public class NhaCungCap
    {
        [Key]
        public int ncc_id { get; set; }

        [Required]
        [StringLength(160)]
        public string ten { get; set; } = string.Empty;

        public string? dia_chi { get; set; }

        [StringLength(20)]
        public string? sdt { get; set; }

    }
}
