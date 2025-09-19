using BTL.Web.Models;

namespace BTL.Web.Services
{
    public interface IOrderService
    {
        Task<(int OrderId, bool Success, string Message)> CreateOrderAsync(OrderCreateViewModel model);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);

        // New methods for pagination and cancellation
        Task<PagedResult<OrderWithDetails>> GetOrdersWithPaginationAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchKeyword = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? filterType = null,
            string? status = null);

        Task<OrderWithDetails?> GetOrderDetailsAsync(int orderId);
        Task<(bool Success, string Message)> CancelOrderAsync(int orderId, int employeeId, string? reason = null);
        Task<object> GetOrderStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, string? filterType = null);
    }
}
