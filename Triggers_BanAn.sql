-- =============================================
-- Triggers for BanAn Table
-- =============================================

-- 1. Trigger to check if BanAn is being used before UPDATE
-- =============================================
CREATE OR ALTER TRIGGER tr_BanAn_Update_Check
ON dbo.BanAn
INSTEAD OF UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ban_id INT;
    DECLARE @old_loai_ban_id INT;
    DECLARE @new_loai_ban_id INT;
    DECLARE @old_so_hieu NVARCHAR(10);
    DECLARE @new_so_hieu NVARCHAR(10);
    DECLARE @order_count INT;
    DECLARE @existing_so_hieu_count INT;
    
    -- Get the updated record details
    SELECT 
        @ban_id = ban_id,
        @new_loai_ban_id = loai_ban_id,
        @new_so_hieu = so_hieu
    FROM inserted;
    
    -- Get the old record details
    SELECT 
        @old_loai_ban_id = loai_ban_id,
        @old_so_hieu = so_hieu
    FROM deleted;
    
    -- Check if there are any active orders using this table
    SELECT @order_count = COUNT(*)
    FROM dbo.[Order] o
    LEFT JOIN dbo.HoaDon h ON h.order_id = o.order_id
    WHERE o.ban_id = @ban_id
    AND h.order_id IS NULL; -- Order chưa thanh toán
    
    -- If there are active orders, prevent update
    IF @order_count > 0
    BEGIN
        RAISERROR(N'Không thể sửa bàn này vì có đơn hàng đang sử dụng.', 16, 1);
        RETURN;
    END
    
    -- Check if so_hieu already exists (for different ban_id)
    SELECT @existing_so_hieu_count = COUNT(*)
    FROM dbo.BanAn
    WHERE so_hieu = @new_so_hieu
    AND ban_id != @ban_id;
    
    IF @existing_so_hieu_count > 0
    BEGIN
        RAISERROR(N'Số hiệu bàn "%s" đã tồn tại.', 16, 1, @new_so_hieu);
        RETURN;
    END
    
    -- Check if the new LoaiBan exists
    IF NOT EXISTS (SELECT 1 FROM dbo.LoaiBan WHERE loai_ban_id = @new_loai_ban_id)
    BEGIN
        RAISERROR(N'Loại bàn với ID %d không tồn tại.', 16, 1, @new_loai_ban_id);
        RETURN;
    END
    
    -- If all checks pass, perform the update
    UPDATE dbo.BanAn
    SET 
        loai_ban_id = @new_loai_ban_id,
        so_hieu = @new_so_hieu
    WHERE ban_id = @ban_id;
END
GO

-- 2. Trigger to check if BanAn is being used before DELETE
-- =============================================
CREATE OR ALTER TRIGGER tr_BanAn_Delete_Check
ON dbo.BanAn
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ban_id INT;
    DECLARE @order_count INT;
    DECLARE @so_hieu NVARCHAR(10);
    
    -- Get the deleted record details
    SELECT 
        @ban_id = ban_id,
        @so_hieu = so_hieu
    FROM deleted;
    
    -- Check if there are any active orders using this table
    SELECT @order_count = COUNT(*)
    FROM dbo.[Order] o
    LEFT JOIN dbo.HoaDon h ON h.order_id = o.order_id
    WHERE o.ban_id = @ban_id
    AND h.order_id IS NULL; -- Order chưa thanh toán
    
    -- If there are active orders, prevent deletion
    IF @order_count > 0
    BEGIN
        RAISERROR(N'Không thể xóa bàn "%s" vì có đơn hàng đang sử dụng.', 16, 1, @so_hieu);
        RETURN;
    END
    
    -- Check if there are any historical orders (for audit purposes)
    SELECT @order_count = COUNT(*)
    FROM dbo.[Order]
    WHERE ban_id = @ban_id;
    
    IF @order_count > 0
    BEGIN
        -- Allow deletion but log a warning
        PRINT CONCAT(N'Cảnh báo: Xóa bàn "', @so_hieu, '" có ', @order_count, ' đơn hàng lịch sử.');
    END
    
    -- If all checks pass, perform the deletion
    DELETE FROM dbo.BanAn
    WHERE ban_id = @ban_id;
END
GO

-- 3. Trigger to validate BanAn before INSERT
-- =============================================
CREATE OR ALTER TRIGGER tr_BanAn_Insert_Check
ON dbo.BanAn
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @loai_ban_id INT;
    DECLARE @so_hieu NVARCHAR(10);
    DECLARE @existing_so_hieu_count INT;
    DECLARE @loai_ban_count INT;
    DECLARE @current_ban_count INT;
    
    -- Get the inserted record details
    SELECT 
        @loai_ban_id = loai_ban_id,
        @so_hieu = so_hieu
    FROM inserted;
    
    -- Check if so_hieu already exists
    SELECT @existing_so_hieu_count = COUNT(*)
    FROM dbo.BanAn
    WHERE so_hieu = @so_hieu;
    
    IF @existing_so_hieu_count > 0
    BEGIN
        RAISERROR(N'Số hiệu bàn "%s" đã tồn tại.', 16, 1, @so_hieu);
        RETURN;
    END
    
    -- Check if the LoaiBan exists
    IF NOT EXISTS (SELECT 1 FROM dbo.LoaiBan WHERE loai_ban_id = @loai_ban_id)
    BEGIN
        RAISERROR(N'Loại bàn với ID %d không tồn tại.', 16, 1, @loai_ban_id);
        RETURN;
    END
    
    -- Check if adding this table would exceed the LoaiBan quantity limit
    SELECT @current_ban_count = COUNT(*)
    FROM dbo.BanAn
    WHERE loai_ban_id = @loai_ban_id;
    
    SELECT @loai_ban_count = so_luong
    FROM dbo.LoaiBan
    WHERE loai_ban_id = @loai_ban_id;
    
    IF @current_ban_count >= @loai_ban_count
    BEGIN
        RAISERROR(N'Không thể thêm bàn vì đã đạt giới hạn số lượng của loại bàn này (%d bàn).', 16, 1, @loai_ban_count);
        RETURN;
    END
    
    -- If all checks pass, perform the insert
    INSERT INTO dbo.BanAn (loai_ban_id, so_hieu)
    SELECT loai_ban_id, so_hieu
    FROM inserted;
END
GO

-- 4. Trigger to maintain referential integrity with LoaiBan
-- =============================================
CREATE OR ALTER TRIGGER tr_BanAn_LoaiBan_Integrity
ON dbo.LoaiBan
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @loai_ban_id INT;
    DECLARE @new_so_luong INT;
    DECLARE @old_so_luong INT;
    DECLARE @ban_count INT;
    
    -- Handle DELETE
    IF EXISTS (SELECT 1 FROM deleted) AND NOT EXISTS (SELECT 1 FROM inserted)
    BEGIN
        SELECT @loai_ban_id = loai_ban_id FROM deleted;
        
        -- Check if there are any tables using this LoaiBan
        SELECT @ban_count = COUNT(*)
        FROM dbo.BanAn
        WHERE loai_ban_id = @loai_ban_id;
        
        IF @ban_count > 0
        BEGIN
            RAISERROR(N'Không thể xóa loại bàn vì có %d bàn đang sử dụng loại này.', 16, 1, @ban_count);
            ROLLBACK TRANSACTION;
            RETURN;
        END
    END
    
    -- Handle UPDATE
    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
    BEGIN
        SELECT 
            @loai_ban_id = loai_ban_id,
            @new_so_luong = so_luong
        FROM inserted;
        
        SELECT @old_so_luong = so_luong FROM deleted;
        
        -- If reducing quantity, check if it would affect existing tables
        IF @new_so_luong < @old_so_luong
        BEGIN
            SELECT @ban_count = COUNT(*)
            FROM dbo.BanAn
            WHERE loai_ban_id = @loai_ban_id;
            
            IF @ban_count > @new_so_luong
            BEGIN
                RAISERROR(N'Không thể giảm số lượng xuống %d vì hiện có %d bàn đang sử dụng loại này.', 16, 1, @new_so_luong, @ban_count);
                ROLLBACK TRANSACTION;
                RETURN;
            END
        END
    END
END
GO

-- =============================================
-- Drop Triggers (if needed to recreate)
-- =============================================
/*
DROP TRIGGER IF EXISTS tr_BanAn_Update_Check;
DROP TRIGGER IF EXISTS tr_BanAn_Delete_Check;
DROP TRIGGER IF EXISTS tr_BanAn_Insert_Check;
DROP TRIGGER IF EXISTS tr_BanAn_LoaiBan_Integrity;
*/
