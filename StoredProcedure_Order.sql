-- =============================================
-- Stored Procedures for All Orders Management
-- =============================================

-- 1. Get All Orders with Pagination and Search
-- =============================================
CREATE OR ALTER PROCEDURE sp_Order_GetOrdersWithPagination
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchKeyword NVARCHAR(100) = NULL,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @FilterType NVARCHAR(20) = NULL, -- 'w', 'm'
    @Status NVARCHAR(20) = NULL,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Calculate date range based on filter type
    DECLARE @StartDate DATETIME, @EndDate DATETIME;
    
    IF @FilterType = 'w'
    BEGIN
       -- weekday duoc danh so tu 1-6 (CN->T7), de tinh ra thu 2, lay ngay hien tai tru di 2 de xem lui ve may ngay.
        SET @StartDate = DATEADD(DAY, -(DATEPART(WEEKDAY, GETDATE()) - 2), CAST(GETDATE() AS DATE));
        SET @EndDate = DATEADD(DAY, 6, @StartDate);
    END
    ELSE IF @FilterType = 'm'
    BEGIN
        SET @StartDate = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
        SET @EndDate = EOMONTH(GETDATE());
    END
    ELSE IF @FromDate IS NOT NULL AND @ToDate IS NOT NULL
    BEGIN
        SET @StartDate = @FromDate;
        SET @EndDate = @ToDate;
    END
    
    -- Get total count
    SELECT @TotalCount = COUNT(1)
    FROM [Order] o
    LEFT JOIN KhachHang kh ON o.kh_id = kh.kh_id
    LEFT JOIN BanAn ba ON o.ban_id = ba.ban_id
    WHERE 
        (@SearchKeyword IS NULL OR 
         o.order_id LIKE '%' + @SearchKeyword + '%' OR
         kh.ho_ten COLLATE SQL_Latin1_General_CP1_CI_AI LIKE '%' + @SearchKeyword + '%' COLLATE SQL_Latin1_General_CP1_CI_AI OR
         kh.so_dien_thoai LIKE '%' + @SearchKeyword + '%' OR
         ba.so_hieu LIKE '%' + @SearchKeyword + '%')
    AND (@StartDate IS NULL OR o.thoi_diem_dat >= @StartDate)
    AND (@EndDate IS NULL OR o.thoi_diem_dat <= @EndDate)
    AND (@Status IS NULL OR o.trang_thai = @Status);
    
    -- Get paginated results
    SELECT 
        o.order_id,
        o.kh_id,
        o.ban_id,
        o.la_mang_ve,
        o.trang_thai,
        o.so_khach,
        o.thoi_diem_dat,
        o.tong_tien,
        -- Customer info
        kh.ho_ten AS khach_hang_ten,
        kh.so_dien_thoai AS khach_hang_sdt,
        -- Table info
        ba.so_hieu AS ban_so_hieu,
        lb.suc_chua AS ban_suc_chua,
        -- Order statistics
        (SELECT COUNT(1) FROM OrderItem oi WHERE oi.order_id = o.order_id) AS so_mon,
        (SELECT ISNULL(SUM(oi.so_luong), 0) FROM OrderItem oi WHERE oi.order_id = o.order_id) AS tong_so_luong
    FROM [Order] o
    LEFT JOIN KhachHang kh ON o.kh_id = kh.kh_id
    LEFT JOIN BanAn ba ON o.ban_id = ba.ban_id
    LEFT JOIN LoaiBan lb ON ba.loai_ban_id = lb.loai_ban_id
    WHERE 
        (@SearchKeyword IS NULL OR 
         o.order_id LIKE '%' + @SearchKeyword + '%' OR
         kh.ho_ten COLLATE SQL_Latin1_General_CP1_CI_AI LIKE '%' + @SearchKeyword + '%' COLLATE SQL_Latin1_General_CP1_CI_AI OR
         kh.so_dien_thoai LIKE '%' + @SearchKeyword + '%' OR
         ba.so_hieu LIKE '%' + @SearchKeyword + '%')
    AND (@StartDate IS NULL OR o.thoi_diem_dat >= @StartDate)
    AND (@EndDate IS NULL OR o.thoi_diem_dat < @EndDate + 1)
    AND (@Status IS NULL OR o.trang_thai = @Status)
    ORDER BY o.thoi_diem_dat DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- 2. Get Order Details with Items
-- =============================================
CREATE OR ALTER PROCEDURE sp_Order_GetOrderDetails
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get order basic info
    SELECT 
        o.order_id,
        o.kh_id,
        o.ban_id,
        o.la_mang_ve,
        o.trang_thai,
        o.so_khach,
        o.thoi_diem_dat,
        o.tong_tien,
        -- Customer info
        kh.ho_ten AS khach_hang_ten,
        kh.so_dien_thoai AS khach_hang_sdt,
        -- Table info
        ba.so_hieu AS ban_so_hieu,
        lb.suc_chua AS ban_suc_chua
    FROM [Order] o
    LEFT JOIN KhachHang kh ON o.kh_id = kh.kh_id
    LEFT JOIN BanAn ba ON o.ban_id = ba.ban_id
    LEFT JOIN LoaiBan lb ON ba.loai_ban_id = lb.loai_ban_id
    WHERE o.order_id = @OrderId;
    
    -- Get order items
    SELECT 
        oi.order_item_id,
        oi.order_id,
        oi.so_luong,
        m.gia,
        m.ten_mon,
        m.loai_mon
    FROM OrderItem oi
    INNER JOIN Mon m ON oi.mon_id = m.mon_id
    WHERE oi.order_id = @OrderId
    ORDER BY oi.order_item_id;
    
    -- Get order history
    SELECT 
        lsh.lich_su_id,
        lsh.order_id,
        lsh.buoc_id,
        lsh.nv_id,
        lsh.thoi_diem_bat_dau,
        lsh.thoi_diem_ket_thuc,
        lsh.trang_thai,
        lsh.ghi_chu,
        bx.ten_buoc,
        nv.ho_ten AS nhan_vien_ten
    FROM LichSuThucHien lsh
    LEFT JOIN BuocXuLy bx ON lsh.buoc_id = bx.buoc_id
    LEFT JOIN NhanVien nv ON lsh.nv_id = nv.nv_id
    WHERE lsh.order_id = @OrderId
    ORDER BY lsh.thoi_diem_bat_dau;
END
GO

-- 3. Cancel Order
-- =============================================
CREATE OR ALTER PROCEDURE sp_Order_CancelOrder
    @OrderId INT,
    @EmployeeId INT,
    @Reason NVARCHAR(500) = NULL,
    @Success BIT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if order exists and can be cancelled
        IF NOT EXISTS (SELECT 1 FROM [Order] WHERE order_id = @OrderId)
        BEGIN
            SET @Success = 0;
            SET @Message = N'Đơn hàng không tồn tại.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        DECLARE @CurrentStatus NVARCHAR(20);
        SELECT @CurrentStatus = trang_thai FROM [Order] WHERE order_id = @OrderId;
        
        IF @CurrentStatus = 'HOAN_THANH'
        BEGIN
            SET @Success = 0;
            SET @Message = N'Không thể hủy đơn hàng đã hoàn thành.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Update order status to cancelled
        UPDATE [Order]
        SET trang_thai = 'DA_HUY'
        WHERE order_id = @OrderId;                
        
        -- Add cancellation record to history
        INSERT INTO LichSuThucHien (
            order_id, 
            buoc_id, 
            nv_id, 
            thoi_diem_bat_dau, 
            trang_thai, 
            ghi_chu
        )
		SELECT TOP 1 
			@OrderId, 
			ISNULL(buoc_id, 1), 
			@EmployeeId, 
			GETDATE(), 
			'DA_HUY', 
			ISNULL(@Reason, N'Đơn hàng đã được hủy bởi nhân viên')
		FROM LichSuThucHien WHERE order_id = @OrderId ORDER BY lich_su_id DESC;

        SET @Success = 1;
        SET @Message = N'Đơn hàng đã được hủy thành công.';
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SET @Success = 0;
        SET @Message = N'Lỗi khi hủy đơn hàng: ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- 4. Get Order Statistics
-- =============================================
CREATE OR ALTER PROCEDURE sp_Order_GetStatistics
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @FilterType NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Calculate date range based on filter type
    DECLARE @StartDate DATETIME, @EndDate DATETIME;
    
    IF @FilterType = 'w'
    BEGIN
        SET @StartDate = DATEADD(DAY, -(DATEPART(WEEKDAY, GETDATE()) - 2), CAST(GETDATE() AS DATE));
        SET @EndDate = DATEADD(DAY, 6, @StartDate);
    END
    ELSE IF @FilterType = 'm'
    BEGIN
        SET @StartDate = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
        SET @EndDate = EOMONTH(GETDATE());
    END
    ELSE IF @FromDate IS NOT NULL AND @ToDate IS NOT NULL
    BEGIN
        SET @StartDate = @FromDate;
        SET @EndDate = @ToDate;
    END
    
    -- Get statistics
    SELECT 
        COUNT(*) AS total_orders,
        SUM(CASE WHEN trang_thai = 'CHUA_HOAN_THANH' THEN 1 ELSE 0 END) AS pending_orders,
        SUM(CASE WHEN trang_thai = 'CHUA_HOAN_THANH' THEN 1 ELSE 0 END) AS confirmed_orders,
        SUM(CASE WHEN trang_thai = 'CHUA_HOAN_THANH' THEN 1 ELSE 0 END) AS in_progress_orders,
        SUM(CASE WHEN trang_thai = 'HOAN_THANH' THEN 1 ELSE 0 END) AS completed_orders,
        SUM(CASE WHEN trang_thai = 'CHUA_HOAN_THANH' THEN 1 ELSE 0 END) AS cancelled_orders,
        SUM(CASE WHEN la_mang_ve = 1 THEN 1 ELSE 0 END) AS takeaway_orders,
        SUM(CASE WHEN la_mang_ve = 0 THEN 1 ELSE 0 END) AS dine_in_orders,
        ISNULL(SUM(tong_tien), 0) AS total_revenue
    FROM [Order]
    WHERE 
        (@StartDate IS NULL OR thoi_diem_dat >= @StartDate)
    AND (@EndDate IS NULL OR thoi_diem_dat < @EndDate + 1);
END
GO
