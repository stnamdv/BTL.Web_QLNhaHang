CREATE OR ALTER PROCEDURE sp_HoaDon_GetByOrderId
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        hd_id,
        order_id,
        thoi_diem_tt,
        phuong_thuc
    FROM HoaDon 
    WHERE order_id = @OrderId;
END
GO

-- =============================================
-- Procedure: sp_HoaDon_Create
-- Description: Create new HoaDon
-- Parameters: 
--   @OrderId - ID of the order
--   @PhuongThuc - Payment method (optional)
--   @HdId - OUTPUT: ID of created HoaDon
--   @Success - OUTPUT: Success flag
--   @Message - OUTPUT: Result message
-- =============================================
CREATE OR ALTER PROCEDURE sp_HoaDon_Create
    @OrderId INT,
    @PhuongThuc NVARCHAR(20) = NULL,
    @HdId INT OUTPUT,
    @Success BIT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Check if order exists
        IF NOT EXISTS (SELECT 1 FROM [Order] WHERE order_id = @OrderId)
        BEGIN
            SET @Success = 0;
            SET @Message = N'Không tìm thấy đơn hàng.';
            SET @HdId = 0;
            RETURN;
        END
        
        -- Check if HoaDon already exists for this order
        IF EXISTS (SELECT 1 FROM HoaDon WHERE order_id = @OrderId)
        BEGIN
            SET @Success = 0;
            SET @Message = N'Hoá đơn đã tồn tại cho đơn hàng này.';
            SET @HdId = 0;
            RETURN;
        END
        
        -- Insert new HoaDon
        INSERT INTO HoaDon (order_id, thoi_diem_tt, phuong_thuc)
        VALUES (@OrderId, GETDATE(), @PhuongThuc);
        
        -- Get the new HoaDon ID
        SET @HdId = SCOPE_IDENTITY();
        
        SET @Success = 1;
        SET @Message = N'Hoá đơn đã được tạo thành công.';
        
    END TRY
    BEGIN CATCH
        SET @Success = 0;
        SET @Message = N'Lỗi khi tạo hoá đơn: ' + ERROR_MESSAGE();
        SET @HdId = 0;
    END CATCH
END
GO

-- =============================================
-- Procedure: sp_HoaDon_GetById
-- Description: Get HoaDon by ID
-- Parameters: @HdId - ID of the HoaDon
-- Returns: HoaDon information
-- =============================================
CREATE OR ALTER PROCEDURE sp_HoaDon_GetById
    @HdId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        hd_id,
        order_id,
        thoi_diem_tt,
        phuong_thuc
    FROM HoaDon 
    WHERE hd_id = @HdId;
END
GO

-- =============================================
-- Procedure: sp_HoaDon_GetAll
-- Description: Get all HoaDons
-- Returns: List of all HoaDons ordered by creation date
-- =============================================
CREATE OR ALTER PROCEDURE sp_HoaDon_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        hd_id,
        order_id,
        thoi_diem_tt,
        phuong_thuc
    FROM HoaDon 
    ORDER BY thoi_diem_tt DESC;
END
GO

-- =============================================
-- Procedure: sp_HoaDon_GetWithOrderDetails
-- Description: Get HoaDon with complete order details for display
-- Parameters: @OrderId - ID of the order
-- Returns: Multiple result sets:
--   1. HoaDon information
--   2. Order details with customer and table info
--   3. Order items with menu details
-- =============================================
CREATE OR ALTER PROCEDURE sp_HoaDon_GetWithOrderDetails
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get HoaDon info
    SELECT 
        h.hd_id,
        h.order_id,
        h.thoi_diem_tt,
        h.phuong_thuc
    FROM HoaDon h
    WHERE h.order_id = @OrderId;
    
    -- Get Order details with customer and table information
    SELECT 
        o.order_id,
        o.kh_id,
        o.ban_id,
        o.la_mang_ve,
        o.trang_thai,
        o.so_khach,
        o.thoi_diem_dat,
        o.tong_tien,
        ba.so_hieu as ban_so_hieu,
        lb.suc_chua as ban_suc_chua,
        kh.ho_ten as khach_hang_ten,
        kh.so_dien_thoai as khach_hang_sdt
    FROM [Order] o
    LEFT JOIN BanAn ba ON o.ban_id = ba.ban_id
    LEFT JOIN LoaiBan lb ON ba.loai_ban_id = lb.loai_ban_id
    LEFT JOIN KhachHang kh ON o.kh_id = kh.kh_id
    WHERE o.order_id = @OrderId;
    
    -- Get Order Items with menu details
    SELECT 
        oi.order_item_id,
        oi.order_id,
        oi.mon_id,
        oi.so_luong,
        oi.t_dat,
        oi.t_hoan_thanh,
        oi.t_phuc_vu,
        m.ma_mon,
        m.ten_mon,
        m.loai_mon,
        m.gia
    FROM OrderItem oi
    INNER JOIN Mon m ON oi.mon_id = m.mon_id
    WHERE oi.order_id = @OrderId
    ORDER BY oi.order_item_id;
END
GO