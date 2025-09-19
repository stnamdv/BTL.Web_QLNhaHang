using System.ComponentModel.DataAnnotations;

namespace BTL.Web.Models
{
    public class LuongNhanVien
    {
        public int nv_id { get; set; }
        public string ma_nv { get; set; } = string.Empty;
        public string ho_ten { get; set; } = string.Empty;
        public string loai_nv { get; set; } = string.Empty;
        public decimal luong_co_ban { get; set; }
        public int tong_so_khach_trong_thang { get; set; }
        public int so_lan_thuong { get; set; }
        public decimal phan_tram_thuong { get; set; }
        public decimal tien_thuong { get; set; }
        public decimal tong_luong { get; set; }
        public string trang_thai { get; set; } = string.Empty;
        public DateTime? ngay_vao_lam { get; set; }
    }

    public class ThongTinTongQuanLuong
    {
        public int thang { get; set; }
        public int nam { get; set; }
        public int tong_so_khach_trong_thang { get; set; }
        public int so_lan_thuong { get; set; }
        public decimal phan_tram_thuong_toi_da { get; set; }
        public int so_nhan_vien_duoc_tinh_luong { get; set; }
    }

    public class LuongNhanVienViewModel
    {
        public List<LuongNhanVien> DanhSachLuong { get; set; } = new List<LuongNhanVien>();
        public ThongTinTongQuanLuong ThongTinTongQuan { get; set; } = new ThongTinTongQuanLuong();
        public int Thang { get; set; }
        public int Nam { get; set; }
    }
}
