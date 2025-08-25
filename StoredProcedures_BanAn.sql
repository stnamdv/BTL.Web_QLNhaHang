-- =============================================
-- Stored Procedures for BanAn Table
-- =============================================

-- 1. Get All BanAn Records with LoaiBan info
-- =============================================
CREATE OR ALTER PROCEDURE sp_BanAn_GetAll
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
    ORDER BY b.ban_id;
END
GO

-- 2. Get BanAn by ID with LoaiBan info
-- =============================================
CREATE OR ALTER PROCEDURE sp_BanAn_GetById
    @Id INT
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
    WHERE b.ban_id = @Id;
END
GO

-- 3. Get BanAn by LoaiBan ID
-- =============================================
CREATE OR ALTER PROCEDURE sp_BanAn_GetByLoaiBanId
    @LoaiBanId INT
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
    WHERE b.loai_ban_id = @LoaiBanId
    ORDER BY b.ban_id;
END
GO

-- 4. Create New BanAn
-- =============================================
CREATE OR ALTER PROCEDURE sp_BanAn_Create
    @loai_ban_id INT,
    @so_hieu NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @existing_count INT;
    DECLARE @loai_ban_so_luong INT;
    DECLARE @new_id INT;
    
    -- Check if LoaiBan exists
    SELECT @loai_ban_so_luong = so_luong
    FROM dbo.LoaiBan
    WHERE loai_ban_id = @loai_ban_id;
    
    IF @loai_ban_so_luong IS NULL
    BEGIN
        RAISERROR(N'Loại bàn không tồn tại.', 16, 1);
        RETURN;
    END
    
    -- Check if so_hieu already exists
    SELECT @existing_count = COUNT(*)
    FROM dbo.BanAn
    WHERE so_hieu = @so_hieu;
    
    IF @existing_count > 0
    BEGIN
        RAISERROR(N'Số hiệu bàn đã tồn tại.', 16, 1);
        RETURN;
    END
    
    -- Check if we can create more tables of this type
    SELECT @existing_count = COUNT(*)
    FROM dbo.BanAn
    WHERE loai_ban_id = @loai_ban_id;
    
    IF @existing_count >= @loai_ban_so_luong
    BEGIN
        RAISERROR(N'Đã đạt giới hạn số lượng bàn cho loại bàn này.', 16, 1);
        RETURN;
    END
    
    -- Create new BanAn
    INSERT INTO dbo.BanAn (loai_ban_id, so_hieu)
    VALUES (@loai_ban_id, @so_hieu);
    
    -- Return the newly created ID
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ban_id;
END
GO

-- 5. Update BanAn
-- =============================================
CREATE OR ALTER PROCEDURE sp_BanAn_Update
    @ban_id INT,
    @loai_ban_id INT,
    @so_hieu NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @existing_count INT;
    DECLARE @loai_ban_so_luong INT;
    DECLARE @current_loai_ban_id INT;
    DECLARE @rows_affected INT;
    
    -- Check if BanAn exists
    SELECT @current_loai_ban_id = loai_ban_id
    FROM dbo.BanAn
    WHERE ban_id = @ban_id;
    
    IF @current_loai_ban_id IS NULL
    BEGIN
        RAISERROR(N'Bàn không tồn tại.', 16, 1);
        RETURN;
    END
    
    -- Check if LoaiBan exists
    SELECT @loai_ban_so_luong = so_luong
    FROM dbo.LoaiBan
    WHERE loai_ban_id = @loai_ban_id;
    
    IF @loai_ban_so_luong IS NULL
    BEGIN
        RAISERROR(N'Loại bàn không tồn tại.', 16, 1);
        RETURN;
    END
    
    -- Check if so_hieu already exists (excluding current record)
    SELECT @existing_count = COUNT(*)
    FROM dbo.BanAn
    WHERE so_hieu = @so_hieu AND ban_id != @ban_id;
    
    IF @existing_count > 0
    BEGIN
        RAISERROR(N'Số hiệu bàn đã tồn tại.', 16, 1);
        RETURN;
    END
    
    -- If changing loai_ban_id, check if new type can accommodate
    IF @current_loai_ban_id != @loai_ban_id
    BEGIN
        SELECT @existing_count = COUNT(*)
        FROM dbo.BanAn
        WHERE loai_ban_id = @loai_ban_id;
        
        IF @existing_count >= @loai_ban_so_luong
        BEGIN
            RAISERROR(N'Đã đạt giới hạn số lượng bàn cho loại bàn mới.', 16, 1);
            RETURN;
        END
    END
    
    -- Update BanAn
    UPDATE dbo.BanAn
    SET 
        loai_ban_id = @loai_ban_id,
        so_hieu = @so_hieu
    WHERE ban_id = @ban_id;
    
    -- Return the number of rows affected
    SELECT @@ROWCOUNT;
END
GO

-- 6. Delete BanAn
-- =============================================
CREATE OR ALTER PROCEDURE sp_BanAn_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @order_count INT;
    DECLARE @rows_affected INT;
    
    -- Check if there are any active orders using this table
    SELECT @order_count = COUNT(*)
    FROM dbo.[Order] o
    WHERE o.ban_id = @Id
    AND o.thoi_diem_dat >= DATEADD(day, -1, GETDATE()); -- Orders from last 24 hours
    
    IF @order_count > 0
    BEGIN
        RAISERROR(N'Không thể xóa bàn này vì có đơn hàng đang sử dụng.', 16, 1);
        RETURN;
    END
    
    -- Delete BanAn
    DELETE FROM dbo.BanAn
    WHERE ban_id = @Id;
    
    -- Return the number of rows affected
    SELECT @@ROWCOUNT;
END
GO

-- 7. Get BanAn Details with Usage Information
-- =============================================
CREATE OR ALTER PROCEDURE sp_BanAn_GetDetailsWithUsage
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        b.ban_id,
        b.loai_ban_id,
        b.so_hieu,
        lb.loai_ban_id as split_column,
        lb.suc_chua,
        lb.so_luong,
        (SELECT COUNT(*) FROM dbo.[Order] o WHERE o.ban_id = b.ban_id) as total_orders,
        (SELECT COUNT(*) FROM dbo.[Order] o WHERE o.ban_id = b.ban_id AND o.thoi_diem_dat >= DATEADD(day, -1, GETDATE())) as recent_orders
    FROM dbo.BanAn b
    INNER JOIN dbo.LoaiBan lb ON b.loai_ban_id = lb.loai_ban_id
    WHERE b.ban_id = @Id;
END
GO

-- 8. Check if BanAn can be updated
-- =============================================
CREATE OR ALTER PROCEDURE sp_BanAn_CanUpdate
    @ban_id INT,
    @loai_ban_id INT,
    @so_hieu NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @can_update BIT = 1;
    DECLARE @message NVARCHAR(500) = '';
    DECLARE @existing_count INT;
    DECLARE @loai_ban_so_luong INT;
    DECLARE @current_loai_ban_id INT;
    
    -- Check if BanAn exists
    SELECT @current_loai_ban_id = loai_ban_id
    FROM dbo.BanAn
    WHERE ban_id = @ban_id;
    
    IF @current_loai_ban_id IS NULL
    BEGIN
        SET @can_update = 0;
        SET @message = N'Bàn không tồn tại.';
    END
    ELSE
    BEGIN
        -- Check if LoaiBan exists
        SELECT @loai_ban_so_luong = so_luong
        FROM dbo.LoaiBan
        WHERE loai_ban_id = @loai_ban_id;
        
        IF @loai_ban_so_luong IS NULL
        BEGIN
            SET @can_update = 0;
            SET @message = N'Loại bàn không tồn tại.';
        END
        ELSE
        BEGIN
            -- Check if so_hieu already exists (excluding current record)
            SELECT @existing_count = COUNT(*)
            FROM dbo.BanAn
            WHERE so_hieu = @so_hieu AND ban_id != @ban_id;
            
            IF @existing_count > 0
            BEGIN
                SET @can_update = 0;
                SET @message = N'Số hiệu bàn đã tồn tại.';
            END
            ELSE
            BEGIN
                -- If changing loai_ban_id, check if new type can accommodate
                IF @current_loai_ban_id != @loai_ban_id
                BEGIN
                    SELECT @existing_count = COUNT(*)
                    FROM dbo.BanAn
                    WHERE loai_ban_id = @loai_ban_id;
                    
                    IF @existing_count >= @loai_ban_so_luong
                    BEGIN
                        SET @can_update = 0;
                        SET @message = N'Đã đạt giới hạn số lượng bàn cho loại bàn mới.';
                    END
                END
            END
        END
    END
    
    SELECT @can_update AS can_update, @message AS message;
END
GO

-- 9. Check if BanAn can be deleted
-- =============================================
CREATE OR ALTER PROCEDURE sp_BanAn_CanDelete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @can_delete BIT = 1;
    DECLARE @message NVARCHAR(500) = '';
    DECLARE @order_count INT;
    
    -- Check if BanAn exists
    IF NOT EXISTS (SELECT 1 FROM dbo.BanAn WHERE ban_id = @Id)
    BEGIN
        SET @can_delete = 0;
        SET @message = N'Bàn không tồn tại.';
    END
    ELSE
    BEGIN
        -- Check if there are any active orders using this table
        SELECT @order_count = COUNT(*)
        FROM dbo.[Order] o
        WHERE o.ban_id = @Id
        AND o.thoi_diem_dat >= DATEADD(day, -1, GETDATE()); -- Orders from last 24 hours
        
        IF @order_count > 0
        BEGIN
            SET @can_delete = 0;
            SET @message = N'Không thể xóa bàn này vì có đơn hàng đang sử dụng.';
        END
    END
    
    SELECT @can_delete AS can_delete, @message AS message;
END
GO

-- 10. Get Available BanAn for Order
-- =============================================
CREATE OR ALTER PROCEDURE sp_BanAn_GetAvailable
    @capacity INT = NULL
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
    WHERE (@capacity IS NULL OR lb.suc_chua >= @capacity)
    AND b.ban_id NOT IN (
        SELECT DISTINCT o.ban_id 
        FROM dbo.[Order] o 
        WHERE o.thoi_diem_dat >= DATEADD(day, -1, GETDATE())
        AND o.trang_thai IN ('pending', 'confirmed', 'preparing')
    )
    ORDER BY lb.suc_chua, b.ban_id;
END
GO
