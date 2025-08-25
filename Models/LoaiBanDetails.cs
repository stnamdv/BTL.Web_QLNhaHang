using System.ComponentModel.DataAnnotations;

namespace BTL.Web.Models
{
    public class LoaiBanDetails
    {
        public int loai_ban_id { get; set; }
        public int suc_chua { get; set; }
        public int so_luong { get; set; }
    }

    public class BanAnInfo
    {
        public int ban_id { get; set; }
        public string so_hieu { get; set; } = string.Empty;
        public int loai_ban_id { get; set; }
        public bool has_active_orders { get; set; }
    }

    public class LoaiBanUpdateCheck
    {
        public bool can_update { get; set; }
        public string error_message { get; set; } = string.Empty;
        public int ban_count { get; set; }
        public int active_order_count { get; set; }
    }

    public class LoaiBanDeleteCheck
    {
        public bool can_delete { get; set; }
        public string error_message { get; set; } = string.Empty;
        public int ban_count { get; set; }
        public int active_order_count { get; set; }
    }
}
