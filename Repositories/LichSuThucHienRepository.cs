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

        public async Task<IEnumerable<LichSuThucHienWithDetails>> GetByOrderAsync(int orderId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<LichSuThucHienWithDetails>(
                "sp_LichSuThucHien_GetByOrder",
                new { OrderId = orderId },
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
            parameters.Add("@OrderId", lichSu.order_id);
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

        public async Task<IEnumerable<dynamic>> GetTrangThaiAsync(int orderId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<dynamic>(
                "sp_LichSuThucHien_GetTrangThai",
                new { OrderId = orderId },
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

        public async Task<IEnumerable<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<OrderItem>(
                "SELECT * FROM dbo.OrderItem WHERE order_id = @orderId",
                new { orderId });
        }

        public async Task<bool> StartStepForOrderAsync(int orderId, int stepId, int employeeId)
        {
            try
            {
                using var connection = _context.CreateConnection();

                // Kiểm tra xem LichSuThucHien đã tồn tại cho đơn hàng và bước này chưa
                var existing = await connection.QuerySingleOrDefaultAsync<int>(
                    "SELECT COUNT(*) FROM dbo.LichSuThucHien WHERE order_id = @orderId AND buoc_id = @stepId",
                    new { orderId, stepId });

                if (existing > 0)
                {
                    // Cập nhật bản ghi hiện có
                    await connection.ExecuteAsync(
                        "UPDATE dbo.LichSuThucHien SET nv_id = @employeeId, trang_thai = @status, thoi_diem_bat_dau = @startTime WHERE order_id = @orderId AND buoc_id = @stepId",
                        new
                        {
                            employeeId,
                            status = TrangThaiThucHien.CHUA_HOAN_THANH,
                            startTime = DateTime.Now,
                            orderId,
                            stepId
                        });
                }
                else
                {
                    // Tạo bản ghi mới
                    await connection.ExecuteAsync(
                        "INSERT INTO dbo.LichSuThucHien (order_id, buoc_id, nv_id, trang_thai, thoi_diem_bat_dau, thoi_diem_tao) VALUES (@orderId, @stepId, @employeeId, @status, @startTime, @createTime)",
                        new
                        {
                            orderId,
                            stepId,
                            employeeId,
                            status = TrangThaiThucHien.CHUA_HOAN_THANH,
                            startTime = DateTime.Now,
                            createTime = DateTime.Now
                        });
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in StartStepForOrderAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CompleteStepForOrderAsync(int orderId, int stepId, int employeeId)
        {
            try
            {
                using var connection = _context.CreateConnection();

                // Cập nhật bản ghi hiện có để đánh dấu là đã hoàn thành
                var rowsAffected = await connection.ExecuteAsync(
                    "UPDATE dbo.LichSuThucHien SET trang_thai = @status, thoi_diem_ket_thuc = @endTime WHERE order_id = @orderId AND buoc_id = @stepId AND nv_id = @employeeId",
                    new
                    {
                        status = TrangThaiThucHien.HOAN_THANH,
                        endTime = DateTime.Now,
                        orderId,
                        stepId,
                        employeeId
                    });

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CompleteStepForOrderAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateStepStatusForOrderAsync(int orderId, int stepId, int employeeId, string action)
        {
            try
            {
                using var connection = _context.CreateConnection();

                var result = await connection.QuerySingleOrDefaultAsync<int>(
                    "sp_LichSuThucHien_UpdateStepStatus",
                    new
                    {
                        OrderId = orderId,
                        StepId = stepId,
                        EmployeeId = employeeId,
                        Action = action
                    },
                    commandType: CommandType.StoredProcedure);

                return result == 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateStepStatusForOrderAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CheckAndUpdateOrderStatusAsync(int orderId)
        {
            try
            {
                using var connection = _context.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@OrderId", orderId);
                parameters.Add("@Success", dbType: DbType.Boolean, direction: ParameterDirection.Output);
                parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    "sp_Order_CheckAndUpdateStatus",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                var success = parameters.Get<bool>("@Success");
                var message = parameters.Get<string>("@Message");

                Console.WriteLine($"CheckAndUpdateOrderStatus result: {success}, Message: {message}");
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckAndUpdateOrderStatusAsync: {ex.Message}");
                return false;
            }
        }
    }
}
