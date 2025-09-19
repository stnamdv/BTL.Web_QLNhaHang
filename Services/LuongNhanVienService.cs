using BTL.Web.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BTL.Web.Services
{
    public class LuongNhanVienService : ILuongNhanVienService
    {
        private readonly string _connectionString;
        private readonly ILogger<LuongNhanVienService> _logger;

        public LuongNhanVienService(IConfiguration configuration, ILogger<LuongNhanVienService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<LuongNhanVienViewModel> TinhLuongTheoThangAsync(int thang, int nam)
        {
            var result = new LuongNhanVienViewModel
            {
                Thang = thang,
                Nam = nam
            };

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_NhanVien_TinhLuongTheoThang", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@thang", thang);
                command.Parameters.AddWithValue("@nam", nam);

                using var reader = await command.ExecuteReaderAsync();

                // Đọc result set đầu tiên - danh sách lương nhân viên
                var danhSachLuong = new List<LuongNhanVien>();
                while (await reader.ReadAsync())
                {
                    danhSachLuong.Add(new LuongNhanVien
                    {
                        nv_id = reader.GetInt32("nv_id"),
                        ma_nv = reader.GetString("ma_nv"),
                        ho_ten = reader.GetString("ho_ten"),
                        loai_nv = reader.GetString("loai_nv"),
                        luong_co_ban = reader.GetDecimal("luong_co_ban"),
                        tong_so_khach_trong_thang = reader.GetInt32("tong_so_khach_trong_thang"),
                        so_lan_thuong = reader.GetInt32("so_lan_thuong"),
                        phan_tram_thuong = reader.GetDecimal("phan_tram_thuong"),
                        tien_thuong = reader.GetDecimal("tien_thuong"),
                        tong_luong = reader.GetDecimal("tong_luong"),
                        trang_thai = reader.GetString("trang_thai"),
                        ngay_vao_lam = reader.IsDBNull("ngay_vao_lam") ? null : reader.GetDateTime("ngay_vao_lam")
                    });
                }

                result.DanhSachLuong = danhSachLuong;

                // Đọc result set thứ hai - thông tin tổng quan
                if (await reader.NextResultAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        result.ThongTinTongQuan = new ThongTinTongQuanLuong
                        {
                            thang = reader.GetInt32("thang"),
                            nam = reader.GetInt32("nam"),
                            tong_so_khach_trong_thang = reader.GetInt32("tong_so_khach_trong_thang"),
                            so_lan_thuong = reader.GetInt32("so_lan_thuong"),
                            phan_tram_thuong_toi_da = reader.GetDecimal("phan_tram_thuong_toi_da"),
                            so_nhan_vien_duoc_tinh_luong = reader.GetInt32("so_nhan_vien_duoc_tinh_luong")
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tính lương theo tháng {Thang}/{Nam}", thang, nam);
                throw;
            }

            return result;
        }

        public async Task<LuongNhanVienViewModel> TinhLuongThangHienTaiAsync()
        {
            var thangHienTai = DateTime.Now.Month;
            var namHienTai = DateTime.Now.Year;

            return await TinhLuongTheoThangAsync(thangHienTai, namHienTai);
        }
    }
}
