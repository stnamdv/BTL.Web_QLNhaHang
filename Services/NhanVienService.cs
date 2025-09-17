using BTL.Web.Models;
using BTL.Web.Repositories;
using OfficeOpenXml;

namespace BTL.Web.Services
{
    public class NhanVienService : INhanVienService
    {
        private readonly INhanVienRepository _nhanVienRepository;
        private readonly ILoaiNhanVienRepository _loaiNhanVienRepository;

        public NhanVienService(INhanVienRepository nhanVienRepository, ILoaiNhanVienRepository loaiNhanVienRepository)
        {
            _nhanVienRepository = nhanVienRepository;
            _loaiNhanVienRepository = loaiNhanVienRepository;
        }

        public async Task<IEnumerable<NhanVienWithLoaiNhanVien>> GetAllAsync()
        {
            return await _nhanVienRepository.GetAllAsync();
        }

        public async Task<PagedResult<NhanVienWithLoaiNhanVien>> GetAllPagedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentException("Số trang phải lớn hơn 0", nameof(pageNumber));
            }

            if (pageSize < 1 || pageSize > 100)
            {
                throw new ArgumentException("Kích thước trang phải từ 1 đến 100", nameof(pageSize));
            }

            return await _nhanVienRepository.GetAllPagedAsync(pageNumber, pageSize);
        }

        public async Task<NhanVien?> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _nhanVienRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<NhanVienWithLoaiNhanVien>> GetByLoaiNvAsync(string loaiNv)
        {
            if (string.IsNullOrWhiteSpace(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không được để trống", nameof(loaiNv));
            }

            // Kiểm tra loại nhân viên có tồn tại không
            var loaiNhanVien = await _loaiNhanVienRepository.GetByTypeAsync(loaiNv);
            if (loaiNhanVien == null)
            {
                throw new InvalidOperationException("Loại nhân viên không tồn tại");
            }

            return await _nhanVienRepository.GetByLoaiNvAsync(loaiNv);
        }

        public async Task<PagedResult<NhanVienWithLoaiNhanVien>> GetByLoaiNvPagedAsync(string loaiNv, int pageNumber, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(loaiNv))
            {
                throw new ArgumentException("Loại nhân viên không được để trống", nameof(loaiNv));
            }

            if (pageNumber < 1)
            {
                throw new ArgumentException("Số trang phải lớn hơn 0", nameof(pageNumber));
            }

            if (pageSize < 1 || pageSize > 100)
            {
                throw new ArgumentException("Kích thước trang phải từ 1 đến 100", nameof(pageSize));
            }

            // Kiểm tra loại nhân viên có tồn tại không
            var loaiNhanVien = await _loaiNhanVienRepository.GetByTypeAsync(loaiNv);
            if (loaiNhanVien == null)
            {
                throw new InvalidOperationException("Loại nhân viên không tồn tại");
            }

            return await _nhanVienRepository.GetByLoaiNvPagedAsync(loaiNv, pageNumber, pageSize);
        }

        public async Task<NhanVien> CreateAsync(NhanVien nhanVien)
        {
            if (nhanVien == null)
            {
                throw new ArgumentNullException(nameof(nhanVien));
            }

            if (string.IsNullOrWhiteSpace(nhanVien.ho_ten))
            {
                throw new ArgumentException("Họ tên không được để trống");
            }

            if (string.IsNullOrWhiteSpace(nhanVien.loai_nv))
            {
                throw new ArgumentException("Loại nhân viên không được để trống");
            }

            // Kiểm tra loại nhân viên có tồn tại không
            var loaiNhanVien = await _loaiNhanVienRepository.GetByTypeAsync(nhanVien.loai_nv);
            if (loaiNhanVien == null)
            {
                throw new InvalidOperationException("Loại nhân viên không tồn tại");
            }

            // Validation cho ngày vào làm
            if (nhanVien.ngay_vao_lam.HasValue && nhanVien.ngay_vao_lam.Value > DateTime.Today)
            {
                throw new ArgumentException("Ngày vào làm không được là ngày tương lai");
            }

            // Validation cho trạng thái
            if (!TrangThaiNv.All.Contains(nhanVien.trang_thai))
            {
                throw new ArgumentException("Trạng thái không hợp lệ");
            }

            return await _nhanVienRepository.CreateAsync(nhanVien);
        }

        public async Task<NhanVien> UpdateAsync(NhanVien nhanVien)
        {
            if (nhanVien == null)
            {
                throw new ArgumentNullException(nameof(nhanVien));
            }

            if (nhanVien.nv_id <= 0)
            {
                throw new ArgumentException("ID nhân viên không hợp lệ");
            }

            if (string.IsNullOrWhiteSpace(nhanVien.ho_ten))
            {
                throw new ArgumentException("Họ tên không được để trống");
            }

            if (string.IsNullOrWhiteSpace(nhanVien.loai_nv))
            {
                throw new ArgumentException("Loại nhân viên không được để trống");
            }

            // Kiểm tra nhân viên có tồn tại không
            var existingNhanVien = await _nhanVienRepository.GetByIdAsync(nhanVien.nv_id);
            if (existingNhanVien == null)
            {
                throw new InvalidOperationException("Nhân viên không tồn tại");
            }

            // Kiểm tra loại nhân viên có tồn tại không
            var loaiNhanVien = await _loaiNhanVienRepository.GetByTypeAsync(nhanVien.loai_nv);
            if (loaiNhanVien == null)
            {
                throw new InvalidOperationException("Loại nhân viên không tồn tại");
            }

            // Validation cho ngày vào làm
            if (nhanVien.ngay_vao_lam.HasValue && nhanVien.ngay_vao_lam.Value > DateTime.Today)
            {
                throw new ArgumentException("Ngày vào làm không được là ngày tương lai");
            }

            // Validation cho trạng thái
            if (!TrangThaiNv.All.Contains(nhanVien.trang_thai))
            {
                throw new ArgumentException("Trạng thái không hợp lệ");
            }

            return await _nhanVienRepository.UpdateAsync(nhanVien);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            // Kiểm tra nhân viên có tồn tại không
            var existingNhanVien = await _nhanVienRepository.GetByIdAsync(id);
            if (existingNhanVien == null)
            {
                throw new InvalidOperationException("Nhân viên không tồn tại");
            }

            // Kiểm tra có thể xóa không
            var canDelete = await _nhanVienRepository.CanDeleteAsync(id);
            if (!canDelete.can_delete)
            {
                throw new InvalidOperationException(canDelete.message);
            }

            return await _nhanVienRepository.DeleteAsync(id);
        }

        public async Task<NhanVien?> GetDetailsWithLoaiNhanVienAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _nhanVienRepository.GetDetailsWithLoaiNhanVienAsync(id);
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _nhanVienRepository.ExistsByIdAsync(id);
        }

        public async Task<(bool can_delete, string message)> CanDeleteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
            }

            return await _nhanVienRepository.CanDeleteAsync(id);
        }

        public async Task<IEnumerable<NhanVienWithLoaiNhanVien>> GetActiveAsync()
        {
            return await _nhanVienRepository.GetActiveAsync();
        }

        public async Task<PagedResult<NhanVienWithLoaiNhanVien>> GetActivePagedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentException("Số trang phải lớn hơn 0", nameof(pageNumber));
            }

            if (pageSize < 1 || pageSize > 100)
            {
                throw new ArgumentException("Kích thước trang phải từ 1 đến 100", nameof(pageSize));
            }

            return await _nhanVienRepository.GetActivePagedAsync(pageNumber, pageSize);
        }

        public async Task<IEnumerable<NhanVienWithLoaiNhanVien>> SearchByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentException("Từ khóa tìm kiếm không được để trống", nameof(searchTerm));
            }

            return await _nhanVienRepository.SearchByNameAsync(searchTerm);
        }

        public async Task<PagedResult<NhanVienWithLoaiNhanVien>> SearchByNamePagedAsync(string searchTerm, int pageNumber, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentException("Từ khóa tìm kiếm không được để trống", nameof(searchTerm));
            }

            if (pageNumber < 1)
            {
                throw new ArgumentException("Số trang phải lớn hơn 0", nameof(pageNumber));
            }

            if (pageSize < 1 || pageSize > 100)
            {
                throw new ArgumentException("Kích thước trang phải từ 1 đến 100", nameof(pageSize));
            }

            return await _nhanVienRepository.SearchByNamePagedAsync(searchTerm, pageNumber, pageSize);
        }

        public async Task<IEnumerable<dynamic>> GetStatsByLoaiAsync()
        {
            return await _nhanVienRepository.GetStatsByLoaiAsync();
        }

        public async Task<NhanVienBulkResult> ProcessExcelUploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File không được để trống");
            }

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Chỉ chấp nhận file Excel (.xlsx)");
            }

            var nhanVienDataList = new List<NhanVienExcelData>();

            try
            {
                using var stream = file.OpenReadStream();
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];

                if (worksheet == null)
                {
                    throw new InvalidOperationException("Không tìm thấy worksheet trong file Excel");
                }

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount < 2)
                {
                    throw new InvalidOperationException("File Excel phải có ít nhất 2 dòng (header + data)");
                }

                // Đọc từ dòng 2 (bỏ qua header)
                for (int row = 2; row <= rowCount; row++)
                {
                    var stt = worksheet.Cells[row, 1].GetValue<int?>();
                    var maNv = worksheet.Cells[row, 2].GetValue<string>()?.Trim();
                    var hoTen = worksheet.Cells[row, 3].GetValue<string>()?.Trim();
                    var boPhan = worksheet.Cells[row, 4].GetValue<string>()?.Trim();

                    // Xử lý ngày vào làm - có thể là DateTime, double (serial date), hoặc string
                    string? ngayVaoLam = null;
                    var ngayVaoLamValue = worksheet.Cells[row, 5].Value;
                    if (ngayVaoLamValue != null)
                    {
                        if (ngayVaoLamValue is DateTime dateTime)
                        {
                            ngayVaoLam = dateTime.ToString("yyyy-MM-dd");
                        }
                        else if (ngayVaoLamValue is double serialDate)
                        {
                            // Excel serial date: số ngày từ 1900-01-01
                            var dateTimeFromSerial = DateTime.FromOADate(serialDate);
                            ngayVaoLam = dateTimeFromSerial.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            ngayVaoLam = ngayVaoLamValue.ToString()?.Trim();
                        }
                    }

                    var trangThai = worksheet.Cells[row, 6].GetValue<string>()?.Trim();

                    // Bỏ qua dòng trống
                    if (string.IsNullOrWhiteSpace(hoTen) && string.IsNullOrWhiteSpace(boPhan))
                        continue;

                    var nhanVienData = new NhanVienExcelData
                    {
                        STT = stt ?? 0,
                        MaNhanVien = maNv,
                        HoTen = hoTen ?? string.Empty,
                        BoPhan = boPhan ?? string.Empty,
                        NgayVaoLam = ngayVaoLam,
                        TrangThai = trangThai ?? string.Empty
                    };

                    // Debug logging (có thể xóa sau khi test xong)
                    // Console.WriteLine($"Row {row}: HoTen={nhanVienData.HoTen}, NgayVaoLam='{nhanVienData.NgayVaoLam}', Parsed={nhanVienData.GetNgayVaoLamAsDateTime()}");

                    nhanVienDataList.Add(nhanVienData);
                }

                if (nhanVienDataList.Count == 0)
                {
                    throw new InvalidOperationException("Không có dữ liệu hợp lệ trong file Excel");
                }

                // Gọi repository để bulk upsert
                return await _nhanVienRepository.BulkUpsertAsync(nhanVienDataList);
            }
            catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException))
            {
                throw new InvalidOperationException($"Lỗi khi đọc file Excel: {ex.Message}", ex);
            }
        }
    }
}
