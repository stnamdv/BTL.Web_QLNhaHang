/* =========================================================
   STORED PROCEDURES CHO CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG
   Tự động cập nhật trạng thái đơn hàng dựa trên tiến độ xử lý
   ========================================================= */

USE NhaHang
GO

-- =========================================================
-- 1. sp_Order_CheckAndUpdateStatus - Kiểm tra và cập nhật trạng thái đơn hàng
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_Order_CheckAndUpdateStatus
    @OrderId INT,
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
        
        -- Kiem tra don hang co ton tai khong
        IF NOT EXISTS (SELECT 1 FROM dbo.[Order] WHERE order_id = @OrderId)
        BEGIN
            SET @Message = N'Đơn hàng không tồn tại.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Lấy trạng thái hiện tại của đơn hàng
        DECLARE @CurrentOrderStatus NVARCHAR(20);
        SELECT @CurrentOrderStatus = trang_thai
        FROM dbo.[Order]
        WHERE order_id = @OrderId;
        
        -- Nếu đơn hàng đã hoàn thành, không cần xử lý
        IF @CurrentOrderStatus = 'HOAN_THANH'
        BEGIN
            SET @Success = 1;
            SET @Message = N'Đơn hàng đã hoàn thành.';
            COMMIT TRANSACTION;
            RETURN;
        END
        
        -- Lấy tất cả các bước xử lý và trạng thái của chúng
        DECLARE @TotalSteps INT, @CompletedSteps INT;
        
        -- Đếm tổng số bước xử lý
        SELECT @TotalSteps = COUNT(*)
        FROM dbo.BuocXuLy;
        
        -- Đếm số bước đã hoàn thành cho đơn hàng này
        SELECT @CompletedSteps = COUNT(*)
        FROM dbo.LichSuThucHien lsh
        JOIN dbo.BuocXuLy bx ON bx.buoc_id = lsh.buoc_id
        WHERE lsh.order_id = @OrderId 
          AND lsh.trang_thai = 'HOAN_THANH';
        
        -- Nếu tất cả bước đã hoàn thành, cập nhật trạng thái đơn hàng
        IF @CompletedSteps = @TotalSteps AND @TotalSteps > 0
        BEGIN
            UPDATE dbo.[Order]
            SET trang_thai = 'HOAN_THANH'
            WHERE order_id = @OrderId;
            
            SET @Success = 1;
            SET @Message = N'Đơn hàng đã được cập nhật thành HOAN_THANH.';
        END
        ELSE
        BEGIN
            SET @Success = 1;
            SET @Message = N'Đơn hàng chưa hoàn thành tất cả các bước. (' + CAST(@CompletedSteps AS NVARCHAR(10)) + '/' + CAST(@TotalSteps AS NVARCHAR(10)) + ')';
        END
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        SET @Success = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

-- =========================================================
-- 2. sp_Order_GetProcessingStatus - Lấy trạng thái xử lý của đơn hàng
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_Order_GetProcessingStatus
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Lấy thông tin tổng quan về trạng thái xử lý
    SELECT 
        o.order_id,
        o.trang_thai as order_status,
        COUNT(bx.buoc_id) as total_steps,
        COUNT(CASE WHEN lsh.trang_thai = 'HOAN_THANH' THEN 1 END) as completed_steps,
        COUNT(CASE WHEN lsh.trang_thai = 'DANG_THUC_HIEN' THEN 1 END) as in_progress_steps,
        COUNT(CASE WHEN lsh.trang_thai = 'CHUA_BAT_DAU' THEN 1 END) as pending_steps,
        CASE 
            WHEN COUNT(bx.buoc_id) = COUNT(CASE WHEN lsh.trang_thai = 'HOAN_THANH' THEN 1 END) 
            THEN 'SAN_SANG_HOAN_THANH'
            ELSE 'DANG_XU_LY'
        END as processing_status
    FROM dbo.[Order] o
    CROSS JOIN dbo.BuocXuLy bx
    LEFT JOIN dbo.LichSuThucHien lsh ON lsh.order_id = o.order_id AND lsh.buoc_id = bx.buoc_id
    WHERE o.order_id = @OrderId
    GROUP BY o.order_id, o.trang_thai;
    
    -- Lấy chi tiết từng bước
    SELECT 
        bx.buoc_id,
        bx.ten_buoc,
        bx.thu_tu,
        bx.mo_ta,
        ISNULL(lsh.trang_thai, 'CHUA_BAT_DAU') as step_status,
        lsh.thoi_diem_bat_dau,
        lsh.thoi_diem_ket_thuc,
        lsh.nv_id,
        nv.ho_ten as nv_ho_ten,
        lsh.ghi_chu
    FROM dbo.BuocXuLy bx
    LEFT JOIN dbo.LichSuThucHien lsh ON lsh.order_id = @OrderId AND lsh.buoc_id = bx.buoc_id
    LEFT JOIN dbo.NhanVien nv ON nv.nv_id = lsh.nv_id
    ORDER BY bx.thu_tu;
END
GO

-- =========================================================
-- 3. sp_Order_ForceComplete - Buộc hoàn thành đơn hàng (admin only)
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_Order_ForceComplete
    @OrderId INT,
    @AdminId INT,
    @Reason NVARCHAR(500) = NULL,
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
        
        -- Kiem tra don hang co ton tai khong
        IF NOT EXISTS (SELECT 1 FROM dbo.[Order] WHERE order_id = @OrderId)
        BEGIN
            SET @Message = N'Đơn hàng không tồn tại.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Kiểm tra admin có quyền không (có thể thêm logic kiểm tra role)
        IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien WHERE nv_id = @AdminId AND trang_thai = 'ACTIVE')
        BEGIN
            SET @Message = N'Không có quyền thực hiện thao tác này.';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Cập nhật trạng thái đơn hàng
        UPDATE dbo.[Order]
        SET trang_thai = 'HOAN_THANH'
        WHERE order_id = @OrderId;
        
        -- Ghi log hoạt động (có thể tạo bảng log riêng)
        PRINT 'Đơn hàng ' + CAST(@OrderId AS NVARCHAR(10)) + ' được buộc hoàn thành bởi admin ' + CAST(@AdminId AS NVARCHAR(10)) + '. Lý do: ' + ISNULL(@Reason, 'Không có lý do');
        
        SET @Success = 1;
        SET @Message = N'Đơn hàng đã được buộc hoàn thành thành công.';
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        SET @Success = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO
