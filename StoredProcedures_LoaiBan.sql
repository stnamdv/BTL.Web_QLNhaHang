-- =============================================
-- Stored Procedures for LoaiBan Table
-- =============================================

-- 1. Get All LoaiBan Records
-- =============================================
CREATE OR ALTER PROCEDURE sp_LoaiBan_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        loai_ban_id,
        suc_chua,
        so_luong
    FROM dbo.LoaiBan
    ORDER BY loai_ban_id;
END
GO

-- 2. Get LoaiBan by ID
-- =============================================
CREATE OR ALTER PROCEDURE sp_LoaiBan_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        loai_ban_id,
        suc_chua,
        so_luong
    FROM dbo.LoaiBan
    WHERE loai_ban_id = @Id;
END
GO

-- 3. Create New LoaiBan (Updated logic: merge by capacity)
-- =============================================
CREATE OR ALTER PROCEDURE sp_LoaiBan_Create
    @suc_chua INT,
    @so_luong INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @existing_id INT;
    DECLARE @new_id INT;
    
    -- Check if a record with the same capacity already exists
    SELECT @existing_id = loai_ban_id
    FROM dbo.LoaiBan
    WHERE suc_chua = @suc_chua;
    
    IF @existing_id IS NOT NULL
    BEGIN
        -- Update existing record by adding the new quantity
        UPDATE dbo.LoaiBan
        SET so_luong = so_luong + @so_luong
        WHERE loai_ban_id = @existing_id;
        
        -- Return the existing ID
        SELECT @existing_id AS loai_ban_id;
    END
    ELSE
    BEGIN
        -- Create new record if no existing record with same capacity
        INSERT INTO dbo.LoaiBan (suc_chua, so_luong)
        VALUES (@suc_chua, @so_luong);
        
        -- Return the newly created ID
        SELECT CAST(SCOPE_IDENTITY() AS INT) AS loai_ban_id;
    END
END
GO

-- 4. Update LoaiBan
-- =============================================
CREATE OR ALTER PROCEDURE sp_LoaiBan_Update
    @loai_ban_id INT,
    @suc_chua INT,
    @so_luong INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.LoaiBan
    SET 
        suc_chua = @suc_chua,
        so_luong = @so_luong
    WHERE loai_ban_id = @loai_ban_id;
    
    -- Return the number of rows affected
    SELECT @@ROWCOUNT;
END
GO

-- 5. Delete LoaiBan
-- =============================================
CREATE OR ALTER PROCEDURE sp_LoaiBan_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM dbo.LoaiBan
    WHERE loai_ban_id = @Id;
    
    -- Return the number of rows affected
    SELECT @@ROWCOUNT;
END
GO

-- =============================================
-- Optional: Additional Stored Procedures
-- =============================================

-- 6. Get LoaiBan by Capacity (Optional)
-- =============================================
CREATE OR ALTER PROCEDURE sp_LoaiBan_GetByCapacity
    @suc_chua INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        loai_ban_id,
        suc_chua,
        so_luong
    FROM dbo.LoaiBan
    WHERE suc_chua = @suc_chua
    ORDER BY loai_ban_id;
END
GO

-- 7. Get LoaiBan with Available Quantity (Optional)
-- =============================================
CREATE OR ALTER PROCEDURE sp_LoaiBan_GetAvailable
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        loai_ban_id,
        suc_chua,
        so_luong
    FROM dbo.LoaiBan
    WHERE so_luong > 0
    ORDER BY suc_chua, loai_ban_id;
END
GO

-- 8. Check if LoaiBan exists by Capacity (New)
-- =============================================
CREATE OR ALTER PROCEDURE sp_LoaiBan_ExistsByCapacity
    @suc_chua INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CASE 
            WHEN EXISTS (SELECT 1 FROM dbo.LoaiBan WHERE suc_chua = @suc_chua) 
            THEN 1 
            ELSE 0 
        END AS exists_flag;
END
GO

-- 9. Get LoaiBan Details with Usage Information (New)
-- =============================================
CREATE OR ALTER PROCEDURE sp_LoaiBan_GetDetailsWithUsage
    @loai_ban_id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get basic LoaiBan information
    SELECT 
        lb.loai_ban_id,
        lb.suc_chua,
        lb.so_luong,
        -- Count of tables using this type
        (SELECT COUNT(*) FROM dbo.BanAn WHERE loai_ban_id = lb.loai_ban_id) AS ban_count,
        -- Count of active orders using tables of this type
        (SELECT COUNT(*) 
         FROM dbo.[Order] o
         INNER JOIN dbo.BanAn b ON o.ban_id = b.ban_id
         WHERE b.loai_ban_id = lb.loai_ban_id
         AND o.thoi_diem_dat >= DATEADD(day, -1, GETDATE())) AS active_order_count
    FROM dbo.LoaiBan lb
    WHERE lb.loai_ban_id = @loai_ban_id;
END
GO

-- 10. Get Tables by LoaiBan ID (New)
-- =============================================
CREATE OR ALTER PROCEDURE sp_LoaiBan_GetTables
    @loai_ban_id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        b.ban_id,
        b.so_hieu,
        b.loai_ban_id,
        -- Check if table has active orders
        CASE 
            WHEN EXISTS (
                SELECT 1 FROM dbo.[Order] o 
                WHERE o.ban_id = b.ban_id 
                AND o.thoi_diem_dat >= DATEADD(day, -1, GETDATE())
            ) THEN 1 
            ELSE 0 
        END AS has_active_orders
    FROM dbo.BanAn b
    WHERE b.loai_ban_id = @loai_ban_id
    ORDER BY b.so_hieu;
END
GO

-- 11. Check if LoaiBan can be updated (New)
-- =============================================
CREATE OR ALTER PROCEDURE sp_LoaiBan_CanUpdate
    @loai_ban_id INT,
    @new_suc_chua INT,
    @new_so_luong INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @old_suc_chua INT;
    DECLARE @old_so_luong INT;
    DECLARE @ban_count INT;
    DECLARE @order_count INT;
    DECLARE @can_update BIT = 1;
    DECLARE @error_message NVARCHAR(500) = '';
    
    -- Get current values
    SELECT @old_suc_chua = suc_chua, @old_so_luong = so_luong
    FROM dbo.LoaiBan
    WHERE loai_ban_id = @loai_ban_id;
    
    -- Count tables using this type
    SELECT @ban_count = COUNT(*)
    FROM dbo.BanAn
    WHERE loai_ban_id = @loai_ban_id;
    
    -- Count active orders
    SELECT @order_count = COUNT(*)
    FROM dbo.[Order] o
    INNER JOIN dbo.BanAn b ON o.ban_id = b.ban_id
    WHERE b.loai_ban_id = @loai_ban_id
    AND o.thoi_diem_dat >= DATEADD(day, -1, GETDATE());
    
    -- Check conditions
    IF @order_count > 0
    BEGIN
        SET @can_update = 0;
        SET @error_message = N'Không thể sửa loại bàn này vì có đơn hàng đang sử dụng bàn thuộc loại này.';
    END
    ELSE IF @new_so_luong < @old_so_luong AND @ban_count > @new_so_luong
    BEGIN
        SET @can_update = 0;
        SET @error_message = CONCAT(N'Không thể giảm số lượng xuống ', @new_so_luong, N' vì hiện có ', @ban_count, N' bàn đang sử dụng loại này.');
    END
    ELSE IF @new_suc_chua != @old_suc_chua AND @ban_count > 0
    BEGIN
        SET @can_update = 0;
        SET @error_message = CONCAT(N'Không thể thay đổi sức chứa vì có ', @ban_count, ' bàn đang sử dụng loại này.');
    END
    
    -- Return result
    SELECT 
        @can_update AS can_update,
        @error_message AS error_message,
        @ban_count AS ban_count,
        @order_count AS active_order_count;
END
GO

-- 12. Check if LoaiBan can be deleted (New)
-- =============================================
CREATE OR ALTER PROCEDURE sp_LoaiBan_CanDelete
    @loai_ban_id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ban_count INT;
    DECLARE @order_count INT;
    DECLARE @can_delete BIT = 1;
    DECLARE @error_message NVARCHAR(500) = '';
    
    -- Count tables using this type
    SELECT @ban_count = COUNT(*)
    FROM dbo.BanAn
    WHERE loai_ban_id = @loai_ban_id;
    
    -- Count active orders
    SELECT @order_count = COUNT(*)
    FROM dbo.[Order] o
    INNER JOIN dbo.BanAn b ON o.ban_id = b.ban_id
    WHERE b.loai_ban_id = @loai_ban_id
    AND o.thoi_diem_dat >= DATEADD(day, -1, GETDATE());
    
    -- Check conditions
    IF @order_count > 0
    BEGIN
        SET @can_delete = 0;
        SET @error_message = N'Không thể xóa loại bàn này vì có đơn hàng đang sử dụng bàn thuộc loại này.';
    END
    ELSE IF @ban_count > 0
    BEGIN
        SET @can_delete = 0;
        SET @error_message = CONCAT(N'Không thể xóa loại bàn này vì có ', @ban_count, N' bàn đang sử dụng loại này.');
    END
    
    -- Return result
    SELECT 
        @can_delete AS can_delete,
        @error_message AS error_message,
        @ban_count AS ban_count,
        @order_count AS active_order_count;
END
GO

-- =============================================
-- Drop Procedures (if needed to recreate)
-- =============================================
/*
DROP PROCEDURE IF EXISTS sp_LoaiBan_GetAll;
DROP PROCEDURE IF EXISTS sp_LoaiBan_GetById;
DROP PROCEDURE IF EXISTS sp_LoaiBan_Create;
DROP PROCEDURE IF EXISTS sp_LoaiBan_Update;
DROP PROCEDURE IF EXISTS sp_LoaiBan_Delete;
DROP PROCEDURE IF EXISTS sp_LoaiBan_GetByCapacity;
DROP PROCEDURE IF EXISTS sp_LoaiBan_GetAvailable;
DROP PROCEDURE IF EXISTS sp_LoaiBan_ExistsByCapacity;
DROP PROCEDURE IF EXISTS sp_LoaiBan_GetDetailsWithUsage;
DROP PROCEDURE IF EXISTS sp_LoaiBan_GetTables;
DROP PROCEDURE IF EXISTS sp_LoaiBan_CanUpdate;
DROP PROCEDURE IF EXISTS sp_LoaiBan_CanDelete;
*/
