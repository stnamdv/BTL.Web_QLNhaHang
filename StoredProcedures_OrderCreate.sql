/* =========================================================
   STORED PROCEDURES CHO TẠO ĐƠN HÀNG MỚI
   Xử lý logic tạo đơn hàng mới cho khách hàng
   ========================================================= */

USE NhaHang
GO

-- =========================================================
-- 1. Tạo User-Defined Table Type cho OrderItems
-- =========================================================
IF NOT EXISTS (SELECT * FROM sys.types WHERE name = 'OrderItemsType')
BEGIN
    CREATE TYPE dbo.OrderItemsType AS TABLE (
        MonId INT NOT NULL,
        SoLuong INT NOT NULL
    );
END
GO

-- =========================================================
-- 2. sp_Order_Create - Tạo đơn hàng mới
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_Order_Create
    @LaMangVe BIT,
    @BanId INT = NULL,
    @SoKhach INT = NULL,
    @KhachHangTen NVARCHAR(120) = NULL,
    @KhachHangSdt NVARCHAR(20) = NULL,
    @OrderItems dbo.OrderItemsType READONLY, -- Table-Valued Parameter
    @OrderId INT OUTPUT,
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
        SET @OrderId = 0;
        
        -- Validation cơ bản
        IF @LaMangVe = 0 AND (@BanId IS NULL OR @SoKhach IS NULL)
        BEGIN
            SET @Message = N'Đơn hàng ăn tại chỗ phải có bàn và số khách.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        IF @LaMangVe = 1 AND @BanId IS NOT NULL
        BEGIN
            SET @Message = N'Đơn hàng mang về không được có bàn.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation bàn ăn (nếu ăn tại chỗ)
        IF @LaMangVe = 0
        BEGIN
            -- Kiểm tra bàn có tồn tại không
            IF NOT EXISTS (SELECT 1 FROM dbo.BanAn WHERE ban_id = @BanId)
            BEGIN
                SET @Message = N'Bàn ăn không tồn tại.';
                ROLLBACK TRANSACTION;
                RETURN;
            END
            
            -- Kiểm tra số khách có phù hợp với sức chứa bàn không
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
            
            -- Kiểm tra bàn có đang được sử dụng không
            IF EXISTS (
                SELECT 1 FROM dbo.[Order] 
                WHERE ban_id = @BanId 
                AND trang_thai IN ('pending', 'confirmed', 'preparing')
                AND thoi_diem_dat >= DATEADD(day, -1, GETDATE())
            )
            BEGIN
                SET @Message = N'Bàn đang được sử dụng bởi đơn hàng khác.';
                ROLLBACK TRANSACTION;
                RETURN;
            END
        END
        
        -- Tạo hoặc lấy khách hàng
        DECLARE @KhId INT = NULL;
        IF @KhachHangTen IS NOT NULL AND @KhachHangSdt IS NOT NULL
        BEGIN
            -- Kiểm tra khách hàng đã tồn tại chưa
            SELECT @KhId = kh_id 
            FROM dbo.KhachHang 
            WHERE so_dien_thoai = @KhachHangSdt;
            
            -- Tạo khách hàng mới nếu chưa tồn tại
            IF @KhId IS NULL
            BEGIN
                INSERT INTO dbo.KhachHang (ho_ten, so_dien_thoai)
                VALUES (@KhachHangTen, @KhachHangSdt);
                SET @KhId = SCOPE_IDENTITY();
            END
        END
        
        -- Tạo đơn hàng
        INSERT INTO dbo.[Order] (kh_id, ban_id, la_mang_ve, so_khach, thoi_diem_dat, trang_thai)
        VALUES (@KhId, @BanId, @LaMangVe, @SoKhach, GETDATE(), 'pending');
        
        SET @OrderId = SCOPE_IDENTITY();
        
        -- Xử lý OrderItems từ Table-Valued Parameter
        DECLARE @ItemCount INT;
        SELECT @ItemCount = COUNT(*) FROM @OrderItems;
        
        IF @ItemCount = 0
        BEGIN
            SET @Message = N'Đơn hàng phải có ít nhất một món ăn.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validate OrderItems
        DECLARE @ValidItems INT = 0;
        SELECT @ValidItems = COUNT(*) 
        FROM @OrderItems oi
        JOIN dbo.Mon m ON oi.MonId = m.mon_id
        WHERE oi.MonId > 0 AND oi.SoLuong > 0;
        
        IF @ValidItems = 0
        BEGIN
            SET @Message = N'Không có món ăn hợp lệ trong đơn hàng.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Tạo OrderItems
        INSERT INTO dbo.OrderItem (order_id, mon_id, so_luong, t_dat)
        SELECT @OrderId, oi.MonId, oi.SoLuong, GETDATE()
        FROM @OrderItems oi
        JOIN dbo.Mon m ON oi.MonId = m.mon_id
        WHERE oi.MonId > 0 AND oi.SoLuong > 0;
        
        -- Thành công
        SET @Success = 1;
        SET @Message = CASE 
            WHEN @LaMangVe = 1 THEN N'Đơn hàng mang về đã được tạo thành công.'
            ELSE N'Đơn hàng ăn tại chỗ đã được tạo thành công.'
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
-- 2. sp_Order_CreateWithItems - Tạo đơn hàng với danh sách món ăn
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_Order_CreateWithItems
    @LaMangVe BIT,
    @BanId INT = NULL,
    @SoKhach INT = NULL,
    @KhachHangTen NVARCHAR(120) = NULL,
    @KhachHangSdt NVARCHAR(20) = NULL,
    @OrderId INT OUTPUT,
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
        SET @OrderId = 0;
        
        -- Validation cơ bản
        IF @LaMangVe = 0 AND (@BanId IS NULL OR @SoKhach IS NULL)
        BEGIN
            SET @Message = N'Đơn hàng ăn tại chỗ phải có bàn và số khách.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        IF @LaMangVe = 1 AND @BanId IS NOT NULL
        BEGIN
            SET @Message = N'Đơn hàng mang về không được có bàn.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation bàn ăn (nếu ăn tại chỗ)
        IF @LaMangVe = 0
        BEGIN
            -- Kiểm tra bàn có tồn tại không
            IF NOT EXISTS (SELECT 1 FROM dbo.BanAn WHERE ban_id = @BanId)
            BEGIN
                SET @Message = N'Bàn ăn không tồn tại.';
                ROLLBACK TRANSACTION;
                RETURN;
            END
            
            -- Kiểm tra số khách có phù hợp với sức chứa bàn không
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
            
            -- Kiểm tra bàn có đang được sử dụng không
            IF EXISTS (
                SELECT 1 FROM dbo.[Order] 
                WHERE ban_id = @BanId 
                AND trang_thai IN ('pending', 'confirmed', 'preparing')
                AND thoi_diem_dat >= DATEADD(day, -1, GETDATE())
            )
            BEGIN
                SET @Message = N'Bàn đang được sử dụng bởi đơn hàng khác.';
                ROLLBACK TRANSACTION;
                RETURN;
            END
        END
        
        -- Tạo hoặc lấy khách hàng
        DECLARE @KhId INT = NULL;
        IF @KhachHangTen IS NOT NULL AND @KhachHangSdt IS NOT NULL
        BEGIN
            -- Kiểm tra khách hàng đã tồn tại chưa
            SELECT @KhId = kh_id 
            FROM dbo.KhachHang 
            WHERE so_dien_thoai = @KhachHangSdt;
            
            -- Tạo khách hàng mới nếu chưa tồn tại
            IF @KhId IS NULL
            BEGIN
                INSERT INTO dbo.KhachHang (ho_ten, so_dien_thoai)
                VALUES (@KhachHangTen, @KhachHangSdt);
                SET @KhId = SCOPE_IDENTITY();
            END
        END
        
        -- Tạo đơn hàng
        INSERT INTO dbo.[Order] (kh_id, ban_id, la_mang_ve, so_khach, thoi_diem_dat, trang_thai)
        VALUES (@KhId, @BanId, @LaMangVe, @SoKhach, GETDATE(), 'pending');
        
        SET @OrderId = SCOPE_IDENTITY();
        
        -- Thành công
        SET @Success = 1;
        SET @Message = CASE 
            WHEN @LaMangVe = 1 THEN N'Đơn hàng mang về đã được tạo thành công.'
            ELSE N'Đơn hàng ăn tại chỗ đã được tạo thành công.'
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
-- 3. sp_OrderItem_Create - Tạo OrderItem cho đơn hàng
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_OrderItem_Create
    @OrderId INT,
    @MonId INT,
    @SoLuong INT,
    @Success BIT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        SET @Success = 0;
        SET @Message = '';
        
        -- Validation
        IF NOT EXISTS (SELECT 1 FROM dbo.[Order] WHERE order_id = @OrderId)
        BEGIN
            SET @Message = N'Đơn hàng không tồn tại.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        IF NOT EXISTS (SELECT 1 FROM dbo.Mon WHERE mon_id = @MonId)
        BEGIN
            SET @Message = N'Món ăn không tồn tại.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        IF @SoLuong <= 0
        BEGIN
            SET @Message = N'Số lượng phải lớn hơn 0.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Tạo OrderItem
        INSERT INTO dbo.OrderItem (order_id, mon_id, so_luong, t_dat)
        VALUES (@OrderId, @MonId, @SoLuong, GETDATE());
        
        SET @Success = 1;
        SET @Message = N'Đã thêm món ăn vào đơn hàng.';
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SET @Success = 0;
        SET @Message = N'Lỗi: ' + ERROR_MESSAGE();
    END CATCH
END
GO
