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
                loai_nv = loaiNhanVien.loai_nv,
                luong_co_ban = loaiNhanVien.luong_co_ban
            };

            await connection.ExecuteAsync(
                "sp_LoaiNhanVien_Update",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            return loaiNhanVien;
        }

        public async Task<bool> DeleteAsync(string loaiNv)
        {
            using var connection = _context.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "sp_LoaiNhanVien_Delete",
                new { loai_nv = loaiNv },
                commandType: System.Data.CommandType.StoredProcedure);

            return rowsAffected > 0;
        }

        public async Task<bool> ExistsByTypeAsync(string loaiNv)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<LoaiNhanVien>(
                "sp_LoaiNhanVien_GetByType",
                new { LoaiNv = loaiNv },
                commandType: System.Data.CommandType.StoredProcedure);

            return result != null;
        }

        public async Task<LoaiNhanVienDetails?> GetDetailsWithUsageAsync(string loaiNv)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<LoaiNhanVienDetails>(
                "sp_LoaiNhanVien_GetDetailsWithUsage",
                new { loai_nv = loaiNv },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<NhanVienInfo>> GetEmployeesAsync(string loaiNv)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<NhanVienInfo>(
                "SELECT nv.nv_id, nv.ho_ten, nv.loai_nv, nv.trang_thai, 0 as has_active_orders " +
                "FROM dbo.NhanVien nv WHERE nv.loai_nv = @loai_nv",
                new { loai_nv = loaiNv });
        }

        public async Task<LoaiNhanVienUpdateCheck> CanUpdateAsync(string loaiNv, decimal newLuongCoBan)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstAsync<LoaiNhanVienUpdateCheck>(
                "sp_LoaiNhanVien_CanUpdate",
                new { loai_nv = loaiNv, luong_co_ban = newLuongCoBan },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<LoaiNhanVienDeleteCheck> CanDeleteAsync(string loaiNv)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryFirstAsync<LoaiNhanVienDeleteCheck>(
                "sp_LoaiNhanVien_CanDelete",
                new { loai_nv = loaiNv },
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

        public async Task<LoaiNhanVienValidation> ValidateTypeAsync(string loaiNv)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstAsync<LoaiNhanVienValidation>(
                "sp_LoaiNhanVien_ValidateType",
                new { loai_nv = loaiNv },
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
