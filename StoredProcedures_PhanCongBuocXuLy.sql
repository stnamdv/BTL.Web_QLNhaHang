/* =========================================================
   STORED PROCEDURES CHO QUẢN LÝ PHÂN CÔNG BƯỚC XỬ LÝ
   Quản lý phân công nhân viên cho từng bước xử lý
   ========================================================= */

USE NhaHang
GO

-- =========================================================
-- 1. sp_PhanCongBuocXuLy_GetAll - Lấy tất cả phân công
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_PhanCongBuocXuLy_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pcbx.phan_cong_buoc_id,
        pcbx.buoc_id,
        bx.ten_buoc,
        bx.thu_tu,
        pcbx.loai_nv_id,
        lnv.loai_nv,
        pcbx.vai_tro,
        pcbx.trang_thai,
        pcbx.thoi_diem_tao
    FROM dbo.PhanCongBuocXuLy pcbx
    JOIN dbo.BuocXuLy bx ON bx.buoc_id = pcbx.buoc_id
    JOIN dbo.LoaiNhanVien lnv ON lnv.loai_nv_id = pcbx.loai_nv_id
    ORDER BY bx.thu_tu, lnv.loai_nv;
END
GO

-- =========================================================
-- 2. sp_PhanCongBuocXuLy_GetByBuoc - Lấy phân công theo bước
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_PhanCongBuocXuLy_GetByBuoc
    @BuocId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pcbx.phan_cong_buoc_id,
        pcbx.buoc_id,
        bx.ten_buoc,
        bx.thu_tu,
        pcbx.loai_nv_id,
        lnv.loai_nv,
        pcbx.vai_tro,
        pcbx.trang_thai,
        pcbx.thoi_diem_tao
    FROM dbo.PhanCongBuocXuLy pcbx
    JOIN dbo.BuocXuLy bx ON bx.buoc_id = pcbx.buoc_id
    JOIN dbo.LoaiNhanVien lnv ON lnv.loai_nv_id = pcbx.loai_nv_id
    WHERE pcbx.buoc_id = @BuocId
    ORDER BY lnv.loai_nv;
END
GO

-- =========================================================
-- 3. sp_PhanCongBuocXuLy_GetByLoaiNhanVien - Lấy phân công theo loại nhân viên
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_PhanCongBuocXuLy_GetByLoaiNhanVien
    @LoaiNvId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pcbx.phan_cong_buoc_id,
        pcbx.buoc_id,
        bx.ten_buoc,
        bx.thu_tu,
        pcbx.loai_nv_id,
        lnv.loai_nv,
        pcbx.vai_tro,
        pcbx.trang_thai,
        pcbx.thoi_diem_tao
    FROM dbo.PhanCongBuocXuLy pcbx
    JOIN dbo.BuocXuLy bx ON bx.buoc_id = pcbx.buoc_id
    JOIN dbo.LoaiNhanVien lnv ON lnv.loai_nv_id = pcbx.loai_nv_id
    WHERE pcbx.loai_nv_id = @LoaiNvId
    ORDER BY bx.thu_tu;
END
GO

-- =========================================================
-- 4. sp_PhanCongBuocXuLy_GetNhanVienChoBuoc - Lấy nhân viên được phân công cho bước
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_PhanCongBuocXuLy_GetNhanVienChoBuoc
    @BuocId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pcbx.loai_nv_id,
        lnv.loai_nv,
        pcbx.vai_tro,
        nv.nv_id,
        nv.ho_ten,
        nv.trang_thai
    FROM dbo.PhanCongBuocXuLy pcbx
    JOIN dbo.LoaiNhanVien lnv ON lnv.loai_nv_id = pcbx.loai_nv_id
    LEFT JOIN dbo.NhanVien nv ON nv.loai_nv_id = pcbx.loai_nv_id AND nv.trang_thai = 'ACTIVE'
    WHERE pcbx.buoc_id = @BuocId 
      AND pcbx.trang_thai = 'ACTIVE';
END
GO

-- =========================================================
-- 5. sp_PhanCongBuocXuLy_Create - Tạo phân công mới
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_PhanCongBuocXuLy_Create
    @BuocId INT,
    @LoaiNvId INT,
    @VaiTro NVARCHAR(50) = NULL,
    @PhanCongBuocId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validation: Kiểm tra bước có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.BuocXuLy WHERE buoc_id = @BuocId)
        BEGIN
            RAISERROR(N'Bước xử lý không tồn tại.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation: Kiểm tra loại nhân viên có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.LoaiNhanVien WHERE loai_nv_id = @LoaiNvId)
        BEGIN
            RAISERROR(N'Loại nhân viên không tồn tại.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation: Kiểm tra phân công đã tồn tại chưa
        IF EXISTS (SELECT 1 FROM dbo.PhanCongBuocXuLy WHERE buoc_id = @BuocId AND loai_nv_id = @LoaiNvId)
        BEGIN
            RAISERROR(N'Phân công này đã tồn tại.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Tạo phân công mới
        INSERT INTO dbo.PhanCongBuocXuLy (buoc_id, loai_nv_id, vai_tro)
        VALUES (@BuocId, @LoaiNvId, @VaiTro);
        
        SET @PhanCongBuocId = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =========================================================
-- 6. sp_PhanCongBuocXuLy_Update - Cập nhật phân công
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_PhanCongBuocXuLy_Update
    @PhanCongBuocId INT,
    @VaiTro NVARCHAR(50) = NULL,
    @TrangThai NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validation: Kiểm tra phân công có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.PhanCongBuocXuLy WHERE phan_cong_buoc_id = @PhanCongBuocId)
        BEGIN
            RAISERROR(N'Phân công không tồn tại.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Cập nhật phân công
        UPDATE dbo.PhanCongBuocXuLy
        SET vai_tro = ISNULL(@VaiTro, vai_tro),
            trang_thai = ISNULL(@TrangThai, trang_thai)
        WHERE phan_cong_buoc_id = @PhanCongBuocId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =========================================================
-- 7. sp_PhanCongBuocXuLy_Delete - Xóa phân công
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_PhanCongBuocXuLy_Delete
    @PhanCongBuocId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validation: Kiểm tra phân công có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.PhanCongBuocXuLy WHERE phan_cong_buoc_id = @PhanCongBuocId)
        BEGIN
            RAISERROR(N'Phân công không tồn tại.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation: Kiểm tra có đang được sử dụng trong lịch sử không
        IF EXISTS (SELECT 1 FROM dbo.LichSuThucHien lsh 
                   JOIN dbo.PhanCongBuocXuLy pcbx ON pcbx.buoc_id = lsh.buoc_id
                   WHERE pcbx.phan_cong_buoc_id = @PhanCongBuocId)
        BEGIN
            RAISERROR(N'Không thể xóa phân công đang được sử dụng trong lịch sử thực hiện.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Xóa phân công
        DELETE FROM dbo.PhanCongBuocXuLy
        WHERE phan_cong_buoc_id = @PhanCongBuocId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
