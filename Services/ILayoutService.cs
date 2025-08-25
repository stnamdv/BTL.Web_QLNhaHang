using BTL.Web.Models;

namespace BTL.Web.Services
{
    public interface ILayoutService
    {
        Task<LayoutResponse?> GetLayoutAsync(string? layoutName = null);
        Task<List<LayoutResponse>> GetLayoutListAsync();
        Task<int> SaveLayoutAsync(LayoutSaveRequest request);
        Task<bool> DeleteLayoutAsync(int layoutId);
        Task<bool> LayoutExistsAsync(string layoutName);
    }
}
