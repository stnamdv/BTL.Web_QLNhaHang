USE [NhaHang]
GO
/****** Object:  StoredProcedure [dbo].[sp_LoaiNhanVien_CanDelete]    Script Date: 9/18/2025 11:28:16 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_LoaiNhanVien_CanDelete]
    @loai_nv_id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @can_delete BIT = 1;
    DECLARE @error_message NVARCHAR(500) = '';
    DECLARE @employee_count INT = 0;
    DECLARE @active_employee_count INT = 0;
    
    -- Validate input parameters
    IF @loai_nv_id IS NULL OR @loai_nv_id <= 0
    BEGIN
        SET @can_delete = 0;
        SET @error_message = N'ID loại nhân viên không hợp lệ.';
    END
    ELSE IF NOT EXISTS (SELECT 1 FROM dbo.LoaiNhanVien WHERE loai_nv_id = @loai_nv_id)
    BEGIN
        SET @can_delete = 0;
        SET @error_message = N'Loại nhân viên không tồn tại.';
    END
    ELSE
    BEGIN
        -- Check if there are any employees using this type
        SELECT @employee_count = COUNT(*)
        FROM dbo.NhanVien nv
        WHERE nv.loai_nv_id = @loai_nv_id;
        
        -- Check active employees
        SELECT @active_employee_count = COUNT(*)
        FROM dbo.NhanVien nv
        WHERE nv.loai_nv_id = @loai_nv_id AND nv.trang_thai = 'ACTIVE';
        
        IF @employee_count > 0
        BEGIN
            SET @can_delete = 0;
            SET @error_message = N'Không thể xóa loại nhân viên này vì có nhân viên đang sử dụng.';
        END
        ELSE
        BEGIN
            SET @can_delete = 1;
            SET @error_message = N'Có thể xóa loại nhân viên này.';
        END
    END
    
    SELECT @can_delete AS can_delete, @error_message AS [error_message], @employee_count AS employee_count, @active_employee_count AS active_employee_count;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_LoaiNhanVien_CanUpdate]    Script Date: 9/18/2025 11:28:16 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 7. Check if LoaiNhanVien can be updated
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_LoaiNhanVien_CanUpdate]
    @loai_nv_id INT,
    @luong_co_ban DECIMAL(12,2)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @can_update BIT = 1;
    DECLARE @message NVARCHAR(500) = '';
    DECLARE @employee_count INT = 0;
    DECLARE @active_employee_count INT = 0;
    
    -- Validate input parameters
    IF @loai_nv_id IS NULL OR @loai_nv_id <= 0
    BEGIN
        SET @can_update = 0;
        SET @message = N'ID loại nhân viên không hợp lệ.';
    END
    ELSE IF @luong_co_ban IS NULL OR @luong_co_ban < 0
    BEGIN
        SET @can_update = 0;
        SET @message = N'Lương cơ bản không được âm.';
    END
    ELSE IF NOT EXISTS (SELECT 1 FROM dbo.LoaiNhanVien WHERE loai_nv_id = @loai_nv_id)
    BEGIN
        SET @can_update = 0;
        SET @message = N'Loại nhân viên không tồn tại.';
    END
    ELSE
    BEGIN
        -- Lấy số lượng nhân viên
        SELECT @employee_count = COUNT(1)
        FROM dbo.NhanVien nv
        WHERE nv.loai_nv_id = @loai_nv_id;
        
        -- Lấy số lượng nhân viên đang hoạt động
        SELECT @active_employee_count = COUNT(1)
        FROM dbo.NhanVien nv
        WHERE nv.loai_nv_id = @loai_nv_id AND nv.trang_thai = 'ACTIVE';
    END
    
    SELECT @can_update AS can_update, @message AS message, @employee_count AS employee_count, @active_employee_count AS active_employee_count;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_LoaiNhanVien_Create]    Script Date: 9/18/2025 11:28:16 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 3. Create New LoaiNhanVien
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_LoaiNhanVien_Create]
    @loai_nv NVARCHAR(20),
    @luong_co_ban DECIMAL(12,2)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @existing_count INT;
    
    -- Validate input parameters
    IF @loai_nv IS NULL OR LTRIM(RTRIM(@loai_nv)) = ''
    BEGIN
        RAISERROR(N'Loại nhân viên không được để trống.', 16, 1);
        RETURN;
    END
    
    IF @luong_co_ban IS NULL OR @luong_co_ban < 0
    BEGIN
        RAISERROR(N'Lương cơ bản không được âm.', 16, 1);
        RETURN;
    END
    
    -- Check if LoaiNhanVien already exists
    SELECT @existing_count = COUNT(1)
    FROM dbo.LoaiNhanVien
    WHERE loai_nv = @loai_nv;
    
    IF @existing_count > 0
    BEGIN
        RAISERROR(N'Loại nhân viên đã tồn tại.', 16, 1);
        RETURN;
    END
    
    -- Create new LoaiNhanVien
    INSERT INTO dbo.LoaiNhanVien (loai_nv, luong_co_ban)
    VALUES (@loai_nv, @luong_co_ban);
    
    -- Return success message
    SELECT N'Tạo loại nhân viên thành công.' AS message;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_LoaiNhanVien_Delete]    Script Date: 9/18/2025 11:28:16 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 5. Delete LoaiNhanVien
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_LoaiNhanVien_Delete]
    @loai_nv_id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @employee_count INT;
    DECLARE @rows_affected INT;
    
    -- Validate input parameters
    IF @loai_nv_id IS NULL OR @loai_nv_id <= 0
    BEGIN
        RAISERROR(N'ID loại nhân viên không hợp lệ.', 16, 1);
        RETURN;
    END
    
    -- Check if LoaiNhanVien exists
    IF NOT EXISTS (SELECT 1 FROM dbo.LoaiNhanVien WHERE loai_nv_id = @loai_nv_id)
    BEGIN
        RAISERROR(N'Loại nhân viên không tồn tại.', 16, 1);
        RETURN;
    END
    
    -- Check if there are any employees using this type
    SELECT @employee_count = COUNT(1)
    FROM dbo.NhanVien nv
    WHERE nv.loai_nv_id = @loai_nv_id;
    
    IF @employee_count > 0
    BEGIN
        RAISERROR(N'Không thể xóa loại nhân viên này vì có nhân viên đang sử dụng.', 16, 1);
        RETURN;
    END
    
    -- Delete LoaiNhanVien
    DELETE FROM dbo.LoaiNhanVien
    WHERE loai_nv_id = @loai_nv_id;
    
    -- Return the number of rows affected
    SELECT @@ROWCOUNT AS rows_affected;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_LoaiNhanVien_GetAll]    Script Date: 9/18/2025 11:28:16 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Stored Procedures for LoaiNhanVien Table
-- =============================================

-- 1. Get All LoaiNhanVien Records
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_LoaiNhanVien_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        loai_nv_id,
        loai_nv,
        luong_co_ban
    FROM dbo.LoaiNhanVien
    ORDER BY loai_nv;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_LoaiNhanVien_GetAvailable]    Script Date: 9/18/2025 11:28:16 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 9. Get Available LoaiNhanVien Types (not used by any employee)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_LoaiNhanVien_GetAvailable]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        lnv.loai_nv_id,
        lnv.loai_nv,
        lnv.luong_co_ban
    FROM dbo.LoaiNhanVien lnv
    WHERE lnv.loai_nv_id NOT IN (
        SELECT DISTINCT nv.loai_nv_id 
        FROM dbo.NhanVien nv 
        WHERE nv.trang_thai = 'ACTIVE'
    )
    ORDER BY lnv.loai_nv;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_LoaiNhanVien_GetByType]    Script Date: 9/18/2025 11:28:16 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 2. Get LoaiNhanVien by Type
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_LoaiNhanVien_GetByType]
    @LoaiNvId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        loai_nv_id,
        loai_nv,
        luong_co_ban
    FROM dbo.LoaiNhanVien
    WHERE loai_nv_id = @LoaiNvId;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_LoaiNhanVien_GetDetailsWithUsage]    Script Date: 9/18/2025 11:28:16 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_LoaiNhanVien_GetDetailsWithUsage]
    @loai_nv_id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        lnv.loai_nv_id,
        lnv.loai_nv,
        lnv.luong_co_ban,
        (SELECT COUNT(1) FROM dbo.NhanVien nv WHERE nv.loai_nv_id = lnv.loai_nv_id) as total_employees,
        (SELECT COUNT(1) FROM dbo.NhanVien nv WHERE nv.loai_nv_id = lnv.loai_nv_id AND nv.trang_thai = 'ACTIVE') as active_employees
    FROM dbo.LoaiNhanVien lnv
    WHERE lnv.loai_nv_id = @loai_nv_id;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_LoaiNhanVien_Update]    Script Date: 9/18/2025 11:28:16 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 4. Update LoaiNhanVien
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_LoaiNhanVien_Update]
    @loai_nv_id INT,
    @loai_nv NVARCHAR(20),
    @luong_co_ban DECIMAL(12,2)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @rows_affected INT;
    
    -- Validate input parameters
    IF @loai_nv_id IS NULL OR @loai_nv_id <= 0
    BEGIN
        RAISERROR(N'ID loại nhân viên không hợp lệ.', 16, 1);
        RETURN;
    END
    
    IF @loai_nv IS NULL OR LTRIM(RTRIM(@loai_nv)) = ''
    BEGIN
        RAISERROR(N'Loại nhân viên không được để trống.', 16, 1);
        RETURN;
    END
    
    IF @luong_co_ban IS NULL OR @luong_co_ban < 0
    BEGIN
        RAISERROR(N'Lương cơ bản không được âm.', 16, 1);
        RETURN;
    END
    
    -- Check if LoaiNhanVien exists
    IF NOT EXISTS (SELECT 1 FROM dbo.LoaiNhanVien WHERE loai_nv_id = @loai_nv_id)
    BEGIN
        RAISERROR(N'Loại nhân viên không tồn tại.', 16, 1);
        RETURN;
    END
    
    -- Update LoaiNhanVien
    UPDATE dbo.LoaiNhanVien
    SET loai_nv = @loai_nv,
        luong_co_ban = @luong_co_ban
    WHERE loai_nv_id = @loai_nv_id;
    
    -- Return the number of rows affected
    SELECT @@ROWCOUNT AS rows_affected;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_LoaiNhanVien_ValidateType]    Script Date: 9/18/2025 11:28:16 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 10. Validate LoaiNhanVien Type
