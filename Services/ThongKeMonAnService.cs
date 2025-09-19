using BTL.Web.Data;
using BTL.Web.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;

namespace BTL.Web.Services
{
    public class ThongKeMonAnService : IThongKeMonAnService
    {
        private readonly DatabaseContext _context;
        private readonly string _connectionString;

        public ThongKeMonAnService(DatabaseContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("Connection string not found");
        }

        public async Task<List<ThongKeMonAnTanSuat>> GetThongKeTanSuatTheoThangAsync(int thang, int nam)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Thang", thang);
            parameters.Add("@Nam", nam);

            var results = await connection.QueryAsync<ThongKeMonAnTanSuat>(
                "sp_ThongKeMonAn_TanSuatTheoThang",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return results.ToList();
        }

        public async Task<List<ThongKeMonAnChiTiet>> GetThongKeChiTietTheoNgayAsync(int thang, int nam)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Thang", thang);
            parameters.Add("@Nam", nam);

            var results = await connection.QueryAsync<ThongKeMonAnChiTiet>(
                "sp_ThongKeMonAn_ChiTietTheoNgay",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return results.ToList();
        }

        public async Task<List<ThongKeMonAnTop>> GetTopMonAnAsync(int thang, int nam, int topN = 10)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Thang", thang);
            parameters.Add("@Nam", nam);
            parameters.Add("@TopN", topN);

            var results = await connection.QueryAsync<ThongKeMonAnTop>(
                "sp_ThongKeMonAn_TopMonAn",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return results.ToList();
        }

        public async Task<List<ThongKeMonAnSoSanh>> GetThongKeSoSanhThangAsync(int thang1, int nam1, int thang2, int nam2)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Thang1", thang1);
            parameters.Add("@Nam1", nam1);
            parameters.Add("@Thang2", thang2);
            parameters.Add("@Nam2", nam2);

            var results = await connection.QueryAsync<ThongKeMonAnSoSanh>(
                "sp_ThongKeMonAn_SoSanhThang",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return results.ToList();
        }

        public async Task<List<ThongKeMonAnTheoLoai>> GetThongKeTheoLoaiMonAsync(int thang, int nam)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Thang", thang);
            parameters.Add("@Nam", nam);

            var results = await connection.QueryAsync<ThongKeMonAnTheoLoai>(
                "sp_ThongKeMonAn_TheoLoaiMon",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return results.ToList();
        }

        public async Task<ThongKeMonAnDashboard> GetDashboardAsync(int thang, int nam)
        {
            var dashboard = new ThongKeMonAnDashboard
            {
                Thang = thang,
                Nam = nam
            };

            // Lấy thống kê tần suất
            dashboard.ThongKeTanSuat = await GetThongKeTanSuatTheoThangAsync(thang, nam);

            // Lấy top món ăn
            dashboard.TopMonAn = await GetTopMonAnAsync(thang, nam, 10);

            // Lấy thống kê theo loại món
            dashboard.ThongKeTheoLoai = await GetThongKeTheoLoaiMonAsync(thang, nam);

            // Tính tổng hợp
            dashboard.TongSoMon = dashboard.ThongKeTanSuat.Count;
            dashboard.TongSoMonCoDat = dashboard.ThongKeTanSuat.Count(x => x.so_lan_duoc_dat > 0);
            dashboard.TongSoLanDat = dashboard.ThongKeTanSuat.Sum(x => x.so_lan_duoc_dat);
            dashboard.TongDoanhThu = dashboard.ThongKeTanSuat.Sum(x => x.tong_doanh_thu_mon);

            if (dashboard.TongSoMon > 0)
            {
                dashboard.TyLeMonCoDat = Math.Round((decimal)dashboard.TongSoMonCoDat / dashboard.TongSoMon * 100, 2);
            }

            return dashboard;
        }
    }
}
