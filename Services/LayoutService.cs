using BTL.Web.Models;
using BTL.Web.Repositories;

namespace BTL.Web.Services
{
    public class LayoutService : ILayoutService
    {
        private readonly ILayoutRepository _layoutRepository;

        public LayoutService(ILayoutRepository layoutRepository)
        {
            _layoutRepository = layoutRepository;
        }

        public async Task<LayoutResponse?> GetLayoutAsync(string? layoutName = null)
        {
            return await _layoutRepository.GetLayoutAsync(layoutName);
        }

        public async Task<List<LayoutResponse>> GetLayoutListAsync()
        {
            return await _layoutRepository.GetLayoutListAsync();
        }

        public async Task<int> SaveLayoutAsync(LayoutSaveRequest request)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(request.layout_name))
            {
                throw new ArgumentException("Tên layout không được để trống");
            }

            if (request.grid_size < 5 || request.grid_size > 20)
            {
                throw new ArgumentException("Kích thước grid phải từ 5 đến 20");
            }

            return await _layoutRepository.SaveLayoutAsync(request);
        }

        public async Task<bool> DeleteLayoutAsync(int layoutId)
        {
            if (layoutId <= 0)
            {
                throw new ArgumentException("ID layout không hợp lệ");
            }

            return await _layoutRepository.DeleteLayoutAsync(layoutId);
        }

        public async Task<bool> LayoutExistsAsync(string layoutName)
        {
            if (string.IsNullOrWhiteSpace(layoutName))
            {
                return false;
            }

            return await _layoutRepository.LayoutExistsAsync(layoutName);
        }
    }
}
