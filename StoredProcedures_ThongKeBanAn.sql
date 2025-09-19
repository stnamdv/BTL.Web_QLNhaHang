/* =========================================================
   STORED PROCEDURES CHO THỐNG KÊ BÀN ĂN
   Thống kê số lượng từng loại bàn được khách ngồi trung bình theo ngày trong tháng
   ========================================================= */

USE NhaHang
GO

-- =========================================================
-- 1. sp_ThongKeBanAn_TrungBinhTheoNgay - Thống kê bàn ăn theo ngày trong tháng
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_ThongKeBanAn_TrungBinhTheoNgay
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
    
    -- Tạo bảng tạm để lưu kết quả
    ;WITH DailyTableUsage AS (
        -- Lấy dữ liệu sử dụng bàn theo ngày
        SELECT 
            CAST(o.thoi_diem_dat AS DATE) AS ngay,
            lb.suc_chua,
            COUNT(DISTINCT o.ban_id) AS so_ban_su_dung,
            COUNT(o.order_id) AS so_lan_dat_ban,
            SUM(o.so_khach) AS tong_so_khach
        FROM [Order] o
        INNER JOIN BanAn ba ON o.ban_id = ba.ban_id
        INNER JOIN LoaiBan lb ON ba.loai_ban_id = lb.loai_ban_id
        WHERE 
            MONTH(o.thoi_diem_dat) = @Thang
            AND YEAR(o.thoi_diem_dat) = @Nam
            AND o.trang_thai IN ('HOAN_THANH') -- Chỉ tính các đơn đã hoàn thành
        GROUP BY 
            CAST(o.thoi_diem_dat AS DATE),
            lb.suc_chua
    ),
    MonthlyStats AS (
        -- Tính thống kê trung bình theo loại bàn
        SELECT 
            suc_chua,
            COUNT(DISTINCT ngay) AS so_ngay_co_don,
            AVG(CAST(so_ban_su_dung AS FLOAT)) AS trung_binh_ban_su_dung_ngay,
            AVG(CAST(so_lan_dat_ban AS FLOAT)) AS trung_binh_lan_dat_ban_ngay,
            AVG(CAST(tong_so_khach AS FLOAT)) AS trung_binh_so_khach_ngay,
            SUM(so_ban_su_dung) AS tong_ban_su_dung_thang,
            SUM(so_lan_dat_ban) AS tong_lan_dat_ban_thang,
            SUM(tong_so_khach) AS tong_so_khach_thang
        FROM DailyTableUsage
        GROUP BY suc_chua
    ),
    AllTableTypes AS (
        -- Lấy tất cả loại bàn có trong hệ thống
        SELECT DISTINCT suc_chua
        FROM LoaiBan
    )
    
    -- Kết quả cuối cùng
    SELECT 
        att.suc_chua AS suc_chua_ban,
        CONCAT(N'Bàn ', att.suc_chua, N' người') ten_loai_ban,
        ISNULL(ms.so_ngay_co_don, 0) AS so_ngay_co_don_trong_thang,
        ISNULL(ROUND(ms.trung_binh_ban_su_dung_ngay, 2), 0) AS trung_binh_ban_su_dung_ngay,
        ISNULL(ROUND(ms.trung_binh_lan_dat_ban_ngay, 2), 0) AS trung_binh_lan_dat_ban_ngay,
        ISNULL(ROUND(ms.trung_binh_so_khach_ngay, 2), 0) AS trung_binh_so_khach_ngay,
        ISNULL(ms.tong_ban_su_dung_thang, 0) AS tong_ban_su_dung_thang,
        ISNULL(ms.tong_lan_dat_ban_thang, 0) AS tong_lan_dat_ban_thang,
        ISNULL(ms.tong_so_khach_thang, 0) AS tong_so_khach_thang,
        -- Tính tỷ lệ sử dụng (so với tổng số bàn có sẵn)
        ISNULL(ROUND(
            (ms.trung_binh_ban_su_dung_ngay / 
             (SELECT so_luong FROM LoaiBan WHERE suc_chua = att.suc_chua)) * 100, 2
        ), 0) AS ty_le_su_dung_trung_binh_ngay
    FROM AllTableTypes att
    LEFT JOIN MonthlyStats ms ON att.suc_chua = ms.suc_chua
    ORDER BY att.suc_chua;
    
END
GO

-- =========================================================
-- 2. sp_ThongKeBanAn_ChiTietTheoNgay - Thống kê chi tiết theo từng ngày
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_ThongKeBanAn_ChiTietTheoNgay
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
    DailyTableUsage AS (
        -- Lấy dữ liệu sử dụng bàn theo ngày
        SELECT 
            CAST(o.thoi_diem_dat AS DATE) AS ngay,
            lb.suc_chua,
            COUNT(DISTINCT o.ban_id) AS so_ban_su_dung,
            COUNT(o.order_id) AS so_lan_dat_ban,
            SUM(o.so_khach) AS tong_so_khach
        FROM [Order] o
        INNER JOIN BanAn ba ON o.ban_id = ba.ban_id
        INNER JOIN LoaiBan lb ON ba.loai_ban_id = lb.loai_ban_id
        WHERE 
            MONTH(o.thoi_diem_dat) = @Thang
            AND YEAR(o.thoi_diem_dat) = @Nam
            AND o.trang_thai IN ('HOAN_THANH')
        GROUP BY 
            CAST(o.thoi_diem_dat AS DATE),
            lb.suc_chua
    ),
    AllTableTypes AS (
        -- Lấy tất cả loại bàn có trong hệ thống
        SELECT DISTINCT suc_chua
        FROM LoaiBan
    )
    
    -- Kết quả chi tiết theo ngày
    SELECT 
        adim.ngay,
        att.suc_chua AS suc_chua_ban,
        CONCAT(N'Bàn ', att.suc_chua, N' người') ten_loai_ban,
        ISNULL(dtu.so_ban_su_dung, 0) AS so_ban_su_dung,
        ISNULL(dtu.so_lan_dat_ban, 0) AS so_lan_dat_ban,
        ISNULL(dtu.tong_so_khach, 0) AS tong_so_khach,
        -- Tính tỷ lệ sử dụng trong ngày
        ISNULL(ROUND(
            (dtu.so_ban_su_dung / 
             (SELECT so_luong FROM LoaiBan WHERE suc_chua = att.suc_chua)) * 100, 2
        ), 0) AS ty_le_su_dung_ngay
    FROM AllDaysInMonth adim
    CROSS JOIN AllTableTypes att
    LEFT JOIN DailyTableUsage dtu ON adim.ngay = dtu.ngay AND att.suc_chua = dtu.suc_chua
    ORDER BY adim.ngay, att.suc_chua;
    
END
GO

-- =========================================================
-- 3. sp_ThongKeBanAn_TongHop - Thống kê tổng hợp theo tháng
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_ThongKeBanAn_TongHop
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
    
    -- Thống kê tổng hợp
    ;WITH MonthlySummary AS (
        SELECT 
            lb.suc_chua,
            lb.so_luong AS tong_so_ban_co_san,
            COUNT(DISTINCT o.ban_id) AS so_ban_da_su_dung,
            COUNT(o.order_id) AS tong_so_don_hang,
            SUM(o.so_khach) AS tong_so_khach,
            AVG(CAST(o.so_khach AS FLOAT)) AS trung_binh_khach_moi_don,
            COUNT(DISTINCT CAST(o.thoi_diem_dat AS DATE)) AS so_ngay_hoat_dong
        FROM LoaiBan lb
        LEFT JOIN BanAn ba ON lb.loai_ban_id = ba.loai_ban_id
        LEFT JOIN [Order] o ON ba.ban_id = o.ban_id
            AND MONTH(o.thoi_diem_dat) = @Thang
            AND YEAR(o.thoi_diem_dat) = @Nam
            AND o.trang_thai IN ('HOAN_THANH')
        GROUP BY lb.suc_chua, lb.so_luong
    )
    
    SELECT 
        ms.suc_chua AS suc_chua_ban,
        CONCAT(N'Bàn ', ms.suc_chua, N' người') ten_loai_ban,
        ms.tong_so_ban_co_san,
        ms.so_ban_da_su_dung,
        ms.tong_so_don_hang,
        ms.tong_so_khach,
        ROUND(ms.trung_binh_khach_moi_don, 2) AS trung_binh_khach_moi_don,
        ms.so_ngay_hoat_dong,
        -- Tính các tỷ lệ
        ROUND((CAST(ms.so_ban_da_su_dung AS FLOAT) / ms.tong_so_ban_co_san) * 100, 2) AS ty_le_ban_da_su_dung,
        ROUND((CAST(ms.tong_so_don_hang AS FLOAT) / ms.so_ngay_hoat_dong), 2) AS trung_binh_don_hang_ngay,
        ROUND((CAST(ms.tong_so_khach AS FLOAT) / ms.so_ngay_hoat_dong), 2) AS trung_binh_khach_ngay
    FROM MonthlySummary ms
    ORDER BY ms.suc_chua;
    
END
GO

-- =========================================================
-- 4. sp_ThongKeBanAn_SoSanhThang - So sánh thống kê giữa các tháng
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_ThongKeBanAn_SoSanhThang
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
            lb.suc_chua,
            COUNT(DISTINCT o.ban_id) AS so_ban_su_dung,
            COUNT(o.order_id) AS so_don_hang,
            SUM(o.so_khach) AS tong_khach,
            AVG(CAST(o.so_khach AS FLOAT)) AS trung_binh_khach_don
        FROM LoaiBan lb
        LEFT JOIN BanAn ba ON lb.loai_ban_id = ba.loai_ban_id
        LEFT JOIN [Order] o ON ba.ban_id = o.ban_id
            AND MONTH(o.thoi_diem_dat) = @Thang1
            AND YEAR(o.thoi_diem_dat) = @Nam1
            AND o.trang_thai IN ('HOAN_THANH')
        GROUP BY lb.suc_chua
    ),
    -- Thống kê tháng 2
    Month2Stats AS (
        SELECT 
            lb.suc_chua,
            COUNT(DISTINCT o.ban_id) AS so_ban_su_dung,
            COUNT(o.order_id) AS so_don_hang,
            SUM(o.so_khach) AS tong_khach,
            AVG(CAST(o.so_khach AS FLOAT)) AS trung_binh_khach_don
        FROM LoaiBan lb
        LEFT JOIN BanAn ba ON lb.loai_ban_id = ba.loai_ban_id
        LEFT JOIN [Order] o ON ba.ban_id = o.ban_id
            AND MONTH(o.thoi_diem_dat) = @Thang2
            AND YEAR(o.thoi_diem_dat) = @Nam2
            AND o.trang_thai IN ('HOAN_THANH')
        GROUP BY lb.suc_chua
    ),
    AllTableTypes AS (
        SELECT DISTINCT suc_chua FROM LoaiBan
    )
    
    SELECT 
        att.suc_chua AS suc_chua_ban,
        CONCAT(N'Bàn ', att.suc_chua, N' người') ten_loai_ban,
        -- Tháng 1
        ISNULL(m1.so_ban_su_dung, 0) AS thang1_so_ban_su_dung,
        ISNULL(m1.so_don_hang, 0) AS thang1_so_don_hang,
        ISNULL(m1.tong_khach, 0) AS thang1_tong_khach,
        ROUND(ISNULL(m1.trung_binh_khach_don, 0), 2) AS thang1_trung_binh_khach_don,
        -- Tháng 2
        ISNULL(m2.so_ban_su_dung, 0) AS thang2_so_ban_su_dung,
        ISNULL(m2.so_don_hang, 0) AS thang2_so_don_hang,
        ISNULL(m2.tong_khach, 0) AS thang2_tong_khach,
        ROUND(ISNULL(m2.trung_binh_khach_don, 0), 2) AS thang2_trung_binh_khach_don,
        -- So sánh
        ISNULL(m2.so_ban_su_dung, 0) - ISNULL(m1.so_ban_su_dung, 0) AS chenh_lech_ban_su_dung,
        ISNULL(m2.so_don_hang, 0) - ISNULL(m1.so_don_hang, 0) AS chenh_lech_don_hang,
        ISNULL(m2.tong_khach, 0) - ISNULL(m1.tong_khach, 0) AS chenh_lech_tong_khach,
        -- Tỷ lệ thay đổi (%)
        CASE 
            WHEN ISNULL(m1.so_ban_su_dung, 0) = 0 THEN 
                CASE WHEN ISNULL(m2.so_ban_su_dung, 0) > 0 THEN 100 ELSE 0 END
            ELSE ROUND(((CAST(ISNULL(m2.so_ban_su_dung, 0) AS FLOAT) - ISNULL(m1.so_ban_su_dung, 0)) / ISNULL(m1.so_ban_su_dung, 1)) * 100, 2)
        END AS ty_le_thay_doi_ban_su_dung,
        CASE 
            WHEN ISNULL(m1.so_don_hang, 0) = 0 THEN 
                CASE WHEN ISNULL(m2.so_don_hang, 0) > 0 THEN 100 ELSE 0 END
            ELSE ROUND(((CAST(ISNULL(m2.so_don_hang, 0) AS FLOAT) - ISNULL(m1.so_don_hang, 0)) / ISNULL(m1.so_don_hang, 1)) * 100, 2)
        END AS ty_le_thay_doi_don_hang
    FROM AllTableTypes att
    LEFT JOIN Month1Stats m1 ON att.suc_chua = m1.suc_chua
    LEFT JOIN Month2Stats m2 ON att.suc_chua = m2.suc_chua
    ORDER BY att.suc_chua;
    
END
GO