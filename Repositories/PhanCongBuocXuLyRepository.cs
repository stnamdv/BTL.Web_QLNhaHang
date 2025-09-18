using BTL.Web.Data;
using BTL.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BTL.Web.Repositories
{
    public class PhanCongBuocXuLyRepository : IPhanCongBuocXuLyRepository
    {
        private readonly DatabaseContext _context;

        public PhanCongBuocXuLyRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BuocXuLyWithPhanCong>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<BuocXuLyWithPhanCong>(
                "sp_PhanCongBuocXuLy_GetAll",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<BuocXuLyWithPhanCong>> GetByBuocAsync(int buocId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<BuocXuLyWithPhanCong>(
                "sp_PhanCongBuocXuLy_GetByBuoc",
                new { BuocId = buocId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<BuocXuLyWithPhanCong>> GetByLoaiNhanVienAsync(int loaiNvId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<BuocXuLyWithPhanCong>(
                "sp_PhanCongBuocXuLy_GetByLoaiNhanVien",
                new { LoaiNvId = loaiNvId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<dynamic>> GetNhanVienChoBuocAsync(int buocId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<dynamic>(
                "sp_PhanCongBuocXuLy_GetNhanVienChoBuoc",
                new { BuocId = buocId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<PhanCongBuocXuLy> CreateAsync(PhanCongBuocXuLy phanCong)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@BuocId", phanCong.buoc_id);
            parameters.Add("@LoaiNvId", phanCong.loai_nv_id);
            parameters.Add("@VaiTro", phanCong.vai_tro);
            parameters.Add("@PhanCongBuocId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "sp_PhanCongBuocXuLy_Create",
                parameters,
                commandType: CommandType.StoredProcedure);

            var phanCongBuocId = parameters.Get<int>("@PhanCongBuocId");
            phanCong.phan_cong_buoc_id = phanCongBuocId;

            return phanCong;
        }

        public async Task<PhanCongBuocXuLy> UpdateAsync(PhanCongBuocXuLy phanCong)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PhanCongBuocId", phanCong.phan_cong_buoc_id);
            parameters.Add("@VaiTro", phanCong.vai_tro);
            parameters.Add("@TrangThai", phanCong.trang_thai);

            await connection.ExecuteAsync(
                "sp_PhanCongBuocXuLy_Update",
                parameters,
                commandType: CommandType.StoredProcedure);

            return phanCong;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();

            try
            {
                await connection.ExecuteAsync(
                    "sp_PhanCongBuocXuLy_Delete",
                    new { PhanCongBuocId = id },
                    commandType: CommandType.StoredProcedure);
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
                "SELECT COUNT(*) FROM dbo.PhanCongBuocXuLy WHERE phan_cong_buoc_id = @id",
                new { id });
            return count > 0;
        }
    }
}
