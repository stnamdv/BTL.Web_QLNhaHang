CREATE OR ALTER PROCEDURE [dbo].[sp_LichSuThucHien_UpdateStepStatus]
    @OrderId INT,
    @StepId INT,
    @EmployeeId INT,
    @Action NVARCHAR(20) -- 'start' or 'complete'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Status NVARCHAR(20);
    DECLARE @CurrentTime DATETIME = GETDATE();
    
    -- Determine status based on action
    IF @Action = 'start'
        SET @Status = 'DANG_THUC_HIEN';
    ELSE IF @Action = 'complete'
        SET @Status = 'HOAN_THANH';
    ELSE
    BEGIN
        RAISERROR('Invalid action. Must be "start" or "complete".', 16, 1);
        RETURN;
    END
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if LichSuThucHien already exists for this order and step
        IF EXISTS (SELECT 1 FROM dbo.LichSuThucHien 
                  WHERE order_id = @OrderId AND buoc_id = @StepId)
        BEGIN
            -- Update existing record
            IF @Action = 'start'
            BEGIN
                UPDATE dbo.LichSuThucHien 
                SET nv_id = @EmployeeId, 
                    trang_thai = @Status, 
                    thoi_diem_bat_dau = @CurrentTime
                WHERE order_id = @OrderId AND buoc_id = @StepId;
            END
            ELSE IF @Action = 'complete'
            BEGIN
                UPDATE dbo.LichSuThucHien 
                SET trang_thai = @Status, 
                    thoi_diem_ket_thuc = @CurrentTime
                WHERE order_id = @OrderId 
                  AND buoc_id = @StepId 
                  AND nv_id = @EmployeeId;
            END
        END
        ELSE
        BEGIN
            -- Create new record (only for 'start' action)
            IF @Action = 'start'
            BEGIN
                INSERT INTO dbo.LichSuThucHien 
                (order_id, buoc_id, nv_id, trang_thai, thoi_diem_bat_dau, thoi_diem_tao)
                VALUES 
                (@OrderId, @StepId, @EmployeeId, @Status, @CurrentTime, @CurrentTime);
            END
        END
        
        -- Kiểm tra xem đây có phải là bước cuối cùng và cập nhật trạng thái đơn hàng
        IF @Action = 'complete'
        BEGIN
            -- Lấy thứ tự bước cao nhất (thu_tu) từ tất cả các bước xử lý
            DECLARE @MaxStepOrder INT;
            SELECT @MaxStepOrder = MAX(bx.thu_tu)
            FROM dbo.BuocXuLy bx;
            
            -- Lấy thứ tự bước hiện tại
            DECLARE @CurrentStepOrder INT;
            SELECT @CurrentStepOrder = bx.thu_tu
            FROM dbo.BuocXuLy bx
            WHERE bx.buoc_id = @StepId;
            
            -- Nếu đây là bước cuối cùng, cập nhật trạng thái đơn hàng thành HOAN_THANH
            IF @CurrentStepOrder = @MaxStepOrder
            BEGIN
                UPDATE dbo.[Order]
                SET trang_thai = 'HOAN_THANH'
                WHERE order_id = @OrderId;
                
                PRINT 'Trạng thái đơn hàng đã được cập nhật thành HOAN_THANH - Bước cuối đã hoàn thành';
            END
        END
        
        COMMIT TRANSACTION;
        
        -- Trả về kết quả thành công
        SELECT 1 AS Success;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        -- Ném lại lỗi
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
