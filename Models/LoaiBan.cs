using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Web.Models
{
    public class LoaiBan
    {
        [Key]
        public int loai_ban_id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Sức chứa phải lớn hơn 0")]
        public int suc_chua { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public int so_luong { get; set; }

        // Navigation properties
        public virtual ICollection<BanAn> BanAns { get; set; } = new List<BanAn>();
    }
}
