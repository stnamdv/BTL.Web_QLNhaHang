/* =========================================================
   STORED PROCEDURES CHO QUẢN LÝ NGUYÊN LIỆU
   Bao gồm validation nghiệp vụ và quản lý nguồn gốc
   ========================================================= */

USE NhaHang
GO

-- =========================================================
-- 1. sp_NguyenLieu_Create - Tạo nguyên liệu mới với validation
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_NguyenLieu_Create
    @Ten NVARCHAR(160),
    @DonVi NVARCHAR(20),
    @NguonGoc NVARCHAR(MAX),
    @NccId INT,
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
        INSERT INTO dbo.NguyenLieu (ten, don_vi, nguon_goc, ncc_id)
        VALUES (@Ten, @DonVi, @NguonGoc, @NccId);
        
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
-- 2. sp_NguyenLieu_Update - Cập nhật nguyên liệu với validation
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_NguyenLieu_Update
    @NlId INT,
    @Ten NVARCHAR(160),
    @DonVi NVARCHAR(20),
    @NguonGoc NVARCHAR(MAX),
    @NccId INT
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
        
        -- Validation 7: Kiểm tra nhà cung cấp có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.NhaCungCap WHERE ncc_id = @NccId)
        BEGIN
            RAISERROR(N'Nhà cung cấp không tồn tại.', 16, 1);
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
            nguon_goc = @NguonGoc,
            ncc_id = @NccId
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
-- 3. sp_NguyenLieu_Delete - Xóa nguyên liệu với validation
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_NguyenLieu_Delete
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
-- 4. sp_NguyenLieu_GetById - Lấy thông tin nguyên liệu theo ID
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_NguyenLieu_GetById
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
        nl.nguon_goc,
        nl.ncc_id,
        ncc.ten AS ncc_ten,
        ncc.dia_chi AS ncc_dia_chi,
        ncc.sdt AS ncc_sdt
    FROM dbo.NguyenLieu nl
    JOIN dbo.NhaCungCap ncc ON ncc.ncc_id = nl.ncc_id
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

-- =========================================================
-- 5. sp_NguyenLieu_GetStats - Thống kê sử dụng nguyên liệu
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_NguyenLieu_GetStats
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
            nl.ncc_id,
            ncc.ten AS ncc_ten,
            COUNT(DISTINCT ct.mon_id) AS so_mon_su_dung,
            COUNT(DISTINCT nl_ncc.phieu_nhap_id) AS so_lan_nhap,
            ISNULL(SUM(nl_ncc.so_luong), 0) AS tong_so_luong_nhap,
            ISNULL(AVG(nl_ncc.don_gia), 0) AS gia_trung_binh,
            MAX(nl_ncc.ngay_nhap) AS lan_nhap_cuoi
        FROM dbo.NguyenLieu nl
        JOIN dbo.NhaCungCap ncc ON ncc.ncc_id = nl.ncc_id
        LEFT JOIN dbo.CongThuc ct ON ct.nl_id = nl.nl_id
        LEFT JOIN dbo.NL_NCC nl_ncc ON nl_ncc.nl_id = nl.nl_id
        GROUP BY nl.nl_id, nl.ten, nl.don_vi, nl.ncc_id, ncc.ten
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
            nl.ncc_id,
            ncc.ten AS ncc_ten,
            ncc.dia_chi AS ncc_dia_chi,
            ncc.sdt AS ncc_sdt,
            COUNT(DISTINCT ct.mon_id) AS so_mon_su_dung,
            COUNT(DISTINCT nl_ncc.phieu_nhap_id) AS so_lan_nhap,
            ISNULL(SUM(nl_ncc.so_luong), 0) AS tong_so_luong_nhap,
            ISNULL(AVG(nl_ncc.don_gia), 0) AS gia_trung_binh,
            MAX(nl_ncc.ngay_nhap) AS lan_nhap_cuoi,
            MIN(nl_ncc.ngay_nhap) AS lan_nhap_dau
        FROM dbo.NguyenLieu nl
        JOIN dbo.NhaCungCap ncc ON ncc.ncc_id = nl.ncc_id
        LEFT JOIN dbo.CongThuc ct ON ct.nl_id = nl.nl_id
        LEFT JOIN dbo.NL_NCC nl_ncc ON nl_ncc.nl_id = nl.nl_id
        WHERE nl.nl_id = @NlId
        GROUP BY nl.nl_id, nl.ten, nl.don_vi, nl.nguon_goc, nl.ncc_id, ncc.ten, ncc.dia_chi, ncc.sdt;
        
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
-- 6. sp_NguyenLieu_Search - Tìm kiếm nguyên liệu theo nhiều tiêu chí
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_NguyenLieu_Search
    @SearchTerm NVARCHAR(160) = NULL,
    @DonVi NVARCHAR(20) = NULL,
    @NguonGoc NVARCHAR(MAX) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Đếm tổng số records
    SELECT @TotalCount = COUNT(1)
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
        nl.ncc_id,
        ncc.ten AS ncc_ten,
        COUNT(DISTINCT ct.mon_id) AS so_mon_su_dung
    FROM dbo.NguyenLieu nl
    JOIN dbo.NhaCungCap ncc ON ncc.ncc_id = nl.ncc_id
    LEFT JOIN dbo.CongThuc ct ON ct.nl_id = nl.nl_id
    WHERE (@SearchTerm IS NULL OR nl.ten LIKE N'%' + @SearchTerm + N'%')
      AND (@DonVi IS NULL OR nl.don_vi = @DonVi)
      AND (@NguonGoc IS NULL OR nl.nguon_goc LIKE N'%' + @NguonGoc + N'%')
    GROUP BY nl.nl_id, nl.ten, nl.don_vi, nl.nguon_goc, nl.ncc_id, ncc.ten
    ORDER BY nl.ten
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =========================================================
-- 7. sp_NguyenLieu_ValidateNguonGoc - Kiểm tra tính hợp lệ của nguồn gốc
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_NguyenLieu_ValidateNguonGoc
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

-- =========================================================
-- 8. sp_NguyenLieu_GetAllWithNhaCungCap - Lấy danh sách nguyên liệu với thông tin nhà cung cấp (có phân trang)
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_NguyenLieu_GetAllWithNhaCungCap
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
    FROM dbo.NguyenLieu nl
    JOIN dbo.NhaCungCap ncc ON ncc.ncc_id = nl.ncc_id
    WHERE (@SearchTerm IS NULL OR nl.ten LIKE N'%' + @SearchTerm + N'%');
    
    -- Lấy dữ liệu phân trang với thông tin nhà cung cấp
    SELECT 
        nl.nl_id,
        nl.ten,
        nl.don_vi,
        nl.nguon_goc,
        nl.ncc_id,
        ncc.ten AS ncc_ten,
        ncc.dia_chi AS ncc_dia_chi,
        ncc.sdt AS ncc_sdt,
        COUNT(DISTINCT ct.mon_id) AS so_mon_su_dung,
        COUNT(DISTINCT nl_ncc.phieu_nhap_id) AS so_lan_nhap,
        ISNULL(SUM(nl_ncc.so_luong), 0) AS tong_so_luong_nhap,
        ISNULL(AVG(nl_ncc.don_gia), 0) AS gia_trung_binh,
        MAX(nl_ncc.ngay_nhap) AS lan_nhap_cuoi,
        MIN(nl_ncc.ngay_nhap) AS lan_nhap_dau
    FROM dbo.NguyenLieu nl
    JOIN dbo.NhaCungCap ncc ON ncc.ncc_id = nl.ncc_id
    LEFT JOIN dbo.CongThuc ct ON ct.nl_id = nl.nl_id
    LEFT JOIN dbo.NL_NCC nl_ncc ON nl_ncc.nl_id = nl.nl_id
    WHERE (@SearchTerm IS NULL OR nl.ten LIKE N'%' + @SearchTerm + N'%')
    GROUP BY nl.nl_id, nl.ten, nl.don_vi, nl.nguon_goc, nl.ncc_id, ncc.ten, ncc.dia_chi, ncc.sdt
    ORDER BY nl.ten
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO
