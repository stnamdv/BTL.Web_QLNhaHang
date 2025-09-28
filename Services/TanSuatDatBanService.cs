using BTL.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BTL.Web.Services
{
    public class TanSuatDatBanService : ITanSuatDatBanService
    {
        private readonly string _connectionString;

        public TanSuatDatBanService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<List<TanSuatDatBan>> GetTanSuatDatBanAsync(int? sucChua = null, int? ngayTrongThang = null)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Suc_Chua", sucChua, DbType.Int32);
            parameters.Add("@Ngay_Trong_Thang", ngayTrongThang, DbType.Int32);

            var result = await connection.QueryAsync<TanSuatDatBan>(
                "sp_BanAn_TanSuatBan",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<List<LoaiBanInfo>> GetDanhSachLoaiBanAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            var result = await connection.QueryAsync<LoaiBanInfo>(
                "SELECT loai_ban_id, suc_chua, so_luong FROM LoaiBan ORDER BY suc_chua");
            return result.ToList();
        }
    }
}
