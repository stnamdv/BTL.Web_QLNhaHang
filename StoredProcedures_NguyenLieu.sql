/* =========================================================
   STORED PROCEDURES CHO QUẢN LÝ NGUYÊN LIỆU
   Bao gồm validation nghiệp vụ và quản lý nguồn gốc
   ========================================================= */

USE NhaHang
GO

-- =========================================================
-- 1. SP_CreateNguyenLieu - Tạo nguyên liệu mới với validation
-- =========================================================
DROP PROCEDURE IF EXISTS dbo.SP_CreateNguyenLieu
GO
CREATE PROCEDURE dbo.SP_CreateNguyenLieu
    @Ten NVARCHAR(160),
    @DonVi NVARCHAR(20),
    @NguonGoc NVARCHAR(MAX),
    @NlId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validation 1: Kiểm tra tên nguyên liệu có trùng không
        IF EXISTS (SELECT 1 FROM dbo.NguyenLieu WHERE ten = @Ten)
        BEGIN
            RAISERROR(N'Tên nguyên liệu đã tồn tại.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 2: Kiểm tra tên không được rỗng
        IF LTRIM(RTRIM(@Ten)) = N''
        BEGIN
            RAISERROR(N'Tên nguyên liệu không được để trống.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 3: Kiểm tra đơn vị không được rỗng
        IF LTRIM(RTRIM(@DonVi)) = N''
        BEGIN
            RAISERROR(N'Đơn vị không được để trống.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 4: Kiểm tra nguồn gốc không được rỗng (yêu cầu nghiệp vụ)
        IF LTRIM(RTRIM(@NguonGoc)) = N''
        BEGIN
            RAISERROR(N'Thông tin nguồn gốc nguyên liệu là bắt buộc và phải rõ ràng.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 5: Kiểm tra độ dài nguồn gốc (phải đủ chi tiết)
        IF LEN(LTRIM(RTRIM(@NguonGoc))) < 10
        BEGIN
            RAISERROR(N'Thông tin nguồn gốc phải có ít nhất 10 ký tự để đảm bảo tính rõ ràng.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Tạo nguyên liệu mới
        INSERT INTO dbo.NguyenLieu (ten, don_vi, nguon_goc)
        VALUES (@Ten, @DonVi, @NguonGoc);
        
        SET @NlId = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =========================================================
-- 2. SP_UpdateNguyenLieu - Cập nhật nguyên liệu với validation
-- =========================================================
DROP PROCEDURE IF EXISTS dbo.SP_UpdateNguyenLieu
GO
CREATE PROCEDURE dbo.SP_UpdateNguyenLieu
    @NlId INT,
    @Ten NVARCHAR(160),
    @DonVi NVARCHAR(20),
    @NguonGoc NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validation 1: Kiểm tra nguyên liệu có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.NguyenLieu WHERE nl_id = @NlId)
        BEGIN
            RAISERROR(N'Nguyên liệu không tồn tại.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 2: Kiểm tra tên nguyên liệu có trùng với nguyên liệu khác không
        IF EXISTS (SELECT 1 FROM dbo.NguyenLieu WHERE ten = @Ten AND nl_id != @NlId)
        BEGIN
            RAISERROR(N'Tên nguyên liệu đã tồn tại ở nguyên liệu khác.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 3: Kiểm tra tên không được rỗng
        IF LTRIM(RTRIM(@Ten)) = N''
        BEGIN
            RAISERROR(N'Tên nguyên liệu không được để trống.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 4: Kiểm tra đơn vị không được rỗng
        IF LTRIM(RTRIM(@DonVi)) = N''
        BEGIN
            RAISERROR(N'Đơn vị không được để trống.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 5: Kiểm tra nguồn gốc không được rỗng (yêu cầu nghiệp vụ)
        IF LTRIM(RTRIM(@NguonGoc)) = N''
        BEGIN
            RAISERROR(N'Thông tin nguồn gốc nguyên liệu là bắt buộc và phải rõ ràng.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 6: Kiểm tra độ dài nguồn gốc (phải đủ chi tiết)
        IF LEN(LTRIM(RTRIM(@NguonGoc))) < 10
        BEGIN
            RAISERROR(N'Thông tin nguồn gốc phải có ít nhất 10 ký tự để đảm bảo tính rõ ràng.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 7: Kiểm tra nguyên liệu có đang được sử dụng trong công thức không
        IF EXISTS (SELECT 1 FROM dbo.CongThuc WHERE nl_id = @NlId)
        BEGIN
            RAISERROR(N'Không thể cập nhật nguyên liệu đang được sử dụng trong công thức món ăn.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Cập nhật thông tin nguyên liệu
        UPDATE dbo.NguyenLieu 
        SET ten = @Ten,
            don_vi = @DonVi,
            nguon_goc = @NguonGoc
        WHERE nl_id = @NlId;
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =========================================================
-- 3. SP_DeleteNguyenLieu - Xóa nguyên liệu với validation
-- =========================================================
DROP PROCEDURE IF EXISTS dbo.SP_DeleteNguyenLieu
GO
CREATE PROCEDURE dbo.SP_DeleteNguyenLieu
    @NlId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validation 1: Kiểm tra nguyên liệu có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.NguyenLieu WHERE nl_id = @NlId)
        BEGIN
            RAISERROR(N'Nguyên liệu không tồn tại.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 2: Kiểm tra nguyên liệu có đang được sử dụng trong công thức không
        IF EXISTS (SELECT 1 FROM dbo.CongThuc WHERE nl_id = @NlId)
        BEGIN
            RAISERROR(N'Không thể xóa nguyên liệu đang được sử dụng trong công thức món ăn.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation 3: Kiểm tra nguyên liệu có trong phiếu nhập không
        IF EXISTS (SELECT 1 FROM dbo.NL_NCC WHERE nl_id = @NlId)
        BEGIN
            RAISERROR(N'Không thể xóa nguyên liệu đã có lịch sử nhập kho.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Xóa nguyên liệu
        DELETE FROM dbo.NguyenLieu WHERE nl_id = @NlId;
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =========================================================
-- 4. SP_GetNguyenLieuStats - Thống kê sử dụng nguyên liệu
-- =========================================================
DROP PROCEDURE IF EXISTS dbo.SP_GetNguyenLieuStats
GO
CREATE PROCEDURE dbo.SP_GetNguyenLieuStats
    @NlId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Nếu không chỉ định nguyên liệu cụ thể, lấy thống kê tất cả
    IF @NlId IS NULL
    BEGIN
        SELECT 
            nl.nl_id,
            nl.ten,
            nl.don_vi,
            COUNT(DISTINCT ct.mon_id) AS so_mon_su_dung,
            COUNT(DISTINCT nl_ncc.phieu_nhap_id) AS so_lan_nhap,
            ISNULL(SUM(nl_ncc.so_luong), 0) AS tong_so_luong_nhap,
            ISNULL(AVG(nl_ncc.don_gia), 0) AS gia_trung_binh,
            MAX(nl_ncc.ngay_nhap) AS lan_nhap_cuoi
        FROM dbo.NguyenLieu nl
        LEFT JOIN dbo.CongThuc ct ON ct.nl_id = nl.nl_id
        LEFT JOIN dbo.NL_NCC nl_ncc ON nl_ncc.nl_id = nl.nl_id
        GROUP BY nl.nl_id, nl.ten, nl.don_vi
        ORDER BY so_mon_su_dung DESC, nl.ten;
    END
    ELSE
    BEGIN
        -- Kiểm tra nguyên liệu có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.NguyenLieu WHERE nl_id = @NlId)
        BEGIN
            RAISERROR(N'Nguyên liệu không tồn tại.', 16, 1);
            RETURN;
        END
        
        -- Thống kê chi tiết cho nguyên liệu cụ thể
        SELECT 
            nl.nl_id,
            nl.ten,
            nl.don_vi,
            nl.nguon_goc,
            COUNT(DISTINCT ct.mon_id) AS so_mon_su_dung,
            COUNT(DISTINCT nl_ncc.phieu_nhap_id) AS so_lan_nhap,
            ISNULL(SUM(nl_ncc.so_luong), 0) AS tong_so_luong_nhap,
            ISNULL(AVG(nl_ncc.don_gia), 0) AS gia_trung_binh,
            MAX(nl_ncc.ngay_nhap) AS lan_nhap_cuoi,
            MIN(nl_ncc.ngay_nhap) AS lan_nhap_dau
        FROM dbo.NguyenLieu nl
        LEFT JOIN dbo.CongThuc ct ON ct.nl_id = nl.nl_id
        LEFT JOIN dbo.NL_NCC nl_ncc ON nl_ncc.nl_id = nl.nl_id
        WHERE nl.nl_id = @NlId
        GROUP BY nl.nl_id, nl.ten, nl.don_vi, nl.nguon_goc;
        
        -- Danh sách món sử dụng nguyên liệu này
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
END
GO

-- =========================================================
-- 5. SP_SearchNguyenLieu - Tìm kiếm nguyên liệu theo nhiều tiêu chí
-- =========================================================
DROP PROCEDURE IF EXISTS dbo.SP_SearchNguyenLieu
GO
CREATE PROCEDURE dbo.SP_SearchNguyenLieu
    @SearchTerm NVARCHAR(160) = NULL,
    @DonVi NVARCHAR(20) = NULL,
    @NguonGoc NVARCHAR(MAX) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Đếm tổng số records
    DECLARE @TotalCount INT;
    SELECT @TotalCount = COUNT(*)
    FROM dbo.NguyenLieu nl
    WHERE (@SearchTerm IS NULL OR nl.ten LIKE N'%' + @SearchTerm + N'%')
      AND (@DonVi IS NULL OR nl.don_vi = @DonVi)
      AND (@NguonGoc IS NULL OR nl.nguon_goc LIKE N'%' + @NguonGoc + N'%');
    
    -- Lấy dữ liệu phân trang
    SELECT 
        nl.nl_id,
        nl.ten,
        nl.don_vi,
        nl.nguon_goc,
        COUNT(DISTINCT ct.mon_id) AS so_mon_su_dung,
        @TotalCount AS TotalCount
    FROM dbo.NguyenLieu nl
    LEFT JOIN dbo.CongThuc ct ON ct.nl_id = nl.nl_id
    WHERE (@SearchTerm IS NULL OR nl.ten LIKE N'%' + @SearchTerm + N'%')
      AND (@DonVi IS NULL OR nl.don_vi = @DonVi)
      AND (@NguonGoc IS NULL OR nl.nguon_goc LIKE N'%' + @NguonGoc + N'%')
    GROUP BY nl.nl_id, nl.ten, nl.don_vi, nl.nguon_goc
    ORDER BY nl.ten
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =========================================================
-- 6. SP_ValidateNguonGoc - Kiểm tra tính hợp lệ của nguồn gốc
-- =========================================================
DROP PROCEDURE IF EXISTS dbo.SP_ValidateNguonGoc
GO
CREATE PROCEDURE dbo.SP_ValidateNguonGoc
    @NguonGoc NVARCHAR(MAX),
    @IsValid BIT OUTPUT,
    @ErrorMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    SET @IsValid = 1;
    SET @ErrorMessage = N'';
    
    -- Kiểm tra không rỗng
    IF LTRIM(RTRIM(@NguonGoc)) = N''
    BEGIN
        SET @IsValid = 0;
        SET @ErrorMessage = N'Thông tin nguồn gốc không được để trống.';
        RETURN;
    END
    
    -- Kiểm tra độ dài tối thiểu
    IF LEN(LTRIM(RTRIM(@NguonGoc))) < 10
    BEGIN
        SET @IsValid = 0;
        SET @ErrorMessage = N'Thông tin nguồn gốc phải có ít nhất 10 ký tự để đảm bảo tính rõ ràng.';
        RETURN;
    END
    
    -- Kiểm tra không chứa ký tự đặc biệt không hợp lệ
    IF @NguonGoc LIKE N'%[<>{}]%'
    BEGIN
        SET @IsValid = 0;
        SET @ErrorMessage = N'Thông tin nguồn gốc không được chứa ký tự đặc biệt: < > { }';
        RETURN;
    END
    
    -- Kiểm tra phải chứa ít nhất một từ có ý nghĩa (không chỉ số)
    IF @NguonGoc NOT LIKE N'%[a-zA-ZàáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđĐ]%'
    BEGIN
        SET @IsValid = 0;
        SET @ErrorMessage = N'Thông tin nguồn gốc phải chứa ít nhất một từ có ý nghĩa (không chỉ số).';
        RETURN;
    END
END
GO
