using BTL.Web.Data;
using BTL.Web.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;

namespace BTL.Web.Services
{
    public class ThongKeBanAnService : IThongKeBanAnService
    {
        private readonly DatabaseContext _context;
        private readonly string _connectionString;

        public ThongKeBanAnService(DatabaseContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("Connection string not found");
        }

        public async Task<List<ThongKeBanAnTrungBinh>> GetThongKeTrungBinhTheoNgayAsync(int thang, int nam)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Thang", thang);
            parameters.Add("@Nam", nam);

            var results = await connection.QueryAsync<ThongKeBanAnTrungBinh>(
                "sp_ThongKeBanAn_TrungBinhTheoNgay",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return results.ToList();
        }

        public async Task<List<ThongKeBanAnChiTiet>> GetThongKeChiTietTheoNgayAsync(int thang, int nam)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Thang", thang);
            parameters.Add("@Nam", nam);

            var results = await connection.QueryAsync<ThongKeBanAnChiTiet>(
                "sp_ThongKeBanAn_ChiTietTheoNgay",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return results.ToList();
        }

        public async Task<List<ThongKeBanAnTongHop>> GetThongKeTongHopAsync(int thang, int nam)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Thang", thang);
            parameters.Add("@Nam", nam);

            var results = await connection.QueryAsync<ThongKeBanAnTongHop>(
                "sp_ThongKeBanAn_TongHop",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return results.ToList();
        }

        public async Task<List<ThongKeBanAnSoSanh>> GetThongKeSoSanhThangAsync(int thang1, int nam1, int thang2, int nam2)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Thang1", thang1);
            parameters.Add("@Nam1", nam1);
            parameters.Add("@Thang2", thang2);
            parameters.Add("@Nam2", nam2);

            var results = await connection.QueryAsync<ThongKeBanAnSoSanh>(
                "sp_ThongKeBanAn_SoSanhThang",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return results.ToList();
        }

        public async Task<ThongKeBanAnDashboard> GetDashboardAsync(int thang, int nam)
        {
            var dashboard = new ThongKeBanAnDashboard
            {
                Thang = thang,
                Nam = nam
            };

            // Lấy thống kê trung bình
            dashboard.ThongKeTrungBinh = await GetThongKeTrungBinhTheoNgayAsync(thang, nam);

            // Lấy thống kê tổng hợp
            dashboard.ThongKeTongHop = await GetThongKeTongHopAsync(thang, nam);

            // Tính tổng hợp
            dashboard.TongSoLoaiBan = dashboard.ThongKeTongHop.Count;
            dashboard.TongSoBanCoSan = dashboard.ThongKeTongHop.Sum(x => x.tong_so_ban_co_san);
            dashboard.TongSoBanDaSuDung = dashboard.ThongKeTongHop.Sum(x => x.so_ban_da_su_dung);

            if (dashboard.TongSoBanCoSan > 0)
            {
                dashboard.TyLeSuDungChung = Math.Round((decimal)dashboard.TongSoBanDaSuDung / dashboard.TongSoBanCoSan * 100, 2);
            }

            return dashboard;
        }
    }
}
