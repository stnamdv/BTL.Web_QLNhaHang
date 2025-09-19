using BTL.Web.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BTL.Web.Services
{
    public class ThongKeNguyenLieuService : IThongKeNguyenLieuService
    {
        private readonly string _connectionString;

        public ThongKeNguyenLieuService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<(List<ThongKeNguyenLieuChiTiet> chiTiet,
                          List<ThongKeNguyenLieuTheoDonVi> theoDonVi,
                          List<ThongKeMonAnSuDungNguyenLieu> monAn)>
            GetThongKeTheoNgayAsync(DateTime ngay)
        {
            var chiTiet = new List<ThongKeNguyenLieuChiTiet>();
            var theoDonVi = new List<ThongKeNguyenLieuTheoDonVi>();
            var monAn = new List<ThongKeMonAnSuDungNguyenLieu>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("sp_ThongKe_NguyenLieuSuDungTheoNgay", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@ngay", ngay.Date);

            using var reader = await command.ExecuteReaderAsync();

            // Đọc kết quả đầu tiên - Chi tiết theo từng nguyên liệu
            if (await reader.ReadAsync())
            {
                do
                {
                    chiTiet.Add(new ThongKeNguyenLieuChiTiet
                    {
                        NlId = reader.GetInt32("nl_id"),
                        TenNguyenLieu = reader.GetString("ten_nguyen_lieu"),
                        DonVi = reader.GetString("don_vi"),
                        NguonGoc = reader.GetString("nguon_goc"),
                        GiaDonVi = reader.GetDecimal("gia_don_vi"),
                        NhaCungCap = reader.GetString("nha_cung_cap"),
                        TongLuongSuDung = reader.GetDecimal("tong_luong_su_dung"),
                        SoMonSuDung = reader.GetInt32("so_mon_su_dung"),
                        TongChiPhiNguyenLieu = reader.GetDecimal("tong_chi_phi_nguyen_lieu"),
                        TyLeChiPhiPhanTram = reader.GetDecimal("ty_le_chi_phi_phan_tram")
                    });
                } while (await reader.ReadAsync());
            }

            // Đọc kết quả thứ hai - Thống kê theo đơn vị
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    theoDonVi.Add(new ThongKeNguyenLieuTheoDonVi
                    {
                        DonVi = reader.GetString("don_vi"),
                        SoLoaiNguyenLieu = reader.GetInt32("so_loai_nguyen_lieu"),
                        TongLuongSuDung = reader.GetDecimal("tong_luong_su_dung"),
                        TongChiPhiTheoDonVi = reader.GetDecimal("tong_chi_phi_theo_don_vi"),
                        TyLeChiPhiPhanTram = reader.GetDecimal("ty_le_chi_phi_phan_tram")
                    });
                }
            }

            // Đọc kết quả thứ ba - Chi tiết món ăn
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    monAn.Add(new ThongKeMonAnSuDungNguyenLieu
                    {
                        NlId = reader.GetInt32("nl_id"),
                        TenNguyenLieu = reader.GetString("ten_nguyen_lieu"),
                        MaMon = reader.GetString("ma_mon"),
                        TenMon = reader.GetString("ten_mon"),
                        LoaiMon = reader.GetString("loai_mon"),
                        DinhLuongCho1Mon = reader.GetDecimal("dinh_luong_cho_1_mon"),
                        TongSoMonDuocDat = reader.GetInt32("tong_so_mon_duoc_dat"),
                        TongLuongNguyenLieuChoMon = reader.GetDecimal("tong_luong_nguyen_lieu_cho_mon"),
                        ChiPhiNguyenLieuChoMon = reader.GetDecimal("chi_phi_nguyen_lieu_cho_mon")
                    });
                }
            }

            return (chiTiet, theoDonVi, monAn);
        }

        public async Task<(ThongKeNguyenLieuKhoangThoiGian tongQuan,
                          List<ThongKeNguyenLieuChiTiet> chiTiet,
                          List<ThongKeNguyenLieuTheoDonVi> theoDonVi,
                          List<ThongKeNguyenLieuTheoNgay> theoNgay)>
            GetThongKeTheoKhoangThoiGianAsync(DateTime tuNgay, DateTime denNgay)
        {
            var tongQuan = new ThongKeNguyenLieuKhoangThoiGian();
            var chiTiet = new List<ThongKeNguyenLieuChiTiet>();
            var theoDonVi = new List<ThongKeNguyenLieuTheoDonVi>();
            var theoNgay = new List<ThongKeNguyenLieuTheoNgay>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("sp_ThongKe_NguyenLieuSuDungTheoKhoangThoiGian", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@tu_ngay", tuNgay.Date);
            command.Parameters.AddWithValue("@den_ngay", denNgay.Date);

            using var reader = await command.ExecuteReaderAsync();

            // Đọc kết quả đầu tiên - Tổng quan
            if (await reader.ReadAsync())
            {
                tongQuan = new ThongKeNguyenLieuKhoangThoiGian
                {
                    TuNgay = reader.GetDateTime("tu_ngay"),
                    DenNgay = reader.GetDateTime("den_ngay"),
                    SoNgay = reader.GetInt32("so_ngay"),
                    SoLoaiNguyenLieuSuDung = reader.GetInt32("so_loai_nguyen_lieu_su_dung"),
                    TongLuongNguyenLieuSuDung = reader.GetDecimal("tong_luong_nguyen_lieu_su_dung"),
                    TongChiPhiNguyenLieu = reader.GetDecimal("tong_chi_phi_nguyen_lieu"),
                    ChiPhiNguyenLieuTrungBinhNgay = reader.GetDecimal("chi_phi_nguyen_lieu_trung_binh_ngay")
                };
            }

            // Đọc kết quả thứ hai - Chi tiết theo từng nguyên liệu
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    chiTiet.Add(new ThongKeNguyenLieuChiTiet
                    {
                        NlId = reader.GetInt32("nl_id"),
                        TenNguyenLieu = reader.GetString("ten_nguyen_lieu"),
                        DonVi = reader.GetString("don_vi"),
                        NguonGoc = reader.GetString("nguon_goc"),
                        GiaDonVi = reader.GetDecimal("gia_don_vi"),
                        NhaCungCap = reader.GetString("nha_cung_cap"),
                        TongLuongSuDung = reader.GetDecimal("tong_luong_su_dung"),
                        SoMonSuDung = reader.GetInt32("so_mon_su_dung"),
                        TongChiPhiNguyenLieu = reader.GetDecimal("tong_chi_phi_nguyen_lieu"),
                        TyLeChiPhiPhanTram = reader.GetDecimal("ty_le_chi_phi_phan_tram")
                    });
                }
            }

            // Đọc kết quả thứ ba - Thống kê theo đơn vị
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    theoDonVi.Add(new ThongKeNguyenLieuTheoDonVi
                    {
                        DonVi = reader.GetString("don_vi"),
                        SoLoaiNguyenLieu = reader.GetInt32("so_loai_nguyen_lieu"),
                        TongLuongSuDung = reader.GetDecimal("tong_luong_su_dung"),
                        TongChiPhiTheoDonVi = reader.GetDecimal("tong_chi_phi_theo_don_vi"),
                        ChiPhiTrungBinhNgayTheoDonVi = reader.IsDBNull("chi_phi_trung_binh_ngay_theo_don_vi")
                            ? null : reader.GetDecimal("chi_phi_trung_binh_ngay_theo_don_vi"),
                        TyLeChiPhiPhanTram = reader.GetDecimal("ty_le_chi_phi_phan_tram")
                    });
                }
            }

            // Đọc kết quả thứ tư - Thống kê theo ngày
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    theoNgay.Add(new ThongKeNguyenLieuTheoNgay
                    {
                        Ngay = reader.GetDateTime("ngay"),
                        SoLoaiNguyenLieuSuDung = reader.GetInt32("so_loai_nguyen_lieu_su_dung"),
                        TongLuongNguyenLieuSuDung = reader.GetDecimal("tong_luong_nguyen_lieu_su_dung"),
                        ChiPhiNguyenLieuNgay = reader.GetDecimal("chi_phi_nguyen_lieu_ngay")
                    });
                }
            }

            return (tongQuan, chiTiet, theoDonVi, theoNgay);
        }

        public async Task<(List<ThongKeNguyenLieuChiTiet> chiTiet,
                          List<ThongKeNguyenLieuTheoDonVi> theoDonVi,
                          List<ThongKeMonAnSuDungNguyenLieu> monAn)>
            GetThongKeNgayHienTaiAsync()
        {
            var chiTiet = new List<ThongKeNguyenLieuChiTiet>();
            var theoDonVi = new List<ThongKeNguyenLieuTheoDonVi>();
            var monAn = new List<ThongKeMonAnSuDungNguyenLieu>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("sp_ThongKe_NguyenLieuSuDungNgayHienTai", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = await command.ExecuteReaderAsync();

            // Đọc kết quả đầu tiên - Chi tiết theo từng nguyên liệu
            if (await reader.ReadAsync())
            {
                do
                {
                    chiTiet.Add(new ThongKeNguyenLieuChiTiet
                    {
                        NlId = reader.GetInt32("nl_id"),
                        TenNguyenLieu = reader.GetString("ten_nguyen_lieu"),
                        DonVi = reader.GetString("don_vi"),
                        NguonGoc = reader.GetString("nguon_goc"),
                        GiaDonVi = reader.GetDecimal("gia_don_vi"),
                        NhaCungCap = reader.GetString("nha_cung_cap"),
                        TongLuongSuDung = reader.GetDecimal("tong_luong_su_dung"),
                        SoMonSuDung = reader.GetInt32("so_mon_su_dung"),
                        TongChiPhiNguyenLieu = reader.GetDecimal("tong_chi_phi_nguyen_lieu"),
                        TyLeChiPhiPhanTram = reader.GetDecimal("ty_le_chi_phi_phan_tram")
                    });
                } while (await reader.ReadAsync());
            }

            // Đọc kết quả thứ hai - Thống kê theo đơn vị
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    theoDonVi.Add(new ThongKeNguyenLieuTheoDonVi
                    {
                        DonVi = reader.GetString("don_vi"),
                        SoLoaiNguyenLieu = reader.GetInt32("so_loai_nguyen_lieu"),
                        TongLuongSuDung = reader.GetDecimal("tong_luong_su_dung"),
                        TongChiPhiTheoDonVi = reader.GetDecimal("tong_chi_phi_theo_don_vi"),
                        TyLeChiPhiPhanTram = reader.GetDecimal("ty_le_chi_phi_phan_tram")
                    });
                }
            }

            // Đọc kết quả thứ ba - Chi tiết món ăn
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    monAn.Add(new ThongKeMonAnSuDungNguyenLieu
                    {
                        NlId = reader.GetInt32("nl_id"),
                        TenNguyenLieu = reader.GetString("ten_nguyen_lieu"),
                        MaMon = reader.GetString("ma_mon"),
                        TenMon = reader.GetString("ten_mon"),
                        LoaiMon = reader.GetString("loai_mon"),
                        DinhLuongCho1Mon = reader.GetDecimal("dinh_luong_cho_1_mon"),
                        TongSoMonDuocDat = reader.GetInt32("tong_so_mon_duoc_dat"),
                        TongLuongNguyenLieuChoMon = reader.GetDecimal("tong_luong_nguyen_lieu_cho_mon"),
                        ChiPhiNguyenLieuChoMon = reader.GetDecimal("chi_phi_nguyen_lieu_cho_mon")
                    });
                }
            }

            return (chiTiet, theoDonVi, monAn);
        }
    }
}
