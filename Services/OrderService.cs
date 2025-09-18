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
    }
}
