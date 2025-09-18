using BTL.Web.Data;
using BTL.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BTL.Web.Repositories
{
    public class OrderReceptionRepository : IOrderReceptionRepository
    {
        private readonly DatabaseContext _context;

        public OrderReceptionRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> ConfirmOrderAsync(int orderId, int employeeId)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@OrderId", orderId);
            parameters.Add("@EmployeeId", employeeId);
            parameters.Add("@Success", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "sp_OrderReception_ConfirmOrder",
                parameters,
                commandType: CommandType.StoredProcedure);

            var success = parameters.Get<bool>("@Success");
            var message = parameters.Get<string>("@Message") ?? "";

            return (success, message);
        }

        public async Task<IEnumerable<BanAn>> GetAvailableTablesAsync(int? capacity = null)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            if (capacity.HasValue)
                parameters.Add("@Capacity", capacity.Value);

            var result = await connection.QueryAsync<BanAnWithLoaiBan>(
                "sp_OrderReception_GetAvailableTables",
                parameters,
                commandType: CommandType.StoredProcedure);

            // Convert to BanAn with LoaiBan populated
            return result.Select(r => new BanAn
            {
                ban_id = r.ban_id,
                loai_ban_id = r.loai_ban_id,
                so_hieu = r.so_hieu,
                LoaiBan = new LoaiBan
                {
                    loai_ban_id = r.loai_ban_id,
                    suc_chua = r.suc_chua,
                    so_luong = r.loai_ban_so_luong
                }
            });
        }

        public async Task<(bool IsValid, string Message)> ValidateTableAsync(int tableId, int customerCount)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@TableId", tableId);
            parameters.Add("@CustomerCount", customerCount);
            parameters.Add("@IsValid", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "sp_OrderReception_ValidateTable",
                parameters,
                commandType: CommandType.StoredProcedure);

            var isValid = parameters.Get<bool>("@IsValid");
            var message = parameters.Get<string>("@Message") ?? "";

            return (isValid, message);
        }

        public async Task<Order?> GetOrderStatusAsync(int orderId)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@OrderId", orderId);

            using var multi = await connection.QueryMultipleAsync(
                "sp_OrderReception_GetOrderStatus",
                parameters,
                commandType: CommandType.StoredProcedure);

            var order = await multi.ReadFirstOrDefaultAsync<Order>();
            if (order == null) return null;

            var orderItems = await multi.ReadAsync<OrderItem>();
            var lichSuThucHiens = await multi.ReadAsync<LichSuThucHien>();

            // Gán OrderItems vào Order
            order.OrderItems = orderItems.ToList();

            return order;
        }

        public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
        {
            using var connection = _context.CreateConnection();

            var result = await connection.QueryAsync<OrderWithDetails>(
                "sp_OrderReception_GetPendingOrders",
                commandType: CommandType.StoredProcedure);

            // Convert OrderWithDetails to Order
            return result.Select(o => new Order
            {
                order_id = o.order_id,
                kh_id = o.kh_id,
                ban_id = o.ban_id,
                la_mang_ve = o.la_mang_ve,
                trang_thai = o.trang_thai,
                so_khach = o.so_khach,
                thoi_diem_dat = o.thoi_diem_dat,
                tong_tien = o.tong_tien,
                KhachHang = o.KhachHang,
                BanAn = o.BanAn,
                OrderItems = o.OrderItems
            });
        }

        public async Task<IEnumerable<OrderWithDetails>> GetAllPendingOrdersAsync()
        {
            using var connection = _context.CreateConnection();

            return await connection.QueryAsync<OrderWithDetails>(
                "sp_OrderReception_GetAllPendingOrders",
                commandType: CommandType.StoredProcedure);
        }
    }
}
