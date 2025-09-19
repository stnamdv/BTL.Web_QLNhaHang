using System.ComponentModel.DataAnnotations;

namespace BTL.Web.Models
{
    public class NguyenLieuWithNhaCungCap
    {
        public int nl_id { get; set; }
        public string ten { get; set; } = string.Empty;
        public string don_vi { get; set; } = string.Empty;
        public string nguon_goc { get; set; } = string.Empty;
        public decimal gia_nhap { get; set; }
        public int ncc_id { get; set; }
        public string ncc_ten { get; set; } = string.Empty;
        public string? ncc_dia_chi { get; set; }
        public string? ncc_sdt { get; set; }
        public int so_mon_su_dung { get; set; }
        public IEnumerable<MonSuDungNguyenLieu> Mons { get; set; } = new List<MonSuDungNguyenLieu>();
    }
}
