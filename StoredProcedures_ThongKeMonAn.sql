/* =========================================================
   STORED PROCEDURES CHO THỐNG KÊ MÓN ĂN
   Thống kê tần suất được khách đặt/gọi cho từng món ăn trong một tháng
   ========================================================= */

USE NhaHang
GO

-- =========================================================
-- 1. sp_ThongKeMonAn_TanSuatTheoThang - Thống kê tần suất món ăn theo tháng
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_ThongKeMonAn_TanSuatTheoThang
    @Thang INT,
    @Nam INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra tham số đầu vào
    IF @Thang < 1 OR @Thang > 12
    BEGIN
        RAISERROR('Tháng phải từ 1 đến 12', 16, 1);
        RETURN;
    END
    
    -- Thống kê tần suất món ăn
    ;WITH MonAnStats AS (
        SELECT 
            m.mon_id,
            m.ten_mon,
            m.loai_mon,
            m.gia,
            COUNT(oi.order_item_id) AS so_lan_duoc_dat,
            SUM(oi.so_luong) AS tong_so_luong_dat,
            COUNT(DISTINCT o.order_id) AS so_don_hang_chua_mon,
            COUNT(DISTINCT CAST(o.thoi_diem_dat AS DATE)) AS so_ngay_co_dat,
            AVG(CAST(oi.so_luong AS FLOAT)) AS trung_binh_so_luong_moi_lan,
            SUM(oi.so_luong * m.gia) AS tong_doanh_thu_mon
        FROM Mon m
        LEFT JOIN OrderItem oi ON m.mon_id = oi.mon_id
        LEFT JOIN [Order] o ON oi.order_id = o.order_id
            AND MONTH(o.thoi_diem_dat) = @Thang
            AND YEAR(o.thoi_diem_dat) = @Nam
            AND o.trang_thai IN ('HOAN_THANH')
        GROUP BY m.mon_id, m.ten_mon, m.loai_mon, m.gia
    ),
    TotalStats AS (
        SELECT 
            COUNT(DISTINCT o.order_id) AS tong_so_don_hang,
            COUNT(DISTINCT CAST(o.thoi_diem_dat AS DATE)) AS tong_so_ngay_hoat_dong,
            SUM(oi.so_luong) AS tong_so_luong_mon_dat
        FROM [Order] o
        INNER JOIN OrderItem oi ON o.order_id = oi.order_id
        WHERE 
            MONTH(o.thoi_diem_dat) = @Thang
            AND YEAR(o.thoi_diem_dat) = @Nam
            AND o.trang_thai IN ('HOAN_THANH')
    )
    
    SELECT 
        mas.mon_id,
        mas.ten_mon,
        mas.loai_mon,
        mas.gia,
        ISNULL(mas.so_lan_duoc_dat, 0) AS so_lan_duoc_dat,
        ISNULL(mas.tong_so_luong_dat, 0) AS tong_so_luong_dat,
        ISNULL(mas.so_don_hang_chua_mon, 0) AS so_don_hang_chua_mon,
        ISNULL(mas.so_ngay_co_dat, 0) AS so_ngay_co_dat,
        ROUND(ISNULL(mas.trung_binh_so_luong_moi_lan, 0), 2) AS trung_binh_so_luong_moi_lan,
        ISNULL(mas.tong_doanh_thu_mon, 0) AS tong_doanh_thu_mon,
        -- Tính tỷ lệ
        CASE 
            WHEN ts.tong_so_don_hang > 0 THEN 
                ROUND((CAST(ISNULL(mas.so_don_hang_chua_mon, 0) AS FLOAT) / ts.tong_so_don_hang) * 100, 2)
            ELSE 0 
        END AS ty_le_don_hang_chua_mon,
        CASE 
            WHEN ts.tong_so_luong_mon_dat > 0 THEN 
                ROUND((CAST(ISNULL(mas.tong_so_luong_dat, 0) AS FLOAT) / ts.tong_so_luong_mon_dat) * 100, 2)
            ELSE 0 
        END AS ty_le_so_luong_dat,
        -- Tính tần suất trung bình
        CASE 
            WHEN ts.tong_so_ngay_hoat_dong > 0 THEN 
                ROUND(CAST(ISNULL(mas.so_lan_duoc_dat, 0) AS FLOAT) / ts.tong_so_ngay_hoat_dong, 2)
            ELSE 0 
        END AS tan_suat_trung_binh_ngay,
        -- Xếp hạng
        ROW_NUMBER() OVER (ORDER BY ISNULL(mas.so_lan_duoc_dat, 0) DESC) AS xep_hang_so_lan_dat,
        ROW_NUMBER() OVER (ORDER BY ISNULL(mas.tong_so_luong_dat, 0) DESC) AS xep_hang_so_luong_dat,
        ROW_NUMBER() OVER (ORDER BY ISNULL(mas.tong_doanh_thu_mon, 0) DESC) AS xep_hang_doanh_thu
    FROM MonAnStats mas
    CROSS JOIN TotalStats ts
    ORDER BY ISNULL(mas.so_lan_duoc_dat, 0) DESC, ISNULL(mas.tong_so_luong_dat, 0) DESC;
    
END
GO

-- =========================================================
-- 2. sp_ThongKeMonAn_ChiTietTheoNgay - Thống kê chi tiết theo từng ngày
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_ThongKeMonAn_ChiTietTheoNgay
    @Thang INT,
    @Nam INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra tham số đầu vào
    IF @Thang < 1 OR @Thang > 12
    BEGIN
        RAISERROR('Tháng phải từ 1 đến 12', 16, 1);
        RETURN;
    END
    
    -- Tạo bảng tạm chứa tất cả ngày trong tháng
    ;WITH AllDaysInMonth AS (
        SELECT 
            DATEADD(DAY, number, DATEFROMPARTS(@Nam, @Thang, 1)) AS ngay
        FROM master.dbo.spt_values
        WHERE type = 'P' 
        AND number < DAY(EOMONTH(DATEFROMPARTS(@Nam, @Thang, 1)))
    ),
    DailyMonAnStats AS (
        -- Lấy dữ liệu món ăn theo ngày
        SELECT 
            CAST(o.thoi_diem_dat AS DATE) AS ngay,
            m.mon_id,
            m.ten_mon,
            m.loai_mon,
            COUNT(oi.order_item_id) AS so_lan_duoc_dat,
            SUM(oi.so_luong) AS tong_so_luong_dat,
            COUNT(DISTINCT o.order_id) AS so_don_hang_chua_mon,
            SUM(oi.so_luong * m.gia) AS doanh_thu_ngay
        FROM Mon m
        INNER JOIN OrderItem oi ON m.mon_id = oi.mon_id
        INNER JOIN [Order] o ON oi.order_id = o.order_id
        WHERE 
            MONTH(o.thoi_diem_dat) = @Thang
            AND YEAR(o.thoi_diem_dat) = @Nam
            AND o.trang_thai IN ('HOAN_THANH')
        GROUP BY 
            CAST(o.thoi_diem_dat AS DATE),
            m.mon_id, m.ten_mon, m.loai_mon
    ),
    AllMonAn AS (
        -- Lấy tất cả món ăn có trong hệ thống
        SELECT DISTINCT mon_id, ten_mon, loai_mon
        FROM Mon
    )
    
    -- Kết quả chi tiết theo ngày
    SELECT 
        adim.ngay,
        ama.mon_id,
        ama.ten_mon,
        ama.loai_mon,
        ISNULL(dmas.so_lan_duoc_dat, 0) AS so_lan_duoc_dat,
        ISNULL(dmas.tong_so_luong_dat, 0) AS tong_so_luong_dat,
        ISNULL(dmas.so_don_hang_chua_mon, 0) AS so_don_hang_chua_mon,
        ISNULL(dmas.doanh_thu_ngay, 0) AS doanh_thu_ngay
    FROM AllDaysInMonth adim
    CROSS JOIN AllMonAn ama
    LEFT JOIN DailyMonAnStats dmas ON adim.ngay = dmas.ngay AND ama.mon_id = dmas.mon_id
    ORDER BY adim.ngay, ama.ten_mon;
    
END
GO

-- =========================================================
-- 3. sp_ThongKeMonAn_TopMonAn - Top món ăn được đặt nhiều nhất
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_ThongKeMonAn_TopMonAn
    @Thang INT,
    @Nam INT,
    @TopN INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra tham số đầu vào
    IF @Thang < 1 OR @Thang > 12
    BEGIN
        RAISERROR('Tháng phải từ 1 đến 12', 16, 1);
        RETURN;
    END
    
    IF @TopN < 1 OR @TopN > 100
    BEGIN
        RAISERROR('TopN phải từ 1 đến 100', 16, 1);
        RETURN;
    END
    
    -- Top món ăn
    ;WITH MonAnStats AS (
        SELECT 
            m.mon_id,
            m.ten_mon,
            m.loai_mon,
            m.gia,
            COUNT(oi.order_item_id) AS so_lan_duoc_dat,
            SUM(oi.so_luong) AS tong_so_luong_dat,
            COUNT(DISTINCT o.order_id) AS so_don_hang_chua_mon,
            SUM(oi.so_luong * m.gia) AS tong_doanh_thu_mon,
            AVG(CAST(oi.so_luong AS FLOAT)) AS trung_binh_so_luong_moi_lan
        FROM Mon m
        INNER JOIN OrderItem oi ON m.mon_id = oi.mon_id
        INNER JOIN [Order] o ON oi.order_id = o.order_id
        WHERE 
            MONTH(o.thoi_diem_dat) = @Thang
            AND YEAR(o.thoi_diem_dat) = @Nam
            AND o.trang_thai IN ('HOAN_THANH')
        GROUP BY m.mon_id, m.ten_mon, m.loai_mon, m.gia
    )
    
    SELECT TOP (@TopN)
        mas.mon_id,
        mas.ten_mon,
        mas.loai_mon,
        mas.gia,
        mas.so_lan_duoc_dat,
        mas.tong_so_luong_dat,
        mas.so_don_hang_chua_mon,
        mas.tong_doanh_thu_mon,
        ROUND(mas.trung_binh_so_luong_moi_lan, 2) AS trung_binh_so_luong_moi_lan,
        -- Xếp hạng
        ROW_NUMBER() OVER (ORDER BY mas.so_lan_duoc_dat DESC) AS xep_hang_so_lan_dat,
        ROW_NUMBER() OVER (ORDER BY mas.tong_so_luong_dat DESC) AS xep_hang_so_luong_dat,
        ROW_NUMBER() OVER (ORDER BY mas.tong_doanh_thu_mon DESC) AS xep_hang_doanh_thu
    FROM MonAnStats mas
    ORDER BY mas.so_lan_duoc_dat DESC, mas.tong_so_luong_dat DESC;
    
END
GO

-- =========================================================
-- 4. sp_ThongKeMonAn_SoSanhThang - So sánh thống kê món ăn giữa các tháng
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_ThongKeMonAn_SoSanhThang
    @Thang1 INT,
    @Nam1 INT,
    @Thang2 INT,
    @Nam2 INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra tham số đầu vào
    IF @Thang1 < 1 OR @Thang1 > 12 OR @Thang2 < 1 OR @Thang2 > 12
    BEGIN
        RAISERROR('Tháng phải từ 1 đến 12', 16, 1);
        RETURN;
    END
    
    -- Thống kê tháng 1
    ;WITH Month1Stats AS (
        SELECT 
            m.mon_id,
            m.ten_mon,
            m.loai_mon,
            COUNT(oi.order_item_id) AS so_lan_duoc_dat,
            SUM(oi.so_luong) AS tong_so_luong_dat,
            COUNT(DISTINCT o.order_id) AS so_don_hang_chua_mon,
            SUM(oi.so_luong * m.gia) AS tong_doanh_thu_mon
        FROM Mon m
        LEFT JOIN OrderItem oi ON m.mon_id = oi.mon_id
        LEFT JOIN [Order] o ON oi.order_id = o.order_id
            AND MONTH(o.thoi_diem_dat) = @Thang1
            AND YEAR(o.thoi_diem_dat) = @Nam1
            AND o.trang_thai IN ('HOAN_THANH')
        GROUP BY m.mon_id, m.ten_mon, m.loai_mon
    ),
    -- Thống kê tháng 2
    Month2Stats AS (
        SELECT 
            m.mon_id,
            m.ten_mon,
            m.loai_mon,
            COUNT(oi.order_item_id) AS so_lan_duoc_dat,
            SUM(oi.so_luong) AS tong_so_luong_dat,
            COUNT(DISTINCT o.order_id) AS so_don_hang_chua_mon,
            SUM(oi.so_luong * m.gia) AS tong_doanh_thu_mon
        FROM Mon m
        LEFT JOIN OrderItem oi ON m.mon_id = oi.mon_id
        LEFT JOIN [Order] o ON oi.order_id = o.order_id
            AND MONTH(o.thoi_diem_dat) = @Thang2
            AND YEAR(o.thoi_diem_dat) = @Nam2
            AND o.trang_thai IN ('HOAN_THANH')
        GROUP BY m.mon_id, m.ten_mon, m.loai_mon
    ),
    AllMonAn AS (
        SELECT DISTINCT mon_id, ten_mon, loai_mon FROM Mon
    )
    
    SELECT 
        ama.mon_id,
        ama.ten_mon,
        ama.loai_mon,
        -- Tháng 1
        ISNULL(m1.so_lan_duoc_dat, 0) AS thang1_so_lan_duoc_dat,
        ISNULL(m1.tong_so_luong_dat, 0) AS thang1_tong_so_luong_dat,
        ISNULL(m1.so_don_hang_chua_mon, 0) AS thang1_so_don_hang_chua_mon,
        ISNULL(m1.tong_doanh_thu_mon, 0) AS thang1_tong_doanh_thu_mon,
        -- Tháng 2
        ISNULL(m2.so_lan_duoc_dat, 0) AS thang2_so_lan_duoc_dat,
        ISNULL(m2.tong_so_luong_dat, 0) AS thang2_tong_so_luong_dat,
        ISNULL(m2.so_don_hang_chua_mon, 0) AS thang2_so_don_hang_chua_mon,
        ISNULL(m2.tong_doanh_thu_mon, 0) AS thang2_tong_doanh_thu_mon,
        -- So sánh
        ISNULL(m2.so_lan_duoc_dat, 0) - ISNULL(m1.so_lan_duoc_dat, 0) AS chenh_lech_so_lan_dat,
        ISNULL(m2.tong_so_luong_dat, 0) - ISNULL(m1.tong_so_luong_dat, 0) AS chenh_lech_so_luong_dat,
        ISNULL(m2.so_don_hang_chua_mon, 0) - ISNULL(m1.so_don_hang_chua_mon, 0) AS chenh_lech_don_hang,
        ISNULL(m2.tong_doanh_thu_mon, 0) - ISNULL(m1.tong_doanh_thu_mon, 0) AS chenh_lech_doanh_thu,
        -- Tỷ lệ thay đổi (%)
        CASE 
            WHEN ISNULL(m1.so_lan_duoc_dat, 0) = 0 THEN 
                CASE WHEN ISNULL(m2.so_lan_duoc_dat, 0) > 0 THEN 100 ELSE 0 END
            ELSE ROUND(((CAST(ISNULL(m2.so_lan_duoc_dat, 0) AS FLOAT) - ISNULL(m1.so_lan_duoc_dat, 0)) / ISNULL(m1.so_lan_duoc_dat, 1)) * 100, 2)
        END AS ty_le_thay_doi_so_lan_dat,
        CASE 
            WHEN ISNULL(m1.tong_so_luong_dat, 0) = 0 THEN 
                CASE WHEN ISNULL(m2.tong_so_luong_dat, 0) > 0 THEN 100 ELSE 0 END
            ELSE ROUND(((CAST(ISNULL(m2.tong_so_luong_dat, 0) AS FLOAT) - ISNULL(m1.tong_so_luong_dat, 0)) / ISNULL(m1.tong_so_luong_dat, 1)) * 100, 2)
        END AS ty_le_thay_doi_so_luong_dat
    FROM AllMonAn ama
    LEFT JOIN Month1Stats m1 ON ama.mon_id = m1.mon_id
    LEFT JOIN Month2Stats m2 ON ama.mon_id = m2.mon_id
    WHERE ISNULL(m1.so_lan_duoc_dat, 0) > 0 OR ISNULL(m2.so_lan_duoc_dat, 0) > 0
    ORDER BY ISNULL(m2.so_lan_duoc_dat, 0) DESC, ISNULL(m1.so_lan_duoc_dat, 0) DESC;
    
END
GO

-- =========================================================
-- 5. sp_ThongKeMonAn_TheoLoaiMon - Thống kê theo loại món
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_ThongKeMonAn_TheoLoaiMon
    @Thang INT,
    @Nam INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra tham số đầu vào
    IF @Thang < 1 OR @Thang > 12
    BEGIN
        RAISERROR('Tháng phải từ 1 đến 12', 16, 1);
        RETURN;
    END
    
    -- Thống kê theo loại món
    ;WITH LoaiMonStats AS (
        SELECT 
            m.loai_mon,
            COUNT(DISTINCT m.mon_id) AS so_mon_trong_loai,
            COUNT(oi.order_item_id) AS so_lan_duoc_dat,
            SUM(oi.so_luong) AS tong_so_luong_dat,
            COUNT(DISTINCT o.order_id) AS so_don_hang_chua_loai,
            SUM(oi.so_luong * m.gia) AS tong_doanh_thu_loai,
            AVG(CAST(oi.so_luong AS FLOAT)) AS trung_binh_so_luong_moi_lan
        FROM Mon m
        LEFT JOIN OrderItem oi ON m.mon_id = oi.mon_id
        LEFT JOIN [Order] o ON oi.order_id = o.order_id
            AND MONTH(o.thoi_diem_dat) = @Thang
            AND YEAR(o.thoi_diem_dat) = @Nam
            AND o.trang_thai IN ('HOAN_THANH')
        GROUP BY m.loai_mon
    ),
    TotalStats AS (
        SELECT 
            COUNT(DISTINCT o.order_id) AS tong_so_don_hang,
            SUM(oi.so_luong) AS tong_so_luong_mon_dat,
            SUM(oi.so_luong * m.gia) AS tong_doanh_thu
        FROM [Order] o
        INNER JOIN OrderItem oi ON o.order_id = oi.order_id
        INNER JOIN Mon m ON oi.mon_id = m.mon_id
        WHERE 
            MONTH(o.thoi_diem_dat) = @Thang
            AND YEAR(o.thoi_diem_dat) = @Nam
            AND o.trang_thai IN ('HOAN_THANH')
    )
    
    SELECT 
        lms.loai_mon,
        lms.so_mon_trong_loai,
        ISNULL(lms.so_lan_duoc_dat, 0) AS so_lan_duoc_dat,
        ISNULL(lms.tong_so_luong_dat, 0) AS tong_so_luong_dat,
        ISNULL(lms.so_don_hang_chua_loai, 0) AS so_don_hang_chua_loai,
        ISNULL(lms.tong_doanh_thu_loai, 0) AS tong_doanh_thu_loai,
        ROUND(ISNULL(lms.trung_binh_so_luong_moi_lan, 0), 2) AS trung_binh_so_luong_moi_lan,
        -- Tính tỷ lệ
        CASE 
            WHEN ts.tong_so_don_hang > 0 THEN 
                ROUND((CAST(ISNULL(lms.so_don_hang_chua_loai, 0) AS FLOAT) / ts.tong_so_don_hang) * 100, 2)
            ELSE 0 
        END AS ty_le_don_hang_chua_loai,
        CASE 
            WHEN ts.tong_so_luong_mon_dat > 0 THEN 
                ROUND((CAST(ISNULL(lms.tong_so_luong_dat, 0) AS FLOAT) / ts.tong_so_luong_mon_dat) * 100, 2)
            ELSE 0 
        END AS ty_le_so_luong_dat,
        CASE 
            WHEN ts.tong_doanh_thu > 0 THEN 
                ROUND((CAST(ISNULL(lms.tong_doanh_thu_loai, 0) AS FLOAT) / ts.tong_doanh_thu) * 100, 2)
            ELSE 0 
        END AS ty_le_doanh_thu,
        -- Xếp hạng
        ROW_NUMBER() OVER (ORDER BY ISNULL(lms.so_lan_duoc_dat, 0) DESC) AS xep_hang_so_lan_dat,
        ROW_NUMBER() OVER (ORDER BY ISNULL(lms.tong_doanh_thu_loai, 0) DESC) AS xep_hang_doanh_thu
    FROM LoaiMonStats lms
    CROSS JOIN TotalStats ts
    ORDER BY ISNULL(lms.so_lan_duoc_dat, 0) DESC, ISNULL(lms.tong_doanh_thu_loai, 0) DESC;
    
END
GO

PRINT 'Đã tạo thành công các stored procedures cho thống kê món ăn!'
PRINT 'Các stored procedures đã tạo:'
PRINT '1. sp_ThongKeMonAn_TanSuatTheoThang - Thống kê tần suất món ăn theo tháng'
PRINT '2. sp_ThongKeMonAn_ChiTietTheoNgay - Thống kê chi tiết theo từng ngày'
PRINT '3. sp_ThongKeMonAn_TopMonAn - Top món ăn được đặt nhiều nhất'
PRINT '4. sp_ThongKeMonAn_SoSanhThang - So sánh thống kê món ăn giữa các tháng'
PRINT '5. sp_ThongKeMonAn_TheoLoaiMon - Thống kê theo loại món'
