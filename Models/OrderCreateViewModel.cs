using System.ComponentModel.DataAnnotations;

namespace BTL.Web.Models
{
    public class OrderCreateViewModel : IValidatableObject
    {
        [Display(Name = "Loại đơn hàng")]
        [Required(ErrorMessage = "Vui lòng chọn loại đơn hàng")]
        public bool LaMangVe { get; set; } = false;

        [Display(Name = "Khách hàng")]
        public int? KhId { get; set; }

        [Display(Name = "Bàn ăn")]
        public int? BanId { get; set; }

        [Display(Name = "Số khách")]
        [Range(1, 20, ErrorMessage = "Số khách phải từ 1 đến 20")]
        public int? SoKhach { get; set; }

        [Display(Name = "Tên khách hàng")]
        [StringLength(120, ErrorMessage = "Tên khách hàng không được vượt quá 120 ký tự")]
        public string? KhachHangTen { get; set; }

        [Display(Name = "Số điện thoại")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string? KhachHangSdt { get; set; }

        [Display(Name = "Danh sách món ăn")]
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một món ăn")]
        public List<OrderItemCreateViewModel> OrderItems { get; set; } = new List<OrderItemCreateViewModel>();

        // Navigation properties cho View
        public IEnumerable<BanAn>? AvailableTables { get; set; }
        public IEnumerable<Mon>? MonAn { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // Validation cho đơn hàng ăn tại chỗ
            if (!LaMangVe)
            {
                if (BanId == null || BanId <= 0)
                {
                    results.Add(new ValidationResult(
                        "Đơn hàng ăn tại chỗ phải có bàn",
                        new[] { nameof(BanId) }));
                }

                if (SoKhach == null || SoKhach <= 0)
                {
                    results.Add(new ValidationResult(
                        "Đơn hàng ăn tại chỗ phải có số khách",
                        new[] { nameof(SoKhach) }));
                }
            }

            // Validation cho đơn hàng mang về
            if (LaMangVe && BanId.HasValue)
            {
                results.Add(new ValidationResult(
                    "Đơn hàng mang về không được có bàn",
                    new[] { nameof(BanId) }));
            }

            return results;
        }
    }

    public class OrderItemCreateViewModel
    {
        [Required]
        public int MonId { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Số lượng phải từ 1 đến 10")]
        public int SoLuong { get; set; } = 1;

        // Thông tin món ăn để hiển thị
        public string? MonTen { get; set; }
        public string? MonMa { get; set; }
        public decimal? MonGia { get; set; }
        public string? MonLoai { get; set; }
    }
}
