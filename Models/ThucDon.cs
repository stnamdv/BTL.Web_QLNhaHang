using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public class ThucDon
    {
        [Key]
        public int thuc_don_id { get; set; }

        [Required]
        [StringLength(120)]
        public string ten { get; set; } = string.Empty;

        [Required]
        public DateTime hieu_luc_tu { get; set; }

        public DateTime? hieu_luc_den { get; set; }

        // Navigation properties
        public virtual ICollection<ThucDonMon> ThucDonMons { get; set; } = new List<ThucDonMon>();
    }
}
