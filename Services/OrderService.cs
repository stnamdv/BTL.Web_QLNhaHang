using BTL.Web.Models;
using BTL.Web.Repositories;

namespace BTL.Web.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;

        public OrderService(IOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<(int OrderId, bool Success, string Message)> CreateOrderAsync(OrderCreateViewModel model)
        {
            return await _repository.CreateOrderAsync(model);
        }


        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _repository.GetOrderByIdAsync(orderId);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _repository.GetAllOrdersAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            return await _repository.UpdateOrderStatusAsync(orderId, status);
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
            return await _repository.GetOrdersWithPaginationAsync(pageNumber, pageSize, searchKeyword, fromDate, toDate, filterType, status);
        }

        public async Task<OrderWithDetails?> GetOrderDetailsAsync(int orderId)
        {
            return await _repository.GetOrderDetailsAsync(orderId);
        }

        public async Task<(bool Success, string Message)> CancelOrderAsync(int orderId, int employeeId, string? reason = null)
        {
            return await _repository.CancelOrderAsync(orderId, employeeId, reason);
        }

        public async Task<object> GetOrderStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, string? filterType = null)
        {
            return await _repository.GetOrderStatisticsAsync(fromDate, toDate, filterType);
        }
    }
}
