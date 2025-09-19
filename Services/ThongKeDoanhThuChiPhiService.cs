using BTL.Web.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BTL.Web.Services
{
    public class ThongKeDoanhThuChiPhiService : IThongKeDoanhThuChiPhiService
    {
        private readonly string _connectionString;

        public ThongKeDoanhThuChiPhiService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<(ThongKeDoanhThuChiPhiTongQuan tongQuan,
                          List<ThongKeDoanhThuTheoLoaiMon> doanhThuTheoLoaiMon,
                          List<ThongKeChiPhiTheoNguyenLieu> chiPhiTheoNguyenLieu,
                          List<ThongKeChiPhiLuongTheoLoaiNhanVien> chiPhiLuongTheoLoaiNhanVien)>
            GetThongKeTheoNgayAsync(DateTime ngay)
        {
            var tongQuan = new ThongKeDoanhThuChiPhiTongQuan();
            var doanhThuTheoLoaiMon = new List<ThongKeDoanhThuTheoLoaiMon>();
            var chiPhiTheoNguyenLieu = new List<ThongKeChiPhiTheoNguyenLieu>();
            var chiPhiLuongTheoLoaiNhanVien = new List<ThongKeChiPhiLuongTheoLoaiNhanVien>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("sp_ThongKe_DoanhThuChiPhiTheoNgay", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@ngay", ngay.Date);

            using var reader = await command.ExecuteReaderAsync();

            // Đọc kết quả đầu tiên - Tổng quan
            if (await reader.ReadAsync())
            {
                tongQuan = new ThongKeDoanhThuChiPhiTongQuan
                {
                    Ngay = reader.GetDateTime("ngay"),
                    TongDoanhThu = reader.IsDBNull("tong_doanh_thu") ? 0 : reader.GetDecimal("tong_doanh_thu"),
                    TongChiPhiNguyenLieu = reader.IsDBNull("tong_chi_phi_nguyen_lieu") ? 0 : reader.GetDecimal("tong_chi_phi_nguyen_lieu"),
                    TongChiPhiLuong = reader.IsDBNull("tong_chi_phi_luong") ? 0 : reader.GetDecimal("tong_chi_phi_luong"),
                    TongChiPhi = reader.IsDBNull("tong_chi_phi") ? 0 : reader.GetDecimal("tong_chi_phi"),
                    LoiNhuan = reader.IsDBNull("loi_nhuan") ? 0 : reader.GetDecimal("loi_nhuan"),
                    TyLeLoiNhuan = reader.IsDBNull("ty_le_loi_nhuan") ? 0 : reader.GetDecimal("ty_le_loi_nhuan"),
                    SoDonHang = reader.IsDBNull("so_don_hang") ? 0 : reader.GetInt32("so_don_hang"),
                    SoDonHoanThanh = reader.IsDBNull("so_don_hoan_thanh") ? 0 : reader.GetInt32("so_don_hoan_thanh"),
                    SoKhach = reader.IsDBNull("so_khach") ? 0 : reader.GetInt32("so_khach")
                };
            }

            // Đọc kết quả thứ hai - Doanh thu theo loại món
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    doanhThuTheoLoaiMon.Add(new ThongKeDoanhThuTheoLoaiMon
                    {
                        LoaiMon = reader.IsDBNull("loai_mon") ? "" : reader.GetString("loai_mon"),
                        SoLuongMon = reader.IsDBNull("so_luong_mon") ? 0 : reader.GetInt32("so_luong_mon"),
                        TongSoLuong = reader.IsDBNull("tong_so_luong") ? 0 : reader.GetInt32("tong_so_luong"),
                        DoanhThuTheoLoai = reader.IsDBNull("doanh_thu_theo_loai") ? 0 : reader.GetDecimal("doanh_thu_theo_loai"),
                        TyLeDoanhThuPhanTram = reader.IsDBNull("ty_le_doanh_thu_phan_tram") ? 0 : reader.GetDecimal("ty_le_doanh_thu_phan_tram")
                    });
                }
            }

            // Đọc kết quả thứ ba - Chi phí theo nguyên liệu
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    chiPhiTheoNguyenLieu.Add(new ThongKeChiPhiTheoNguyenLieu
                    {
                        TenNguyenLieu = reader.IsDBNull("ten_nguyen_lieu") ? "" : reader.GetString("ten_nguyen_lieu"),
                        DonVi = reader.IsDBNull("don_vi") ? "" : reader.GetString("don_vi"),
                        TongLuongSuDung = reader.IsDBNull("tong_luong_su_dung") ? 0 : reader.GetDecimal("tong_luong_su_dung"),
                        GiaDonVi = reader.IsDBNull("gia_don_vi") ? 0 : reader.GetDecimal("gia_don_vi"),
                        TongChiPhiNguyenLieu = reader.IsDBNull("tong_chi_phi_nguyen_lieu") ? 0 : reader.GetDecimal("tong_chi_phi_nguyen_lieu"),
                        TyLeChiPhiPhanTram = reader.IsDBNull("ty_le_chi_phi_phan_tram") ? 0 : reader.GetDecimal("ty_le_chi_phi_phan_tram")
                    });
                }
            }

            // Đọc kết quả thứ tư - Chi phí lương theo loại nhân viên
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    chiPhiLuongTheoLoaiNhanVien.Add(new ThongKeChiPhiLuongTheoLoaiNhanVien
                    {
                        LoaiNhanVien = reader.IsDBNull("loai_nv") ? "" : reader.GetString("loai_nv"),
                        SoNhanVien = reader.IsDBNull("so_nhan_vien") ? 0 : reader.GetInt32("so_nhan_vien"),
                        LuongCoBan = reader.IsDBNull("luong_co_ban") ? 0 : reader.GetDecimal("luong_co_ban"),
                        LuongNgay = reader.IsDBNull("luong_ngay") ? 0 : reader.GetDecimal("luong_ngay"),
                        TongChiPhiLuongLoai = reader.IsDBNull("tong_chi_phi_luong_loai") ? 0 : reader.GetDecimal("tong_chi_phi_luong_loai"),
                        TyLeChiPhiLuongPhanTram = reader.IsDBNull("ty_le_chi_phi_luong_phan_tram") ? 0 : reader.GetDecimal("ty_le_chi_phi_luong_phan_tram")
                    });
                }
            }

            return (tongQuan, doanhThuTheoLoaiMon, chiPhiTheoNguyenLieu, chiPhiLuongTheoLoaiNhanVien);
        }

        public async Task<(ThongKeDoanhThuChiPhiKhoangThoiGian tongQuan,
                          List<ThongKeDoanhThuChiPhiTheoNgay> theoNgay)>
            GetThongKeTheoKhoangThoiGianAsync(DateTime tuNgay, DateTime denNgay)
        {
            var tongQuan = new ThongKeDoanhThuChiPhiKhoangThoiGian();
            var theoNgay = new List<ThongKeDoanhThuChiPhiTheoNgay>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("sp_ThongKe_DoanhThuChiPhiTheoKhoangThoiGian", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@tu_ngay", tuNgay.Date);
            command.Parameters.AddWithValue("@den_ngay", denNgay.Date);

            using var reader = await command.ExecuteReaderAsync();

            // Đọc kết quả đầu tiên - Tổng quan
            if (await reader.ReadAsync())
            {
                tongQuan = new ThongKeDoanhThuChiPhiKhoangThoiGian
                {
                    TuNgay = reader.GetDateTime("tu_ngay"),
                    DenNgay = reader.GetDateTime("den_ngay"),
                    SoNgay = reader.IsDBNull("so_ngay") ? 0 : reader.GetInt32("so_ngay"),
                    TongDoanhThu = reader.IsDBNull("tong_doanh_thu") ? 0 : reader.GetDecimal("tong_doanh_thu"),
                    TongChiPhiNguyenLieu = reader.IsDBNull("tong_chi_phi_nguyen_lieu") ? 0 : reader.GetDecimal("tong_chi_phi_nguyen_lieu"),
                    TongChiPhiLuong = reader.IsDBNull("tong_chi_phi_luong") ? 0 : reader.GetDecimal("tong_chi_phi_luong"),
                    TongChiPhi = reader.IsDBNull("tong_chi_phi") ? 0 : reader.GetDecimal("tong_chi_phi"),
                    LoiNhuan = reader.IsDBNull("loi_nhuan") ? 0 : reader.GetDecimal("loi_nhuan"),
                    TyLeLoiNhuan = reader.IsDBNull("ty_le_loi_nhuan") ? 0 : reader.GetDecimal("ty_le_loi_nhuan"),
                    SoDonHang = reader.IsDBNull("so_don_hang") ? 0 : reader.GetInt32("so_don_hang"),
                    SoDonHoanThanh = reader.IsDBNull("so_don_hoan_thanh") ? 0 : reader.GetInt32("so_don_hoan_thanh"),
                    SoKhach = reader.IsDBNull("so_khach") ? 0 : reader.GetInt32("so_khach"),
                    DoanhThuTrungBinhNgay = reader.IsDBNull("doanh_thu_trung_binh_ngay") ? 0 : reader.GetDecimal("doanh_thu_trung_binh_ngay"),
                    LoiNhuanTrungBinhNgay = reader.IsDBNull("loi_nhuan_trung_binh_ngay") ? 0 : reader.GetDecimal("loi_nhuan_trung_binh_ngay")
                };
            }

            // Đọc kết quả thứ hai - Thống kê theo ngày
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    theoNgay.Add(new ThongKeDoanhThuChiPhiTheoNgay
                    {
                        Ngay = reader.GetDateTime("ngay"),
                        SoDonHang = reader.IsDBNull("so_don_hang") ? 0 : reader.GetInt32("so_don_hang"),
                        SoDonHoanThanh = reader.IsDBNull("so_don_hoan_thanh") ? 0 : reader.GetInt32("so_don_hoan_thanh"),
                        DoanhThuNgay = reader.IsDBNull("doanh_thu_ngay") ? 0 : reader.GetDecimal("doanh_thu_ngay"),
                        ChiPhiNguyenLieuNgay = reader.IsDBNull("chi_phi_nguyen_lieu_ngay") ? 0 : reader.GetDecimal("chi_phi_nguyen_lieu_ngay"),
                        ChiPhiLuongNgay = reader.IsDBNull("chi_phi_luong_ngay") ? 0 : reader.GetDecimal("chi_phi_luong_ngay"),
                        TongChiPhiNgay = reader.IsDBNull("tong_chi_phi_ngay") ? 0 : reader.GetDecimal("tong_chi_phi_ngay"),
                        LoiNhuanNgay = reader.IsDBNull("loi_nhuan_ngay") ? 0 : reader.GetDecimal("loi_nhuan_ngay")
                    });
                }
            }

            return (tongQuan, theoNgay);
        }

        public async Task<(ThongKeDoanhThuChiPhiTongQuan tongQuan,
                          List<ThongKeDoanhThuTheoLoaiMon> doanhThuTheoLoaiMon,
                          List<ThongKeChiPhiTheoNguyenLieu> chiPhiTheoNguyenLieu,
                          List<ThongKeChiPhiLuongTheoLoaiNhanVien> chiPhiLuongTheoLoaiNhanVien)>
            GetThongKeNgayHienTaiAsync()
        {
            var tongQuan = new ThongKeDoanhThuChiPhiTongQuan();
            var doanhThuTheoLoaiMon = new List<ThongKeDoanhThuTheoLoaiMon>();
            var chiPhiTheoNguyenLieu = new List<ThongKeChiPhiTheoNguyenLieu>();
            var chiPhiLuongTheoLoaiNhanVien = new List<ThongKeChiPhiLuongTheoLoaiNhanVien>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("sp_ThongKe_DoanhThuChiPhiNgayHienTai", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = await command.ExecuteReaderAsync();

            // Đọc kết quả đầu tiên - Tổng quan
            if (await reader.ReadAsync())
            {
                tongQuan = new ThongKeDoanhThuChiPhiTongQuan
                {
                    Ngay = reader.GetDateTime("ngay"),
                    TongDoanhThu = reader.IsDBNull("tong_doanh_thu") ? 0 : reader.GetDecimal("tong_doanh_thu"),
                    TongChiPhiNguyenLieu = reader.IsDBNull("tong_chi_phi_nguyen_lieu") ? 0 : reader.GetDecimal("tong_chi_phi_nguyen_lieu"),
                    TongChiPhiLuong = reader.IsDBNull("tong_chi_phi_luong") ? 0 : reader.GetDecimal("tong_chi_phi_luong"),
                    TongChiPhi = reader.IsDBNull("tong_chi_phi") ? 0 : reader.GetDecimal("tong_chi_phi"),
                    LoiNhuan = reader.IsDBNull("loi_nhuan") ? 0 : reader.GetDecimal("loi_nhuan"),
                    TyLeLoiNhuan = reader.IsDBNull("ty_le_loi_nhuan") ? 0 : reader.GetDecimal("ty_le_loi_nhuan"),
                    SoDonHang = reader.IsDBNull("so_don_hang") ? 0 : reader.GetInt32("so_don_hang"),
                    SoDonHoanThanh = reader.IsDBNull("so_don_hoan_thanh") ? 0 : reader.GetInt32("so_don_hoan_thanh"),
                    SoKhach = reader.IsDBNull("so_khach") ? 0 : reader.GetInt32("so_khach")
                };
            }

            // Đọc kết quả thứ hai - Doanh thu theo loại món
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    doanhThuTheoLoaiMon.Add(new ThongKeDoanhThuTheoLoaiMon
                    {
                        LoaiMon = reader.IsDBNull("loai_mon") ? "" : reader.GetString("loai_mon"),
                        SoLuongMon = reader.IsDBNull("so_luong_mon") ? 0 : reader.GetInt32("so_luong_mon"),
                        TongSoLuong = reader.IsDBNull("tong_so_luong") ? 0 : reader.GetInt32("tong_so_luong"),
                        DoanhThuTheoLoai = reader.IsDBNull("doanh_thu_theo_loai") ? 0 : reader.GetDecimal("doanh_thu_theo_loai"),
                        TyLeDoanhThuPhanTram = reader.IsDBNull("ty_le_doanh_thu_phan_tram") ? 0 : reader.GetDecimal("ty_le_doanh_thu_phan_tram")
                    });
                }
            }

            // Đọc kết quả thứ ba - Chi phí theo nguyên liệu
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    chiPhiTheoNguyenLieu.Add(new ThongKeChiPhiTheoNguyenLieu
                    {
                        TenNguyenLieu = reader.IsDBNull("ten_nguyen_lieu") ? "" : reader.GetString("ten_nguyen_lieu"),
                        DonVi = reader.IsDBNull("don_vi") ? "" : reader.GetString("don_vi"),
                        TongLuongSuDung = reader.IsDBNull("tong_luong_su_dung") ? 0 : reader.GetDecimal("tong_luong_su_dung"),
                        GiaDonVi = reader.IsDBNull("gia_don_vi") ? 0 : reader.GetDecimal("gia_don_vi"),
                        TongChiPhiNguyenLieu = reader.IsDBNull("tong_chi_phi_nguyen_lieu") ? 0 : reader.GetDecimal("tong_chi_phi_nguyen_lieu"),
                        TyLeChiPhiPhanTram = reader.IsDBNull("ty_le_chi_phi_phan_tram") ? 0 : reader.GetDecimal("ty_le_chi_phi_phan_tram")
                    });
                }
            }

            // Đọc kết quả thứ tư - Chi phí lương theo loại nhân viên
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    chiPhiLuongTheoLoaiNhanVien.Add(new ThongKeChiPhiLuongTheoLoaiNhanVien
                    {
                        LoaiNhanVien = reader.IsDBNull("loai_nv") ? "" : reader.GetString("loai_nv"),
                        SoNhanVien = reader.IsDBNull("so_nhan_vien") ? 0 : reader.GetInt32("so_nhan_vien"),
                        LuongCoBan = reader.IsDBNull("luong_co_ban") ? 0 : reader.GetDecimal("luong_co_ban"),
                        LuongNgay = reader.IsDBNull("luong_ngay") ? 0 : reader.GetDecimal("luong_ngay"),
                        TongChiPhiLuongLoai = reader.IsDBNull("tong_chi_phi_luong_loai") ? 0 : reader.GetDecimal("tong_chi_phi_luong_loai"),
                        TyLeChiPhiLuongPhanTram = reader.IsDBNull("ty_le_chi_phi_luong_phan_tram") ? 0 : reader.GetDecimal("ty_le_chi_phi_luong_phan_tram")
                    });
                }
            }

            return (tongQuan, doanhThuTheoLoaiMon, chiPhiTheoNguyenLieu, chiPhiLuongTheoLoaiNhanVien);
        }

        public async Task<DashboardThongKe> GetDashboardThongKeAsync()
        {
            var (tongQuan, doanhThuTheoLoaiMon, chiPhiTheoNguyenLieu, chiPhiLuongTheoLoaiNhanVien) =
                await GetThongKeNgayHienTaiAsync();

            return new DashboardThongKe
            {
                ThongKeNgay = tongQuan,
                DoanhThuTheoLoaiMon = doanhThuTheoLoaiMon,
                ChiPhiTheoNguyenLieu = chiPhiTheoNguyenLieu,
                ChiPhiLuongTheoLoaiNhanVien = chiPhiLuongTheoLoaiNhanVien
            };
        }
    }
}