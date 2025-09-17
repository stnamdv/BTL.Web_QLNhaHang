/* =========================================================
   STORED PROCEDURES CHO QUẢN LÝ MÓN ĂN
   Bao gồm validation nghiệp vụ và xử lý công thức
   ========================================================= */

USE NhaHang
GO

-- =========================================================
-- 1. SP_GetAllMon - Lấy danh sách tất cả món ăn
-- =========================================================
DROP PROCEDURE IF EXISTS dbo.SP_GetAllMon
GO
CREATE PROCEDURE dbo.SP_GetAllMon
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(120) = NULL,
    @LoaiMon NVARCHAR(12) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Đếm tổng số records
    DECLARE @TotalCount INT;
    SELECT @TotalCount = COUNT(*)
    FROM dbo.Mon m
    WHERE (@SearchTerm IS NULL OR m.ten_mon LIKE N'%' + @SearchTerm + N'%' OR m.ma_mon LIKE N'%' + @SearchTerm + N'%')
      AND (@LoaiMon IS NULL OR m.loai_mon = @LoaiMon);
    
    -- Lấy dữ liệu phân trang
    SELECT 
        m.mon_id,
        m.ma_mon,
        m.ten_mon,
        m.loai_mon,
        m.gia,
        @TotalCount AS TotalCount
    FROM dbo.Mon m
    WHERE (@SearchTerm IS NULL OR m.ten_mon LIKE N'%' + @SearchTerm + N'%' OR m.ma_mon LIKE N'%' + @SearchTerm + N'%')
      AND (@LoaiMon IS NULL OR m.loai_mon = @LoaiMon)
    ORDER BY m.ma_mon
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =========================================================
-- 2. SP_GetMonById - Lấy thông tin món ăn theo ID
-- =========================================================
DROP PROCEDURE IF EXISTS dbo.SP_GetMonById
GO
CREATE PROCEDURE dbo.SP_GetMonById
    @MonId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra món có tồn tại không
    IF NOT EXISTS (SELECT 1 FROM dbo.Mon WHERE mon_id = @MonId)
    BEGIN
        RAISERROR(N'Món ăn không tồn tại.', 16, 1);
        RETURN;
    END
    
    -- Lấy thông tin món
    SELECT 
        m.mon_id,
        m.ma_mon,
        m.ten_mon,
        m.loai_mon,
        m.gia
    FROM dbo.Mon m
    WHERE m.mon_id = @MonId;
    
    -- Lấy công thức của món
    SELECT 
        ct.nl_id,
        nl.ten AS ten_nguyen_lieu,
        nl.don_vi,
        ct.dinh_luong
    FROM dbo.CongThuc ct
    JOIN dbo.NguyenLieu nl ON nl.nl_id = ct.nl_id
    WHERE ct.mon_id = @MonId
    ORDER BY nl.ten;
END
GO

-- =========================================================
-- 3. SP_CreateMon - Tạo món ăn mới với validation
-- =========================================================
DROP PROCEDURE IF EXISTS dbo.SP_CreateMon
GO
CREATE PROCEDURE dbo.SP_CreateMon
    @MaMon NVARCHAR(20),
    @TenMon NVARCHAR(120),
    @LoaiMon NVARCHAR(12),
    @Gia DECIMAL(12,2),
    @CongThucJson NVARCHAR(MAX) = NULL, -- JSON chứa danh sách nguyên liệu
    @MonId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validation 1: Kiểm tra mã món có trùng không
        IF EXISTS (SELECT 1 FROM dbo.Mon WHERE ma_mon = @MaMon)
        BEGIN
            RAISERROR(N'Mã món đã tồn tại.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 2: Kiểm tra loại món hợp lệ
        IF @LoaiMon NOT IN (N'KHAI_VI', N'MON_CHINH', N'TRANG_MIENG')
        BEGIN
            RAISERROR(N'Loại món không hợp lệ. Chỉ chấp nhận: KHAI_VI, MON_CHINH, TRANG_MIENG', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 3: Kiểm tra mã món có đúng format theo loại không
        IF NOT (
            (@LoaiMon = N'KHAI_VI' AND @MaMon LIKE N'KV-%') OR
            (@LoaiMon = N'MON_CHINH' AND @MaMon LIKE N'MC-%') OR
            (@LoaiMon = N'TRANG_MIENG' AND @MaMon LIKE N'TM-%')
        )
        BEGIN
            RAISERROR(N'Mã món phải bắt đầu bằng KV- (khai vị), MC- (món chính), hoặc TM- (tráng miệng) tương ứng với loại món.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 4: Kiểm tra giá không âm
        IF @Gia < 0
        BEGIN
            RAISERROR(N'Giá món không được âm.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Tạo món mới
        INSERT INTO dbo.Mon (ma_mon, ten_mon, loai_mon, gia)
        VALUES (@MaMon, @TenMon, @LoaiMon, @Gia);
        
        SET @MonId = SCOPE_IDENTITY();
        
        -- Xử lý công thức nếu có
        IF @CongThucJson IS NOT NULL AND @CongThucJson != N''
        BEGIN
            -- Parse JSON và insert công thức
            INSERT INTO dbo.CongThuc (mon_id, nl_id, dinh_luong)
            SELECT 
                @MonId,
                JSON_VALUE(value, '$.nl_id') AS nl_id,
                CAST(JSON_VALUE(value, '$.dinh_luong') AS DECIMAL(14,3)) AS dinh_luong
            FROM OPENJSON(@CongThucJson)
            WHERE JSON_VALUE(value, '$.nl_id') IS NOT NULL
              AND JSON_VALUE(value, '$.dinh_luong') IS NOT NULL
              AND CAST(JSON_VALUE(value, '$.dinh_luong') AS DECIMAL(14,3)) > 0;
        END
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =========================================================
-- 4. SP_UpdateMon - Cập nhật món ăn với validation
-- =========================================================
DROP PROCEDURE IF EXISTS dbo.SP_UpdateMon
GO
CREATE PROCEDURE dbo.SP_UpdateMon
    @MonId INT,
    @MaMon NVARCHAR(20),
    @TenMon NVARCHAR(120),
    @LoaiMon NVARCHAR(12),
    @Gia DECIMAL(12,2),
    @CongThucJson NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validation 1: Kiểm tra món có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.Mon WHERE mon_id = @MonId)
        BEGIN
            RAISERROR(N'Món ăn không tồn tại.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 2: Kiểm tra mã món có trùng với món khác không
        IF EXISTS (SELECT 1 FROM dbo.Mon WHERE ma_mon = @MaMon AND mon_id != @MonId)
        BEGIN
            RAISERROR(N'Mã món đã tồn tại ở món khác.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 3: Kiểm tra loại món hợp lệ
        IF @LoaiMon NOT IN (N'KHAI_VI', N'MON_CHINH', N'TRANG_MIENG')
        BEGIN
            RAISERROR(N'Loại món không hợp lệ. Chỉ chấp nhận: KHAI_VI, MON_CHINH, TRANG_MIENG', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 4: Kiểm tra mã món có đúng format theo loại không
        IF NOT (
            (@LoaiMon = N'KHAI_VI' AND @MaMon LIKE N'KV-%') OR
            (@LoaiMon = N'MON_CHINH' AND @MaMon LIKE N'MC-%') OR
            (@LoaiMon = N'TRANG_MIENG' AND @MaMon LIKE N'TM-%')
        )
        BEGIN
            RAISERROR(N'Mã món phải bắt đầu bằng KV- (khai vị), MC- (món chính), hoặc TM- (tráng miệng) tương ứng với loại món.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 5: Kiểm tra giá không âm
        IF @Gia < 0
        BEGIN
            RAISERROR(N'Giá món không được âm.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 6: Kiểm tra món có đang được sử dụng trong đơn hàng không
        IF EXISTS (SELECT 1 FROM dbo.OrderItem WHERE mon_id = @MonId)
        BEGIN
            RAISERROR(N'Không thể cập nhật món đang được sử dụng trong đơn hàng.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Cập nhật thông tin món
        UPDATE dbo.Mon 
        SET ma_mon = @MaMon,
            ten_mon = @TenMon,
            loai_mon = @LoaiMon,
            gia = @Gia
        WHERE mon_id = @MonId;
        
        -- Xử lý công thức nếu có
        IF @CongThucJson IS NOT NULL AND @CongThucJson != N''
        BEGIN
            -- Xóa công thức cũ
            DELETE FROM dbo.CongThuc WHERE mon_id = @MonId;
            
            -- Thêm công thức mới
            INSERT INTO dbo.CongThuc (mon_id, nl_id, dinh_luong)
            SELECT 
                @MonId,
                JSON_VALUE(value, '$.nl_id') AS nl_id,
                CAST(JSON_VALUE(value, '$.dinh_luong') AS DECIMAL(14,3)) AS dinh_luong
            FROM OPENJSON(@CongThucJson)
            WHERE JSON_VALUE(value, '$.nl_id') IS NOT NULL
              AND JSON_VALUE(value, '$.dinh_luong') IS NOT NULL
              AND CAST(JSON_VALUE(value, '$.dinh_luong') AS DECIMAL(14,3)) > 0;
        END
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =========================================================
-- 5. SP_DeleteMon - Xóa món ăn với validation
-- =========================================================
DROP PROCEDURE IF EXISTS dbo.SP_DeleteMon
GO
CREATE PROCEDURE dbo.SP_DeleteMon
    @MonId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validation 1: Kiểm tra món có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.Mon WHERE mon_id = @MonId)
        BEGIN
            RAISERROR(N'Món ăn không tồn tại.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 2: Kiểm tra món có đang được sử dụng trong đơn hàng không
        IF EXISTS (SELECT 1 FROM dbo.OrderItem WHERE mon_id = @MonId)
        BEGIN
            RAISERROR(N'Không thể xóa món đang được sử dụng trong đơn hàng.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 3: Kiểm tra món có trong thực đơn không
        IF EXISTS (SELECT 1 FROM dbo.ThucDon_Mon WHERE mon_id = @MonId)
        BEGIN
            RAISERROR(N'Không thể xóa món đang có trong thực đơn.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Xóa công thức trước (do foreign key constraint)
        DELETE FROM dbo.CongThuc WHERE mon_id = @MonId;
        
        -- Xóa món
        DELETE FROM dbo.Mon WHERE mon_id = @MonId;
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =========================================================
-- 6. SP_GetMonWithCongThuc - Lấy món ăn kèm công thức chi tiết
-- =========================================================
DROP PROCEDURE IF EXISTS dbo.SP_GetMonWithCongThuc
GO
CREATE PROCEDURE dbo.SP_GetMonWithCongThuc
    @MonId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra món có tồn tại không
    IF NOT EXISTS (SELECT 1 FROM dbo.Mon WHERE mon_id = @MonId)
    BEGIN
        RAISERROR(N'Món ăn không tồn tại.', 16, 1);
        RETURN;
    END
    
    -- Lấy thông tin món
    SELECT 
        m.mon_id,
        m.ma_mon,
        m.ten_mon,
        m.loai_mon,
        m.gia
    FROM dbo.Mon m
    WHERE m.mon_id = @MonId;
    
    -- Lấy công thức chi tiết
    SELECT 
        ct.nl_id,
        nl.ten AS ten_nguyen_lieu,
        nl.don_vi,
        nl.nguon_goc,
        ct.dinh_luong,
        -- Tính tổng giá trị nguyên liệu (nếu có thông tin nhập kho)
        ISNULL((
            SELECT TOP 1 nl_ncc.don_gia 
            FROM dbo.NL_NCC nl_ncc 
            WHERE nl_ncc.nl_id = nl.nl_id 
            ORDER BY nl_ncc.ngay_nhap DESC
        ), 0) AS gia_nguyen_lieu
    FROM dbo.CongThuc ct
    JOIN dbo.NguyenLieu nl ON nl.nl_id = ct.nl_id
    WHERE ct.mon_id = @MonId
    ORDER BY nl.ten;
END
GO

-- =========================================================
-- 7. SP_GetAllNguyenLieu - Lấy danh sách nguyên liệu
-- =========================================================
DROP PROCEDURE IF EXISTS dbo.SP_GetAllNguyenLieu
GO
CREATE PROCEDURE dbo.SP_GetAllNguyenLieu
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(160) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Đếm tổng số records
    DECLARE @TotalCount INT;
    SELECT @TotalCount = COUNT(*)
    FROM dbo.NguyenLieu nl
    WHERE (@SearchTerm IS NULL OR nl.ten LIKE N'%' + @SearchTerm + N'%');
    
    -- Lấy dữ liệu phân trang
    SELECT 
        nl.nl_id,
        nl.ten,
        nl.don_vi,
        nl.nguon_goc,
        @TotalCount AS TotalCount
    FROM dbo.NguyenLieu nl
    WHERE (@SearchTerm IS NULL OR nl.ten LIKE N'%' + @SearchTerm + N'%')
    ORDER BY nl.ten
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =========================================================
-- 8. SP_GetNguyenLieuById - Lấy thông tin nguyên liệu theo ID
-- =========================================================
DROP PROCEDURE IF EXISTS dbo.SP_GetNguyenLieuById
GO
CREATE PROCEDURE dbo.SP_GetNguyenLieuById
    @NlId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra nguyên liệu có tồn tại không
    IF NOT EXISTS (SELECT 1 FROM dbo.NguyenLieu WHERE nl_id = @NlId)
    BEGIN
        RAISERROR(N'Nguyên liệu không tồn tại.', 16, 1);
        RETURN;
    END
    
    -- Lấy thông tin nguyên liệu
    SELECT 
        nl.nl_id,
        nl.ten,
        nl.don_vi,
        nl.nguon_goc
    FROM dbo.NguyenLieu nl
    WHERE nl.nl_id = @NlId;
    
    -- Lấy danh sách món sử dụng nguyên liệu này
    SELECT 
        m.mon_id,
        m.ma_mon,
        m.ten_mon,
        m.loai_mon,
        ct.dinh_luong
    FROM dbo.CongThuc ct
    JOIN dbo.Mon m ON m.mon_id = ct.mon_id
    WHERE ct.nl_id = @NlId
    ORDER BY m.ten_mon;
END
GO
