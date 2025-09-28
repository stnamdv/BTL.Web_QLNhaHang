using BTL.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BTL.Web.Services
{
    public class TanSuatMonAnService : ITanSuatMonAnService
    {
        private readonly string _connectionString;

        public TanSuatMonAnService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<List<TanSuatMonAn>> GetTanSuatMonAnAsync(int? monId, int thang, int nam)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@Mon_Id", monId, DbType.Int32);
                parameters.Add("@Thang", thang, DbType.Int32);
                parameters.Add("@Nam", nam, DbType.Int32);

                var result = await connection.QueryAsync<TanSuatMonAn>(
                    "sp_Mon_TanSuatTheoThang",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy tần suất món ăn: {ex.Message}", ex);
            }
        }

        public async Task<List<MonAn>> GetDanhSachMonAnAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var result = await connection.QueryAsync<MonAn>(
                    "SELECT mon_id, ten_mon FROM dbo.Mon ORDER BY ten_mon");

                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách món ăn: {ex.Message}", ex);
            }
        }
    }
}
