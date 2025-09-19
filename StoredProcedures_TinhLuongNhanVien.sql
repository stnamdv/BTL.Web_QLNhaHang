USE [NhaHang]
GO
/****** Object:  StoredProcedure [dbo].[sp_NhanVien_TinhLuongTheoThang]    Script Date: 1/18/2025 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Stored procedure tính lương nhân viên theo tháng
-- Tiền thưởng: cứ 10 khách vào ăn thì mỗi nhân viên được cộng thêm 2% tiền lương cơ bản
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_NhanVien_TinhLuongTheoThang]
    @thang INT,
    @nam INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra tham số đầu vào
    IF @thang < 1 OR @thang > 12
    BEGIN
        RAISERROR(N'Tháng phải từ 1 đến 12', 16, 1);
        RETURN;
    END
    
    IF @nam < 2000 OR @nam > 2100
    BEGIN
        RAISERROR(N'Năm không hợp lệ', 16, 1);
        RETURN;
    END
    
    -- Tính tổng số khách vào ăn trong tháng (chỉ tính các order đã hoàn thành)
    DECLARE @tong_so_khach INT;
    
    SELECT @tong_so_khach = ISNULL(SUM(so_khach), 0)
    FROM [dbo].[Order] o
    INNER JOIN [dbo].[HoaDon] h ON o.order_id = h.order_id
    WHERE YEAR(o.thoi_diem_dat) = @nam 
      AND MONTH(o.thoi_diem_dat) = @thang
      AND o.trang_thai = 'HOAN_THANH'
      AND o.la_mang_ve = 0; -- Chỉ tính khách ăn tại chỗ, không tính mang về
    
    -- Tính số lần thưởng (cứ 10 khách = 1 lần thưởng 2%)
    DECLARE @so_lan_thuong INT = @tong_so_khach / 10;
    DECLARE @phan_tram_thuong DECIMAL(5,2) = @so_lan_thuong * 2.0;
    
    -- Trả về kết quả lương của tất cả nhân viên
    SELECT 
        nv.nv_id,
        nv.ma_nv,
        nv.ho_ten,
        lnv.loai_nv,
        lnv.luong_co_ban,
        @tong_so_khach AS tong_so_khach_trong_thang,
        @so_lan_thuong AS so_lan_thuong,
        @phan_tram_thuong AS phan_tram_thuong,
        (lnv.luong_co_ban * @phan_tram_thuong / 100) AS tien_thuong,
        (lnv.luong_co_ban + (lnv.luong_co_ban * @phan_tram_thuong / 100)) AS tong_luong,
        nv.trang_thai,
        nv.ngay_vao_lam
    FROM [dbo].[NhanVien] nv
    INNER JOIN [dbo].[LoaiNhanVien] lnv ON nv.loai_nv_id = lnv.loai_nv_id
    WHERE nv.trang_thai = 'ACTIVE'
    ORDER BY nv.ho_ten;
    
    -- Trả về thông tin tổng quan
    SELECT 
        @thang AS thang,
        @nam AS nam,
        @tong_so_khach AS tong_so_khach_trong_thang,
        @so_lan_thuong AS so_lan_thuong,
        @phan_tram_thuong AS phan_tram_thuong_toi_da,
        COUNT(*) AS so_nhan_vien_duoc_tinh_luong
    FROM [dbo].[NhanVien] nv
    WHERE nv.trang_thai = 'ACTIVE';
END
GO

/****** Object:  StoredProcedure [dbo].[sp_NhanVien_TinhLuongThangHienTai]    Script Date: 1/18/2025 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Stored procedure tính lương nhân viên tháng hiện tại
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_NhanVien_TinhLuongThangHienTai]
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @thang_hien_tai INT = MONTH(GETDATE());
    DECLARE @nam_hien_tai INT = YEAR(GETDATE());
    
    EXEC [dbo].[sp_NhanVien_TinhLuongTheoThang] @thang_hien_tai, @nam_hien_tai;
END
GO