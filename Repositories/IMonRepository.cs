using BTL.Web.Models;

namespace BTL.Web.Repositories
{
    public interface IMonRepository
    {
        Task<IEnumerable<Mon>> GetAllAsync();
        Task<Mon?> GetByIdAsync(int id);
        Task<Mon?> GetByMaMonAsync(string maMon);
        Task<IEnumerable<Mon>> GetByLoaiMonAsync(LoaiMon loaiMon);
        Task<Mon> CreateAsync(Mon mon);
        Task<Mon> UpdateAsync(Mon mon);
        Task<bool> DeleteAsync(int id);
    }
}
