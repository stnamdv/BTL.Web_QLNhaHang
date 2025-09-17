using System.ComponentModel.DataAnnotations;

namespace BTL.Web.Models
{
    public class NguyenLieuWithMon
    {
        public NguyenLieu NguyenLieu { get; set; } = null!;
        public IEnumerable<MonSuDungNguyenLieu> Mons { get; set; } = new List<MonSuDungNguyenLieu>();
    }

    public class MonSuDungNguyenLieu
    {
        public int mon_id { get; set; }
        public string ma_mon { get; set; } = string.Empty;
        public string ten_mon { get; set; } = string.Empty;
        public string loai_mon { get; set; } = string.Empty;
        public decimal dinh_luong { get; set; }
    }
}
