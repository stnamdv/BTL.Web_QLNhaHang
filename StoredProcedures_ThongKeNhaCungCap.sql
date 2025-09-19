CREATE OR ALTER PROCEDURE dbo.sp_ThongKeNhaCungCap_TheoSoLuongNguyenLieu
    @Thang INT = NULL,
    @Nam INT = NULL,
    @NccId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Nếu không chỉ định tháng/năm, sử dụng tháng/năm hiện tại
    IF @Thang IS NULL
        SET @Thang = MONTH(GETDATE());
    IF @Nam IS NULL
        SET @Nam = YEAR(GETDATE());
    
    -- Kiểm tra tháng hợp lệ
    IF @Thang < 1 OR @Thang > 12
    BEGIN
        RAISERROR(N'Tháng phải từ 1 đến 12.', 16, 1);
        RETURN;
    END   
    
    -- Nếu chỉ định nhà cung cấp cụ thể, kiểm tra tồn tại
    IF @NccId IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.NhaCungCap WHERE ncc_id = @NccId)
        BEGIN
            RAISERROR(N'Nhà cung cấp không tồn tại.', 16, 1);
            RETURN;
        END
    END
    
    -- Thống kê tổng quan các nhà cung cấp
    ;WITH ThongKeNhaCungCap AS (
        SELECT 
            ncc.ncc_id,
            ncc.ten AS ten_nha_cung_cap,
            ncc.dia_chi,
            ncc.sdt,
            COUNT(DISTINCT nl.nl_id) AS tong_so_nguyen_lieu,
            COUNT(DISTINCT ct.mon_id) AS tong_so_mon_su_dung,
            -- Tổng lượng chung
            SUM(ISNULL(oi.so_luong * ct.dinh_luong, 0)) AS tong_luong_su_dung,
            SUM(ISNULL(oi.so_luong * ct.dinh_luong * nl.gia_nhap, 0)) AS tong_gia_tri_su_dung,
            COUNT(DISTINCT oi.order_id) AS tong_so_don_hang,
            COUNT(oi.order_item_id) AS tong_so_mon_duoc_dat
        FROM dbo.NhaCungCap ncc
        LEFT JOIN dbo.NguyenLieu nl ON nl.ncc_id = ncc.ncc_id
        LEFT JOIN dbo.CongThuc ct ON ct.nl_id = nl.nl_id
        LEFT JOIN dbo.OrderItem oi ON oi.mon_id = ct.mon_id
            AND MONTH(oi.t_dat) = @Thang 
            AND YEAR(oi.t_dat) = @Nam
            AND oi.t_hoan_thanh IS NOT NULL  -- Chỉ tính các món đã hoàn thành
        WHERE (@NccId IS NULL OR ncc.ncc_id = @NccId)
        GROUP BY ncc.ncc_id, ncc.ten, ncc.dia_chi, ncc.sdt
    ),
    -- Thống kê lượng theo đơn vị
    ThongKeTheoDonVi AS (
        SELECT 
            ncc.ncc_id,
            nl.don_vi,
            SUM(ISNULL(oi.so_luong * ct.dinh_luong, 0)) AS tong_luong_theo_don_vi
        FROM dbo.NhaCungCap ncc
        LEFT JOIN dbo.NguyenLieu nl ON nl.ncc_id = ncc.ncc_id
        LEFT JOIN dbo.CongThuc ct ON ct.nl_id = nl.nl_id
        LEFT JOIN dbo.OrderItem oi ON oi.mon_id = ct.mon_id
            AND MONTH(oi.t_dat) = @Thang 
            AND YEAR(oi.t_dat) = @Nam
            AND oi.t_hoan_thanh IS NOT NULL
        WHERE (@NccId IS NULL OR ncc.ncc_id = @NccId)
            AND nl.don_vi IS NOT NULL
        GROUP BY ncc.ncc_id, nl.don_vi
    )
    -- Trả về thống kê chính
    SELECT 
        tkncc.ncc_id,
        tkncc.ten_nha_cung_cap,
        tkncc.dia_chi,
        tkncc.sdt,
        tkncc.tong_so_nguyen_lieu,
        tkncc.tong_so_mon_su_dung,
        tkncc.tong_luong_su_dung,
        tkncc.tong_gia_tri_su_dung,
        tkncc.tong_so_don_hang,
        tkncc.tong_so_mon_duoc_dat,
        CASE 
            WHEN tkncc.tong_so_nguyen_lieu > 0 THEN 
                CAST(tkncc.tong_luong_su_dung AS DECIMAL(18,2)) / tkncc.tong_so_nguyen_lieu
            ELSE 0 
        END AS trung_binh_luong_su_dung_per_nguyen_lieu,
        CASE 
            WHEN tkncc.tong_so_mon_su_dung > 0 THEN 
                CAST(tkncc.tong_luong_su_dung AS DECIMAL(18,2)) / tkncc.tong_so_mon_su_dung
            ELSE 0 
        END AS trung_binh_luong_su_dung_per_mon
    FROM ThongKeNhaCungCap tkncc
    ORDER BY tkncc.tong_luong_su_dung DESC, tkncc.tong_gia_tri_su_dung DESC, tkncc.ten_nha_cung_cap;
    
    -- Trả về thống kê theo đơn vị (result set thứ 2)
    SELECT 
        ncc_id,
        don_vi,
        tong_luong_theo_don_vi
    FROM ThongKeTheoDonVi
    WHERE tong_luong_theo_don_vi > 0
    ORDER BY ncc_id, tong_luong_theo_don_vi DESC;
    
    -- Thống kê chi tiết từng nguyên liệu của nhà cung cấp (nếu chỉ định nhà cung cấp cụ thể)
    IF @NccId IS NOT NULL
    BEGIN
        SELECT 
            nl.nl_id,
            nl.ten AS ten_nguyen_lieu,
            nl.don_vi,
            nl.gia_nhap,
            COUNT(DISTINCT ct.mon_id) AS so_mon_su_dung,
            SUM(ISNULL(oi.so_luong * ct.dinh_luong, 0)) AS luong_su_dung,
            SUM(ISNULL(oi.so_luong * ct.dinh_luong * nl.gia_nhap, 0)) AS gia_tri_su_dung,
            COUNT(DISTINCT oi.order_id) AS so_don_hang,
            COUNT(oi.order_item_id) AS so_lan_dat
        FROM dbo.NguyenLieu nl
        LEFT JOIN dbo.CongThuc ct ON ct.nl_id = nl.nl_id
        LEFT JOIN dbo.OrderItem oi ON oi.mon_id = ct.mon_id
            AND MONTH(oi.t_dat) = @Thang 
            AND YEAR(oi.t_dat) = @Nam
            AND oi.t_hoan_thanh IS NOT NULL
        WHERE nl.ncc_id = @NccId
        GROUP BY nl.nl_id, nl.ten, nl.don_vi, nl.gia_nhap
        ORDER BY luong_su_dung DESC, gia_tri_su_dung DESC, nl.ten;
    END
END
GO

-- =========================================================
-- Procedure: sp_ThongKeNhaCungCap_SoSanhThang
-- Mô tả: So sánh thống kê nhà cung cấp giữa 2 tháng
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_ThongKeNhaCungCap_SoSanhThang
    @Thang1 INT,
    @Nam1 INT,
    @Thang2 INT,
    @Nam2 INT,
    @NccId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra tháng hợp lệ
    IF @Thang1 < 1 OR @Thang1 > 12 OR @Thang2 < 1 OR @Thang2 > 12
    BEGIN
        RAISERROR(N'Tháng phải từ 1 đến 12.', 16, 1);
        RETURN;
    END
    
    -- Kiểm tra năm hợp lệ
    IF @Nam1 < 2020 OR @Nam1 > 2030 OR @Nam2 < 2020 OR @Nam2 > 2030
    BEGIN
        RAISERROR(N'Năm phải từ 2020 đến 2030.', 16, 1);
        RETURN;
    END
    
    -- Nếu chỉ định nhà cung cấp cụ thể, kiểm tra tồn tại
    IF @NccId IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.NhaCungCap WHERE ncc_id = @NccId)
        BEGIN
            RAISERROR(N'Nhà cung cấp không tồn tại.', 16, 1);
            RETURN;
        END
    END
    
    -- Thống kê tháng 1
    ;WITH ThongKeThang1 AS (
        SELECT 
            ncc.ncc_id,
            ncc.ten AS ten_nha_cung_cap,
            SUM(ISNULL(oi.so_luong * ct.dinh_luong, 0)) AS luong_su_dung_thang1,
            SUM(ISNULL(oi.so_luong * ct.dinh_luong * nl.gia_nhap, 0)) AS gia_tri_su_dung_thang1,
            COUNT(DISTINCT oi.order_id) AS so_don_hang_thang1
        FROM dbo.NhaCungCap ncc
        LEFT JOIN dbo.NguyenLieu nl ON nl.ncc_id = ncc.ncc_id
        LEFT JOIN dbo.CongThuc ct ON ct.nl_id = nl.nl_id
        LEFT JOIN dbo.OrderItem oi ON oi.mon_id = ct.mon_id
            AND MONTH(oi.t_dat) = @Thang1 
            AND YEAR(oi.t_dat) = @Nam1
            AND oi.t_hoan_thanh IS NOT NULL
        WHERE (@NccId IS NULL OR ncc.ncc_id = @NccId)
        GROUP BY ncc.ncc_id, ncc.ten
    ),
    -- Thống kê tháng 2
    ThongKeThang2 AS (
        SELECT 
            ncc.ncc_id,
            ncc.ten AS ten_nha_cung_cap,
            SUM(ISNULL(oi.so_luong * ct.dinh_luong, 0)) AS luong_su_dung_thang2,
            SUM(ISNULL(oi.so_luong * ct.dinh_luong * nl.gia_nhap, 0)) AS gia_tri_su_dung_thang2,
            COUNT(DISTINCT oi.order_id) AS so_don_hang_thang2
        FROM dbo.NhaCungCap ncc
        LEFT JOIN dbo.NguyenLieu nl ON nl.ncc_id = ncc.ncc_id
        LEFT JOIN dbo.CongThuc ct ON ct.nl_id = nl.nl_id
        LEFT JOIN dbo.OrderItem oi ON oi.mon_id = ct.mon_id
            AND MONTH(oi.t_dat) = @Thang2 
            AND YEAR(oi.t_dat) = @Nam2
            AND oi.t_hoan_thanh IS NOT NULL
        WHERE (@NccId IS NULL OR ncc.ncc_id = @NccId)
        GROUP BY ncc.ncc_id, ncc.ten
    )
    SELECT 
        COALESCE(t1.ncc_id, t2.ncc_id) AS ncc_id,
        COALESCE(t1.ten_nha_cung_cap, t2.ten_nha_cung_cap) AS ten_nha_cung_cap,
        ISNULL(t1.luong_su_dung_thang1, 0) AS luong_su_dung_thang1,
        ISNULL(t1.gia_tri_su_dung_thang1, 0) AS gia_tri_su_dung_thang1,
        ISNULL(t1.so_don_hang_thang1, 0) AS so_don_hang_thang1,
        ISNULL(t2.luong_su_dung_thang2, 0) AS luong_su_dung_thang2,
        ISNULL(t2.gia_tri_su_dung_thang2, 0) AS gia_tri_su_dung_thang2,
        ISNULL(t2.so_don_hang_thang2, 0) AS so_don_hang_thang2,
        ISNULL(t2.luong_su_dung_thang2, 0) - ISNULL(t1.luong_su_dung_thang1, 0) AS chenh_lech_luong,
        ISNULL(t2.gia_tri_su_dung_thang2, 0) - ISNULL(t1.gia_tri_su_dung_thang1, 0) AS chenh_lech_gia_tri,
        ISNULL(t2.so_don_hang_thang2, 0) - ISNULL(t1.so_don_hang_thang1, 0) AS chenh_lech_don_hang,
        CASE 
            WHEN ISNULL(t1.luong_su_dung_thang1, 0) > 0 THEN
                CAST((ISNULL(t2.luong_su_dung_thang2, 0) - ISNULL(t1.luong_su_dung_thang1, 0)) * 100.0 / t1.luong_su_dung_thang1 AS DECIMAL(10,2))
            ELSE 
                CASE WHEN ISNULL(t2.luong_su_dung_thang2, 0) > 0 THEN 100.0 ELSE 0 END
        END AS phan_tram_tang_luong,
        CASE 
            WHEN ISNULL(t1.gia_tri_su_dung_thang1, 0) > 0 THEN
                CAST((ISNULL(t2.gia_tri_su_dung_thang2, 0) - ISNULL(t1.gia_tri_su_dung_thang1, 0)) * 100.0 / t1.gia_tri_su_dung_thang1 AS DECIMAL(10,2))
            ELSE 
                CASE WHEN ISNULL(t2.gia_tri_su_dung_thang2, 0) > 0 THEN 100.0 ELSE 0 END
        END AS phan_tram_tang_gia_tri
    FROM ThongKeThang1 t1
    FULL OUTER JOIN ThongKeThang2 t2 ON t1.ncc_id = t2.ncc_id
    ORDER BY 
        (ISNULL(t2.luong_su_dung_thang2, 0) - ISNULL(t1.luong_su_dung_thang1, 0)) DESC,
        (ISNULL(t2.gia_tri_su_dung_thang2, 0) - ISNULL(t1.gia_tri_su_dung_thang1, 0)) DESC;
END
GO

-- =========================================================
-- Procedure: sp_ThongKeNhaCungCap_TopNguyenLieu
-- Mô tả: Thống kê top nguyên liệu được sử dụng nhiều nhất của từng nhà cung cấp
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_ThongKeNhaCungCap_TopNguyenLieu
    @Thang INT = NULL,
    @Nam INT = NULL,
    @TopN INT = 5,
    @NccId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Nếu không chỉ định tháng/năm, sử dụng tháng/năm hiện tại
    IF @Thang IS NULL
        SET @Thang = MONTH(GETDATE());
    IF @Nam IS NULL
        SET @Nam = YEAR(GETDATE());
    
    -- Kiểm tra tháng hợp lệ
    IF @Thang < 1 OR @Thang > 12
    BEGIN
        RAISERROR(N'Tháng phải từ 1 đến 12.', 16, 1);
        RETURN;
    END
    
    -- Kiểm tra năm hợp lệ
    IF @Nam < 2020 OR @Nam > 2030
    BEGIN
        RAISERROR(N'Năm phải từ 2020 đến 2030.', 16, 1);
        RETURN;
    END
    
    -- Kiểm tra TopN hợp lệ
    IF @TopN < 1 OR @TopN > 50
    BEGIN
        RAISERROR(N'TopN phải từ 1 đến 50.', 16, 1);
        RETURN;
    END
    
    -- Nếu chỉ định nhà cung cấp cụ thể, kiểm tra tồn tại
    IF @NccId IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.NhaCungCap WHERE ncc_id = @NccId)
        BEGIN
            RAISERROR(N'Nhà cung cấp không tồn tại.', 16, 1);
            RETURN;
        END
    END
    
    -- Thống kê top nguyên liệu theo nhà cung cấp
    ;WITH ThongKeNguyenLieu AS (
        SELECT 
            ncc.ncc_id,
            ncc.ten AS ten_nha_cung_cap,
            nl.nl_id,
            nl.ten AS ten_nguyen_lieu,
            nl.don_vi,
            nl.gia_nhap,
            SUM(ISNULL(oi.so_luong * ct.dinh_luong, 0)) AS luong_su_dung,
            SUM(ISNULL(oi.so_luong * ct.dinh_luong * nl.gia_nhap, 0)) AS gia_tri_su_dung,
            COUNT(DISTINCT ct.mon_id) AS so_mon_su_dung,
            COUNT(DISTINCT oi.order_id) AS so_don_hang,
            ROW_NUMBER() OVER (PARTITION BY ncc.ncc_id ORDER BY SUM(ISNULL(oi.so_luong * ct.dinh_luong, 0)) DESC) AS thu_hang
        FROM dbo.NhaCungCap ncc
        LEFT JOIN dbo.NguyenLieu nl ON nl.ncc_id = ncc.ncc_id
        LEFT JOIN dbo.CongThuc ct ON ct.nl_id = nl.nl_id
        LEFT JOIN dbo.OrderItem oi ON oi.mon_id = ct.mon_id
            AND MONTH(oi.t_dat) = @Thang 
            AND YEAR(oi.t_dat) = @Nam
            AND oi.t_hoan_thanh IS NOT NULL
        WHERE (@NccId IS NULL OR ncc.ncc_id = @NccId)
        GROUP BY ncc.ncc_id, ncc.ten, nl.nl_id, nl.ten, nl.don_vi, nl.gia_nhap
    )
    SELECT 
        ncc_id,
        ten_nha_cung_cap,
        nl_id,
        ten_nguyen_lieu,
        don_vi,
        gia_nhap,
        luong_su_dung,
        gia_tri_su_dung,
        so_mon_su_dung,
        so_don_hang,
        thu_hang
    FROM ThongKeNguyenLieu
    WHERE thu_hang <= @TopN
    ORDER BY ncc_id, thu_hang;
END
GO

-- =========================================================
-- Procedure: sp_ThongKeNhaCungCap_TongHop
-- Mô tả: Thống kê tổng hợp về nhà cung cấp
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_ThongKeNhaCungCap_TongHop
    @Thang INT = NULL,
    @Nam INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Nếu không chỉ định tháng/năm, sử dụng tháng/năm hiện tại
    IF @Thang IS NULL
        SET @Thang = MONTH(GETDATE());
    IF @Nam IS NULL
        SET @Nam = YEAR(GETDATE());
    
    -- Kiểm tra tháng hợp lệ
    IF @Thang < 1 OR @Thang > 12
    BEGIN
        RAISERROR(N'Tháng phải từ 1 đến 12.', 16, 1);
        RETURN;
    END
    
    -- Kiểm tra năm hợp lệ
    IF @Nam < 2020 OR @Nam > 2030
    BEGIN
        RAISERROR(N'Năm phải từ 2020 đến 2030.', 16, 1);
        RETURN;
    END
    
    -- Thống kê tổng hợp
    SELECT 
        COUNT(DISTINCT ncc.ncc_id) AS tong_so_nha_cung_cap,
        COUNT(DISTINCT nl.nl_id) AS tong_so_nguyen_lieu,
        COUNT(DISTINCT ct.mon_id) AS tong_so_mon,
        COUNT(DISTINCT oi.order_id) AS tong_so_don_hang,
        COUNT(oi.order_item_id) AS tong_so_mon_duoc_dat,
        SUM(ISNULL(oi.so_luong * ct.dinh_luong, 0)) AS tong_luong_nguyen_lieu_su_dung,
        SUM(ISNULL(oi.so_luong * ct.dinh_luong * nl.gia_nhap, 0)) AS tong_gia_tri_nguyen_lieu_su_dung,
        AVG(ISNULL(oi.so_luong * ct.dinh_luong, 0)) AS trung_binh_luong_su_dung_per_mon,
        AVG(ISNULL(oi.so_luong * ct.dinh_luong * nl.gia_nhap, 0)) AS trung_binh_gia_tri_su_dung_per_mon
    FROM dbo.NhaCungCap ncc
    LEFT JOIN dbo.NguyenLieu nl ON nl.ncc_id = ncc.ncc_id
    LEFT JOIN dbo.CongThuc ct ON ct.nl_id = nl.nl_id
    LEFT JOIN dbo.OrderItem oi ON oi.mon_id = ct.mon_id
        AND MONTH(oi.t_dat) = @Thang 
        AND YEAR(oi.t_dat) = @Nam
        AND oi.t_hoan_thanh IS NOT NULL;
    
    -- Top 5 nhà cung cấp có lượng sử dụng cao nhất
    SELECT TOP 5
        ncc.ncc_id,
        ncc.ten AS ten_nha_cung_cap,
        SUM(ISNULL(oi.so_luong * ct.dinh_luong, 0)) AS luong_su_dung,
        SUM(ISNULL(oi.so_luong * ct.dinh_luong * nl.gia_nhap, 0)) AS gia_tri_su_dung,
        COUNT(DISTINCT oi.order_id) AS so_don_hang
    FROM dbo.NhaCungCap ncc
    LEFT JOIN dbo.NguyenLieu nl ON nl.ncc_id = ncc.ncc_id
    LEFT JOIN dbo.CongThuc ct ON ct.nl_id = nl.nl_id
    LEFT JOIN dbo.OrderItem oi ON oi.mon_id = ct.mon_id
        AND MONTH(oi.t_dat) = @Thang 
        AND YEAR(oi.t_dat) = @Nam
        AND oi.t_hoan_thanh IS NOT NULL
    GROUP BY ncc.ncc_id, ncc.ten
    ORDER BY luong_su_dung DESC;
    
    -- Top 5 nguyên liệu được sử dụng nhiều nhất
    SELECT TOP 5
        nl.nl_id,
        nl.ten AS ten_nguyen_lieu,
        ncc.ten AS ten_nha_cung_cap,
        SUM(ISNULL(oi.so_luong * ct.dinh_luong, 0)) AS luong_su_dung,
        SUM(ISNULL(oi.so_luong * ct.dinh_luong * nl.gia_nhap, 0)) AS gia_tri_su_dung,
        COUNT(DISTINCT ct.mon_id) AS so_mon_su_dung
    FROM dbo.NguyenLieu nl
    JOIN dbo.NhaCungCap ncc ON ncc.ncc_id = nl.ncc_id
    LEFT JOIN dbo.CongThuc ct ON ct.nl_id = nl.nl_id
    LEFT JOIN dbo.OrderItem oi ON oi.mon_id = ct.mon_id
        AND MONTH(oi.t_dat) = @Thang 
        AND YEAR(oi.t_dat) = @Nam
        AND oi.t_hoan_thanh IS NOT NULL
    GROUP BY nl.nl_id, nl.ten, ncc.ten
    ORDER BY luong_su_dung DESC;
END
GO
