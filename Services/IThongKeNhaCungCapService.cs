using BTL.Web.Models;

namespace BTL.Web.Services
{
    public interface IThongKeNhaCungCapService
    {
        /// <summary>
        /// Thống kê thông tin các đơn vị cung cấp nguyên liệu sắp xếp theo số lượng nguyên liệu được sử dụng trong tháng
        /// </summary>
        /// <param name="request">Thông tin tháng, năm và nhà cung cấp cần thống kê</param>
        /// <returns>Danh sách thống kê nhà cung cấp</returns>
        Task<List<ThongKeNhaCungCap>> GetThongKeNhaCungCapTheoSoLuongNguyenLieuAsync(ThongKeNhaCungCapRequest request);

        /// <summary>
        /// Lấy chi tiết nguyên liệu của nhà cung cấp
        /// </summary>
        /// <param name="request">Thông tin tháng, năm và nhà cung cấp</param>
        /// <returns>Danh sách chi tiết nguyên liệu</returns>
        Task<List<ChiTietNguyenLieuNhaCungCap>> GetChiTietNguyenLieuNhaCungCapAsync(ThongKeNhaCungCapRequest request);

        /// <summary>
        /// So sánh thống kê nhà cung cấp giữa 2 tháng
        /// </summary>
        /// <param name="request">Thông tin 2 tháng cần so sánh</param>
        /// <returns>Danh sách so sánh thống kê</returns>
        Task<List<SoSanhThongKeNhaCungCap>> SoSanhThongKeNhaCungCapThangAsync(SoSanhThangRequest request);

        /// <summary>
        /// Lấy top nguyên liệu được sử dụng nhiều nhất của từng nhà cung cấp
        /// </summary>
        /// <param name="request">Thông tin tháng, năm, top N và nhà cung cấp</param>
        /// <returns>Danh sách top nguyên liệu</returns>
        Task<List<TopNguyenLieuNhaCungCap>> GetTopNguyenLieuNhaCungCapAsync(TopNguyenLieuRequest request);

        /// <summary>
        /// Lấy thống kê tổng hợp về nhà cung cấp
        /// </summary>
        /// <param name="thang">Tháng cần thống kê (null = tháng hiện tại)</param>
        /// <param name="nam">Năm cần thống kê (null = năm hiện tại)</param>
        /// <returns>Thống kê tổng hợp</returns>
        Task<ThongKeNhaCungCapTongHop> GetThongKeTongHopAsync(int? thang = null, int? nam = null);

        /// <summary>
        /// Lấy danh sách nhà cung cấp để hiển thị trong dropdown
        /// </summary>
        /// <returns>Danh sách nhà cung cấp</returns>
        Task<List<NhaCungCap>> GetDanhSachNhaCungCapAsync();

        /// <summary>
        /// Lấy thống kê nhà cung cấp nguyên liệu sử dụng stored procedure mới
        /// </summary>
        /// <param name="request">Thông tin tháng, năm và nhà cung cấp cần thống kê</param>
        /// <returns>Danh sách thống kê nhà cung cấp nguyên liệu</returns>
        Task<List<ThongKeNhaCungCapNguyenLieu>> GetThongKeNhaCungCapNguyenLieuAsync(ThongKeNhaCungCapRequest request);

        /// <summary>
        /// Lấy chi tiết nhà cung cấp nguyên liệu sử dụng stored procedure mới
        /// </summary>
        /// <param name="request">Thông tin tháng, năm và nhà cung cấp cần thống kê</param>
        /// <returns>Danh sách chi tiết nhà cung cấp nguyên liệu</returns>
        Task<List<ChiTietNhaCungCapNguyenLieu>> GetChiTietNhaCungCapNguyenLieuAsync(ThongKeNhaCungCapRequest request);

        /// <summary>
        /// Lấy tổng chi nhà cung cấp sử dụng stored procedure mới
        /// </summary>
        /// <param name="request">Thông tin tháng, năm và nhà cung cấp cần thống kê</param>
        /// <returns>Tổng chi nhà cung cấp</returns>
        Task<TongChiNhaCungCap?> GetTongChiNhaCungCapAsync(ThongKeNhaCungCapRequest request);
    }
}
