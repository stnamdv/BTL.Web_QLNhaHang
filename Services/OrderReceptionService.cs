using BTL.Web.Models;
using BTL.Web.Repositories;

namespace BTL.Web.Services
{
    public class OrderReceptionService : IOrderReceptionService
    {
        private readonly IOrderReceptionRepository _repository;

        public OrderReceptionService(IOrderReceptionRepository repository)
        {
            _repository = repository;
        }

        public async Task<(bool Success, string Message)> ConfirmOrderAsync(int orderId, int employeeId)
        {
            return await _repository.ConfirmOrderAsync(orderId, employeeId);
        }

        public async Task<IEnumerable<BanAn>> GetAvailableTablesAsync(int? capacity = null)
        {
            return await _repository.GetAvailableTablesAsync(capacity);
        }

        public async Task<(bool IsValid, string Message)> ValidateTableAsync(int tableId, int customerCount)
        {
            return await _repository.ValidateTableAsync(tableId, customerCount);
        }

        public async Task<Order?> GetOrderStatusAsync(int orderId)
        {
            return await _repository.GetOrderStatusAsync(orderId);
        }

        public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
        {
            return await _repository.GetPendingOrdersAsync();
        }

        public async Task<IEnumerable<OrderWithDetails>> GetAllPendingOrdersAsync()
        {
            return await _repository.GetAllPendingOrdersAsync();
        }
    }
}
