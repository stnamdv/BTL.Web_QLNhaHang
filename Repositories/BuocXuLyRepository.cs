using BTL.Web.Data;
using BTL.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BTL.Web.Repositories
{
    public class BuocXuLyRepository : IBuocXuLyRepository
    {
        private readonly DatabaseContext _context;

        public BuocXuLyRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BuocXuLy>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<BuocXuLy>(
                "sp_BuocXuLy_GetAll",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<BuocXuLy?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<BuocXuLy>(
                "sp_BuocXuLy_GetById",
                new { BuocId = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<BuocXuLy?> GetByThuTuAsync(int thuTu)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<BuocXuLy>(
                "sp_BuocXuLy_GetByThuTu",
                new { ThuTu = thuTu },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<BuocXuLy?> GetBuocTiepTheoAsync(int buocId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<BuocXuLy>(
                "sp_BuocXuLy_GetBuocTiepTheo",
                new { BuocId = buocId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<BuocXuLy?> GetBuocDauTienAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<BuocXuLy>(
                "sp_BuocXuLy_GetBuocDauTien",
                commandType: CommandType.StoredProcedure);
        }
    }
}
