using BTL.Web.Data;
using BTL.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BTL.Web.Repositories
{
    public class LichSuThucHienRepository : ILichSuThucHienRepository
    {
        private readonly DatabaseContext _context;

        public LichSuThucHienRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<LichSuThucHienWithDetails>> GetAllPagedAsync(int pageNumber, int pageSize)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var items = await connection.QueryAsync<LichSuThucHienWithDetails>(
                "sp_LichSuThucHien_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure);

            var totalItems = parameters.Get<int>("@TotalCount");

            return new PagedResult<LichSuThucHienWithDetails>(items, totalItems, pageNumber, pageSize);
        }

        public async Task<IEnumerable<LichSuThucHienWithDetails>> GetByOrderItemAsync(int orderItemId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<LichSuThucHienWithDetails>(
                "sp_LichSuThucHien_GetByOrderItem",
                new { OrderItemId = orderItemId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<LichSuThucHienWithDetails>> GetByBuocAsync(int buocId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<LichSuThucHienWithDetails>(
                "sp_LichSuThucHien_GetByBuoc",
                new { BuocId = buocId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<LichSuThucHienWithDetails>> GetByNhanVienAsync(int nvId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<LichSuThucHienWithDetails>(
                "sp_LichSuThucHien_GetByNhanVien",
                new { NvId = nvId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<LichSuThucHien> CreateAsync(LichSuThucHien lichSu)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@OrderItemId", lichSu.order_item_id);
            parameters.Add("@BuocId", lichSu.buoc_id);
            parameters.Add("@NvId", lichSu.nv_id);
            parameters.Add("@GhiChu", lichSu.ghi_chu);
            parameters.Add("@LichSuId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "sp_LichSuThucHien_Create",
                parameters,
                commandType: CommandType.StoredProcedure);

            var lichSuId = parameters.Get<int>("@LichSuId");
            lichSu.lich_su_id = lichSuId;

            return lichSu;
        }

        public async Task<bool> BatDauAsync(int lichSuId)
        {
            using var connection = _context.CreateConnection();

            try
            {
                await connection.ExecuteAsync(
                    "sp_LichSuThucHien_BatDau",
                    new { LichSuId = lichSuId },
                    commandType: CommandType.StoredProcedure);
                return true;
            }
            catch (SqlException ex) when (ex.Number == 50000) // Custom error from stored procedure
            {
                return false;
            }
        }

        public async Task<bool> HoanThanhAsync(int lichSuId, string? ghiChu = null)
        {
            using var connection = _context.CreateConnection();

            try
            {
                await connection.ExecuteAsync(
                    "sp_LichSuThucHien_HoanThanh",
                    new { LichSuId = lichSuId, GhiChu = ghiChu },
                    commandType: CommandType.StoredProcedure);
                return true;
            }
            catch (SqlException ex) when (ex.Number == 50000) // Custom error from stored procedure
            {
                return false;
            }
        }

        public async Task<IEnumerable<dynamic>> GetTrangThaiAsync(int orderItemId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<dynamic>(
                "sp_LichSuThucHien_GetTrangThai",
                new { OrderItemId = orderItemId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<ThongKeHieuSuat>> GetThongKeAsync(int? nvId = null, int? buocId = null, DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@NvId", nvId);
            parameters.Add("@BuocId", buocId);
            parameters.Add("@TuNgay", tuNgay);
            parameters.Add("@DenNgay", denNgay);

            return await connection.QueryAsync<ThongKeHieuSuat>(
                "sp_LichSuThucHien_GetThongKe",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var count = await connection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM dbo.LichSuThucHien WHERE lich_su_id = @id",
                new { id });
            return count > 0;
        }
    }
}
