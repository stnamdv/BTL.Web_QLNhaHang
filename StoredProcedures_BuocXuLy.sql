/* =========================================================
   STORED PROCEDURES CHO QUẢN LÝ BƯỚC XỬ LÝ
   Quản lý các bước trong quy trình xử lý đơn hàng
   ========================================================= */

USE NhaHang
GO

-- =========================================================
-- 1. sp_BuocXuLy_GetAll - Lấy tất cả bước xử lý
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_BuocXuLy_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        buoc_id,
        ten_buoc,
        mo_ta,
        thu_tu,
        thoi_gian_du_kien
    FROM dbo.BuocXuLy
    ORDER BY thu_tu;
END
GO

-- =========================================================
-- 2. sp_BuocXuLy_GetById - Lấy bước theo ID
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_BuocXuLy_GetById
    @BuocId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        buoc_id,
        ten_buoc,
        mo_ta,
        thu_tu,
        thoi_gian_du_kien
    FROM dbo.BuocXuLy
    WHERE buoc_id = @BuocId;
END
GO

-- =========================================================
-- 3. sp_BuocXuLy_GetByThuTu - Lấy bước theo thứ tự
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_BuocXuLy_GetByThuTu
    @ThuTu INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        buoc_id,
        ten_buoc,
        mo_ta,
        thu_tu,
        thoi_gian_du_kien
    FROM dbo.BuocXuLy
    WHERE thu_tu = @ThuTu;
END
GO

-- =========================================================
-- 4. sp_BuocXuLy_GetBuocTiepTheo - Lấy bước tiếp theo
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_BuocXuLy_GetBuocTiepTheo
    @BuocId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        b2.buoc_id,
        b2.ten_buoc,
        b2.mo_ta,
        b2.thu_tu,
        b2.thoi_gian_du_kien
    FROM dbo.BuocXuLy b1
    JOIN dbo.BuocXuLy b2 ON b2.thu_tu = b1.thu_tu + 1
    WHERE b1.buoc_id = @BuocId;
END
GO

-- =========================================================
-- 5. sp_BuocXuLy_GetBuocDauTien - Lấy bước đầu tiên
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_BuocXuLy_GetBuocDauTien
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        buoc_id,
        ten_buoc,
        mo_ta,
        thu_tu,
        thoi_gian_du_kien
    FROM dbo.BuocXuLy
    WHERE thu_tu = 1;
END
GO

-- =========================================================
-- 6. sp_BuocXuLy_Create - Tạo bước xử lý mới
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_BuocXuLy_Create
    @TenBuoc NVARCHAR(100),
    @MoTa NVARCHAR(500) = NULL,
    @ThuTu INT,
    @ThoiGianDuKien INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Kiểm tra thứ tự đã tồn tại chưa
        IF EXISTS (SELECT 1 FROM dbo.BuocXuLy WHERE thu_tu = @ThuTu)
        BEGIN
            RAISERROR('Thứ tự %d đã tồn tại. Vui lòng chọn thứ tự khác.', 16, 1, @ThuTu);
            RETURN;
        END
        
        -- Thêm bước mới
        INSERT INTO dbo.BuocXuLy (ten_buoc, mo_ta, thu_tu, thoi_gian_du_kien)
        VALUES (@TenBuoc, @MoTa, @ThuTu, @ThoiGianDuKien);
        
        -- Trả về bước vừa tạo
        SELECT 
            buoc_id,
            ten_buoc,
            mo_ta,
            thu_tu,
            thoi_gian_du_kien
        FROM dbo.BuocXuLy
        WHERE buoc_id = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =========================================================
-- 7. sp_BuocXuLy_Update - Cập nhật bước xử lý
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_BuocXuLy_Update
    @BuocId INT,
    @TenBuoc NVARCHAR(100),
    @MoTa NVARCHAR(500) = NULL,
    @ThuTu INT,
    @ThoiGianDuKien INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Kiểm tra bước có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.BuocXuLy WHERE buoc_id = @BuocId)
        BEGIN
            RAISERROR('Không tìm thấy bước xử lý với ID %d', 16, 1, @BuocId);
            RETURN;
        END
        
        -- Kiểm tra thứ tự đã tồn tại chưa (trừ bước hiện tại)
        IF EXISTS (SELECT 1 FROM dbo.BuocXuLy WHERE thu_tu = @ThuTu AND buoc_id != @BuocId)
        BEGIN
            RAISERROR('Thứ tự %d đã tồn tại. Vui lòng chọn thứ tự khác.', 16, 1, @ThuTu);
            RETURN;
        END
        
        -- Cập nhật bước
        UPDATE dbo.BuocXuLy
        SET 
            ten_buoc = @TenBuoc,
            mo_ta = @MoTa,
            thu_tu = @ThuTu,
            thoi_gian_du_kien = @ThoiGianDuKien
        WHERE buoc_id = @BuocId;
        
        -- Trả về bước đã cập nhật
        SELECT 
            buoc_id,
            ten_buoc,
            mo_ta,
            thu_tu,
            thoi_gian_du_kien
        FROM dbo.BuocXuLy
        WHERE buoc_id = @BuocId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =========================================================
-- 8. sp_BuocXuLy_Delete - Xóa bước xử lý
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_BuocXuLy_Delete
    @BuocId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Kiểm tra bước có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.BuocXuLy WHERE buoc_id = @BuocId)
        BEGIN
            RAISERROR('Không tìm thấy bước xử lý với ID %d', 16, 1, @BuocId);
            RETURN;
        END
        
        -- Kiểm tra có phân công nhân viên nào không
        IF EXISTS (SELECT 1 FROM dbo.PhanCongBuocXuLy WHERE buoc_id = @BuocId)
        BEGIN
            RAISERROR('Không thể xóa bước xử lý này vì đã có phân công nhân viên. Vui lòng xóa phân công trước.', 16, 1);
            RETURN;
        END
        
        -- Kiểm tra có lịch sử thực hiện nào không
        IF EXISTS (SELECT 1 FROM dbo.LichSuThucHien WHERE buoc_id = @BuocId)
        BEGIN
            RAISERROR('Không thể xóa bước xử lý này vì đã có lịch sử thực hiện. Vui lòng xóa lịch sử trước.', 16, 1);
            RETURN;
        END
        
        -- Xóa bước
        DELETE FROM dbo.BuocXuLy WHERE buoc_id = @BuocId;
        
        -- Trả về kết quả thành công
        SELECT 1 as Success;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =========================================================
-- 9. sp_BuocXuLy_Exists - Kiểm tra bước có tồn tại không
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_BuocXuLy_Exists
    @BuocId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(1) as ExistsCount
    FROM dbo.BuocXuLy
    WHERE buoc_id = @BuocId;
END
GO