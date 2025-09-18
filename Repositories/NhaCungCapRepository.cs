using BTL.Web.Data;
using BTL.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BTL.Web.Repositories
{
    public class NhaCungCapRepository : INhaCungCapRepository
    {
        private readonly DatabaseContext _context;

        public NhaCungCapRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NhaCungCap>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", 1);
            parameters.Add("@PageSize", 1000);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            return await connection.QueryAsync<NhaCungCap>(
                "sp_NhaCungCap_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<PagedResult<NhaCungCap>> GetAllPagedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var items = await connection.QueryAsync<NhaCungCap>(
                "sp_NhaCungCap_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure);

            var totalItems = parameters.Get<int>("@TotalCount");

            return new PagedResult<NhaCungCap>(items, totalItems, pageNumber, pageSize);
        }

        public async Task<NhaCungCap?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<NhaCungCap>(
                "EXEC sp_NhaCungCap_GetById @NccId",
                new { NccId = id });
        }

        public async Task<NhaCungCap> CreateAsync(NhaCungCap nhaCungCap)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Ten", nhaCungCap.ten);
            parameters.Add("@DiaChi", nhaCungCap.dia_chi);
            parameters.Add("@Sdt", nhaCungCap.sdt);
            parameters.Add("@NccId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "sp_NhaCungCap_Create",
                parameters,
                commandType: CommandType.StoredProcedure);

            var nccId = parameters.Get<int>("@NccId");
            nhaCungCap.ncc_id = nccId;

            return nhaCungCap;
        }

        public async Task<NhaCungCap> UpdateAsync(NhaCungCap nhaCungCap)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@NccId", nhaCungCap.ncc_id);
            parameters.Add("@Ten", nhaCungCap.ten);
            parameters.Add("@DiaChi", nhaCungCap.dia_chi);
            parameters.Add("@Sdt", nhaCungCap.sdt);

            await connection.ExecuteAsync(
                "sp_NhaCungCap_Update",
                parameters,
                commandType: CommandType.StoredProcedure);

            return nhaCungCap;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();

            try
            {
                await connection.ExecuteAsync(
                    "EXEC sp_NhaCungCap_Delete @NccId",
                    new { NccId = id });
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
                "SELECT COUNT(*) FROM dbo.NhaCungCap WHERE ncc_id = @id",
                new { id });
            return count > 0;
        }

        public async Task<bool> ExistsByTenAsync(string ten, int? excludeId = null)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT COUNT(*) FROM dbo.NhaCungCap WHERE ten = @ten";
            object parameters;

            if (excludeId.HasValue)
            {
                sql += " AND ncc_id != @excludeId";
                parameters = new { ten, excludeId = excludeId.Value };
            }
            else
            {
                parameters = new { ten };
            }

            var count = await connection.QuerySingleAsync<int>(sql, parameters);
            return count > 0;
        }

        public async Task<(bool can_delete, string message)> CanDeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();

            // Kiểm tra nhà cung cấp có trong phiếu nhập không
            var phieuNhapCount = await connection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM dbo.NL_NCC WHERE ncc_id = @id",
                new { id });

            if (phieuNhapCount > 0)
            {
                return (false, "Không thể xóa nhà cung cấp đã có lịch sử cung cấp nguyên liệu.");
            }

            return (true, string.Empty);
        }

        public async Task<IEnumerable<NhaCungCap>> SearchAsync(string searchTerm)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", 1);
            parameters.Add("@PageSize", 1000);
            parameters.Add("@SearchTerm", searchTerm);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            return await connection.QueryAsync<NhaCungCap>(
                "sp_NhaCungCap_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<PagedResult<NhaCungCap>> SearchPagedAsync(string searchTerm, int pageNumber, int pageSize)
        {
            return await GetAllPagedAsync(pageNumber, pageSize, searchTerm);
        }

        public async Task<IEnumerable<dynamic>> GetStatsAsync(int? nccId = null)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@NccId", nccId);

            return await connection.QueryAsync<dynamic>(
                "sp_NhaCungCap_GetStats",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<NguyenLieuWithNhaCungCap>> GetNguyenLieuByNhaCungCapAsync(int nccId)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@NccId", nccId);

            return await connection.QueryAsync<NguyenLieuWithNhaCungCap>(
                "sp_NhaCungCap_GetNguyenLieu",
                parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
}
