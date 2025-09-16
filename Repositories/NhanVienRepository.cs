using BTL.Web.Data;
using BTL.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BTL.Web.Repositories
{
    public class NhanVienRepository : INhanVienRepository
    {
        private readonly DatabaseContext _context;

        public NhanVienRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NhanVienWithLoaiNhanVien>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<NhanVienWithLoaiNhanVien>("EXEC sp_NhanVien_GetAll");
        }

        public async Task<NhanVien?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<NhanVien>(
                "EXEC sp_NhanVien_GetById @nv_id",
                new { nv_id = id });
        }

        public async Task<IEnumerable<NhanVienWithLoaiNhanVien>> GetByLoaiNvAsync(string loaiNv)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<NhanVienWithLoaiNhanVien>(
                "EXEC sp_NhanVien_GetByLoaiNv @loai_nv",
                new { loai_nv = loaiNv });
        }

        public async Task<NhanVien> CreateAsync(NhanVien nhanVien)
        {
            using var connection = _context.CreateConnection();
            var sql = "EXEC sp_NhanVien_Create @ho_ten, @loai_nv, @ngay_vao_lam, @trang_thai";

            var id = await connection.QuerySingleAsync<int>(sql, new
            {
                ho_ten = nhanVien.ho_ten,
                loai_nv = nhanVien.loai_nv,
                ngay_vao_lam = nhanVien.ngay_vao_lam,
                trang_thai = nhanVien.trang_thai
            });
            nhanVien.nv_id = id;
            return nhanVien;
        }

        public async Task<NhanVien> UpdateAsync(NhanVien nhanVien)
        {
            using var connection = _context.CreateConnection();
            var sql = "EXEC sp_NhanVien_Update @nv_id, @ho_ten, @loai_nv, @ngay_vao_lam, @trang_thai";

            await connection.ExecuteAsync(sql, new
            {
                nv_id = nhanVien.nv_id,
                ho_ten = nhanVien.ho_ten,
                loai_nv = nhanVien.loai_nv,
                ngay_vao_lam = nhanVien.ngay_vao_lam,
                trang_thai = nhanVien.trang_thai
            });
            return nhanVien;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "EXEC sp_NhanVien_Delete @nv_id",
                new { nv_id = id });
            return rowsAffected > 0;
        }

        public async Task<NhanVien?> GetDetailsWithLoaiNhanVienAsync(int id)
        {
            using var connection = _context.CreateConnection();

            // Load NhanVien with LoaiNhanVien information using a single query
            var sql = @"EXEC sp_NhanVien_GetById @nv_id";

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { nv_id = id });

            if (result == null) return null;

            var nhanVien = new NhanVien
            {
                nv_id = result.nv_id,
                ho_ten = result.ho_ten,
                loai_nv = result.loai_nv,
                ngay_vao_lam = result.ngay_vao_lam,
                trang_thai = result.trang_thai,
                LoaiNhanVien = new LoaiNhanVien
                {
                    loai_nv = result.loai_nv,
                    luong_co_ban = result.luong_co_ban
                }
            };

            return nhanVien;
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var count = await connection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM dbo.NhanVien WHERE nv_id = @nv_id",
                new { nv_id = id });
            return count > 0;
        }

        public async Task<(bool can_delete, string message)> CanDeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "EXEC sp_NhanVien_CanDelete @nv_id",
                new { nv_id = id });

            return (result.can_delete, result.message);
        }

        public async Task<IEnumerable<NhanVienWithLoaiNhanVien>> GetActiveAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<NhanVienWithLoaiNhanVien>("EXEC sp_NhanVien_GetActive");
        }

        public async Task<IEnumerable<NhanVienWithLoaiNhanVien>> SearchByNameAsync(string searchTerm)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<NhanVienWithLoaiNhanVien>(
                "EXEC sp_NhanVien_SearchByName @search_term",
                new { search_term = searchTerm });
        }

        public async Task<IEnumerable<dynamic>> GetStatsByLoaiAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync("EXEC sp_NhanVien_GetStatsByLoai");
        }
    }
}
