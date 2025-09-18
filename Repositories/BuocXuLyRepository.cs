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

        public async Task<BuocXuLy> CreateAsync(BuocXuLy buocXuLy)
        {
            using var connection = _context.CreateConnection();
            var parameters = new
            {
                TenBuoc = buocXuLy.ten_buoc,
                MoTa = buocXuLy.mo_ta,
                ThuTu = buocXuLy.thu_tu,
                ThoiGianDuKien = buocXuLy.thoi_gian_du_kien
            };

            var result = await connection.QueryFirstOrDefaultAsync<BuocXuLy>(
                "sp_BuocXuLy_Create",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result ?? buocXuLy;
        }

        public async Task<BuocXuLy> UpdateAsync(BuocXuLy buocXuLy)
        {
            using var connection = _context.CreateConnection();
            var parameters = new
            {
                BuocId = buocXuLy.buoc_id,
                TenBuoc = buocXuLy.ten_buoc,
                MoTa = buocXuLy.mo_ta,
                ThuTu = buocXuLy.thu_tu,
                ThoiGianDuKien = buocXuLy.thoi_gian_du_kien
            };

            var result = await connection.QueryFirstOrDefaultAsync<BuocXuLy>(
                "sp_BuocXuLy_Update",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result ?? buocXuLy;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            try
            {
                var result = await connection.QueryFirstOrDefaultAsync<int>(
                    "sp_BuocXuLy_Delete",
                    new { BuocId = id },
                    commandType: CommandType.StoredProcedure);

                return result == 1;
            }
            catch (SqlException ex)
            {
                // Re-throw với thông báo lỗi từ stored procedure
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_BuocXuLy_Exists",
                new { BuocId = id },
                commandType: CommandType.StoredProcedure);

            return result > 0;
        }
    }
}
