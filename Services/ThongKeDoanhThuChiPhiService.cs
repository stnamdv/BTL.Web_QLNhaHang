using BTL.Web.Models;
using Dapper;
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
                          List<ThongKeChiPhiTheoNguyenLieu> chiPhiTheoNguyenLieu)>
            GetThongKeTheoNgayAsync(DateTime ngay)
        {
            var tongQuan = new ThongKeDoanhThuChiPhiTongQuan();
            var doanhThuTheoLoaiMon = new List<ThongKeDoanhThuTheoLoaiMon>();
            var chiPhiTheoNguyenLieu = new List<ThongKeChiPhiTheoNguyenLieu>();

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

            return (tongQuan, doanhThuTheoLoaiMon, chiPhiTheoNguyenLieu);
        }

        public async Task<(ThongKeDoanhThuChiPhiKhoangThoiGian tongQuan,
                          List<ThongKeDoanhThuChiPhiTheoNgay> theoNgay)>
            GetThongKeTheoKhoangThoiGianAsync(DateTime tuNgay, DateTime denNgay)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@tu_ngay", tuNgay.Date);
            parameters.Add("@den_ngay", denNgay.Date);

            using var multi = await connection.QueryMultipleAsync(
                "sp_ThongKe_DoanhThuChiPhiTheoKhoangThoiGian",
                parameters,
                commandType: CommandType.StoredProcedure);

            // Đọc kết quả đầu tiên - Tổng quan
            var tongQuanData = await multi.ReadFirstOrDefaultAsync();
            var tongQuan = new ThongKeDoanhThuChiPhiKhoangThoiGian();

            if (tongQuanData != null)
            {
                tongQuan = new ThongKeDoanhThuChiPhiKhoangThoiGian
                {
                    TuNgay = (DateTime)tongQuanData.tu_ngay,
                    DenNgay = (DateTime)tongQuanData.den_ngay,
                    SoNgay = (int)tongQuanData.so_ngay,
                    TongDoanhThu = (decimal)tongQuanData.tong_doanh_thu,
                    TongChiPhiNguyenLieu = (decimal)tongQuanData.tong_chi_phi_nguyen_lieu,
                    TongChiPhi = (decimal)tongQuanData.tong_chi_phi,
                    LoiNhuan = (decimal)tongQuanData.loi_nhuan,
                    TyLeLoiNhuan = (decimal)tongQuanData.ty_le_loi_nhuan,
                    SoDonHang = (int)tongQuanData.so_don_hang,
                    SoDonHoanThanh = (int)tongQuanData.so_don_hoan_thanh,
                    SoKhach = (int)tongQuanData.so_khach,
                    DoanhThuTrungBinhNgay = (decimal)tongQuanData.doanh_thu_trung_binh_ngay,
                    LoiNhuanTrungBinhNgay = (decimal)tongQuanData.loi_nhuan_trung_binh_ngay
                };
            }

            // Đọc kết quả thứ hai - Thống kê theo ngày
            var theoNgayData = await multi.ReadAsync();
            var theoNgay = new List<ThongKeDoanhThuChiPhiTheoNgay>();

            foreach (dynamic item in theoNgayData)
            {
                theoNgay.Add(new ThongKeDoanhThuChiPhiTheoNgay
                {
                    Ngay = (DateTime)item.ngay,
                    SoDonHang = (int)item.so_don_hang,
                    SoDonHoanThanh = (int)item.so_don_hoan_thanh,
                    DoanhThuNgay = (decimal)item.doanh_thu_ngay,
                    ChiPhiNguyenLieuNgay = (decimal)item.chi_phi_nguyen_lieu_ngay,
                    TongChiPhiNgay = (decimal)item.tong_chi_phi_ngay,
                    LoiNhuanNgay = (decimal)item.loi_nhuan_ngay
                });
            }

            return (tongQuan, theoNgay);
        }

        public async Task<(ThongKeDoanhThuChiPhiTongQuan tongQuan,
                          List<ThongKeDoanhThuTheoLoaiMon> doanhThuTheoLoaiMon,
                          List<ThongKeChiPhiTheoNguyenLieu> chiPhiTheoNguyenLieu)>
            GetThongKeNgayHienTaiAsync()
        {
            var tongQuan = new ThongKeDoanhThuChiPhiTongQuan();
            var doanhThuTheoLoaiMon = new List<ThongKeDoanhThuTheoLoaiMon>();
            var chiPhiTheoNguyenLieu = new List<ThongKeChiPhiTheoNguyenLieu>();

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

            return (tongQuan, doanhThuTheoLoaiMon, chiPhiTheoNguyenLieu);
        }

        public async Task<DashboardThongKe> GetDashboardThongKeAsync()
        {
            var (tongQuan, doanhThuTheoLoaiMon, chiPhiTheoNguyenLieu) =
                await GetThongKeNgayHienTaiAsync();

            return new DashboardThongKe
            {
                ThongKeNgay = tongQuan,
                DoanhThuTheoLoaiMon = doanhThuTheoLoaiMon,
                ChiPhiTheoNguyenLieu = chiPhiTheoNguyenLieu
            };
        }
    }
}