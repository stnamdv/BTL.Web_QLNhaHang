using System.ComponentModel.DataAnnotations;

namespace BTL.Web.Models
{
    public class NhanVienWithLoaiNhanVien
    {
        public int nv_id { get; set; }

        [Display(Name = "Họ tên")]
        public string ho_ten { get; set; } = string.Empty;

        [Display(Name = "Loại nhân viên")]
        public string loai_nv { get; set; } = string.Empty;

        [Display(Name = "Ngày vào làm")]
        public DateTime? ngay_vao_lam { get; set; }

        [Display(Name = "Trạng thái")]
        public string trang_thai { get; set; } = string.Empty;

        [Display(Name = "Lương cơ bản")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal luong_co_ban { get; set; }

        // Computed properties
        [Display(Name = "Tên loại nhân viên")]
        public string TenLoaiNhanVien => LoaiNv.GetDisplayName(loai_nv);

        [Display(Name = "Trạng thái hiển thị")]
        public string TrangThaiHienThi => TrangThaiNv.GetDisplayName(trang_thai);

        [Display(Name = "Thâm niên (năm)")]
        public int? ThamNien
        {
            get
            {
                if (ngay_vao_lam.HasValue)
                {
                    var today = DateTime.Today;
                    var years = today.Year - ngay_vao_lam.Value.Year;
                    if (today.Month < ngay_vao_lam.Value.Month ||
                        (today.Month == ngay_vao_lam.Value.Month && today.Day < ngay_vao_lam.Value.Day))
                    {
                        years--;
                    }
                    return years;
                }
                return null;
            }
        }
    }
}
