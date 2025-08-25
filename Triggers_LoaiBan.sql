-- =============================================
-- Triggers for LoaiBan Table
-- =============================================

-- 1. Trigger to check if LoaiBan is being used before UPDATE
-- =============================================
CREATE OR ALTER TRIGGER tr_LoaiBan_Update_Check
ON dbo.LoaiBan
INSTEAD OF UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @loai_ban_id INT;
    DECLARE @old_suc_chua INT;
    DECLARE @new_suc_chua INT;
    DECLARE @old_so_luong INT;
    DECLARE @new_so_luong INT;
    DECLARE @ban_count INT;
    DECLARE @order_count INT;
    
    -- Get the updated record details
    SELECT 
        @loai_ban_id = loai_ban_id,
        @new_suc_chua = suc_chua,
        @new_so_luong = so_luong
    FROM inserted;
    
    -- Get the old record details
    SELECT 
        @old_suc_chua = suc_chua,
        @old_so_luong = so_luong
    FROM deleted;
    
    -- Check if there are any tables (BanAn) using this LoaiBan
    SELECT @ban_count = COUNT(*)
    FROM dbo.BanAn
    WHERE loai_ban_id = @loai_ban_id;
    
    -- Check if there are any active orders using tables of this LoaiBan
    SELECT @order_count = COUNT(*)
    FROM dbo.[Order] o
    INNER JOIN dbo.BanAn b ON o.ban_id = b.ban_id
    WHERE b.loai_ban_id = @loai_ban_id
    AND o.thoi_diem_dat >= DATEADD(day, -1, GETDATE()); -- Orders from last 24 hours
    
    -- If there are active orders, prevent update
    IF @order_count > 0
    BEGIN
        RAISERROR('Không thể sửa loại bàn này vì có đơn hàng đang sử dụng bàn thuộc loại này.', 16, 1);
        RETURN;
    END
    
    -- If reducing quantity and there are tables using this type, check if new quantity is sufficient
    IF @new_so_luong < @old_so_luong AND @ban_count > @new_so_luong
    BEGIN
        RAISERROR('Không thể giảm số lượng xuống %d vì hiện có %d bàn đang sử dụng loại này.', 16, 1, @new_so_luong, @ban_count);
        RETURN;
    END
    
    -- If changing capacity and there are tables using this type, prevent update
    IF @new_suc_chua != @old_suc_chua AND @ban_count > 0
    BEGIN
        RAISERROR('Không thể thay đổi sức chứa vì có %d bàn đang sử dụng loại này.', 16, 1, @ban_count);
        RETURN;
    END
    
    -- If all checks pass, perform the update
    UPDATE dbo.LoaiBan
    SET 
        suc_chua = @new_suc_chua,
        so_luong = @new_so_luong
    WHERE loai_ban_id = @loai_ban_id;
END
GO

-- 2. Trigger to check if LoaiBan is being used before DELETE
-- =============================================
CREATE OR ALTER TRIGGER tr_LoaiBan_Delete_Check
ON dbo.LoaiBan
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @loai_ban_id INT;
    DECLARE @ban_count INT;
    DECLARE @order_count INT;
    
    -- Get the deleted record ID
    SELECT @loai_ban_id = loai_ban_id FROM deleted;
    
    -- Check if there are any tables (BanAn) using this LoaiBan
    SELECT @ban_count = COUNT(*)
    FROM dbo.BanAn
    WHERE loai_ban_id = @loai_ban_id;
    
    -- Check if there are any active orders using tables of this LoaiBan
    SELECT @order_count = COUNT(*)
    FROM dbo.[Order] o
    INNER JOIN dbo.BanAn b ON o.ban_id = b.ban_id
    WHERE b.loai_ban_id = @loai_ban_id
    AND o.thoi_diem_dat >= DATEADD(day, -1, GETDATE()); -- Orders from last 24 hours
    
    -- If there are active orders, prevent deletion
    IF @order_count > 0
    BEGIN
        RAISERROR('Không thể xóa loại bàn này vì có đơn hàng đang sử dụng bàn thuộc loại này.', 16, 1);
        RETURN;
    END
    
    -- If there are tables using this type, prevent deletion
    IF @ban_count > 0
    BEGIN
        RAISERROR('Không thể xóa loại bàn này vì có %d bàn đang sử dụng loại này.', 16, 1, @ban_count);
        RETURN;
    END
    
    -- If all checks pass, perform the deletion
    DELETE FROM dbo.LoaiBan
    WHERE loai_ban_id = @loai_ban_id;
END
GO

-- =============================================
-- Drop Triggers (if needed to recreate)
-- =============================================
/*
DROP TRIGGER IF EXISTS tr_LoaiBan_Update_Check;
DROP TRIGGER IF EXISTS tr_LoaiBan_Delete_Check;
*/
