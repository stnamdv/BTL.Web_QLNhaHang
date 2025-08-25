using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public class KhachHang
    {
        [Key]
        public int kh_id { get; set; }

        [StringLength(120)]
        public string? ho_ten { get; set; }

        [StringLength(20)]
        public string? so_dien_thoai { get; set; }

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
