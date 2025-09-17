using System.ComponentModel.DataAnnotations;

namespace BTL.Web.Models
{
    public class MonWithCongThuc
    {
        public Mon Mon { get; set; } = null!;
        public IEnumerable<CongThucDetail> CongThucs { get; set; } = new List<CongThucDetail>();
    }

    public class CongThucDetail
    {
        public int nl_id { get; set; }
        public string ten_nguyen_lieu { get; set; } = string.Empty;
        public string don_vi { get; set; } = string.Empty;
        public string nguon_goc { get; set; } = string.Empty;
        public decimal dinh_luong { get; set; }
        public decimal gia_nguyen_lieu { get; set; }
    }
}
