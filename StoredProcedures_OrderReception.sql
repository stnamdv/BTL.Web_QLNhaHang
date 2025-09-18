/* =========================================================
   STORED PROCEDURES CHO BƯỚC 1: TIẾP NHẬN ĐƠN HÀNG
   Xử lý logic tiếp nhận đơn hàng cho cả ăn tại chỗ và mang về
   ========================================================= */

USE NhaHang
GO

-- =========================================================
-- 1. sp_OrderReception_ConfirmOrder - Tiếp nhận đơn hàng
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_OrderReception_ConfirmOrder
    @OrderId INT,
    @EmployeeId INT,
    @Success BIT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Khởi tạo biến
        SET @Success = 0;
        SET @Message = '';
        
        -- Validation: Kiểm tra đơn hàng có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.[Order] WHERE order_id = @OrderId)
        BEGIN
            SET @Message = N'Đơn hàng không tồn tại.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation: Kiểm tra trạng thái đơn hàng
        IF NOT EXISTS (SELECT 1 FROM dbo.[Order] WHERE order_id = @OrderId AND trang_thai = 'pending')
        BEGIN
            SET @Message = N'Đơn hàng đã được xử lý hoặc không ở trạng thái chờ xử lý.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation: Kiểm tra nhân viên có tồn tại và hoạt động không
        IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien WHERE nv_id = @EmployeeId AND trang_thai = 'ACTIVE')
        BEGIN
            SET @Message = N'Nhân viên không tồn tại hoặc không hoạt động.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation: Kiểm tra đơn hàng có OrderItem không
        IF NOT EXISTS (SELECT 1 FROM dbo.OrderItem WHERE order_id = @OrderId)
        BEGIN
            SET @Message = N'Đơn hàng không có món ăn nào.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Lấy thông tin đơn hàng
        DECLARE @LaMangVe BIT, @BanId INT, @SoKhach INT, @KhId INT;
        SELECT @LaMangVe = la_mang_ve, @BanId = ban_id, @SoKhach = so_khach, @KhId = kh_id
        FROM dbo.[Order] WHERE order_id = @OrderId;
        
        -- Xử lý theo 2 cases
        IF @LaMangVe = 0 -- Ăn tại chỗ
        BEGIN
            -- Validation: Kiểm tra bàn có được chỉ định không
            IF @BanId IS NULL
            BEGIN
                SET @Message = N'Đơn hàng ăn tại chỗ phải có bàn được chỉ định.';
                ROLLBACK TRANSACTION;
                RETURN;
            END
            
            -- Validation: Kiểm tra số khách có được chỉ định không
            IF @SoKhach IS NULL OR @SoKhach <= 0
            BEGIN
                SET @Message = N'Đơn hàng ăn tại chỗ phải có số khách hợp lệ.';
                ROLLBACK TRANSACTION;
                RETURN;
            END
            
            -- Validation: Kiểm tra bàn có tồn tại không
            IF NOT EXISTS (SELECT 1 FROM dbo.BanAn WHERE ban_id = @BanId)
            BEGIN
                SET @Message = N'Bàn ăn không tồn tại.';
                ROLLBACK TRANSACTION;
                RETURN;
            END
            
            -- Validation: Kiểm tra số khách có phù hợp với sức chứa bàn không
            IF EXISTS (
                SELECT 1 FROM dbo.BanAn b
                JOIN dbo.LoaiBan lb ON b.loai_ban_id = lb.loai_ban_id
                WHERE b.ban_id = @BanId AND @SoKhach > lb.suc_chua
            )
            BEGIN
                DECLARE @SucChua INT;
                SELECT @SucChua = lb.suc_chua
                FROM dbo.BanAn b
                JOIN dbo.LoaiBan lb ON b.loai_ban_id = lb.loai_ban_id
                WHERE b.ban_id = @BanId;
                
                SET @Message = N'Số khách (' + CAST(@SoKhach AS NVARCHAR(10)) + 
                              N') vượt quá sức chứa của bàn (' + CAST(@SucChua AS NVARCHAR(10)) + N' người).';
                ROLLBACK TRANSACTION;
                RETURN;
            END
            
            -- Validation: Kiểm tra bàn có đang được sử dụng không
            IF EXISTS (
                SELECT 1 FROM dbo.[Order] 
                WHERE ban_id = @BanId 
                AND order_id != @OrderId
                AND trang_thai IN ('pending', 'confirmed', 'preparing')
                AND thoi_diem_dat >= DATEADD(day, -1, GETDATE())
            )
            BEGIN
                SET @Message = N'Bàn đang được sử dụng bởi đơn hàng khác.';
                ROLLBACK TRANSACTION;
                RETURN;
            END
        END
        ELSE -- Mang về
        BEGIN
            -- Validation: Kiểm tra bàn phải NULL cho mang về
            IF @BanId IS NOT NULL
            BEGIN
                SET @Message = N'Đơn hàng mang về không được có bàn.';
                ROLLBACK TRANSACTION;
                RETURN;
            END
        END
        
        -- Lấy bước xử lý đầu tiên (thu_tu = 1)
        DECLARE @BuocId INT;
        SELECT @BuocId = buoc_id FROM dbo.BuocXuLy WHERE thu_tu = 1;
        
        IF @BuocId IS NULL
        BEGIN
            SET @Message = N'Không tìm thấy bước xử lý đầu tiên.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Cập nhật trạng thái đơn hàng
        UPDATE dbo.[Order]
        SET trang_thai = 'confirmed'
        WHERE order_id = @OrderId;
        
        -- Tạo LichSuThucHien cho từng OrderItem
        INSERT INTO dbo.LichSuThucHien (order_item_id, buoc_id, nv_id, trang_thai, ghi_chu)
        SELECT 
            oi.order_item_id,
            @BuocId,
            @EmployeeId,
            'CHUA_BAT_DAU',
            CASE 
                WHEN @LaMangVe = 1 THEN N'Đơn hàng mang về đã được tiếp nhận'
                ELSE N'Đơn hàng tại bàn ' + b.so_hieu + N' đã được tiếp nhận'
            END
        FROM dbo.OrderItem oi
        LEFT JOIN dbo.[Order] o ON oi.order_id = o.order_id
        LEFT JOIN dbo.BanAn b ON o.ban_id = b.ban_id
        WHERE oi.order_id = @OrderId;
        
        -- Thành công
        SET @Success = 1;
        SET @Message = CASE 
            WHEN @LaMangVe = 1 THEN N'Đơn hàng mang về đã được tiếp nhận thành công.'
            ELSE N'Đơn hàng ăn tại chỗ đã được tiếp nhận thành công.'
        END;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SET @Success = 0;
        SET @Message = N'Lỗi: ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- =========================================================
-- 2. sp_OrderReception_GetAvailableTables - Lấy bàn có sẵn
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_OrderReception_GetAvailableTables
    @Capacity INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        b.ban_id,
        b.loai_ban_id,
        b.so_hieu,
        lb.suc_chua,
        lb.so_luong as loai_ban_so_luong
    FROM dbo.BanAn b
    INNER JOIN dbo.LoaiBan lb ON b.loai_ban_id = lb.loai_ban_id
    WHERE (@Capacity IS NULL OR lb.suc_chua >= @Capacity)
    AND b.ban_id NOT IN (
        SELECT DISTINCT o.ban_id 
        FROM dbo.[Order] o 
        WHERE o.thoi_diem_dat >= DATEADD(day, -1, GETDATE())
        AND o.trang_thai IN ('pending', 'confirmed', 'preparing')
        AND o.ban_id IS NOT NULL
    )
    ORDER BY lb.suc_chua, b.ban_id;
END
GO

-- =========================================================
-- 3. sp_OrderReception_ValidateTable - Kiểm tra bàn có hợp lệ không
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_OrderReception_ValidateTable
    @TableId INT,
    @CustomerCount INT,
    @IsValid BIT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    SET @IsValid = 0;
    SET @Message = '';
    
    -- Kiểm tra bàn có tồn tại không
    IF NOT EXISTS (SELECT 1 FROM dbo.BanAn WHERE ban_id = @TableId)
    BEGIN
        SET @Message = N'Bàn ăn không tồn tại.';
        RETURN;
    END
    
    -- Kiểm tra sức chứa
    IF EXISTS (
        SELECT 1 FROM dbo.BanAn b
        JOIN dbo.LoaiBan lb ON b.loai_ban_id = lb.loai_ban_id
        WHERE b.ban_id = @TableId AND @CustomerCount > lb.suc_chua
    )
    BEGIN
        DECLARE @SucChua INT;
        SELECT @SucChua = lb.suc_chua
        FROM dbo.BanAn b
        JOIN dbo.LoaiBan lb ON b.loai_ban_id = lb.loai_ban_id
        WHERE b.ban_id = @TableId;
        
        SET @Message = N'Số khách (' + CAST(@CustomerCount AS NVARCHAR(10)) + 
                      N') vượt quá sức chứa của bàn (' + CAST(@SucChua AS NVARCHAR(10)) + N' người).';
        RETURN;
    END
    
    -- Kiểm tra bàn có đang được sử dụng không
    IF EXISTS (
        SELECT 1 FROM dbo.[Order] 
        WHERE ban_id = @TableId 
        AND trang_thai IN ('pending', 'confirmed', 'preparing')
        AND thoi_diem_dat >= DATEADD(day, -1, GETDATE())
    )
    BEGIN
        SET @Message = N'Bàn đang được sử dụng bởi đơn hàng khác.';
        RETURN;
    END
    
    -- Hợp lệ
    SET @IsValid = 1;
    SET @Message = N'Bàn hợp lệ và có sẵn.';
END
GO

-- =========================================================
-- 4. sp_OrderReception_GetOrderStatus - Lấy trạng thái đơn hàng
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_OrderReception_GetOrderStatus
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        o.order_id,
        o.kh_id,
        o.ban_id,
        o.la_mang_ve,
        o.trang_thai,
        o.so_khach,
        o.thoi_diem_dat,
        o.tong_tien,
        b.so_hieu as ban_so_hieu,
        lb.suc_chua as ban_suc_chua,
        kh.ho_ten as khach_hang_ten,
        kh.so_dien_thoai as khach_hang_sdt
    FROM dbo.[Order] o
    LEFT JOIN dbo.BanAn b ON o.ban_id = b.ban_id
    LEFT JOIN dbo.LoaiBan lb ON b.loai_ban_id = lb.loai_ban_id
    LEFT JOIN dbo.KhachHang kh ON o.kh_id = kh.kh_id
    WHERE o.order_id = @OrderId;
    
    -- Lấy danh sách OrderItem
    SELECT 
        oi.order_item_id,
        oi.mon_id,
        oi.so_luong,
        oi.t_dat,
        oi.t_hoan_thanh,
        oi.t_phuc_vu,
        m.ma_mon,
        m.ten_mon,
        m.loai_mon,
        m.gia
    FROM dbo.OrderItem oi
    JOIN dbo.Mon m ON oi.mon_id = m.mon_id
    WHERE oi.order_id = @OrderId;
    
    -- Lấy lịch sử thực hiện
    SELECT 
        lsh.lich_su_id,
        lsh.order_item_id,
        lsh.buoc_id,
        lsh.nv_id,
        lsh.trang_thai,
        lsh.thoi_diem_bat_dau,
        lsh.thoi_diem_ket_thuc,
        lsh.ghi_chu,
        lsh.thoi_diem_tao,
        bx.ten_buoc,
        bx.thu_tu,
        nv.ho_ten as nhan_vien_ten
    FROM dbo.LichSuThucHien lsh
    JOIN dbo.BuocXuLy bx ON lsh.buoc_id = bx.buoc_id
    JOIN dbo.NhanVien nv ON lsh.nv_id = nv.nv_id
    WHERE lsh.order_item_id IN (
        SELECT order_item_id FROM dbo.OrderItem WHERE order_id = @OrderId
    )
    ORDER BY lsh.thoi_diem_tao;
END
GO

-- =========================================================
-- 5. sp_OrderReception_GetPendingOrders - Lấy đơn hàng chờ xử lý
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_OrderReception_GetPendingOrders
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        o.order_id,
        o.kh_id,
        o.ban_id,
        o.la_mang_ve,
        o.trang_thai,
        o.so_khach,
        o.thoi_diem_dat,
        o.tong_tien,
        b.so_hieu as ban_so_hieu,
        lb.suc_chua as ban_suc_chua,
        kh.ho_ten as khach_hang_ten,
        kh.so_dien_thoai as khach_hang_sdt,
        COUNT(oi.order_item_id) as so_mon,
        SUM(oi.so_luong) as tong_so_luong
    FROM dbo.[Order] o
    LEFT JOIN dbo.BanAn b ON o.ban_id = b.ban_id
    LEFT JOIN dbo.LoaiBan lb ON b.loai_ban_id = lb.loai_ban_id
    LEFT JOIN dbo.KhachHang kh ON o.kh_id = kh.kh_id
    LEFT JOIN dbo.OrderItem oi ON o.order_id = oi.order_id
    WHERE o.trang_thai = 'pending'
    GROUP BY o.order_id, o.kh_id, o.ban_id, o.la_mang_ve, o.trang_thai, 
             o.so_khach, o.thoi_diem_dat, o.tong_tien, b.so_hieu, 
             lb.suc_chua, kh.ho_ten, kh.so_dien_thoai
    ORDER BY o.thoi_diem_dat;
END
GO

-- =========================================================
-- 4. sp_OrderReception_GetAllPendingOrders - Lấy tất cả đơn hàng chưa hoàn thành
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_OrderReception_GetAllPendingOrders
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        o.order_id,
        o.kh_id,
        o.ban_id,
        o.la_mang_ve,
        o.trang_thai,
        o.so_khach,
        o.thoi_diem_dat,
        o.tong_tien,
        b.so_hieu as ban_so_hieu,
        lb.suc_chua as ban_suc_chua,
        kh.ho_ten as khach_hang_ten,
        kh.so_dien_thoai as khach_hang_sdt,
        COUNT(oi.order_item_id) as so_mon,
        SUM(oi.so_luong) as tong_so_luong
    FROM dbo.[Order] o
    LEFT JOIN dbo.BanAn b ON o.ban_id = b.ban_id
    LEFT JOIN dbo.LoaiBan lb ON b.loai_ban_id = lb.loai_ban_id
    LEFT JOIN dbo.KhachHang kh ON o.kh_id = kh.kh_id
    LEFT JOIN dbo.OrderItem oi ON o.order_id = oi.order_id
    WHERE o.trang_thai != 'close'
    GROUP BY o.order_id, o.kh_id, o.ban_id, o.la_mang_ve, o.trang_thai, 
             o.so_khach, o.thoi_diem_dat, o.tong_tien, b.so_hieu, 
             lb.suc_chua, kh.ho_ten, kh.so_dien_thoai
    ORDER BY o.thoi_diem_dat DESC;
END
GO