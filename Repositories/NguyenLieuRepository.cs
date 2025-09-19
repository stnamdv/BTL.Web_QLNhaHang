using BTL.Web.Data;
using BTL.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BTL.Web.Repositories
{
    public class NguyenLieuRepository : INguyenLieuRepository
    {
        private readonly DatabaseContext _context;

        public NguyenLieuRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NguyenLieu>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", 1);
            parameters.Add("@PageSize", 1000);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            return await connection.QueryAsync<NguyenLieu>(
                "sp_NguyenLieu_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<PagedResult<NguyenLieu>> GetAllPagedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var items = await connection.QueryAsync<NguyenLieu>(
                "sp_NguyenLieu_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure);

            var totalItems = parameters.Get<int>("@TotalCount");

            return new PagedResult<NguyenLieu>(items, totalItems, pageNumber, pageSize);
        }

        public async Task<NguyenLieu?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<NguyenLieu>(
                "EXEC sp_NguyenLieu_GetById @NlId",
                new { NlId = id });
        }

        public async Task<NguyenLieuWithNhaCungCap?> GetWithNhaCungCapAsync(int id)
        {
            using var connection = _context.CreateConnection();

            using var multi = await connection.QueryMultipleAsync(
                "EXEC sp_NguyenLieu_GetById @NlId",
                new { NlId = id });

            var nguyenLieu = await multi.ReadFirstOrDefaultAsync<NguyenLieuWithNhaCungCap>();
            if (nguyenLieu != null)
            {
                var mons = await multi.ReadAsync<MonSuDungNguyenLieu>();
                nguyenLieu.Mons = mons;
            }

            return nguyenLieu;
        }

        public async Task<NguyenLieuWithMon?> GetWithMonAsync(int id)
        {
            using var connection = _context.CreateConnection();

            using var multi = await connection.QueryMultipleAsync(
                "EXEC sp_NguyenLieu_GetById @NlId",
                new { NlId = id });

            var nguyenLieu = await multi.ReadFirstOrDefaultAsync<NguyenLieu>();
            if (nguyenLieu == null) return null;

            var mons = await multi.ReadAsync<MonSuDungNguyenLieu>();

            return new NguyenLieuWithMon
            {
                NguyenLieu = nguyenLieu,
                Mons = mons
            };
        }

        public async Task<NguyenLieu> CreateAsync(NguyenLieu nguyenLieu)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Ten", nguyenLieu.ten);
            parameters.Add("@DonVi", nguyenLieu.don_vi);
            parameters.Add("@NguonGoc", nguyenLieu.nguon_goc);
            parameters.Add("@GiaNhap", nguyenLieu.gia_nhap);
            parameters.Add("@NccId", nguyenLieu.ncc_id);
            parameters.Add("@NlId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "sp_NguyenLieu_Create",
                parameters,
                commandType: CommandType.StoredProcedure);

            var nlId = parameters.Get<int>("@NlId");
            nguyenLieu.nl_id = nlId;

            return nguyenLieu;
        }

        public async Task<NguyenLieu> UpdateAsync(NguyenLieu nguyenLieu)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@NlId", nguyenLieu.nl_id);
            parameters.Add("@Ten", nguyenLieu.ten);
            parameters.Add("@DonVi", nguyenLieu.don_vi);
            parameters.Add("@NguonGoc", nguyenLieu.nguon_goc);
            parameters.Add("@GiaNhap", nguyenLieu.gia_nhap);
            parameters.Add("@NccId", nguyenLieu.ncc_id);

            await connection.ExecuteAsync(
                "sp_NguyenLieu_Update",
                parameters,
                commandType: CommandType.StoredProcedure);

            return nguyenLieu;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();

            try
            {
                await connection.ExecuteAsync(
                    "EXEC sp_NguyenLieu_Delete @NlId",
                    new { NlId = id });
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
                "SELECT COUNT(*) FROM dbo.NguyenLieu WHERE nl_id = @id",
                new { id });
            return count > 0;
        }

        public async Task<bool> ExistsByTenAsync(string ten, int? excludeId = null)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT COUNT(*) FROM dbo.NguyenLieu WHERE ten = @ten";
            object parameters;

            if (excludeId.HasValue)
            {
                sql += " AND nl_id != @excludeId";
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

            // Kiểm tra nguyên liệu có trong công thức không
            var congThucCount = await connection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM dbo.CongThuc WHERE nl_id = @id",
                new { id });

            if (congThucCount > 0)
            {
                return (false, "Không thể xóa nguyên liệu đang được sử dụng trong công thức món ăn.");
            }

            return (true, string.Empty);
        }

        public async Task<IEnumerable<NguyenLieu>> SearchAsync(string searchTerm)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", 1);
            parameters.Add("@PageSize", 1000);
            parameters.Add("@SearchTerm", searchTerm);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            return await connection.QueryAsync<NguyenLieu>(
                "sp_NguyenLieu_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<PagedResult<NguyenLieu>> SearchPagedAsync(string searchTerm, int pageNumber, int pageSize)
        {
            return await GetAllPagedAsync(pageNumber, pageSize, searchTerm);
        }

        public async Task<IEnumerable<NguyenLieu>> SearchByCriteriaAsync(string? searchTerm, string? donVi, string? nguonGoc, int pageNumber, int pageSize)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@SearchTerm", searchTerm);
            parameters.Add("@DonVi", donVi);
            parameters.Add("@NguonGoc", nguonGoc);
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            return await connection.QueryAsync<NguyenLieu>(
                "sp_NguyenLieu_Search",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<dynamic>> GetStatsAsync(int? nlId = null)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@NlId", nlId);

            return await connection.QueryAsync<dynamic>(
                "sp_NguyenLieu_GetStats",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<(bool isValid, string errorMessage)> ValidateNguonGocAsync(string nguonGoc)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@NguonGoc", nguonGoc);
            parameters.Add("@IsValid", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            parameters.Add("@ErrorMessage", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "sp_NguyenLieu_ValidateNguonGoc",
                parameters,
                commandType: CommandType.StoredProcedure);

            var isValid = parameters.Get<bool>("@IsValid");
            var errorMessage = parameters.Get<string>("@ErrorMessage") ?? string.Empty;

            return (isValid, errorMessage);
        }

        public async Task<PagedResult<NguyenLieuWithNhaCungCap>> GetAllWithNhaCungCapPagedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var items = await connection.QueryAsync<NguyenLieuWithNhaCungCap>(
                "sp_NguyenLieu_GetAllWithNhaCungCap",
                parameters,
                commandType: CommandType.StoredProcedure);

            var totalItems = parameters.Get<int>("@TotalCount");

            return new PagedResult<NguyenLieuWithNhaCungCap>(items, totalItems, pageNumber, pageSize);
        }
    }
}
