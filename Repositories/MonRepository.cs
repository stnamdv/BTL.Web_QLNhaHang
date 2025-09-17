using BTL.Web.Data;
using BTL.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace BTL.Web.Repositories
{
    public class MonRepository : IMonRepository
    {
        private readonly DatabaseContext _context;

        public MonRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Mon>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Mon>("EXEC SP_GetAllMon @PageNumber = 1, @PageSize = 1000");
        }

        public async Task<PagedResult<Mon>> GetAllPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, string? loaiMon = null)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);
            parameters.Add("@LoaiMon", loaiMon);

            var items = await connection.QueryAsync<Mon>(
                "SP_GetAllMon",
                parameters,
                commandType: CommandType.StoredProcedure);

            // Lấy total count từ item đầu tiên
            var totalItems = items.FirstOrDefault()?.GetType().GetProperty("TotalCount")?.GetValue(items.First()) as int? ?? 0;

            return new PagedResult<Mon>(items, totalItems, pageNumber, pageSize);
        }

        public async Task<Mon?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Mon>(
                "EXEC SP_GetMonById @MonId",
                new { MonId = id });
        }

        public async Task<MonWithCongThuc?> GetWithCongThucAsync(int id)
        {
            using var connection = _context.CreateConnection();

            using var multi = await connection.QueryMultipleAsync(
                "EXEC SP_GetMonWithCongThuc @MonId",
                new { MonId = id });

            var mon = await multi.ReadFirstOrDefaultAsync<Mon>();
            if (mon == null) return null;

            var congThucs = await multi.ReadAsync<CongThucDetail>();

            return new MonWithCongThuc
            {
                Mon = mon,
                CongThucs = congThucs
            };
        }

        public async Task<Mon> CreateAsync(Mon mon, IEnumerable<CongThuc>? congThucs = null)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@MaMon", mon.ma_mon);
            parameters.Add("@TenMon", mon.ten_mon);
            parameters.Add("@LoaiMon", mon.loai_mon.ToString());
            parameters.Add("@Gia", mon.gia);
            parameters.Add("@MonId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            // Chuyển đổi công thức thành JSON nếu có
            string? congThucJson = null;
            if (congThucs != null && congThucs.Any())
            {
                var congThucData = congThucs.Select(ct => new
                {
                    nl_id = ct.nl_id,
                    dinh_luong = ct.dinh_luong
                });
                congThucJson = JsonSerializer.Serialize(congThucData);
            }
            parameters.Add("@CongThucJson", congThucJson);

            await connection.ExecuteAsync(
                "SP_CreateMon",
                parameters,
                commandType: CommandType.StoredProcedure);

            var monId = parameters.Get<int>("@MonId");
            mon.mon_id = monId;

            return mon;
        }

        public async Task<Mon> UpdateAsync(Mon mon, IEnumerable<CongThuc>? congThucs = null)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@MonId", mon.mon_id);
            parameters.Add("@MaMon", mon.ma_mon);
            parameters.Add("@TenMon", mon.ten_mon);
            parameters.Add("@LoaiMon", mon.loai_mon.ToString());
            parameters.Add("@Gia", mon.gia);

            // Chuyển đổi công thức thành JSON nếu có
            string? congThucJson = null;
            if (congThucs != null && congThucs.Any())
            {
                var congThucData = congThucs.Select(ct => new
                {
                    nl_id = ct.nl_id,
                    dinh_luong = ct.dinh_luong
                });
                congThucJson = JsonSerializer.Serialize(congThucData);
            }
            parameters.Add("@CongThucJson", congThucJson);

            await connection.ExecuteAsync(
                "SP_UpdateMon",
                parameters,
                commandType: CommandType.StoredProcedure);

            return mon;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();

            try
            {
                await connection.ExecuteAsync(
                    "EXEC SP_DeleteMon @MonId",
                    new { MonId = id });
                return true;
            }
            catch (SqlException ex) when (ex.Number == 50000) // Custom error from stored procedure
            {
                return false;
            }
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var count = await connection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM dbo.Mon WHERE mon_id = @id",
                new { id });
            return count > 0;
        }

        public async Task<bool> ExistsByMaMonAsync(string maMon, int? excludeId = null)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT COUNT(*) FROM dbo.Mon WHERE ma_mon = @maMon";
            object parameters;

            if (excludeId.HasValue)
            {
                sql += " AND mon_id != @excludeId";
                parameters = new { maMon, excludeId = excludeId.Value };
            }
            else
            {
                parameters = new { maMon };
            }

            var count = await connection.QuerySingleAsync<int>(sql, parameters);
            return count > 0;
        }

        public async Task<(bool can_delete, string message)> CanDeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();

            // Kiểm tra món có trong đơn hàng không
            var orderCount = await connection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM dbo.OrderItem WHERE mon_id = @id",
                new { id });

            if (orderCount > 0)
            {
                return (false, "Không thể xóa món đang được sử dụng trong đơn hàng.");
            }

            // Kiểm tra món có trong thực đơn không
            var thucDonCount = await connection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM dbo.ThucDon_Mon WHERE mon_id = @id",
                new { id });

            if (thucDonCount > 0)
            {
                return (false, "Không thể xóa món đang có trong thực đơn.");
            }

            return (true, string.Empty);
        }

        public async Task<IEnumerable<Mon>> GetByLoaiMonAsync(string loaiMon)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Mon>(
                "EXEC SP_GetAllMon @PageNumber = 1, @PageSize = 1000, @LoaiMon = @loaiMon",
                new { loaiMon });
        }

        public async Task<PagedResult<Mon>> GetByLoaiMonPagedAsync(string loaiMon, int pageNumber, int pageSize)
        {
            return await GetAllPagedAsync(pageNumber, pageSize, null, loaiMon);
        }

        public async Task<IEnumerable<Mon>> SearchAsync(string searchTerm)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Mon>(
                "EXEC SP_GetAllMon @PageNumber = 1, @PageSize = 1000, @SearchTerm = @searchTerm",
                new { searchTerm });
        }

        public async Task<PagedResult<Mon>> SearchPagedAsync(string searchTerm, int pageNumber, int pageSize)
        {
            return await GetAllPagedAsync(pageNumber, pageSize, searchTerm, null);
        }

        public async Task<IEnumerable<dynamic>> GetStatsByLoaiAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<dynamic>(
                @"SELECT 
                    loai_mon,
                    COUNT(*) as so_luong,
                    AVG(gia) as gia_trung_binh,
                    MIN(gia) as gia_thap_nhat,
                    MAX(gia) as gia_cao_nhat
                  FROM dbo.Mon 
                  GROUP BY loai_mon
                  ORDER BY loai_mon");
        }

        public async Task<IEnumerable<Mon>> GetMonWithCongThucAsync(int monId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Mon>(
                "EXEC SP_GetMonWithCongThuc @MonId",
                new { MonId = monId });
        }
    }
}