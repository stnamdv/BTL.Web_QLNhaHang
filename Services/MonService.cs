using BTL.Web.Models;
using BTL.Web.Repositories;

namespace BTL.Web.Services
{
    public class MonService : IMonService
    {
        private readonly IMonRepository _monRepository;
        private readonly INguyenLieuRepository _nguyenLieuRepository;

        public MonService(IMonRepository monRepository, INguyenLieuRepository nguyenLieuRepository)
        {
            _monRepository = monRepository;
            _nguyenLieuRepository = nguyenLieuRepository;
        }

        public async Task<IEnumerable<Mon>> GetAllAsync()
        {
            return await _monRepository.GetAllAsync();
        }

        public async Task<PagedResult<Mon>> GetAllPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, string? loaiMon = null)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentException("Số trang phải lớn hơn 0", nameof(pageNumber));
            }

            if (pageSize < 1 || pageSize > 100)
            {
                throw new ArgumentException("Kích thước trang phải từ 1 đến 100", nameof(pageSize));
            }

            return await _monRepository.GetAllPagedAsync(pageNumber, pageSize, searchTerm, loaiMon);
        }

        public async Task<Mon?> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _monRepository.GetByIdAsync(id);
        }

        public async Task<MonWithCongThuc?> GetWithCongThucAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _monRepository.GetWithCongThucAsync(id);
        }

        public async Task<Mon> CreateAsync(Mon mon, IEnumerable<CongThuc>? congThucs = null)
        {
            if (mon == null)
            {
                throw new ArgumentNullException(nameof(mon));
            }

            // Validation business logic
            await ValidateMonAsync(mon, null);

            // Validate công thức nếu có
            if (congThucs != null && congThucs.Any())
            {
                await ValidateCongThucsAsync(congThucs);
            }

            return await _monRepository.CreateAsync(mon, congThucs);
        }

        public async Task<Mon> UpdateAsync(Mon mon, IEnumerable<CongThuc>? congThucs = null)
        {
            if (mon == null)
            {
                throw new ArgumentNullException(nameof(mon));
            }

            if (mon.mon_id <= 0)
            {
                throw new ArgumentException("ID món phải lớn hơn 0", nameof(mon.mon_id));
            }

            // Kiểm tra món có tồn tại không
            if (!await _monRepository.ExistsByIdAsync(mon.mon_id))
            {
                throw new InvalidOperationException("Món ăn không tồn tại.");
            }

            // Validation business logic
            await ValidateMonAsync(mon, mon.mon_id);

            // Validate công thức nếu có
            if (congThucs != null && congThucs.Any())
            {
                await ValidateCongThucsAsync(congThucs);
            }

            return await _monRepository.UpdateAsync(mon, congThucs);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            var (canDelete, message) = await _monRepository.CanDeleteAsync(id);
            if (!canDelete)
            {
                throw new InvalidOperationException(message);
            }

            return await _monRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            if (id <= 0)
            {
                return false;
            }

            return await _monRepository.ExistsByIdAsync(id);
        }

        public async Task<bool> ExistsByMaMonAsync(string maMon, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(maMon))
            {
                return false;
            }

            return await _monRepository.ExistsByMaMonAsync(maMon, excludeId);
        }

        public async Task<(bool can_delete, string message)> CanDeleteAsync(int id)
        {
            if (id <= 0)
            {
                return (false, "ID phải lớn hơn 0");
            }

            return await _monRepository.CanDeleteAsync(id);
        }

        public async Task<IEnumerable<Mon>> GetByLoaiMonAsync(string loaiMon)
        {
            if (string.IsNullOrWhiteSpace(loaiMon))
            {
                throw new ArgumentException("Loại món không được để trống", nameof(loaiMon));
            }

            if (!IsValidLoaiMon(loaiMon))
            {
                throw new ArgumentException("Loại món không hợp lệ", nameof(loaiMon));
            }

            return await _monRepository.GetByLoaiMonAsync(loaiMon);
        }

        public async Task<PagedResult<Mon>> GetByLoaiMonPagedAsync(string loaiMon, int pageNumber, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(loaiMon))
            {
                throw new ArgumentException("Loại món không được để trống", nameof(loaiMon));
            }

            if (!IsValidLoaiMon(loaiMon))
            {
                throw new ArgumentException("Loại món không hợp lệ", nameof(loaiMon));
            }

            return await _monRepository.GetByLoaiMonPagedAsync(loaiMon, pageNumber, pageSize);
        }

        public async Task<IEnumerable<Mon>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await _monRepository.GetAllAsync();
            }

            return await _monRepository.SearchAsync(searchTerm);
        }

        public async Task<PagedResult<Mon>> SearchPagedAsync(string searchTerm, int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentException("Số trang phải lớn hơn 0", nameof(pageNumber));
            }

            if (pageSize < 1 || pageSize > 100)
            {
                throw new ArgumentException("Kích thước trang phải từ 1 đến 100", nameof(pageSize));
            }

            return await _monRepository.SearchPagedAsync(searchTerm, pageNumber, pageSize);
        }

        public async Task<IEnumerable<dynamic>> GetStatsByLoaiAsync()
        {
            return await _monRepository.GetStatsByLoaiAsync();
        }

        public async Task<IEnumerable<NguyenLieu>> GetAllNguyenLieuAsync()
        {
            return await _nguyenLieuRepository.GetAllAsync();
        }

        public async Task<string> GenerateMaMonAsync(string loaiMon)
        {
            if (!IsValidLoaiMon(loaiMon))
            {
                throw new ArgumentException("Loại món không hợp lệ", nameof(loaiMon));
            }

            var prefix = loaiMon switch
            {
                "KHAI_VI" => "KV",
                "MON_CHINH" => "MC",
                "TRANG_MIENG" => "TM",
                _ => throw new ArgumentException("Loại món không hợp lệ", nameof(loaiMon))
            };

            // Lấy số thứ tự tiếp theo
            var existingMons = await _monRepository.GetByLoaiMonAsync(loaiMon);
            var maxNumber = 0;

            foreach (var mon in existingMons)
            {
                if (mon.ma_mon.StartsWith($"{prefix}-"))
                {
                    var numberPart = mon.ma_mon.Substring(3); // Bỏ qua "KV-", "MC-", "TM-"
                    if (int.TryParse(numberPart, out var number))
                    {
                        maxNumber = Math.Max(maxNumber, number);
                    }
                }
            }

            return $"{prefix}-{(maxNumber + 1):D3}";
        }

        public async Task<bool> ValidateMaMonAsync(string maMon, string loaiMon, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(maMon) || string.IsNullOrWhiteSpace(loaiMon))
            {
                return false;
            }

            if (!IsValidLoaiMon(loaiMon))
            {
                return false;
            }

            // Kiểm tra format mã món
            var expectedPrefix = loaiMon switch
            {
                "KHAI_VI" => "KV-",
                "MON_CHINH" => "MC-",
                "TRANG_MIENG" => "TM-",
                _ => null
            };

            if (expectedPrefix == null || !maMon.StartsWith(expectedPrefix))
            {
                return false;
            }

            // Kiểm tra trùng lặp
            return !await _monRepository.ExistsByMaMonAsync(maMon, excludeId);
        }

        private async Task ValidateMonAsync(Mon mon, int? excludeId)
        {
            // Validate tên món
            if (string.IsNullOrWhiteSpace(mon.ten_mon))
            {
                throw new ArgumentException("Tên món không được để trống");
            }

            if (mon.ten_mon.Length > 120)
            {
                throw new ArgumentException("Tên món không được vượt quá 120 ký tự");
            }

            // Validate mã món
            if (string.IsNullOrWhiteSpace(mon.ma_mon))
            {
                throw new ArgumentException("Mã món không được để trống");
            }

            if (!await ValidateMaMonAsync(mon.ma_mon, mon.loai_mon.ToString(), excludeId))
            {
                throw new ArgumentException("Mã món không hợp lệ hoặc đã tồn tại");
            }

            // Validate giá
            if (mon.gia < 0)
            {
                throw new ArgumentException("Giá món không được âm");
            }

            // Validate loại món
            if (!IsValidLoaiMon(mon.loai_mon.ToString()))
            {
                throw new ArgumentException("Loại món không hợp lệ");
            }
        }

        private async Task ValidateCongThucsAsync(IEnumerable<CongThuc> congThucs)
        {
            var nlIds = congThucs.Select(ct => ct.nl_id).Distinct().ToList();

            foreach (var nlId in nlIds)
            {
                if (!await _nguyenLieuRepository.ExistsByIdAsync(nlId))
                {
                    throw new ArgumentException($"Nguyên liệu với ID {nlId} không tồn tại");
                }
            }

            foreach (var congThuc in congThucs)
            {
                if (congThuc.dinh_luong <= 0)
                {
                    throw new ArgumentException("Định lượng nguyên liệu phải lớn hơn 0");
                }
            }
        }

        private static bool IsValidLoaiMon(string loaiMon)
        {
            return loaiMon is "KHAI_VI" or "MON_CHINH" or "TRANG_MIENG";
        }
    }
}