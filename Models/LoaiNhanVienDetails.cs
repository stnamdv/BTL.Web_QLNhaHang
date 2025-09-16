using System.ComponentModel.DataAnnotations;

namespace BTL.Web.Models
{
    public class LoaiNhanVienDetails
    {
        public string loai_nv { get; set; } = string.Empty;
        public decimal luong_co_ban { get; set; }
        public int total_employees { get; set; }
        public int active_employees { get; set; }
    }

    public class NhanVienInfo
    {
        public int nv_id { get; set; }
        public string ho_ten { get; set; } = string.Empty;
        public string loai_nv { get; set; } = string.Empty;
        public string trang_thai { get; set; } = string.Empty;
        public bool has_active_orders { get; set; }
    }

    public class LoaiNhanVienUpdateCheck
    {
        public bool can_update { get; set; }
        public string error_message { get; set; } = string.Empty;
        public int employee_count { get; set; }
        public int active_employee_count { get; set; }
    }

    public class LoaiNhanVienDeleteCheck
    {
        public bool can_delete { get; set; }
        public string error_message { get; set; } = string.Empty;
        public int employee_count { get; set; }
        public int active_employee_count { get; set; }
    }

    public class LoaiNhanVienValidation
    {
        public bool is_valid { get; set; }
        public string message { get; set; } = string.Empty;
    }
}
