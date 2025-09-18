using BTL.Web.Data;
using BTL.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace BTL.Web.Repositories
{
    public class LoaiNhanVienRepository : ILoaiNhanVienRepository
    {
        private readonly DatabaseContext _context;

        public LoaiNhanVienRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LoaiNhanVien>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<LoaiNhanVien>("sp_LoaiNhanVien_GetAll",
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<LoaiNhanVien?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<LoaiNhanVien>(
                "SELECT loai_nv_id, loai_nv, luong_co_ban FROM dbo.LoaiNhanVien WHERE loai_nv_id = @id",
                new { id });
        }

        public async Task<LoaiNhanVien?> GetByTypeAsync(string loaiNv)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<LoaiNhanVien>(
                "sp_LoaiNhanVien_GetByType",
                new { LoaiNv = loaiNv },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<LoaiNhanVien> CreateAsync(LoaiNhanVien loaiNhanVien)
        {
            using var connection = _context.CreateConnection();
            var parameters = new
            {
                loai_nv = loaiNhanVien.loai_nv,
                luong_co_ban = loaiNhanVien.luong_co_ban
            };

            await connection.ExecuteAsync(
                "sp_LoaiNhanVien_Create",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            return loaiNhanVien;
        }

        public async Task<LoaiNhanVien> UpdateAsync(LoaiNhanVien loaiNhanVien)
        {
            using var connection = _context.CreateConnection();
            var parameters = new
            {
                loai_nv_id = loaiNhanVien.loai_nv_id,
                loai_nv = loaiNhanVien.loai_nv,
                luong_co_ban = loaiNhanVien.luong_co_ban
            };

            await connection.ExecuteAsync(
                "sp_LoaiNhanVien_Update",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            return loaiNhanVien;
        }

        public async Task<bool> DeleteAsync(int loaiNvId)
        {
            using var connection = _context.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "sp_LoaiNhanVien_Delete",
                new { loai_nv_id = loaiNvId },
                commandType: System.Data.CommandType.StoredProcedure);

            return rowsAffected > 0;
        }

        public async Task<bool> ExistsByTypeAsync(int loaiNvId)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<LoaiNhanVien>(
                "sp_LoaiNhanVien_GetByType",
                new { LoaiNvId = loaiNvId },
                commandType: System.Data.CommandType.StoredProcedure);

            return result != null;
        }

        public async Task<LoaiNhanVienDetails?> GetDetailsWithUsageAsync(int loaiNvId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<LoaiNhanVienDetails>(
                "sp_LoaiNhanVien_GetDetailsWithUsage",
                new { loai_nv_id = loaiNvId },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<NhanVienInfo>> GetEmployeesAsync(int loaiNvId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<NhanVienInfo>(
                "SELECT nv.nv_id, nv.ho_ten, lnv.loai_nv, nv.trang_thai, 0 as has_active_orders " +
                "FROM dbo.NhanVien nv INNER JOIN dbo.LoaiNhanVien lnv ON nv.loai_nv_id = lnv.loai_nv_id WHERE nv.loai_nv_id = @loai_nv_id",
                new { loai_nv_id = loaiNvId });
        }

        public async Task<LoaiNhanVienUpdateCheck> CanUpdateAsync(int loaiNvId, decimal newLuongCoBan)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstAsync<LoaiNhanVienUpdateCheck>(
                "sp_LoaiNhanVien_CanUpdate",
                new { loai_nv_id = loaiNvId, luong_co_ban = newLuongCoBan },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<LoaiNhanVienDeleteCheck> CanDeleteAsync(int loaiNvId)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryFirstAsync<LoaiNhanVienDeleteCheck>(
                "sp_LoaiNhanVien_CanDelete",
                new { loai_nv_id = loaiNvId },
                commandType: System.Data.CommandType.StoredProcedure);

            // Debug logging
            Console.WriteLine($"Repository CanDelete result: {System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}");

            return result;
        }

        public async Task<IEnumerable<LoaiNhanVien>> GetAvailableAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<LoaiNhanVien>("sp_LoaiNhanVien_GetAvailable",
                commandType: System.Data.CommandType.StoredProcedure);
        }

    }
}
