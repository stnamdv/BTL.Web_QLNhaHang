-- Tạo bảng Layout nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Layout' AND xtype='U')
BEGIN
    CREATE TABLE Layout (
        layout_id INT IDENTITY(1,1) PRIMARY KEY,
        layout_name NVARCHAR(100) NOT NULL,
        grid_size INT NOT NULL DEFAULT 10,
        created_date DATETIME DEFAULT GETDATE(),
        updated_date DATETIME DEFAULT GETDATE(),
        is_active BIT DEFAULT 1
    )
END

-- Tạo bảng LayoutDetail nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LayoutDetail' AND xtype='U')
BEGIN
    CREATE TABLE LayoutDetail (
        layout_detail_id INT IDENTITY(1,1) PRIMARY KEY,
        layout_id INT NOT NULL,
        ban_id INT NOT NULL,
        position_x FLOAT NOT NULL,
        position_y FLOAT NOT NULL,
        created_date DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (layout_id) REFERENCES Layout(layout_id),
        FOREIGN KEY (ban_id) REFERENCES BanAn(ban_id)
    )
END

-- Stored Procedure: Lưu layout
CREATE OR ALTER PROCEDURE sp_SaveLayout
    @LayoutName NVARCHAR(100),
    @GridSize INT,
    @LayoutData NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @LayoutId INT;
    DECLARE @CurrentDate DATETIME = GETDATE();
    
    BEGIN TRANSACTION;
    
    TRY
        -- Tạo layout mới hoặc cập nhật layout hiện tại
        IF EXISTS (SELECT 1 FROM Layout WHERE layout_name = @LayoutName AND is_active = 1)
        BEGIN
            -- Cập nhật layout hiện tại
            UPDATE Layout 
            SET grid_size = @GridSize, 
                updated_date = @CurrentDate
            WHERE layout_name = @LayoutName AND is_active = 1;
            
            SELECT @LayoutId = layout_id FROM Layout WHERE layout_name = @LayoutName AND is_active = 1;
            
            -- Xóa chi tiết layout cũ
            DELETE FROM LayoutDetail WHERE layout_id = @LayoutId;
        END
        ELSE
        BEGIN
            -- Tạo layout mới
            INSERT INTO Layout (layout_name, grid_size, created_date, updated_date)
            VALUES (@LayoutName, @GridSize, @CurrentDate, @CurrentDate);
            
            SET @LayoutId = SCOPE_IDENTITY();
        END
        
        -- Lưu chi tiết layout từ JSON
        INSERT INTO LayoutDetail (layout_id, ban_id, position_x, position_y)
        SELECT 
            @LayoutId,
            CAST(JSON_VALUE(value, '$.id') AS INT),
            CAST(JSON_VALUE(value, '$.position.x') AS FLOAT),
            CAST(JSON_VALUE(value, '$.position.y') AS FLOAT)
        FROM OPENJSON(@LayoutData, '$.tables')
        WHERE JSON_VALUE(value, '$.id') IS NOT NULL
        AND CAST(JSON_VALUE(value, '$.id') AS INT) IS NOT NULL;
        
        COMMIT TRANSACTION;
        
        SELECT @LayoutId AS LayoutId, 'Layout saved successfully' AS Message;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END

-- Stored Procedure: Load layout
CREATE OR ALTER PROCEDURE sp_LoadLayout
    @LayoutName NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @LayoutName IS NULL
    BEGIN
        -- Load layout mặc định (layout đầu tiên)
        SELECT 
            l.layout_id,
            l.layout_name,
            l.grid_size,
            l.created_date,
            l.updated_date
        FROM Layout l
        WHERE l.is_active = 1
        ORDER BY l.layout_id
        OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY;
    END
    ELSE
    BEGIN
        -- Load layout theo tên
        SELECT 
            l.layout_id,
            l.layout_name,
            l.grid_size,
            l.created_date,
            l.updated_date
        FROM Layout l
        WHERE l.layout_name = @LayoutName AND l.is_active = 1;
    END
    
    -- Load chi tiết layout
    SELECT 
        ld.layout_detail_id,
        ld.layout_id,
        ld.ban_id,
        ld.position_x,
        ld.position_y,
        ba.so_hieu,
        ba.suc_chua,
        ba.loai_ban_id
    FROM LayoutDetail ld
    INNER JOIN BanAn ba ON ld.ban_id = ba.ban_id
    INNER JOIN Layout l ON ld.layout_id = l.layout_id
    WHERE (@LayoutName IS NULL AND l.is_active = 1) 
       OR (l.layout_name = @LayoutName AND l.is_active = 1)
    ORDER BY ld.layout_detail_id;
END

-- Stored Procedure: Lấy danh sách layout
CREATE OR ALTER PROCEDURE sp_GetLayoutList
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        layout_id,
        layout_name,
        grid_size,
        created_date,
        updated_date,
        (SELECT COUNT(*) FROM LayoutDetail WHERE layout_id = l.layout_id) AS table_count
    FROM Layout l
    WHERE is_active = 1
    ORDER BY updated_date DESC;
END

-- Stored Procedure: Xóa layout
CREATE OR ALTER PROCEDURE sp_DeleteLayout
    @LayoutId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    TRY
        -- Xóa chi tiết layout
        DELETE FROM LayoutDetail WHERE layout_id = @LayoutId;
        
        -- Xóa layout (soft delete)
        UPDATE Layout SET is_active = 0 WHERE layout_id = @LayoutId;
        
        COMMIT TRANSACTION;
        
        SELECT 'Layout deleted successfully' AS Message;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
