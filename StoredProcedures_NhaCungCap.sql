USE NhaHang
GO

-- =========================================================
-- 1. sp_NhaCungCap_Create - Tạo nhà cung cấp mới với validation
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_NhaCungCap_Create
    @Ten NVARCHAR(160),
    @DiaChi NVARCHAR(MAX) = NULL,
    @Sdt NVARCHAR(20) = NULL,
    @NccId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validation 1: Kiểm tra tên nhà cung cấp có trùng không
        IF EXISTS (SELECT 1 FROM dbo.NhaCungCap WHERE ten = @Ten)
        BEGIN
            RAISERROR(N'Tên nhà cung cấp đã tồn tại.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 2: Kiểm tra tên không được rỗng
        IF LTRIM(RTRIM(@Ten)) = N''
        BEGIN
            RAISERROR(N'Tên nhà cung cấp không được để trống.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 3: Kiểm tra số điện thoại nếu có
        IF @Sdt IS NOT NULL AND LTRIM(RTRIM(@Sdt)) != N''
        BEGIN
            -- Kiểm tra format số điện thoại (chỉ chứa số, dấu +, dấu -)
            IF @Sdt NOT LIKE N'%[0-9+%-]%' OR LEN(LTRIM(RTRIM(@Sdt))) < 8
            BEGIN
                RAISERROR(N'Số điện thoại không hợp lệ. Phải có ít nhất 8 ký tự và chỉ chứa số, dấu +, dấu -.', 16, 1);
                ROLLBACK TRANSACTION;
                RETURN;
            END
        END
        
        -- Tạo nhà cung cấp mới
        INSERT INTO dbo.NhaCungCap (ten, dia_chi, sdt)
        VALUES (@Ten, @DiaChi, @Sdt);
        
        SET @NccId = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =========================================================
-- 2. sp_NhaCungCap_Update - Cập nhật nhà cung cấp với validation
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_NhaCungCap_Update
    @NccId INT,
    @Ten NVARCHAR(160),
    @DiaChi NVARCHAR(MAX) = NULL,
    @Sdt NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validation 1: Kiểm tra nhà cung cấp có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.NhaCungCap WHERE ncc_id = @NccId)
        BEGIN
            RAISERROR(N'Nhà cung cấp không tồn tại.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 2: Kiểm tra tên nhà cung cấp có trùng với nhà cung cấp khác không
        IF EXISTS (SELECT 1 FROM dbo.NhaCungCap WHERE ten = @Ten AND ncc_id != @NccId)
        BEGIN
            RAISERROR(N'Tên nhà cung cấp đã tồn tại ở nhà cung cấp khác.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 3: Kiểm tra tên không được rỗng
        IF LTRIM(RTRIM(@Ten)) = N''
        BEGIN
            RAISERROR(N'Tên nhà cung cấp không được để trống.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 4: Kiểm tra số điện thoại nếu có
        IF @Sdt IS NOT NULL AND LTRIM(RTRIM(@Sdt)) != N''
        BEGIN
            -- Kiểm tra format số điện thoại
            IF @Sdt NOT LIKE N'%[0-9+%-]%' OR LEN(LTRIM(RTRIM(@Sdt))) < 8
            BEGIN
                RAISERROR(N'Số điện thoại không hợp lệ. Phải có ít nhất 8 ký tự và chỉ chứa số, dấu +, dấu -.', 16, 1);
                ROLLBACK TRANSACTION;
                RETURN;
            END
        END
        
        -- Cập nhật thông tin nhà cung cấp
        UPDATE dbo.NhaCungCap 
        SET ten = @Ten,
            dia_chi = @DiaChi,
            sdt = @Sdt
        WHERE ncc_id = @NccId;
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =========================================================
-- 3. sp_NhaCungCap_Delete - Xóa nhà cung cấp với validation
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_NhaCungCap_Delete
    @NccId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validation 1: Kiểm tra nhà cung cấp có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.NhaCungCap WHERE ncc_id = @NccId)
        BEGIN
            RAISERROR(N'Nhà cung cấp không tồn tại.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END      
        
        -- Xóa nhà cung cấp
        DELETE FROM dbo.NhaCungCap WHERE ncc_id = @NccId;
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =========================================================
-- 4. sp_NhaCungCap_GetAll - Lấy danh sách nhà cung cấp
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_NhaCungCap_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(160) = NULL,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Đếm tổng số records
    SELECT @TotalCount = COUNT(1)
    FROM dbo.NhaCungCap ncc
    WHERE (@SearchTerm IS NULL OR ncc.ten LIKE N'%' + @SearchTerm + N'%' OR ncc.dia_chi LIKE N'%' + @SearchTerm + N'%');
    
    -- Lấy dữ liệu phân trang
    SELECT 
        ncc.ncc_id,
        ncc.ten,
        ncc.dia_chi,
        ncc.sdt
    FROM dbo.NhaCungCap ncc
    WHERE (@SearchTerm IS NULL OR ncc.ten LIKE N'%' + @SearchTerm + N'%' OR ncc.dia_chi LIKE N'%' + @SearchTerm + N'%')
    GROUP BY ncc.ncc_id, ncc.ten, ncc.dia_chi, ncc.sdt
    ORDER BY ncc.ten
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =========================================================
-- 5. sp_NhaCungCap_GetById - Lấy thông tin nhà cung cấp theo ID
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_NhaCungCap_GetById
    @NccId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra nhà cung cấp có tồn tại không
    IF NOT EXISTS (SELECT 1 FROM dbo.NhaCungCap WHERE ncc_id = @NccId)
    BEGIN
        RAISERROR(N'Nhà cung cấp không tồn tại.', 16, 1);
        RETURN;
    END
    
    -- Lấy thông tin nhà cung cấp
    SELECT 
        ncc.ncc_id,
        ncc.ten,
        ncc.dia_chi,
        ncc.sdt
    FROM dbo.NhaCungCap ncc
    WHERE ncc.ncc_id = @NccId;
   
END
GO