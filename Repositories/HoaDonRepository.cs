using BTL.Web.Data;
using BTL.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BTL.Web.Repositories
{
    public class HoaDonRepository : IHoaDonRepository
    {
        private readonly DatabaseContext _context;

        public HoaDonRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<HoaDon?> GetHoaDonByOrderIdAsync(int orderId)
        {
            using var connection = _context.CreateConnection();

            var hoaDon = await connection.QueryFirstOrDefaultAsync<HoaDon>(
                "sp_HoaDon_GetByOrderId",
                new { OrderId = orderId },
                commandType: CommandType.StoredProcedure);

            return hoaDon;
        }

        public async Task<HoaDon> CreateHoaDonAsync(int orderId, string? phuongThuc = null)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@OrderId", orderId);
            parameters.Add("@PhuongThuc", phuongThuc);
            parameters.Add("@HdId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@Success", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            parameters.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "sp_HoaDon_Create",
                parameters,
                commandType: CommandType.StoredProcedure);

            var success = parameters.Get<bool>("@Success");
            var message = parameters.Get<string>("@Message") ?? "";
            var hdId = parameters.Get<int>("@HdId");

            if (!success)
            {
                throw new Exception(message);
            }

            // Lấy hoá đơn vừa tạo
            var hoaDon = await connection.QueryFirstOrDefaultAsync<HoaDon>(
                "sp_HoaDon_GetById",
                new { HdId = hdId },
                commandType: CommandType.StoredProcedure);

            return hoaDon ?? throw new Exception("Không thể tạo hoá đơn");
        }

        public async Task<HoaDon?> GetHoaDonByIdAsync(int hdId)
        {
            using var connection = _context.CreateConnection();

            var hoaDon = await connection.QueryFirstOrDefaultAsync<HoaDon>(
                "sp_HoaDon_GetById",
                new { HdId = hdId },
                commandType: CommandType.StoredProcedure);

            return hoaDon;
        }

        public async Task<IEnumerable<HoaDon>> GetAllHoaDonsAsync()
        {
            using var connection = _context.CreateConnection();

            var hoaDons = await connection.QueryAsync<HoaDon>(
                "sp_HoaDon_GetAll",
                commandType: CommandType.StoredProcedure);

            return hoaDons;
        }

        public async Task<(HoaDon? HoaDon, OrderWithDetails? OrderDetails)> GetHoaDonWithOrderDetailsAsync(int orderId)
        {
            using var connection = _context.CreateConnection();

            using var multi = await connection.QueryMultipleAsync(
                "sp_HoaDon_GetWithOrderDetails",
                new { OrderId = orderId },
                commandType: CommandType.StoredProcedure);

            var hoaDon = await multi.ReadFirstOrDefaultAsync<HoaDon>();
            var orderDetails = await multi.ReadFirstOrDefaultAsync<OrderWithDetails>();

            if (orderDetails != null)
            {
                var orderItems = await multi.ReadAsync<OrderItem>();
                orderDetails.OrderItems = orderItems.ToList();
            }

            return (hoaDon, orderDetails);
        }
    }
}
