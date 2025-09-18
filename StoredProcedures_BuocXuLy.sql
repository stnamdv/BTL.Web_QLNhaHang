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
