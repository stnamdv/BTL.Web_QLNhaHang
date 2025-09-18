using BTL.Web.Data;
using BTL.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BTL.Web.Repositories
{
    public class NhanVienRepository : INhanVienRepository
    {
        private readonly DatabaseContext _context;

        public NhanVienRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NhanVienWithLoaiNhanVien>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<NhanVienWithLoaiNhanVien>("EXEC sp_NhanVien_GetAll");
        }

        public async Task<PagedResult<NhanVienWithLoaiNhanVien>> GetAllPagedAsync(int pageNumber, int pageSize)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@page_number", pageNumber);
            parameters.Add("@page_size", pageSize);
            parameters.Add("@total_count", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

            var items = await connection.QueryAsync<NhanVienWithLoaiNhanVien>(
                "sp_NhanVien_GetAllPaged",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            var totalItems = parameters.Get<int>("@total_count");

            return new PagedResult<NhanVienWithLoaiNhanVien>(items, totalItems, pageNumber, pageSize);
        }

        public async Task<NhanVien?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<NhanVien>(
                "EXEC sp_NhanVien_GetById @nv_id",
                new { nv_id = id });
        }

        public async Task<IEnumerable<NhanVienWithLoaiNhanVien>> GetByLoaiNvAsync(int loaiNvId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<NhanVienWithLoaiNhanVien>(
                "EXEC sp_NhanVien_GetByLoaiNv @loai_nv_id",
                new { loai_nv_id = loaiNvId });
        }

        public async Task<PagedResult<NhanVienWithLoaiNhanVien>> GetByLoaiNvPagedAsync(int loaiNvId, int pageNumber, int pageSize)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@loai_nv_id", loaiNvId);
            parameters.Add("@page_number", pageNumber);
            parameters.Add("@page_size", pageSize);
            parameters.Add("@total_count", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

            var items = await connection.QueryAsync<NhanVienWithLoaiNhanVien>(
                "sp_NhanVien_GetByLoaiNvPaged",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            var totalItems = parameters.Get<int>("@total_count");

            return new PagedResult<NhanVienWithLoaiNhanVien>(items, totalItems, pageNumber, pageSize);
        }

        public async Task<NhanVien> CreateAsync(NhanVien nhanVien)
        {
            using var connection = _context.CreateConnection();
            var sql = "EXEC sp_NhanVien_Create @ho_ten, @loai_nv_id, @ngay_vao_lam, @trang_thai";

            var id = await connection.QuerySingleAsync<int>(sql, new
            {
                ho_ten = nhanVien.ho_ten,
                loai_nv_id = nhanVien.loai_nv_id,
                ngay_vao_lam = nhanVien.ngay_vao_lam,
                trang_thai = nhanVien.trang_thai
            });
            nhanVien.nv_id = id;
            return nhanVien;
        }

        public async Task<NhanVien> UpdateAsync(NhanVien nhanVien)
        {
            using var connection = _context.CreateConnection();
            var sql = "EXEC sp_NhanVien_Update @nv_id, @ho_ten, @loai_nv_id, @ngay_vao_lam, @trang_thai";

            var result = await connection.QuerySingleAsync<dynamic>(sql, new
            {
                nv_id = nhanVien.nv_id,
                ho_ten = nhanVien.ho_ten,
                loai_nv_id = nhanVien.loai_nv_id,
                ngay_vao_lam = nhanVien.ngay_vao_lam,
                trang_thai = nhanVien.trang_thai
            });

            var rowsAffected = (int)result.rows_affected;
            Console.WriteLine($"Update rows affected: {rowsAffected}");

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException("Không thể cập nhật nhân viên. Có thể nhân viên không tồn tại.");
            }

            return nhanVien;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QuerySingleAsync<dynamic>(
                "EXEC sp_NhanVien_Delete @nv_id",
                new { nv_id = id });

            var rowsAffected = (int)result.rows_affected;
            Console.WriteLine($"Rows affected: {rowsAffected}");
            return rowsAffected > 0;
        }

        public async Task<NhanVien?> GetDetailsWithLoaiNhanVienAsync(int id)
        {
            using var connection = _context.CreateConnection();

            // Load NhanVien with LoaiNhanVien information using a single query
            var sql = @"EXEC sp_NhanVien_GetById @nv_id";

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { nv_id = id });

            if (result == null) return null;

            var nhanVien = new NhanVien
            {
                nv_id = result.nv_id,
                ma_nv = result.ma_nv,
                ho_ten = result.ho_ten,
                loai_nv_id = result.loai_nv_id,
                ngay_vao_lam = result.ngay_vao_lam,
                trang_thai = result.trang_thai,
                LoaiNhanVien = new LoaiNhanVien
                {
                    loai_nv_id = result.loai_nv_id,
                    loai_nv = result.loai_nv,
                    luong_co_ban = result.luong_co_ban
                }
            };

            return nhanVien;
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var count = await connection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM dbo.NhanVien WHERE nv_id = @nv_id",
                new { nv_id = id });
            return count > 0;
        }

        public async Task<(bool can_delete, string message)> CanDeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "EXEC sp_NhanVien_CanDelete @nv_id",
                new { nv_id = id });

            if (result == null)
            {
                return (false, "Không thể kiểm tra quyền xóa nhân viên");
            }

            return (result.can_delete, result.message);
        }

        public async Task<IEnumerable<NhanVienWithLoaiNhanVien>> GetActiveAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<NhanVienWithLoaiNhanVien>("EXEC sp_NhanVien_GetActive");
        }

        public async Task<PagedResult<NhanVienWithLoaiNhanVien>> GetActivePagedAsync(int pageNumber, int pageSize)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@page_number", pageNumber);
            parameters.Add("@page_size", pageSize);
            parameters.Add("@total_count", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

            var items = await connection.QueryAsync<NhanVienWithLoaiNhanVien>(
                "sp_NhanVien_GetActivePaged",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            var totalItems = parameters.Get<int>("@total_count");

            return new PagedResult<NhanVienWithLoaiNhanVien>(items, totalItems, pageNumber, pageSize);
        }

        public async Task<IEnumerable<NhanVienWithLoaiNhanVien>> SearchByNameAsync(string searchTerm)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<NhanVienWithLoaiNhanVien>(
                "EXEC sp_NhanVien_SearchByName @search_term",
                new { search_term = searchTerm });
        }

        public async Task<PagedResult<NhanVienWithLoaiNhanVien>> SearchByNamePagedAsync(string searchTerm, int pageNumber, int pageSize)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@search_term", searchTerm);
            parameters.Add("@page_number", pageNumber);
            parameters.Add("@page_size", pageSize);
            parameters.Add("@total_count", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

            var items = await connection.QueryAsync<NhanVienWithLoaiNhanVien>(
                "sp_NhanVien_SearchByNamePaged",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            var totalItems = parameters.Get<int>("@total_count");

            return new PagedResult<NhanVienWithLoaiNhanVien>(items, totalItems, pageNumber, pageSize);
        }

        public async Task<IEnumerable<dynamic>> GetStatsByLoaiAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync("EXEC sp_NhanVien_GetStatsByLoai");
        }

        public async Task<NhanVienBulkResult> BulkUpsertAsync(IEnumerable<NhanVienExcelData> nhanVienData)
        {
            using var connection = _context.CreateConnection();

            // Tạo DataTable từ IEnumerable
            var dataTable = new System.Data.DataTable();
            dataTable.Columns.Add("ma_nv", typeof(string));
            dataTable.Columns.Add("ho_ten", typeof(string));
            dataTable.Columns.Add("loai_nv", typeof(string));
            dataTable.Columns.Add("ngay_vao_lam", typeof(DateTime));
            dataTable.Columns.Add("trang_thai", typeof(string));

            foreach (var item in nhanVienData)
            {
                var ngayVaoLam = item.GetNgayVaoLamAsDateTime();
                dataTable.Rows.Add(
                    string.IsNullOrWhiteSpace(item.MaNhanVien) ? DBNull.Value : item.MaNhanVien,
                    item.HoTen,
                    item.GetLoaiNvFromBoPhan(),
                    ngayVaoLam.HasValue ? (object)ngayVaoLam.Value : DBNull.Value,
                    item.GetTrangThaiFromDisplay()
                );
            }

            // Sử dụng SqlCommand trực tiếp cho table-valued parameter
            using var command = new SqlCommand("EXEC sp_NhanVien_BulkUpsert @NhanVienData", (SqlConnection)connection);
            var parameter = new SqlParameter("@NhanVienData", SqlDbType.Structured)
            {
                TypeName = "dbo.NhanVienBulkType",
                Value = dataTable
            };
            command.Parameters.Add(parameter);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new NhanVienBulkResult
                {
                    InsertedCount = reader.GetInt32("inserted_count"),
                    UpdatedCount = reader.GetInt32("updated_count"),
                    ErrorCount = reader.GetInt32("error_count"),
                    ErrorMessages = reader.GetString("error_messages")
                };
            }

            return new NhanVienBulkResult
            {
                InsertedCount = 0,
                UpdatedCount = 0,
                ErrorCount = 1,
                ErrorMessages = "Không thể thực hiện bulk upsert"
            };
        }
    }
}
