using BTL.Web.Models;

namespace BTL.Web.Repositories
{
    public interface IOrderRepository
    {
        Task<(int OrderId, bool Success, string Message)> CreateOrderAsync(OrderCreateViewModel model);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
    }
}
