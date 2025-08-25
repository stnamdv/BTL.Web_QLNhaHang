using System.ComponentModel.DataAnnotations;

namespace BTL.Web.Models
{
    public class Layout
    {
        public int layout_id { get; set; }

        [Required(ErrorMessage = "Tên layout không được để trống")]
        [StringLength(100, ErrorMessage = "Tên layout không được quá 100 ký tự")]
        public string layout_name { get; set; } = string.Empty;

        [Range(5, 20, ErrorMessage = "Kích thước grid phải từ 5 đến 20")]
        public int grid_size { get; set; } = 10;

        public DateTime created_date { get; set; }
        public DateTime updated_date { get; set; }
        public bool is_active { get; set; } = true;

        // Navigation property
        public List<LayoutDetail> LayoutDetails { get; set; } = new List<LayoutDetail>();
    }

    public class LayoutDetail
    {
        public int layout_detail_id { get; set; }
        public int layout_id { get; set; }
        public int ban_id { get; set; }
        public float position_x { get; set; }
        public float position_y { get; set; }
        public DateTime created_date { get; set; }

        // Thông tin bàn (từ join)
        public string so_hieu { get; set; } = string.Empty;
        public int suc_chua { get; set; }
        public int loai_ban_id { get; set; }

        // Navigation properties
        public Layout Layout { get; set; } = null!;
        public BanAn BanAn { get; set; } = null!;
    }

    public class LayoutSaveRequest
    {
        [Required(ErrorMessage = "Tên layout không được để trống")]
        public string layout_name { get; set; } = string.Empty;

        [Range(5, 20, ErrorMessage = "Kích thước grid phải từ 5 đến 20")]
        public int grid_size { get; set; } = 10;

        public List<LayoutTableData> tables { get; set; } = new List<LayoutTableData>();
    }

    public class LayoutTableData
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public int capacity { get; set; }
        public int type { get; set; }
        public LayoutPosition position { get; set; } = new LayoutPosition();
    }

    public class LayoutPosition
    {
        public float x { get; set; }
        public float y { get; set; }
    }

    public class LayoutResponse
    {
        public int layout_id { get; set; }
        public string layout_name { get; set; } = string.Empty;
        public int grid_size { get; set; }
        public DateTime created_date { get; set; }
        public DateTime updated_date { get; set; }
        public int table_count { get; set; }
        public List<LayoutTableData> tables { get; set; } = new List<LayoutTableData>();
    }
}
