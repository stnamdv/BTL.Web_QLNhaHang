using BTL.Web.Models;

namespace BTL.Web.Services
{
    public interface IOrderReceptionService
    {
        Task<(bool Success, string Message)> ConfirmOrderAsync(int orderId, int employeeId);
        Task<IEnumerable<BanAn>> GetAvailableTablesAsync(int? capacity = null);
        Task<(bool IsValid, string Message)> ValidateTableAsync(int tableId, int customerCount);
        Task<Order?> GetOrderStatusAsync(int orderId);
        Task<IEnumerable<Order>> GetPendingOrdersAsync();
        Task<IEnumerable<OrderWithDetails>> GetAllPendingOrdersAsync();
    }
}
