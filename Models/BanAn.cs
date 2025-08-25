using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public class BanAn
    {
        [Key]
        public int ban_id { get; set; }

        [Required]
        public int loai_ban_id { get; set; }

        [Required]
        [StringLength(10)]
        public string so_hieu { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("loai_ban_id")]
        public virtual LoaiBan LoaiBan { get; set; } = null!;

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
