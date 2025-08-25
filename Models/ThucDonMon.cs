using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public class ThucDonMon
    {
        [Key]
        public int thuc_don_id { get; set; }

        [Key]
        public int mon_id { get; set; }

        // Navigation properties
        [ForeignKey("thuc_don_id")]
        public virtual ThucDon ThucDon { get; set; } = null!;

        [ForeignKey("mon_id")]
        public virtual Mon Mon { get; set; } = null!;
    }
}
