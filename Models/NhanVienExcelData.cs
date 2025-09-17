using System.ComponentModel.DataAnnotations;

namespace BTL.Web.Models
{
    public class NhanVienExcelData
    {
        [Display(Name = "STT")]
        public int STT { get; set; }

        [Display(Name = "Mã nhân viên")]
        public string? MaNhanVien { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bộ phận không được để trống")]
        [Display(Name = "Bộ phận")]
        public string BoPhan { get; set; } = string.Empty;

        [Display(Name = "Ngày vào làm")]
        public string? NgayVaoLam { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = string.Empty;

        // Mapping từ Excel data sang database values
        public string GetLoaiNvFromBoPhan()
        {
            return BoPhan?.ToUpper() switch
            {
                "ĐẦU BẾP" => "BEP",
                "PHỤC VỤ" => "PHUC_VU",
                "DỊCH VỤ" => "DICH_VU",
                "THU NGÂN" => "THU_NGAN",
                _ => BoPhan ?? ""
            };
        }

        public string GetTrangThaiFromDisplay()
        {
            return TrangThai?.ToUpper() switch
            {
                "ĐANG LÀM VIỆC" => "ACTIVE",
                "ĐÃ NGHỈ" => "INACTIVE",
                _ => TrangThai ?? ""
            };
        }

        public DateTime? GetNgayVaoLamAsDateTime()
        {
            if (string.IsNullOrWhiteSpace(NgayVaoLam))
                return null;

            if (DateTime.TryParseExact(NgayVaoLam, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime result))
                return result;

            return null;
        }
    }

    public class NhanVienBulkResult
    {
        public int InsertedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int ErrorCount { get; set; }
        public string ErrorMessages { get; set; } = string.Empty;
    }
}
