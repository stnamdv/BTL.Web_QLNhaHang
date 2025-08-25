using BTL.Web.Data;
using BTL.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BTL.Web.Repositories
{
    public class LoaiBanRepository : ILoaiBanRepository
    {
        private readonly DatabaseContext _context;

        public LoaiBanRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LoaiBan>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<LoaiBan>("sp_LoaiBan_GetAll",
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<LoaiBan?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<LoaiBan>(
                "sp_LoaiBan_GetById",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<LoaiBan> CreateAsync(LoaiBan loaiBan)
        {
            using var connection = _context.CreateConnection();
            var parameters = new
            {
                suc_chua = loaiBan.suc_chua,
                so_luong = loaiBan.so_luong
            };

            var id = await connection.QuerySingleAsync<int>(
                "sp_LoaiBan_Create",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            loaiBan.loai_ban_id = id;
            return loaiBan;
        }

        public async Task<LoaiBan> UpdateAsync(LoaiBan loaiBan)
        {
            using var connection = _context.CreateConnection();
            var parameters = new
            {
                loai_ban_id = loaiBan.loai_ban_id,
                suc_chua = loaiBan.suc_chua,
                so_luong = loaiBan.so_luong
            };

            await connection.ExecuteAsync(
                "sp_LoaiBan_Update",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            return loaiBan;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "sp_LoaiBan_Delete",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);

            return rowsAffected > 0;
        }

        public async Task<bool> ExistsByCapacityAsync(int sucChua)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QuerySingleAsync<int>(
                "sp_LoaiBan_ExistsByCapacity",
                new { suc_chua = sucChua },
                commandType: System.Data.CommandType.StoredProcedure);

            return result == 1;
        }

        public async Task<LoaiBanDetails?> GetDetailsWithUsageAsync(int id)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<LoaiBanDetails>(
                "sp_LoaiBan_GetDetailsWithUsage",
                new { loai_ban_id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<BanAnInfo>> GetTablesAsync(int loaiBanId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<BanAnInfo>(
                "sp_LoaiBan_GetTables",
                new { loai_ban_id = loaiBanId },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<LoaiBanUpdateCheck> CanUpdateAsync(int id, int newSucChua, int newSoLuong)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstAsync<LoaiBanUpdateCheck>(
                "sp_LoaiBan_CanUpdate",
                new { loai_ban_id = id, new_suc_chua = newSucChua, new_so_luong = newSoLuong },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<LoaiBanDeleteCheck> CanDeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstAsync<LoaiBanDeleteCheck>(
                "sp_LoaiBan_CanDelete",
                new { loai_ban_id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
