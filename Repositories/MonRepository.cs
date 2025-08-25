using BTL.Web.Data;
using BTL.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;

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
            return await connection.QueryAsync<Mon>("SELECT * FROM dbo.Mon");
        }

        public async Task<Mon?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Mon>(
                "SELECT * FROM dbo.Mon WHERE mon_id = @Id",
                new { Id = id });
        }

        public async Task<Mon?> GetByMaMonAsync(string maMon)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Mon>(
                "SELECT * FROM dbo.Mon WHERE ma_mon = @MaMon",
                new { MaMon = maMon });
        }

        public async Task<IEnumerable<Mon>> GetByLoaiMonAsync(LoaiMon loaiMon)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Mon>(
                "SELECT * FROM dbo.Mon WHERE loai_mon = @LoaiMon",
                new { LoaiMon = loaiMon.ToString() });
        }

        public async Task<Mon> CreateAsync(Mon mon)
        {
            using var connection = _context.CreateConnection();
            var sql = @"
                INSERT INTO dbo.Mon (ma_mon, ten_mon, loai_mon, gia) 
                VALUES (@MaMon, @TenMon, @LoaiMon, @Gia);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            var id = await connection.QuerySingleAsync<int>(sql, new
            {
                mon.ma_mon,
                mon.ten_mon,
                LoaiMon = mon.loai_mon.ToString(),
                mon.gia
            });
            mon.mon_id = id;
            return mon;
        }

        public async Task<Mon> UpdateAsync(Mon mon)
        {
            using var connection = _context.CreateConnection();
            var sql = @"
                UPDATE dbo.Mon 
                SET ma_mon = @MaMon, ten_mon = @TenMon, loai_mon = @LoaiMon, gia = @Gia 
                WHERE mon_id = @MonId";

            await connection.ExecuteAsync(sql, new
            {
                mon.ma_mon,
                mon.ten_mon,
                LoaiMon = mon.loai_mon.ToString(),
                mon.gia,
                mon.mon_id
            });
            return mon;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM dbo.Mon WHERE mon_id = @Id",
                new { Id = id });
            return rowsAffected > 0;
        }
    }
}
