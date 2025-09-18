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
        
        -- Get all order items for this order
        DECLARE @OrderItemId INT;
        DECLARE order_cursor CURSOR FOR
            SELECT order_item_id 
            FROM dbo.OrderItem 
            WHERE order_id = @OrderId;
        
        OPEN order_cursor;
        FETCH NEXT FROM order_cursor INTO @OrderItemId;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Check if LichSuThucHien already exists for this order item and step
            IF EXISTS (SELECT 1 FROM dbo.LichSuThucHien 
                      WHERE order_item_id = @OrderItemId AND buoc_id = @StepId)
            BEGIN
                -- Update existing record
                IF @Action = 'start'
                BEGIN
                    UPDATE dbo.LichSuThucHien 
                    SET nv_id = @EmployeeId, 
                        trang_thai = @Status, 
                        thoi_diem_bat_dau = @CurrentTime
                    WHERE order_item_id = @OrderItemId AND buoc_id = @StepId;
                END
                ELSE IF @Action = 'complete'
                BEGIN
                    UPDATE dbo.LichSuThucHien 
                    SET trang_thai = @Status, 
                        thoi_diem_ket_thuc = @CurrentTime
                    WHERE order_item_id = @OrderItemId 
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
                    (order_item_id, buoc_id, nv_id, trang_thai, thoi_diem_bat_dau, thoi_diem_tao)
                    VALUES 
                    (@OrderItemId, @StepId, @EmployeeId, @Status, @CurrentTime, @CurrentTime);
                END
            END
            
            FETCH NEXT FROM order_cursor INTO @OrderItemId;
        END
        
        CLOSE order_cursor;
        DEALLOCATE order_cursor;
        
        COMMIT TRANSACTION;
        
        -- Return success
        SELECT 1 AS Success;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        -- Re-raise the error
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
