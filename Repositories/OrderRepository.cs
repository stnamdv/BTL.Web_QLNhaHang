using BTL.Web.Data;
using BTL.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BTL.Web.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DatabaseContext _context;

        public OrderRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<(int OrderId, bool Success, string Message)> CreateOrderAsync(OrderCreateViewModel model)
        {
            using var connection = _context.CreateConnection();

            // Tạo DataTable cho Table-Valued Parameter
            var orderItemsTable = new DataTable();
            orderItemsTable.Columns.Add("MonId", typeof(int));
            orderItemsTable.Columns.Add("SoLuong", typeof(int));

            // Thêm dữ liệu vào DataTable
            var validOrderItems = model.OrderItems?.Where(x => x.MonId > 0 && x.SoLuong > 0) ?? new List<OrderItemCreateViewModel>();
            foreach (var item in validOrderItems)
            {
                orderItemsTable.Rows.Add(item.MonId, item.SoLuong);
            }

            Console.WriteLine($"OrderItems Table-Valued Parameter: {validOrderItems.Count()} items");
            foreach (var item in validOrderItems)
            {
                Console.WriteLine($"- MonId: {item.MonId}, SoLuong: {item.SoLuong}");
            }

            var parameters = new DynamicParameters();
            parameters.Add("@LaMangVe", model.LaMangVe);
            parameters.Add("@BanId", model.BanId);
            parameters.Add("@SoKhach", model.SoKhach);
            parameters.Add("@KhachHangTen", model.KhachHangTen);
            parameters.Add("@KhachHangSdt", model.KhachHangSdt);
            parameters.Add("@OrderItems", orderItemsTable.AsTableValuedParameter("dbo.OrderItemsType"));
            parameters.Add("@OrderId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@Success", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "sp_Order_Create",
                parameters,
                commandType: CommandType.StoredProcedure);

            var orderId = parameters.Get<int>("@OrderId");
            var success = parameters.Get<bool>("@Success");
            var message = parameters.Get<string>("@Message") ?? "";

            return (orderId, success, message);
        }


        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            using var connection = _context.CreateConnection();

            var order = await connection.QueryFirstOrDefaultAsync<Order>(
                "SELECT * FROM [Order] WHERE order_id = @OrderId",
                new { OrderId = orderId });

            if (order != null)
            {
                // Load OrderItems
                var orderItems = await connection.QueryAsync<OrderItem>(
                    "SELECT * FROM OrderItem WHERE order_id = @OrderId",
                    new { OrderId = orderId });

                order.OrderItems = orderItems.ToList();
            }

            return order;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            using var connection = _context.CreateConnection();

            var orders = await connection.QueryAsync<Order>(
                "SELECT * FROM [Order] ORDER BY thoi_diem_dat DESC");

            return orders;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            using var connection = _context.CreateConnection();

            var affectedRows = await connection.ExecuteAsync(
                "UPDATE [Order] SET trang_thai = @Status WHERE order_id = @OrderId",
                new { OrderId = orderId, Status = status });

            return affectedRows > 0;
        }

        public async Task<PagedResult<OrderWithDetails>> GetOrdersWithPaginationAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchKeyword = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? filterType = null,
            string? status = null)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchKeyword", searchKeyword);
            parameters.Add("@FromDate", fromDate);
            parameters.Add("@ToDate", toDate);
            parameters.Add("@FilterType", filterType);
            parameters.Add("@Status", status);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var orders = await connection.QueryAsync<OrderWithDetails>(
                "sp_Order_GetOrdersWithPagination",
                parameters,
                commandType: CommandType.StoredProcedure);

            var totalCount = parameters.Get<int>("@TotalCount");

            return new PagedResult<OrderWithDetails>(orders, totalCount, pageNumber, pageSize);
        }

        public async Task<OrderWithDetails?> GetOrderDetailsAsync(int orderId)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@OrderId", orderId);

            using var multi = await connection.QueryMultipleAsync(
                "sp_Order_GetOrderDetails",
                parameters,
                commandType: CommandType.StoredProcedure);

            var order = await multi.ReadFirstOrDefaultAsync<OrderWithDetails>();
            if (order != null)
            {
                var orderItems = await multi.ReadAsync<OrderItem>();
                var orderHistory = await multi.ReadAsync<LichSuThucHien>();

                order.OrderItems = orderItems.ToList();
                order.LichSuThucHiens = orderHistory.ToList();
            }

            return order;
        }

        public async Task<(bool Success, string Message)> CancelOrderAsync(int orderId, int employeeId, string? reason = null)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@OrderId", orderId);
            parameters.Add("@EmployeeId", employeeId);
            parameters.Add("@Reason", reason);
            parameters.Add("@Success", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "sp_Order_CancelOrder",
                parameters,
                commandType: CommandType.StoredProcedure);

            var success = parameters.Get<bool>("@Success");
            var message = parameters.Get<string>("@Message") ?? "";

            return (success, message);
        }

        public async Task<object> GetOrderStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, string? filterType = null)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@FromDate", fromDate);
            parameters.Add("@ToDate", toDate);
            parameters.Add("@FilterType", filterType);

            var statistics = await connection.QueryFirstOrDefaultAsync(
                "sp_Order_GetStatistics",
                parameters,
                commandType: CommandType.StoredProcedure);

            return statistics ?? new { };
        }
    }
}
