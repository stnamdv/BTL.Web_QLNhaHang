namespace BTL.Web.Models
{
    public class BanAnWithLoaiBan
    {
        public int ban_id { get; set; }
        public int loai_ban_id { get; set; }
        public string so_hieu { get; set; } = string.Empty;
        public int suc_chua { get; set; }
        public int loai_ban_so_luong { get; set; }
    }
}
