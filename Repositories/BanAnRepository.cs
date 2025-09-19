using BTL.Web.Data;
using BTL.Web.Models;
using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace BTL.Web.Repositories
{
    public class BanAnRepository : IBanAnRepository
    {
        private readonly DatabaseContext _context;

        public BanAnRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BanAnWithLoaiBan>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<BanAnWithLoaiBan>("EXEC sp_BanAn_GetAll");
        }

        public async Task<BanAn?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<BanAn>(
                "EXEC sp_BanAn_GetById @Id",
                new { Id = id });
        }

        public async Task<IEnumerable<BanAnWithLoaiBan>> GetByLoaiBanIdAsync(int loaiBanId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<BanAnWithLoaiBan>(
                "EXEC sp_BanAn_GetByLoaiBanId @LoaiBanId",
                new { LoaiBanId = loaiBanId });
        }

        public async Task<BanAn> CreateAsync(BanAn banAn)
        {
            using var connection = _context.CreateConnection();
            var sql = "EXEC sp_BanAn_Create @loai_ban_id, @so_hieu";

            var id = await connection.QuerySingleAsync<int>(sql, new
            {
                loai_ban_id = banAn.loai_ban_id,
                so_hieu = banAn.so_hieu
            });
            banAn.ban_id = id;
            return banAn;
        }

        public async Task<BanAn> UpdateAsync(BanAn banAn)
        {
            using var connection = _context.CreateConnection();
            var sql = "EXEC sp_BanAn_Update @ban_id, @loai_ban_id, @so_hieu";

            await connection.ExecuteAsync(sql, new
            {
                ban_id = banAn.ban_id,
                loai_ban_id = banAn.loai_ban_id,
                so_hieu = banAn.so_hieu
            });
            return banAn;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "EXEC sp_BanAn_Delete @Id",
                new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<BanAn?> GetDetailsWithUsageAsync(int id)
        {
            using var connection = _context.CreateConnection();

            // Load BanAn with LoaiBan information using a single query
            var sql = @"EXEC sp_BanAn_GetDetailsWithUsage @Id";

            var banAn = await connection.QueryAsync<BanAn, LoaiBan, BanAn>(
                sql,
                (ban, loaiBan) =>
                {
                    ban.LoaiBan = loaiBan;
                    return ban;
                },
                new { Id = id },
                splitOn: "split_column");

            return banAn.FirstOrDefault();
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync(int banId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Order>(
                "SELECT * FROM dbo.[Order] WHERE ban_id = @BanId ORDER BY thoi_diem_dat DESC",
                new { BanId = banId });
        }

        public async Task<bool> ExistsBySoHieuAsync(string soHieu, int? excludeId = null)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT COUNT(*) FROM dbo.BanAn WHERE so_hieu = @SoHieu";
            if (excludeId.HasValue)
            {
                sql += " AND ban_id != @ExcludeId";
            }

            var count = await connection.QuerySingleAsync<int>(sql, new
            {
                SoHieu = soHieu,
                ExcludeId = excludeId
            });
            return count > 0;
        }

        public async Task<(bool can_update, string message)> CanUpdateAsync(int id, int loaiBanId, string soHieu)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "EXEC sp_BanAn_CanUpdate @ban_id, @loai_ban_id, @so_hieu",
                new { ban_id = id, loai_ban_id = loaiBanId, so_hieu = soHieu });

            return (result?.can_update ?? false, result?.message ?? "Không thể cập nhật");
        }

        public async Task<(bool can_delete, string message)> CanDeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "EXEC sp_BanAn_CanDelete @Id",
                new { Id = id });

            return (result?.can_delete ?? false, result?.message ?? "Không thể xóa");
        }

        public async Task<IEnumerable<BanAnWithLoaiBan>> GetAvailableAsync(int? capacity = null)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<BanAnWithLoaiBan>(
                "EXEC sp_BanAn_GetAvailable @capacity",
                new { capacity });
        }

        public async Task<int> GetCountByLoaiBanIdAsync(int loaiBanId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM dbo.BanAn WHERE loai_ban_id = @LoaiBanId",
                new { LoaiBanId = loaiBanId });
        }

        public async Task<PagedResult<BanAnWithLoaiBan>> SearchPagedAsync(string? searchTerm, int? loaiBanId, int? capacity, int page, int pageSize)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", page);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);
            parameters.Add("@LoaiBanId", loaiBanId);
            parameters.Add("@Capacity", capacity);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var items = await connection.QueryAsync<BanAnWithLoaiBan>(
                "sp_BanAn_SearchWithPagination",
                parameters,
                commandType: CommandType.StoredProcedure);

            var totalCount = parameters.Get<int>("@TotalCount");

            return new PagedResult<BanAnWithLoaiBan>(items, totalCount, page, pageSize);
        }

        public async Task<IEnumerable<dynamic>> GetTableStatusAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync(
                "sp_BanAn_GetTableStatus",
                commandType: CommandType.StoredProcedure);
        }
    }
}
