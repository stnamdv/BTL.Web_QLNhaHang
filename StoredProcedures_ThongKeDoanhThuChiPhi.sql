USE [NhaHang]
GO
/****** Object:  StoredProcedure [dbo].[sp_ThongKe_DoanhThuChiPhiTheoNgay]    Script Date: 1/18/2025 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Stored procedure tính tổng doanh thu và chi phí của nhà hàng theo ngày
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ThongKe_DoanhThuChiPhiTheoNgay]
    @ngay DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra tham số đầu vào
    IF @ngay IS NULL
    BEGIN
        RAISERROR(N'Ngày không được để trống', 16, 1);
        RETURN;
    END
    
    -- Tính tổng doanh thu trong ngày (chỉ tính order đã hoàn thành và có hóa đơn)
    DECLARE @tong_doanh_thu DECIMAL(12,2);
    
    SELECT @tong_doanh_thu = ISNULL(SUM(o.tong_tien), 0)
    FROM [dbo].[Order] o
    INNER JOIN [dbo].[HoaDon] h ON o.order_id = h.order_id
    WHERE CAST(o.thoi_diem_dat AS DATE) = @ngay
      AND o.trang_thai = 'HOAN_THANH';
    
    -- Tính tổng chi phí nguyên liệu trong ngày
    DECLARE @tong_chi_phi_nguyen_lieu DECIMAL(12,2);
    
    SELECT @tong_chi_phi_nguyen_lieu = ISNULL(SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap), 0)
    FROM [dbo].[Order] o
    INNER JOIN [dbo].[HoaDon] h ON o.order_id = h.order_id
    INNER JOIN [dbo].[OrderItem] oi ON o.order_id = oi.order_id
    INNER JOIN [dbo].[CongThuc] ct ON oi.mon_id = ct.mon_id
    INNER JOIN [dbo].[NguyenLieu] nl ON ct.nl_id = nl.nl_id
    WHERE CAST(o.thoi_diem_dat AS DATE) = @ngay
      AND o.trang_thai = 'HOAN_THANH';
    
    -- Tính tổng chi phí lương nhân viên trong ngày (ước tính dựa trên lương cơ bản)
    DECLARE @tong_chi_phi_luong DECIMAL(12,2);
    
    SELECT @tong_chi_phi_luong = ISNULL(SUM(lnv.luong_co_ban / 30), 0) -- Chia cho 30 ngày để có lương/ngày
    FROM [dbo].[NhanVien] nv
    INNER JOIN [dbo].[LoaiNhanVien] lnv ON nv.loai_nv_id = lnv.loai_nv_id
    WHERE nv.trang_thai = 'ACTIVE';
    
    -- Tính tổng chi phí
    DECLARE @tong_chi_phi DECIMAL(12,2) = @tong_chi_phi_nguyen_lieu + @tong_chi_phi_luong;
    
    -- Tính lợi nhuận
    DECLARE @loi_nhuan DECIMAL(12,2) = @tong_doanh_thu - @tong_chi_phi;
    
    -- Tính tỷ lệ lợi nhuận
    DECLARE @ty_le_loi_nhuan DECIMAL(5,2);
    IF @tong_doanh_thu > 0
        SET @ty_le_loi_nhuan = (@loi_nhuan / @tong_doanh_thu) * 100;
    ELSE
        SET @ty_le_loi_nhuan = 0;
    
    -- Thống kê số lượng đơn hàng
    DECLARE @so_don_hang INT;
    DECLARE @so_don_hoan_thanh INT;
    DECLARE @so_khach INT;
    
    SELECT 
        @so_don_hang = COUNT(*),
        @so_don_hoan_thanh = SUM(CASE WHEN trang_thai = 'HOAN_THANH' THEN 1 ELSE 0 END),
        @so_khach = ISNULL(SUM(so_khach), 0)
    FROM [dbo].[Order]
    WHERE CAST(thoi_diem_dat AS DATE) = @ngay;
    
    -- Trả về kết quả tổng quan
    SELECT 
        @ngay AS ngay,
        @tong_doanh_thu AS tong_doanh_thu,
        @tong_chi_phi_nguyen_lieu AS tong_chi_phi_nguyen_lieu,
        @tong_chi_phi_luong AS tong_chi_phi_luong,
        @tong_chi_phi AS tong_chi_phi,
        @loi_nhuan AS loi_nhuan,
        @ty_le_loi_nhuan AS ty_le_loi_nhuan,
        @so_don_hang AS so_don_hang,
        @so_don_hoan_thanh AS so_don_hoan_thanh,
        @so_khach AS so_khach;
    
    -- Trả về chi tiết doanh thu theo loại món
    SELECT 
        m.loai_mon,
        COUNT(oi.order_item_id) AS so_luong_mon,
        SUM(oi.so_luong) AS tong_so_luong,
        SUM(oi.so_luong * m.gia) AS doanh_thu_theo_loai,
        -- Tính tỷ lệ phần trăm doanh thu
        CASE 
            WHEN @tong_doanh_thu > 0
            THEN (SUM(oi.so_luong * m.gia) * 100.0) / @tong_doanh_thu
            ELSE 0
        END AS ty_le_doanh_thu_phan_tram
    FROM [dbo].[Order] o
    INNER JOIN [dbo].[HoaDon] h ON o.order_id = h.order_id
    INNER JOIN [dbo].[OrderItem] oi ON o.order_id = oi.order_id
    INNER JOIN [dbo].[Mon] m ON oi.mon_id = m.mon_id
    WHERE CAST(o.thoi_diem_dat AS DATE) = @ngay
      AND o.trang_thai = 'HOAN_THANH'
    GROUP BY m.loai_mon
    ORDER BY doanh_thu_theo_loai DESC;
    
    -- Trả về chi tiết chi phí theo nguyên liệu
    SELECT 
        nl.ten AS ten_nguyen_lieu,
        nl.don_vi,
        SUM(ct.dinh_luong * oi.so_luong) AS tong_luong_su_dung,
        nl.gia_nhap AS gia_don_vi,
        SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap) AS tong_chi_phi_nguyen_lieu,
        -- Tính tỷ lệ phần trăm chi phí nguyên liệu
        CASE 
            WHEN @tong_chi_phi_nguyen_lieu > 0
            THEN (SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap) * 100.0) / @tong_chi_phi_nguyen_lieu
            ELSE 0
        END AS ty_le_chi_phi_phan_tram
    FROM [dbo].[Order] o
    INNER JOIN [dbo].[HoaDon] h ON o.order_id = h.order_id
    INNER JOIN [dbo].[OrderItem] oi ON o.order_id = oi.order_id
    INNER JOIN [dbo].[CongThuc] ct ON oi.mon_id = ct.mon_id
    INNER JOIN [dbo].[NguyenLieu] nl ON ct.nl_id = nl.nl_id
    WHERE CAST(o.thoi_diem_dat AS DATE) = @ngay
      AND o.trang_thai = 'HOAN_THANH'
    GROUP BY nl.nl_id, nl.ten, nl.don_vi, nl.gia_nhap
    ORDER BY tong_chi_phi_nguyen_lieu DESC;
    
    -- Trả về chi tiết chi phí lương theo loại nhân viên
    SELECT 
        lnv.loai_nv,
        COUNT(nv.nv_id) AS so_nhan_vien,
        lnv.luong_co_ban,
        (lnv.luong_co_ban / 30) AS luong_ngay,
        (COUNT(nv.nv_id) * lnv.luong_co_ban / 30) AS tong_chi_phi_luong_loai,
        -- Tính tỷ lệ phần trăm chi phí lương
        CASE 
            WHEN @tong_chi_phi_luong > 0
            THEN ((COUNT(nv.nv_id) * lnv.luong_co_ban / 30) * 100.0) / @tong_chi_phi_luong
            ELSE 0
        END AS ty_le_chi_phi_luong_phan_tram
    FROM [dbo].[NhanVien] nv
    INNER JOIN [dbo].[LoaiNhanVien] lnv ON nv.loai_nv_id = lnv.loai_nv_id
    WHERE nv.trang_thai = 'ACTIVE'
    GROUP BY lnv.loai_nv_id, lnv.loai_nv, lnv.luong_co_ban
    ORDER BY tong_chi_phi_luong_loai DESC;
END
GO

/****** Object:  StoredProcedure [dbo].[sp_ThongKe_DoanhThuChiPhiTheoKhoangThoiGian]    Script Date: 1/18/2025 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Stored procedure tính tổng doanh thu và chi phí theo khoảng thời gian
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ThongKe_DoanhThuChiPhiTheoKhoangThoiGian]
    @tu_ngay DATE,
    @den_ngay DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra tham số đầu vào
    IF @tu_ngay IS NULL OR @den_ngay IS NULL
    BEGIN
        RAISERROR(N'Ngày bắt đầu và ngày kết thúc không được để trống', 16, 1);
        RETURN;
    END
    
    IF @tu_ngay > @den_ngay
    BEGIN
        RAISERROR(N'Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc', 16, 1);
        RETURN;
    END
    
    DECLARE @so_ngay INT = DATEDIFF(DAY, @tu_ngay, @den_ngay) + 1;
    
    -- Tính tổng doanh thu trong khoảng thời gian
    DECLARE @tong_doanh_thu DECIMAL(12,2);
    
    SELECT @tong_doanh_thu = ISNULL(SUM(o.tong_tien), 0)
    FROM [dbo].[Order] o
    INNER JOIN [dbo].[HoaDon] h ON o.order_id = h.order_id
    WHERE CAST(o.thoi_diem_dat AS DATE) BETWEEN @tu_ngay AND @den_ngay
      AND o.trang_thai = 'HOAN_THANH';
    
    -- Tính tổng chi phí nguyên liệu trong khoảng thời gian
    DECLARE @tong_chi_phi_nguyen_lieu DECIMAL(12,2);
    
    SELECT @tong_chi_phi_nguyen_lieu = ISNULL(SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap), 0)
    FROM [dbo].[Order] o
    INNER JOIN [dbo].[HoaDon] h ON o.order_id = h.order_id
    INNER JOIN [dbo].[OrderItem] oi ON o.order_id = oi.order_id
    INNER JOIN [dbo].[CongThuc] ct ON oi.mon_id = ct.mon_id
    INNER JOIN [dbo].[NguyenLieu] nl ON ct.nl_id = nl.nl_id
    WHERE CAST(o.thoi_diem_dat AS DATE) BETWEEN @tu_ngay AND @den_ngay
      AND o.trang_thai = 'HOAN_THANH';
    
    -- Tính tổng chi phí lương nhân viên trong khoảng thời gian
    DECLARE @tong_chi_phi_luong DECIMAL(12,2);
    
    SELECT @tong_chi_phi_luong = ISNULL(SUM(lnv.luong_co_ban * @so_ngay / 30), 0)
    FROM [dbo].[NhanVien] nv
    INNER JOIN [dbo].[LoaiNhanVien] lnv ON nv.loai_nv_id = lnv.loai_nv_id
    WHERE nv.trang_thai = 'ACTIVE';
    
    -- Tính tổng chi phí
    DECLARE @tong_chi_phi DECIMAL(12,2) = @tong_chi_phi_nguyen_lieu + @tong_chi_phi_luong;
    
    -- Tính lợi nhuận
    DECLARE @loi_nhuan DECIMAL(12,2) = @tong_doanh_thu - @tong_chi_phi;
    
    -- Tính tỷ lệ lợi nhuận
    DECLARE @ty_le_loi_nhuan DECIMAL(5,2);
    IF @tong_doanh_thu > 0
        SET @ty_le_loi_nhuan = (@loi_nhuan / @tong_doanh_thu) * 100;
    ELSE
        SET @ty_le_loi_nhuan = 0;
    
    -- Thống kê số lượng đơn hàng
    DECLARE @so_don_hang INT;
    DECLARE @so_don_hoan_thanh INT;
    DECLARE @so_khach INT;
    
    SELECT 
        @so_don_hang = COUNT(*),
        @so_don_hoan_thanh = SUM(CASE WHEN trang_thai = 'HOAN_THANH' THEN 1 ELSE 0 END),
        @so_khach = ISNULL(SUM(so_khach), 0)
    FROM [dbo].[Order]
    WHERE CAST(thoi_diem_dat AS DATE) BETWEEN @tu_ngay AND @den_ngay;
    
    -- Trả về kết quả tổng quan
    SELECT 
        @tu_ngay AS tu_ngay,
        @den_ngay AS den_ngay,
        @so_ngay AS so_ngay,
        @tong_doanh_thu AS tong_doanh_thu,
        @tong_chi_phi_nguyen_lieu AS tong_chi_phi_nguyen_lieu,
        @tong_chi_phi_luong AS tong_chi_phi_luong,
        @tong_chi_phi AS tong_chi_phi,
        @loi_nhuan AS loi_nhuan,
        @ty_le_loi_nhuan AS ty_le_loi_nhuan,
        @so_don_hang AS so_don_hang,
        @so_don_hoan_thanh AS so_don_hoan_thanh,
        @so_khach AS so_khach,
        (@tong_doanh_thu / @so_ngay) AS doanh_thu_trung_binh_ngay,
        (@loi_nhuan / @so_ngay) AS loi_nhuan_trung_binh_ngay;
    
    -- Trả về thống kê theo ngày
    SELECT 
        CAST(o.thoi_diem_dat AS DATE) AS ngay,
        COUNT(*) AS so_don_hang,
        SUM(CASE WHEN o.trang_thai = 'HOAN_THANH' THEN 1 ELSE 0 END) AS so_don_hoan_thanh,
        ISNULL(SUM(CASE WHEN o.trang_thai = 'HOAN_THANH' THEN o.tong_tien ELSE 0 END), 0) AS doanh_thu_ngay,
        ISNULL(SUM(CASE WHEN o.trang_thai = 'HOAN_THANH' THEN ct.dinh_luong * oi.so_luong * nl.gia_nhap ELSE 0 END), 0) AS chi_phi_nguyen_lieu_ngay,
        (@tong_chi_phi_luong / @so_ngay) AS chi_phi_luong_ngay,
        (ISNULL(SUM(CASE WHEN o.trang_thai = 'HOAN_THANH' THEN ct.dinh_luong * oi.so_luong * nl.gia_nhap ELSE 0 END), 0) + (@tong_chi_phi_luong / @so_ngay)) AS tong_chi_phi_ngay,
        (ISNULL(SUM(CASE WHEN o.trang_thai = 'HOAN_THANH' THEN o.tong_tien ELSE 0 END), 0) - (ISNULL(SUM(CASE WHEN o.trang_thai = 'HOAN_THANH' THEN ct.dinh_luong * oi.so_luong * nl.gia_nhap ELSE 0 END), 0) + (@tong_chi_phi_luong / @so_ngay))) AS loi_nhuan_ngay
    FROM [dbo].[Order] o
    LEFT JOIN [dbo].[HoaDon] h ON o.order_id = h.order_id
    LEFT JOIN [dbo].[OrderItem] oi ON o.order_id = oi.order_id
    LEFT JOIN [dbo].[CongThuc] ct ON oi.mon_id = ct.mon_id
    LEFT JOIN [dbo].[NguyenLieu] nl ON ct.nl_id = nl.nl_id
    WHERE CAST(o.thoi_diem_dat AS DATE) BETWEEN @tu_ngay AND @den_ngay
    GROUP BY CAST(o.thoi_diem_dat AS DATE)
    ORDER BY ngay;
END
GO

/****** Object:  StoredProcedure [dbo].[sp_ThongKe_DoanhThuChiPhiNgayHienTai]    Script Date: 1/18/2025 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Stored procedure tính tổng doanh thu và chi phí ngày hiện tại
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ThongKe_DoanhThuChiPhiNgayHienTai]
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ngay_hien_tai DATE = CAST(GETDATE() AS DATE);
    
    EXEC [dbo].[sp_ThongKe_DoanhThuChiPhiTheoNgay] @ngay_hien_tai;
END
GO

-- =============================================
-- Ví dụ sử dụng các stored procedure
-- =============================================

/*
-- Tính doanh thu và chi phí ngày cụ thể
EXEC [dbo].[sp_ThongKe_DoanhThuChiPhiTheoNgay] @ngay = '2025-01-18';

-- Tính doanh thu và chi phí ngày hiện tại
EXEC [dbo].[sp_ThongKe_DoanhThuChiPhiNgayHienTai];

-- Tính doanh thu và chi phí theo khoảng thời gian
EXEC [dbo].[sp_ThongKe_DoanhThuChiPhiTheoKhoangThoiGian] 
    @tu_ngay = '2025-01-01', 
    @den_ngay = '2025-01-31';
*/
