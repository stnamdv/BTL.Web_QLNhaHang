/* =========================================================
   STORED PROCEDURES CHO BẢNG NHANVIEN
   ========================================================= */

USE NhaHang;
GO

-- 1. Lấy tất cả nhân viên với thông tin loại nhân viên
CREATE OR ALTER PROCEDURE sp_NhanVien_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        nv.nv_id,
        nv.ho_ten,
        nv.loai_nv,
        nv.ngay_vao_lam,
        nv.trang_thai,
        lnv.luong_co_ban
    FROM dbo.NhanVien nv
    INNER JOIN dbo.LoaiNhanVien lnv ON nv.loai_nv = lnv.loai_nv
    ORDER BY nv.ho_ten;
END
GO

-- 2. Lấy nhân viên theo ID
CREATE OR ALTER PROCEDURE sp_NhanVien_GetById
    @nv_id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validation
    IF @nv_id IS NULL OR @nv_id <= 0
    BEGIN
        RAISERROR(N'ID nhân viên không hợp lệ', 16, 1);
        RETURN;
    END
    
    SELECT 
        nv.nv_id,
        nv.ho_ten,
        nv.loai_nv,
        nv.ngay_vao_lam,
        nv.trang_thai,
        lnv.luong_co_ban
    FROM dbo.NhanVien nv
    INNER JOIN dbo.LoaiNhanVien lnv ON nv.loai_nv = lnv.loai_nv
    WHERE nv.nv_id = @nv_id;
END
GO

-- 3. Lấy nhân viên theo loại nhân viên
CREATE OR ALTER PROCEDURE sp_NhanVien_GetByLoaiNv
    @loai_nv NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validation
    IF @loai_nv IS NULL OR LEN(TRIM(@loai_nv)) = 0
    BEGIN
        RAISERROR(N'Loại nhân viên không được để trống', 16, 1);
        RETURN;
    END
    
    -- Kiểm tra loại nhân viên có tồn tại không
    IF NOT EXISTS (SELECT 1 FROM dbo.LoaiNhanVien WHERE loai_nv = @loai_nv)
    BEGIN
        RAISERROR(N'Loại nhân viên không tồn tại', 16, 1);
        RETURN;
    END
    
    SELECT 
        nv.nv_id,
        nv.ho_ten,
        nv.loai_nv,
        nv.ngay_vao_lam,
        nv.trang_thai,
        lnv.luong_co_ban
    FROM dbo.NhanVien nv
    INNER JOIN dbo.LoaiNhanVien lnv ON nv.loai_nv = lnv.loai_nv
    WHERE nv.loai_nv = @loai_nv
    ORDER BY nv.ho_ten;
END
GO

-- 4. Tạo nhân viên mới
CREATE OR ALTER PROCEDURE sp_NhanVien_Create
    @ho_ten NVARCHAR(120),
    @loai_nv NVARCHAR(20),
    @ngay_vao_lam DATE = NULL,
    @trang_thai NVARCHAR(20) = N'ACTIVE'
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validation
    IF @ho_ten IS NULL OR LEN(TRIM(@ho_ten)) = 0
    BEGIN
        RAISERROR(N'Họ tên không được để trống', 16, 1);
        RETURN;
    END
    
    IF LEN(@ho_ten) > 120
    BEGIN
        RAISERROR(N'Họ tên không được vượt quá 120 ký tự', 16, 1);
        RETURN;
    END
    
    IF @loai_nv IS NULL OR LEN(TRIM(@loai_nv)) = 0
    BEGIN
        RAISERROR(N'Loại nhân viên không được để trống', 16, 1);
        RETURN;
    END
    
    -- Kiểm tra loại nhân viên có tồn tại không
    IF NOT EXISTS (SELECT 1 FROM dbo.LoaiNhanVien WHERE loai_nv = @loai_nv)
    BEGIN
        RAISERROR(N'Loại nhân viên không tồn tại', 16, 1);
        RETURN;
    END
    
    -- Validation cho ngày vào làm
    IF @ngay_vao_lam IS NOT NULL AND @ngay_vao_lam > GETDATE()
    BEGIN
        RAISERROR(N'Ngày vào làm không được là ngày tương lai', 16, 1);
        RETURN;
    END
    
    -- Validation cho trạng thái
    IF @trang_thai NOT IN (N'ACTIVE', N'INACTIVE')
    BEGIN
        RAISERROR(N'Trạng thái phải là ACTIVE hoặc INACTIVE', 16, 1);
        RETURN;
    END
    
    -- Insert nhân viên mới
    INSERT INTO dbo.NhanVien (ho_ten, loai_nv, ngay_vao_lam, trang_thai)
    VALUES (@ho_ten, @loai_nv, @ngay_vao_lam, @trang_thai);
    
    -- Trả về ID của nhân viên vừa tạo
    SELECT SCOPE_IDENTITY() AS nv_id;
END
GO

-- 5. Cập nhật nhân viên
CREATE OR ALTER PROCEDURE sp_NhanVien_Update
    @nv_id INT,
    @ho_ten NVARCHAR(120),
    @loai_nv NVARCHAR(20),
    @ngay_vao_lam DATE = NULL,
    @trang_thai NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validation
    IF @nv_id IS NULL OR @nv_id <= 0
    BEGIN
        RAISERROR(N'ID nhân viên không hợp lệ', 16, 1);
        RETURN;
    END
    
    -- Kiểm tra nhân viên có tồn tại không
    IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien WHERE nv_id = @nv_id)
    BEGIN
        RAISERROR(N'Nhân viên không tồn tại', 16, 1);
        RETURN;
    END
    
    IF @ho_ten IS NULL OR LEN(TRIM(@ho_ten)) = 0
    BEGIN
        RAISERROR(N'Họ tên không được để trống', 16, 1);
        RETURN;
    END
    
    IF LEN(@ho_ten) > 120
    BEGIN
        RAISERROR(N'Họ tên không được vượt quá 120 ký tự', 16, 1);
        RETURN;
    END
    
    IF @loai_nv IS NULL OR LEN(TRIM(@loai_nv)) = 0
    BEGIN
        RAISERROR(N'Loại nhân viên không được để trống', 16, 1);
        RETURN;
    END
    
    -- Kiểm tra loại nhân viên có tồn tại không
    IF NOT EXISTS (SELECT 1 FROM dbo.LoaiNhanVien WHERE loai_nv = @loai_nv)
    BEGIN
        RAISERROR(N'Loại nhân viên không tồn tại', 16, 1);
        RETURN;
    END
    
    -- Validation cho ngày vào làm
    IF @ngay_vao_lam IS NOT NULL AND @ngay_vao_lam > GETDATE()
    BEGIN
        RAISERROR(N'Ngày vào làm không được là ngày tương lai', 16, 1);
        RETURN;
    END
    
    -- Validation cho trạng thái
    IF @trang_thai NOT IN (N'ACTIVE', N'INACTIVE')
    BEGIN
        RAISERROR(N'Trạng thái phải là ACTIVE hoặc INACTIVE', 16, 1);
        RETURN;
    END
    
    -- Update nhân viên
    UPDATE dbo.NhanVien 
    SET 
        ho_ten = @ho_ten,
        loai_nv = @loai_nv,
        ngay_vao_lam = @ngay_vao_lam,
        trang_thai = @trang_thai
    WHERE nv_id = @nv_id;
    
    -- Trả về số dòng được cập nhật
    SELECT @@ROWCOUNT AS rows_affected;
END
GO

-- 6. Xóa nhân viên
CREATE OR ALTER PROCEDURE sp_NhanVien_Delete
    @nv_id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validation
    IF @nv_id IS NULL OR @nv_id <= 0
    BEGIN
        RAISERROR(N'ID nhân viên không hợp lệ', 16, 1);
        RETURN;
    END
    
    -- Kiểm tra nhân viên có tồn tại không
    IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien WHERE nv_id = @nv_id)
    BEGIN
        RAISERROR(N'Nhân viên không tồn tại', 16, 1);
        RETURN;
    END
    
    -- TODO: Kiểm tra ràng buộc với các bảng khác khi có
    -- Hiện tại chưa có bảng nào tham chiếu đến nv_id
    
    -- Xóa nhân viên
    DELETE FROM dbo.NhanVien WHERE nv_id = @nv_id;
    
    -- Trả về số dòng được xóa
    SELECT @@ROWCOUNT AS rows_affected;
END
GO

-- 7. Kiểm tra có thể xóa nhân viên không
CREATE OR ALTER PROCEDURE sp_NhanVien_CanDelete
    @nv_id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @can_delete BIT = 1;
    DECLARE @message NVARCHAR(500) = N'Có thể xóa nhân viên';
    
    -- Validation
    IF @nv_id IS NULL OR @nv_id <= 0
    BEGIN
        SET @can_delete = 0;
        SET @message = N'ID nhân viên không hợp lệ';
    END
    ELSE IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien WHERE nv_id = @nv_id)
    BEGIN
        SET @can_delete = 0;
        SET @message = N'Nhân viên không tồn tại';
    END
    ELSE
    BEGIN
        -- TODO: Kiểm tra ràng buộc với các bảng khác khi có
        -- Hiện tại chưa có bảng nào tham chiếu đến nv_id
        
        -- Ví dụ kiểm tra nếu nhân viên đang có đơn hàng:
        -- IF EXISTS (SELECT 1 FROM dbo.[Order] WHERE nv_id = @nv_id)
        -- BEGIN
        --     SET @can_delete = 0;
        --     SET @message = N'Không thể xóa nhân viên đang có đơn hàng';
        -- END
    END
    
    SELECT @can_delete AS can_delete, @message AS message;
END
GO

-- 8. Lấy thống kê nhân viên theo loại
CREATE OR ALTER PROCEDURE sp_NhanVien_GetStatsByLoai
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        lnv.loai_nv,
        lnv.luong_co_ban,
        COUNT(nv.nv_id) AS so_luong_nv,
        COUNT(CASE WHEN nv.trang_thai = N'ACTIVE' THEN 1 END) AS so_luong_active,
        COUNT(CASE WHEN nv.trang_thai = N'INACTIVE' THEN 1 END) AS so_luong_inactive
    FROM dbo.LoaiNhanVien lnv
    LEFT JOIN dbo.NhanVien nv ON lnv.loai_nv = nv.loai_nv
    GROUP BY lnv.loai_nv, lnv.luong_co_ban
    ORDER BY lnv.loai_nv;
END
GO

-- 9. Lấy nhân viên đang hoạt động
CREATE OR ALTER PROCEDURE sp_NhanVien_GetActive
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        nv.nv_id,
        nv.ho_ten,
        nv.loai_nv,
        nv.ngay_vao_lam,
        nv.trang_thai,
        lnv.luong_co_ban
    FROM dbo.NhanVien nv
    INNER JOIN dbo.LoaiNhanVien lnv ON nv.loai_nv = lnv.loai_nv
    WHERE nv.trang_thai = N'ACTIVE'
    ORDER BY nv.ho_ten;
END
GO

-- 10. Tìm kiếm nhân viên theo tên
CREATE OR ALTER PROCEDURE sp_NhanVien_SearchByName
    @search_term NVARCHAR(120)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validation
    IF @search_term IS NULL OR LEN(TRIM(@search_term)) = 0
    BEGIN
        RAISERROR(N'Từ khóa tìm kiếm không được để trống', 16, 1);
        RETURN;
    END
    
    SELECT 
        nv.nv_id,
        nv.ho_ten,
        nv.loai_nv,
        nv.ngay_vao_lam,
        nv.trang_thai,
        lnv.luong_co_ban
    FROM dbo.NhanVien nv
    INNER JOIN dbo.LoaiNhanVien lnv ON nv.loai_nv = lnv.loai_nv
    WHERE nv.ho_ten LIKE N'%' + @search_term + N'%'
    ORDER BY nv.ho_ten;
END
GO
