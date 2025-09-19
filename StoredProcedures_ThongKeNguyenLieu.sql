USE [NhaHang]
GO
/****** Object:  StoredProcedure [dbo].[sp_ThongKe_NguyenLieuSuDungTheoNgay]    Script Date: 1/18/2025 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Stored procedure thống kê lượng nguyên liệu được sử dụng trong chế biến món ăn theo ngày
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ThongKe_NguyenLieuSuDungTheoNgay]
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
    
    ---- Thống kê tổng quan nguyên liệu sử dụng trong ngày
    --SELECT 
    --    @ngay AS ngay,
    --    COUNT(DISTINCT nl.nl_id) AS so_loai_nguyen_lieu_su_dung,
    --    SUM(ct.dinh_luong * oi.so_luong) AS tong_luong_nguyen_lieu_su_dung,
    --    SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap) AS tong_chi_phi_nguyen_lieu
    --FROM [dbo].[Order] o
    --INNER JOIN [dbo].[HoaDon] h ON o.order_id = h.order_id
    --INNER JOIN [dbo].[OrderItem] oi ON o.order_id = oi.order_id
    --INNER JOIN [dbo].[CongThuc] ct ON oi.mon_id = ct.mon_id
    --INNER JOIN [dbo].[NguyenLieu] nl ON ct.nl_id = nl.nl_id
    --WHERE CAST(o.thoi_diem_dat AS DATE) = @ngay
    --  AND o.trang_thai = 'HOAN_THANH';
    
    -- Chi tiết lượng nguyên liệu sử dụng theo từng loại
    SELECT 
        nl.nl_id,
        nl.ten AS ten_nguyen_lieu,
        nl.don_vi,
        nl.nguon_goc,
        nl.gia_nhap AS gia_don_vi,
        ncc.ten AS nha_cung_cap,
        SUM(ct.dinh_luong * oi.so_luong) AS tong_luong_su_dung,
        COUNT(DISTINCT oi.mon_id) AS so_mon_su_dung,
        SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap) AS tong_chi_phi_nguyen_lieu,
        -- Tính tỷ lệ phần trăm so với tổng chi phí
        CASE 
            WHEN (SELECT SUM(ct2.dinh_luong * oi2.so_luong * nl2.gia_nhap)
                  FROM [dbo].[Order] o2
                  INNER JOIN [dbo].[HoaDon] h2 ON o2.order_id = h2.order_id
                  INNER JOIN [dbo].[OrderItem] oi2 ON o2.order_id = oi2.order_id
                  INNER JOIN [dbo].[CongThuc] ct2 ON oi2.mon_id = ct2.mon_id
                  INNER JOIN [dbo].[NguyenLieu] nl2 ON ct2.nl_id = nl2.nl_id
                  WHERE CAST(o2.thoi_diem_dat AS DATE) = @ngay
                    AND o2.trang_thai = 'HOAN_THANH') > 0
            THEN (SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap) * 100.0) / 
                 (SELECT SUM(ct2.dinh_luong * oi2.so_luong * nl2.gia_nhap)
                  FROM [dbo].[Order] o2
                  INNER JOIN [dbo].[HoaDon] h2 ON o2.order_id = h2.order_id
                  INNER JOIN [dbo].[OrderItem] oi2 ON o2.order_id = oi2.order_id
                  INNER JOIN [dbo].[CongThuc] ct2 ON oi2.mon_id = ct2.mon_id
                  INNER JOIN [dbo].[NguyenLieu] nl2 ON ct2.nl_id = nl2.nl_id
                  WHERE CAST(o2.thoi_diem_dat AS DATE) = @ngay
                    AND o2.trang_thai = 'HOAN_THANH')
            ELSE 0
        END AS ty_le_chi_phi_phan_tram
    FROM [dbo].[Order] o
    INNER JOIN [dbo].[HoaDon] h ON o.order_id = h.order_id
    INNER JOIN [dbo].[OrderItem] oi ON o.order_id = oi.order_id
    INNER JOIN [dbo].[CongThuc] ct ON oi.mon_id = ct.mon_id
    INNER JOIN [dbo].[NguyenLieu] nl ON ct.nl_id = nl.nl_id
    INNER JOIN [dbo].[NhaCungCap] ncc ON nl.ncc_id = ncc.ncc_id
    WHERE CAST(o.thoi_diem_dat AS DATE) = @ngay
      AND o.trang_thai = 'HOAN_THANH'
    GROUP BY nl.nl_id, nl.ten, nl.don_vi, nl.nguon_goc, nl.gia_nhap, ncc.ten
    ORDER BY tong_chi_phi_nguyen_lieu DESC;
    
    -- Thống kê tổng lượng nguyên liệu sử dụng theo đơn vị
    SELECT 
        nl.don_vi,
        COUNT(DISTINCT nl.nl_id) AS so_loai_nguyen_lieu,
        SUM(ct.dinh_luong * oi.so_luong) AS tong_luong_su_dung,
        SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap) AS tong_chi_phi_theo_don_vi,
        -- Tính tỷ lệ phần trăm so với tổng chi phí
        CASE 
            WHEN (SELECT SUM(ct2.dinh_luong * oi2.so_luong * nl2.gia_nhap)
                  FROM [dbo].[Order] o2
                  INNER JOIN [dbo].[HoaDon] h2 ON o2.order_id = h2.order_id
                  INNER JOIN [dbo].[OrderItem] oi2 ON o2.order_id = oi2.order_id
                  INNER JOIN [dbo].[CongThuc] ct2 ON oi2.mon_id = ct2.mon_id
                  INNER JOIN [dbo].[NguyenLieu] nl2 ON ct2.nl_id = nl2.nl_id
                  WHERE CAST(o2.thoi_diem_dat AS DATE) = @ngay
                    AND o2.trang_thai = 'HOAN_THANH') > 0
            THEN (SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap) * 100.0) / 
                 (SELECT SUM(ct2.dinh_luong * oi2.so_luong * nl2.gia_nhap)
                  FROM [dbo].[Order] o2
                  INNER JOIN [dbo].[HoaDon] h2 ON o2.order_id = h2.order_id
                  INNER JOIN [dbo].[OrderItem] oi2 ON o2.order_id = oi2.order_id
                  INNER JOIN [dbo].[CongThuc] ct2 ON oi2.mon_id = ct2.mon_id
                  INNER JOIN [dbo].[NguyenLieu] nl2 ON ct2.nl_id = nl2.nl_id
                  WHERE CAST(o2.thoi_diem_dat AS DATE) = @ngay
                    AND o2.trang_thai = 'HOAN_THANH')
            ELSE 0
        END AS ty_le_chi_phi_phan_tram
    FROM [dbo].[Order] o
    INNER JOIN [dbo].[HoaDon] h ON o.order_id = h.order_id
    INNER JOIN [dbo].[OrderItem] oi ON o.order_id = oi.order_id
    INNER JOIN [dbo].[CongThuc] ct ON oi.mon_id = ct.mon_id
    INNER JOIN [dbo].[NguyenLieu] nl ON ct.nl_id = nl.nl_id
    WHERE CAST(o.thoi_diem_dat AS DATE) = @ngay
      AND o.trang_thai = 'HOAN_THANH'
    GROUP BY nl.don_vi
    ORDER BY tong_chi_phi_theo_don_vi DESC;
    
    -- Chi tiết món ăn sử dụng từng nguyên liệu
    SELECT 
        nl.nl_id,
        nl.ten AS ten_nguyen_lieu,
        m.ma_mon,
        m.ten_mon,
        m.loai_mon,
        ct.dinh_luong AS dinh_luong_cho_1_mon,
        SUM(oi.so_luong) AS tong_so_mon_duoc_dat,
        SUM(ct.dinh_luong * oi.so_luong) AS tong_luong_nguyen_lieu_cho_mon,
        SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap) AS chi_phi_nguyen_lieu_cho_mon
    FROM [dbo].[Order] o
    INNER JOIN [dbo].[HoaDon] h ON o.order_id = h.order_id
    INNER JOIN [dbo].[OrderItem] oi ON o.order_id = oi.order_id
    INNER JOIN [dbo].[CongThuc] ct ON oi.mon_id = ct.mon_id
    INNER JOIN [dbo].[NguyenLieu] nl ON ct.nl_id = nl.nl_id
    INNER JOIN [dbo].[Mon] m ON oi.mon_id = m.mon_id
    WHERE CAST(o.thoi_diem_dat AS DATE) = @ngay
      AND o.trang_thai = 'HOAN_THANH'
    GROUP BY nl.nl_id, nl.ten, m.ma_mon, m.ten_mon, m.loai_mon, ct.dinh_luong
    ORDER BY nl.ten, chi_phi_nguyen_lieu_cho_mon DESC;
END
GO

/****** Object:  StoredProcedure [dbo].[sp_ThongKe_NguyenLieuSuDungTheoKhoangThoiGian]    Script Date: 1/18/2025 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Stored procedure thống kê lượng nguyên liệu được sử dụng theo khoảng thời gian
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ThongKe_NguyenLieuSuDungTheoKhoangThoiGian]
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
    
    -- Thống kê tổng quan nguyên liệu sử dụng trong khoảng thời gian
    SELECT 
        @tu_ngay AS tu_ngay,
        @den_ngay AS den_ngay,
        @so_ngay AS so_ngay,
        COUNT(DISTINCT nl.nl_id) AS so_loai_nguyen_lieu_su_dung,
        SUM(ct.dinh_luong * oi.so_luong) AS tong_luong_nguyen_lieu_su_dung,
        SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap) AS tong_chi_phi_nguyen_lieu,
        (SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap) / @so_ngay) AS chi_phi_nguyen_lieu_trung_binh_ngay
    FROM [dbo].[Order] o
    INNER JOIN [dbo].[HoaDon] h ON o.order_id = h.order_id
    INNER JOIN [dbo].[OrderItem] oi ON o.order_id = oi.order_id
    INNER JOIN [dbo].[CongThuc] ct ON oi.mon_id = ct.mon_id
    INNER JOIN [dbo].[NguyenLieu] nl ON ct.nl_id = nl.nl_id
    WHERE CAST(o.thoi_diem_dat AS DATE) BETWEEN @tu_ngay AND @den_ngay
      AND o.trang_thai = 'HOAN_THANH';
    
    -- Chi tiết lượng nguyên liệu sử dụng theo từng loại trong khoảng thời gian
    SELECT 
        nl.nl_id,
        nl.ten AS ten_nguyen_lieu,
        nl.don_vi,
        nl.nguon_goc,
        nl.gia_nhap AS gia_don_vi,
        ncc.ten AS nha_cung_cap,
        SUM(ct.dinh_luong * oi.so_luong) AS tong_luong_su_dung,
        COUNT(DISTINCT oi.mon_id) AS so_mon_su_dung,
        COUNT(DISTINCT CAST(o.thoi_diem_dat AS DATE)) AS so_ngay_su_dung,
        SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap) AS tong_chi_phi_nguyen_lieu,
        (SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap) / @so_ngay) AS chi_phi_trung_binh_ngay,
        -- Tính tỷ lệ phần trăm so với tổng chi phí
        CASE 
            WHEN (SELECT SUM(ct2.dinh_luong * oi2.so_luong * nl2.gia_nhap)
                  FROM [dbo].[Order] o2
                  INNER JOIN [dbo].[HoaDon] h2 ON o2.order_id = h2.order_id
                  INNER JOIN [dbo].[OrderItem] oi2 ON o2.order_id = oi2.order_id
                  INNER JOIN [dbo].[CongThuc] ct2 ON oi2.mon_id = ct2.mon_id
                  INNER JOIN [dbo].[NguyenLieu] nl2 ON ct2.nl_id = nl2.nl_id
                  WHERE CAST(o2.thoi_diem_dat AS DATE) BETWEEN @tu_ngay AND @den_ngay
                    AND o2.trang_thai = 'HOAN_THANH') > 0
            THEN (SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap) * 100.0) / 
                 (SELECT SUM(ct2.dinh_luong * oi2.so_luong * nl2.gia_nhap)
                  FROM [dbo].[Order] o2
                  INNER JOIN [dbo].[HoaDon] h2 ON o2.order_id = h2.order_id
                  INNER JOIN [dbo].[OrderItem] oi2 ON o2.order_id = oi2.order_id
                  INNER JOIN [dbo].[CongThuc] ct2 ON oi2.mon_id = ct2.mon_id
                  INNER JOIN [dbo].[NguyenLieu] nl2 ON ct2.nl_id = nl2.nl_id
                  WHERE CAST(o2.thoi_diem_dat AS DATE) BETWEEN @tu_ngay AND @den_ngay
                    AND o2.trang_thai = 'HOAN_THANH')
            ELSE 0
        END AS ty_le_chi_phi_phan_tram
    FROM [dbo].[Order] o
    INNER JOIN [dbo].[HoaDon] h ON o.order_id = h.order_id
    INNER JOIN [dbo].[OrderItem] oi ON o.order_id = oi.order_id
    INNER JOIN [dbo].[CongThuc] ct ON oi.mon_id = ct.mon_id
    INNER JOIN [dbo].[NguyenLieu] nl ON ct.nl_id = nl.nl_id
    INNER JOIN [dbo].[NhaCungCap] ncc ON nl.ncc_id = ncc.ncc_id
    WHERE CAST(o.thoi_diem_dat AS DATE) BETWEEN @tu_ngay AND @den_ngay
      AND o.trang_thai = 'HOAN_THANH'
    GROUP BY nl.nl_id, nl.ten, nl.don_vi, nl.nguon_goc, nl.gia_nhap, ncc.ten
    ORDER BY tong_chi_phi_nguyen_lieu DESC;
    
    -- Thống kê tổng lượng nguyên liệu sử dụng theo đơn vị trong khoảng thời gian
    SELECT 
        nl.don_vi,
        COUNT(DISTINCT nl.nl_id) AS so_loai_nguyen_lieu,
        SUM(ct.dinh_luong * oi.so_luong) AS tong_luong_su_dung,
        SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap) AS tong_chi_phi_theo_don_vi,
        (SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap) / @so_ngay) AS chi_phi_trung_binh_ngay_theo_don_vi,
        -- Tính tỷ lệ phần trăm so với tổng chi phí
        CASE 
            WHEN (SELECT SUM(ct2.dinh_luong * oi2.so_luong * nl2.gia_nhap)
                  FROM [dbo].[Order] o2
                  INNER JOIN [dbo].[HoaDon] h2 ON o2.order_id = h2.order_id
                  INNER JOIN [dbo].[OrderItem] oi2 ON o2.order_id = oi2.order_id
                  INNER JOIN [dbo].[CongThuc] ct2 ON oi2.mon_id = ct2.mon_id
                  INNER JOIN [dbo].[NguyenLieu] nl2 ON ct2.nl_id = nl2.nl_id
                  WHERE CAST(o2.thoi_diem_dat AS DATE) BETWEEN @tu_ngay AND @den_ngay
                    AND o2.trang_thai = 'HOAN_THANH') > 0
            THEN (SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap) * 100.0) / 
                 (SELECT SUM(ct2.dinh_luong * oi2.so_luong * nl2.gia_nhap)
                  FROM [dbo].[Order] o2
                  INNER JOIN [dbo].[HoaDon] h2 ON o2.order_id = h2.order_id
                  INNER JOIN [dbo].[OrderItem] oi2 ON o2.order_id = oi2.order_id
                  INNER JOIN [dbo].[CongThuc] ct2 ON oi2.mon_id = ct2.mon_id
                  INNER JOIN [dbo].[NguyenLieu] nl2 ON ct2.nl_id = nl2.nl_id
                  WHERE CAST(o2.thoi_diem_dat AS DATE) BETWEEN @tu_ngay AND @den_ngay
                    AND o2.trang_thai = 'HOAN_THANH')
            ELSE 0
        END AS ty_le_chi_phi_phan_tram
    FROM [dbo].[Order] o
    INNER JOIN [dbo].[HoaDon] h ON o.order_id = h.order_id
    INNER JOIN [dbo].[OrderItem] oi ON o.order_id = oi.order_id
    INNER JOIN [dbo].[CongThuc] ct ON oi.mon_id = ct.mon_id
    INNER JOIN [dbo].[NguyenLieu] nl ON ct.nl_id = nl.nl_id
    WHERE CAST(o.thoi_diem_dat AS DATE) BETWEEN @tu_ngay AND @den_ngay
      AND o.trang_thai = 'HOAN_THANH'
    GROUP BY nl.don_vi
    ORDER BY tong_chi_phi_theo_don_vi DESC;
    
    -- Thống kê theo ngày
    SELECT 
        CAST(o.thoi_diem_dat AS DATE) AS ngay,
        COUNT(DISTINCT nl.nl_id) AS so_loai_nguyen_lieu_su_dung,
        SUM(ct.dinh_luong * oi.so_luong) AS tong_luong_nguyen_lieu_su_dung,
        SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap) AS chi_phi_nguyen_lieu_ngay
    FROM [dbo].[Order] o
    INNER JOIN [dbo].[HoaDon] h ON o.order_id = h.order_id
    INNER JOIN [dbo].[OrderItem] oi ON o.order_id = oi.order_id
    INNER JOIN [dbo].[CongThuc] ct ON oi.mon_id = ct.mon_id
    INNER JOIN [dbo].[NguyenLieu] nl ON ct.nl_id = nl.nl_id
    WHERE CAST(o.thoi_diem_dat AS DATE) BETWEEN @tu_ngay AND @den_ngay
      AND o.trang_thai = 'HOAN_THANH'
    GROUP BY CAST(o.thoi_diem_dat AS DATE)
    ORDER BY ngay;
END
GO

/****** Object:  StoredProcedure [dbo].[sp_ThongKe_NguyenLieuSuDungNgayHienTai]    Script Date: 1/18/2025 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Stored procedure thống kê lượng nguyên liệu được sử dụng ngày hiện tại
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ThongKe_NguyenLieuSuDungNgayHienTai]
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ngay_hien_tai DATE = CAST(GETDATE() AS DATE);
    
    EXEC [dbo].[sp_ThongKe_NguyenLieuSuDungTheoNgay] @ngay_hien_tai;
END
GO

-- =============================================
-- Ví dụ sử dụng các stored procedure
-- =============================================

/*
-- Thống kê nguyên liệu sử dụng ngày cụ thể
EXEC [dbo].[sp_ThongKe_NguyenLieuSuDungTheoNgay] @ngay = '2025-01-18';

-- Thống kê nguyên liệu sử dụng ngày hiện tại
EXEC [dbo].[sp_ThongKe_NguyenLieuSuDungNgayHienTai];

-- Thống kê nguyên liệu sử dụng theo khoảng thời gian
EXEC [dbo].[sp_ThongKe_NguyenLieuSuDungTheoKhoangThoiGian] 
    @tu_ngay = '2025-01-01', 
    @den_ngay = '2025-01-31';
*/
