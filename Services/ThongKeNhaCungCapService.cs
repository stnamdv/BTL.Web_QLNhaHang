using BTL.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BTL.Web.Services
{
    public class ThongKeNhaCungCapService : IThongKeNhaCungCapService
    {
        private readonly string _connectionString;

        public ThongKeNhaCungCapService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
        }


        public async Task<List<ThongKeNhaCungCap>> GetThongKeNhaCungCapTheoSoLuongNguyenLieuAsync(ThongKeNhaCungCapRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@Thang", request.Thang);
                parameters.Add("@Nam", request.Nam);
                parameters.Add("@NccId", request.NccId);

                using var multi = await connection.QueryMultipleAsync(
                    "sp_ThongKeNhaCungCap_TheoSoLuongNguyenLieu",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                // Đọc kết quả đầu tiên (thống kê chính)
                var thongKeChinh = await multi.ReadAsync<ThongKeNhaCungCap>();
                var result = thongKeChinh.ToList();

                // Đọc kết quả thứ 2 (lượng theo đơn vị)
                var luongTheoDonVi = await multi.ReadAsync<LuuongTheoDonVi>();
                var luongTheoDonViList = luongTheoDonVi.ToList();

                // Gộp dữ liệu lượng theo đơn vị vào thống kê chính
                foreach (var item in result)
                {
                    item.LuongTheoDonVi = luongTheoDonViList
                        .Where(x => x.ncc_id == item.ncc_id)
                        .ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy thống kê nhà cung cấp: {ex.Message}", ex);
            }
        }

        public async Task<List<ChiTietNguyenLieuNhaCungCap>> GetChiTietNguyenLieuNhaCungCapAsync(ThongKeNhaCungCapRequest request)
        {
            try
            {
                if (request.NccId == null)
                {
                    throw new ArgumentException("NccId là bắt buộc để lấy chi tiết nguyên liệu");
                }

                using var connection = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@Thang", request.Thang);
                parameters.Add("@Nam", request.Nam);
                parameters.Add("@NccId", request.NccId);

                using var multi = await connection.QueryMultipleAsync(
                    "sp_ThongKeNhaCungCap_TheoSoLuongNguyenLieu",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                // Bỏ qua kết quả đầu tiên (thống kê tổng quan)
                await multi.ReadAsync<ThongKeNhaCungCap>();

                // Đọc kết quả thứ 2 (chi tiết nguyên liệu)
                var result = await multi.ReadAsync<ChiTietNguyenLieuNhaCungCap>();

                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy chi tiết nguyên liệu nhà cung cấp: {ex.Message}", ex);
            }
        }

        public async Task<List<SoSanhThongKeNhaCungCap>> SoSanhThongKeNhaCungCapThangAsync(SoSanhThangRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@Thang1", request.Thang1);
                parameters.Add("@Nam1", request.Nam1);
                parameters.Add("@Thang2", request.Thang2);
                parameters.Add("@Nam2", request.Nam2);
                parameters.Add("@NccId", request.NccId);

                var result = await connection.QueryAsync<SoSanhThongKeNhaCungCap>(
                    "sp_ThongKeNhaCungCap_SoSanhThang",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi so sánh thống kê nhà cung cấp: {ex.Message}", ex);
            }
        }

        public async Task<List<TopNguyenLieuNhaCungCap>> GetTopNguyenLieuNhaCungCapAsync(TopNguyenLieuRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@Thang", request.Thang);
                parameters.Add("@Nam", request.Nam);
                parameters.Add("@TopN", request.TopN ?? 5);
                parameters.Add("@NccId", request.NccId);

                var result = await connection.QueryAsync<TopNguyenLieuNhaCungCap>(
                    "sp_ThongKeNhaCungCap_TopNguyenLieu",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy top nguyên liệu nhà cung cấp: {ex.Message}", ex);
            }
        }

        public async Task<ThongKeNhaCungCapTongHop> GetThongKeTongHopAsync(int? thang = null, int? nam = null)
        {
            try
            {
                var result = new ThongKeNhaCungCapTongHop();

                using var connection = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@Thang", thang);
                parameters.Add("@Nam", nam);

                using var multi = await connection.QueryMultipleAsync(
                    "sp_ThongKeNhaCungCap_TongHop",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                // Đọc kết quả đầu tiên (thống kê tổng hợp)
                var tongHop = await multi.ReadFirstOrDefaultAsync<ThongKeTongHopNhaCungCap>();
                if (tongHop != null)
                {
                    result.TongHop = tongHop;
                }

                // Đọc kết quả thứ 2 (top nhà cung cấp)
                var topNhaCungCap = await multi.ReadAsync<TopNhaCungCap>();
                result.TopNhaCungCap = topNhaCungCap.ToList();

                // Đọc kết quả thứ 3 (top nguyên liệu)
                var topNguyenLieu = await multi.ReadAsync<TopNguyenLieu>();
                result.TopNguyenLieu = topNguyenLieu.ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy thống kê tổng hợp: {ex.Message}", ex);
            }
        }

        public async Task<List<NhaCungCap>> GetDanhSachNhaCungCapAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var result = await connection.QueryAsync<NhaCungCap>(
                    "SELECT ncc_id, ten, dia_chi, sdt FROM dbo.NhaCungCap ORDER BY ten");

                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách nhà cung cấp: {ex.Message}", ex);
            }
        }
    }
}