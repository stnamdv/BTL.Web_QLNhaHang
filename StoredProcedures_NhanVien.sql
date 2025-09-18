USE [NhaHang]
GO
/****** Object:  StoredProcedure [dbo].[sp_NhanVien_BulkUpsert]    Script Date: 9/18/2025 11:29:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 11. Bulk Insert/Update nhân viên từ table-valued parameter
CREATE OR ALTER PROCEDURE [dbo].[sp_NhanVien_BulkUpsert]
    @NhanVienData [dbo].[NhanVienBulkType] READONLY
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @inserted_count INT = 0;
    DECLARE @updated_count INT = 0;
    DECLARE @error_count INT = 0;
    DECLARE @error_messages NVARCHAR(MAX) = N'';
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validation: Kiểm tra dữ liệu không hợp lệ
        IF EXISTS (
            SELECT 1 FROM @NhanVienData 
            WHERE ho_ten IS NULL OR LEN(TRIM(ho_ten)) = 0
        )
        BEGIN
            SET @error_messages = @error_messages + N'Có dòng có họ tên trống; ';
            SET @error_count = @error_count + (SELECT COUNT(*) FROM @NhanVienData WHERE ho_ten IS NULL OR LEN(TRIM(ho_ten)) = 0);
        END
        
        IF EXISTS (
            SELECT 1 FROM @NhanVienData 
            WHERE LEN(ho_ten) > 120
        )
        BEGIN
            SET @error_messages = @error_messages + N'Có dòng có họ tên vượt quá 120 ký tự; ';
            SET @error_count = @error_count + (SELECT COUNT(*) FROM @NhanVienData WHERE LEN(ho_ten) > 120);
        END
        
        IF EXISTS (
            SELECT 1 FROM @NhanVienData 
            WHERE loai_nv IS NULL OR LEN(TRIM(loai_nv)) = 0
        )
        BEGIN
            SET @error_messages = @error_messages + N'Có dòng có loại nhân viên trống; ';
            SET @error_count = @error_count + (SELECT COUNT(*) FROM @NhanVienData WHERE loai_nv IS NULL OR LEN(TRIM(loai_nv)) = 0);
        END
        
        IF EXISTS (
            SELECT 1 FROM @NhanVienData d
            WHERE d.loai_nv IS NOT NULL 
            AND d.loai_nv NOT IN (SELECT loai_nv FROM dbo.LoaiNhanVien)
        )
        BEGIN
            SET @error_messages = @error_messages + N'Có dòng có loại nhân viên không tồn tại; ';
            SET @error_count = @error_count + (SELECT COUNT(*) FROM @NhanVienData d WHERE d.loai_nv IS NOT NULL AND d.loai_nv NOT IN (SELECT loai_nv FROM dbo.LoaiNhanVien));
        END
        
        IF EXISTS (
            SELECT 1 FROM @NhanVienData 
            WHERE ngay_vao_lam IS NOT NULL AND ngay_vao_lam > GETDATE()
        )
        BEGIN
            SET @error_messages = @error_messages + N'Có dòng có ngày vào làm là ngày tương lai; ';
            SET @error_count = @error_count + (SELECT COUNT(*) FROM @NhanVienData WHERE ngay_vao_lam IS NOT NULL AND ngay_vao_lam > GETDATE());
        END
        
        IF EXISTS (
            SELECT 1 FROM @NhanVienData 
            WHERE trang_thai NOT IN (N'ACTIVE', N'INACTIVE')
        )
        BEGIN
            SET @error_messages = @error_messages + N'Có dòng có trạng thái không hợp lệ; ';
            SET @error_count = @error_count + (SELECT COUNT(*) FROM @NhanVienData WHERE trang_thai NOT IN (N'ACTIVE', N'INACTIVE'));
        END
        
        -- Nếu có lỗi validation, rollback và trả về
        IF @error_count > 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 
                0 AS inserted_count,
                0 AS updated_count,
                @error_count AS error_count,
                @error_messages AS error_messages;
            RETURN;
        END
        
        -- 1. UPDATE: Cập nhật các nhân viên có ma_nv
        UPDATE nv
        SET ho_ten = d.ho_ten,
            loai_nv_id = (SELECT loai_nv_id FROM dbo.LoaiNhanVien WHERE loai_nv = d.loai_nv),   
            ngay_vao_lam = d.ngay_vao_lam,
            trang_thai = d.trang_thai
        FROM dbo.NhanVien nv
        INNER JOIN @NhanVienData d ON nv.ma_nv = d.ma_nv
        WHERE d.ma_nv IS NOT NULL AND LEN(TRIM(d.ma_nv)) > 0;
        
        SET @updated_count = @@ROWCOUNT;
        
        -- 2. INSERT: Thêm mới các nhân viên không có ma_nv
        -- Sử dụng OUTPUT clause để lấy ID và sinh mã trực tiếp
        DECLARE @InsertedIds TABLE (nv_id INT, ho_ten NVARCHAR(120));
        
        -- Insert với mã tạm thời để tránh unique constraint violation
        INSERT INTO dbo.NhanVien (ma_nv, ho_ten, loai_nv_id, ngay_vao_lam, trang_thai)
        OUTPUT INSERTED.nv_id, INSERTED.ho_ten INTO @InsertedIds
        SELECT 
            N'TEMP_' + CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS NVARCHAR(10)),
            ho_ten, 
            (SELECT loai_nv_id FROM dbo.LoaiNhanVien WHERE loai_nv = d.loai_nv),
            ngay_vao_lam, 
            trang_thai
        FROM @NhanVienData d
        WHERE ma_nv IS NULL OR LEN(TRIM(ma_nv)) = 0;
        
        -- Cập nhật mã nhân viên chính thức cho các record vừa insert
        UPDATE nv
        SET ma_nv = N'NV' + CAST(nv.nv_id AS NVARCHAR(10))
        FROM dbo.NhanVien nv
        INNER JOIN @InsertedIds i ON nv.nv_id = i.nv_id;
        
        SET @inserted_count = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        -- Trả về kết quả
        SELECT 
            @inserted_count AS inserted_count,
            @updated_count AS updated_count,
            @error_count AS error_count,
            @error_messages AS error_messages;
            
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        -- Trả về lỗi
        SELECT 
            0 AS inserted_count,
            0 AS updated_count,
            1 AS error_count,
            ERROR_MESSAGE() AS error_messages;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[sp_NhanVien_CanDelete]    Script Date: 9/18/2025 11:29:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 7. Kiểm tra có thể xóa nhân viên không
CREATE OR ALTER PROCEDURE [dbo].[sp_NhanVien_CanDelete]
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
    --ELSE
    --BEGIN
        -- TODO: Kiểm tra ràng buộc với các bảng khác khi có
        -- Hiện tại chưa có bảng nào tham chiếu đến nv_id
        
        -- Ví dụ kiểm tra nếu nhân viên đang có đơn hàng:
        -- IF EXISTS (SELECT 1 FROM dbo.[Order] WHERE nv_id = @nv_id)
        -- BEGIN
        --     SET @can_delete = 0;
        --     SET @message = N'Không thể xóa nhân viên đang có đơn hàng';
        -- END
    --END
    
    SELECT @can_delete AS can_delete, @message AS message;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_NhanVien_Create]    Script Date: 9/18/2025 11:29:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 4. Tạo nhân viên mới
CREATE OR ALTER PROCEDURE [dbo].[sp_NhanVien_Create]
    @ho_ten NVARCHAR(120),
    @loai_nv_id INT,
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
    
    IF @loai_nv_id IS NULL OR @loai_nv_id <= 0
    BEGIN
        RAISERROR(N'Loại nhân viên không được để trống', 16, 1);
        RETURN;
    END
    
    -- Kiểm tra loại nhân viên có tồn tại không
    IF NOT EXISTS (SELECT 1 FROM dbo.LoaiNhanVien WHERE loai_nv_id = @loai_nv_id)
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
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Insert nhân viên mới với mã tạm thời unique
        DECLARE @temp_ma_nv NVARCHAR(10) = N'TEMP_' + CAST(ABS(CHECKSUM(NEWID())) AS NVARCHAR(10));
        INSERT INTO dbo.NhanVien (ma_nv, ho_ten, loai_nv_id, ngay_vao_lam, trang_thai)
        VALUES (@temp_ma_nv, @ho_ten, @loai_nv_id, @ngay_vao_lam, @trang_thai);
        
        -- Lấy ID của nhân viên vừa tạo
        DECLARE @new_nv_id INT = SCOPE_IDENTITY();
        
        -- Cập nhật mã nhân viên chính thức
        DECLARE @ma_nv NVARCHAR(10) = N'NV' + CAST(@new_nv_id AS NVARCHAR(10));
        UPDATE dbo.NhanVien 
        SET ma_nv = @ma_nv 
        WHERE nv_id = @new_nv_id;
        
        COMMIT TRANSACTION;
        
        -- Trả về ID và ma_nv của nhân viên vừa tạo
        SELECT @new_nv_id AS nv_id, @ma_nv AS ma_nv;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        -- Re-throw the error
        THROW;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[sp_NhanVien_Delete]    Script Date: 9/18/2025 11:29:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 6. Xóa nhân viên
CREATE OR ALTER PROCEDURE [dbo].[sp_NhanVien_Delete]
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
/****** Object:  StoredProcedure [dbo].[sp_NhanVien_GetActive]    Script Date: 9/18/2025 11:29:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 9. Lấy nhân viên đang hoạt động
CREATE OR ALTER PROCEDURE [dbo].[sp_NhanVien_GetActive]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        nv.nv_id,
        nv.ma_nv,
        nv.ho_ten,
        nv.loai_nv_id,
        lnv.loai_nv,
        nv.ngay_vao_lam,
        nv.trang_thai,
        lnv.luong_co_ban
    FROM dbo.NhanVien nv
    INNER JOIN dbo.LoaiNhanVien lnv ON nv.loai_nv_id = lnv.loai_nv_id
    WHERE nv.trang_thai = N'ACTIVE'
    ORDER BY nv.ho_ten;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_NhanVien_GetActivePaged]    Script Date: 9/18/2025 11:29:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[sp_NhanVien_GetActivePaged]
    @page_number INT,
    @page_size INT,
    @total_count INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @offset INT = (@page_number - 1) * @page_size;
    
    -- Lấy tổng số bản ghi active
    SELECT @total_count = COUNT(*)
    FROM dbo.NhanVien nv
    INNER JOIN dbo.LoaiNhanVien lnv ON nv.loai_nv_id = lnv.loai_nv_id
    WHERE nv.trang_thai = 'ACTIVE';
    
    -- Lấy dữ liệu phân trang active
    SELECT 
        nv.nv_id,
        nv.ma_nv,
        nv.ho_ten,
        nv.loai_nv_id,
        lnv.loai_nv,
        nv.ngay_vao_lam,
        nv.trang_thai,
        lnv.luong_co_ban
    FROM dbo.NhanVien nv
    INNER JOIN dbo.LoaiNhanVien lnv ON nv.loai_nv_id = lnv.loai_nv_id
    WHERE nv.trang_thai = 'ACTIVE'
    ORDER BY nv.nv_id
    OFFSET @offset ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_NhanVien_GetAll]    Script Date: 9/18/2025 11:29:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 1. Lấy tất cả nhân viên với thông tin loại nhân viên
CREATE OR ALTER PROCEDURE [dbo].[sp_NhanVien_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        nv.nv_id,
        nv.ma_nv,
        nv.ho_ten,
        nv.loai_nv_id,
        lnv.loai_nv,
        nv.ngay_vao_lam,
        nv.trang_thai,
        lnv.luong_co_ban
    FROM dbo.NhanVien nv
    INNER JOIN dbo.LoaiNhanVien lnv ON nv.loai_nv_id = lnv.loai_nv_id
    ORDER BY nv.ho_ten;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_NhanVien_GetAllPaged]    Script Date: 9/18/2025 11:29:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_NhanVien_GetAllPaged]
    @page_number INT,
    @page_size INT,
    @total_count INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @offset INT = (@page_number - 1) * @page_size;
    
    -- Lấy tổng số bản ghi
    SELECT @total_count = COUNT(*)
    FROM dbo.NhanVien nv
    INNER JOIN dbo.LoaiNhanVien lnv ON nv.loai_nv_id = lnv.loai_nv_id;
    
    -- Lấy dữ liệu phân trang
    SELECT 
        nv.nv_id,
        nv.ma_nv,
        nv.ho_ten,
        nv.loai_nv_id,
        lnv.loai_nv,
        nv.ngay_vao_lam,
        nv.trang_thai,
        lnv.luong_co_ban
    FROM dbo.NhanVien nv
    INNER JOIN dbo.LoaiNhanVien lnv ON nv.loai_nv_id = lnv.loai_nv_id
    ORDER BY nv.nv_id
    OFFSET @offset ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_NhanVien_GetById]    Script Date: 9/18/2025 11:29:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 2. Lấy nhân viên theo ID
CREATE OR ALTER PROCEDURE [dbo].[sp_NhanVien_GetById]
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
        nv.ma_nv,
        nv.ho_ten,
        nv.loai_nv_id,
        lnv.loai_nv,
        nv.ngay_vao_lam,
        nv.trang_thai,
        lnv.luong_co_ban
    FROM dbo.NhanVien nv
    INNER JOIN dbo.LoaiNhanVien lnv ON nv.loai_nv_id = lnv.loai_nv_id
    WHERE nv.nv_id = @nv_id;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_NhanVien_GetByLoaiNv]    Script Date: 9/18/2025 11:29:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 3. Lấy nhân viên theo loại nhân viên
CREATE OR ALTER PROCEDURE [dbo].[sp_NhanVien_GetByLoaiNv]
    @loai_nv_id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validation
    IF @loai_nv_id IS NULL OR @loai_nv_id <= 0
    BEGIN
        RAISERROR(N'ID loại nhân viên không hợp lệ', 16, 1);
        RETURN;
    END
    
    -- Kiểm tra loại nhân viên có tồn tại không
    IF NOT EXISTS (SELECT 1 FROM dbo.LoaiNhanVien WHERE loai_nv_id = @loai_nv_id)
    BEGIN
        RAISERROR(N'Loại nhân viên không tồn tại', 16, 1);
        RETURN;
    END
    
    SELECT 
        nv.nv_id,
        nv.ma_nv,
        nv.ho_ten,
        nv.loai_nv_id,
        lnv.loai_nv,
        nv.ngay_vao_lam,
        nv.trang_thai,
        lnv.luong_co_ban
    FROM dbo.NhanVien nv
    INNER JOIN dbo.LoaiNhanVien lnv ON nv.loai_nv_id = lnv.loai_nv_id
    WHERE nv.loai_nv_id = @loai_nv_id
    ORDER BY nv.ho_ten;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_NhanVien_GetByLoaiNvPaged]    Script Date: 9/18/2025 11:29:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[sp_NhanVien_GetByLoaiNvPaged]
    @loai_nv_id INT,
    @page_number INT,
    @page_size INT,
    @total_count INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @offset INT = (@page_number - 1) * @page_size;
    
    -- Lấy tổng số bản ghi theo loại nhân viên
    SELECT @total_count = COUNT(*)
    FROM dbo.NhanVien nv
    WHERE nv.loai_nv_id = @loai_nv_id;
    
    -- Lấy dữ liệu phân trang theo loại nhân viên
    SELECT 
        nv.nv_id,
        nv.ma_nv,
        nv.ho_ten,
        nv.loai_nv_id,
        lnv.loai_nv,
        nv.ngay_vao_lam,
        nv.trang_thai,
        lnv.luong_co_ban
    FROM dbo.NhanVien nv
    INNER JOIN dbo.LoaiNhanVien lnv ON nv.loai_nv_id = lnv.loai_nv_id
    WHERE nv.loai_nv_id = @loai_nv_id
    ORDER BY nv.nv_id
    OFFSET @offset ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_NhanVien_GetStatsByLoai]    Script Date: 9/18/2025 11:29:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 8. Lấy thống kê nhân viên theo loại
CREATE OR ALTER PROCEDURE [dbo].[sp_NhanVien_GetStatsByLoai]
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
    LEFT JOIN dbo.NhanVien nv ON lnv.loai_nv_id = nv.loai_nv_id
    GROUP BY lnv.loai_nv, lnv.luong_co_ban
    ORDER BY lnv.loai_nv;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_NhanVien_SearchByName]    Script Date: 9/18/2025 11:29:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 10. Tìm kiếm nhân viên theo tên
CREATE OR ALTER PROCEDURE [dbo].[sp_NhanVien_SearchByName]
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
        nv.ma_nv,
        nv.ho_ten,
        nv.loai_nv_id,
        lnv.loai_nv,
        nv.ngay_vao_lam,
        nv.trang_thai,
        lnv.luong_co_ban
    FROM dbo.NhanVien nv
    INNER JOIN dbo.LoaiNhanVien lnv ON nv.loai_nv_id = lnv.loai_nv_id
    WHERE nv.ho_ten COLLATE SQL_Latin1_General_CP1_CI_AI LIKE N'%' + @search_term + N'%' COLLATE SQL_Latin1_General_CP1_CI_AI
    ORDER BY nv.ho_ten;

	--Su dung COLLATE SQL_Latin1_General_CP1_CI_AI de search khong dau
	-- co the gan collate khi tao bang, vi du: Name NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AI
END
GO
/****** Object:  StoredProcedure [dbo].[sp_NhanVien_SearchByNamePaged]    Script Date: 9/18/2025 11:29:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[sp_NhanVien_SearchByNamePaged]
    @search_term NVARCHAR(100),
    @page_number INT,
    @page_size INT,
    @total_count INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @offset INT = (@page_number - 1) * @page_size;
    
    -- Lấy tổng số bản ghi tìm kiếm
    SELECT @total_count = COUNT(1)
    FROM dbo.NhanVien nv
    INNER JOIN dbo.LoaiNhanVien lnv ON nv.loai_nv_id = lnv.loai_nv_id
    WHERE nv.ho_ten COLLATE SQL_Latin1_General_CP1_CI_AI LIKE N'%' + @search_term + N'%' COLLATE SQL_Latin1_General_CP1_CI_AI
    
    -- Lấy dữ liệu phân trang tìm kiếm
    SELECT 
        nv.nv_id,
        nv.ma_nv,
        nv.ho_ten,
        nv.loai_nv_id,
        lnv.loai_nv,
        nv.ngay_vao_lam,
        nv.trang_thai,
        lnv.luong_co_ban
    FROM dbo.NhanVien nv
    INNER JOIN dbo.LoaiNhanVien lnv ON nv.loai_nv_id = lnv.loai_nv_id
    WHERE nv.ho_ten COLLATE SQL_Latin1_General_CP1_CI_AI LIKE N'%' + @search_term + N'%' COLLATE SQL_Latin1_General_CP1_CI_AI
    ORDER BY nv.nv_id DESC
    OFFSET @offset ROWS
    FETCH NEXT @page_size ROWS ONLY;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_NhanVien_Update]    Script Date: 9/18/2025 11:29:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 5. Cập nhật nhân viên
CREATE OR ALTER PROCEDURE [dbo].[sp_NhanVien_Update]
    @nv_id INT,
    @ho_ten NVARCHAR(120),
    @loai_nv_id INT,
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
    
    IF @loai_nv_id IS NULL OR @loai_nv_id <= 0
    BEGIN
        RAISERROR(N'Loại nhân viên không được để trống', 16, 1);
        RETURN;
    END
    
    -- Kiểm tra loại nhân viên có tồn tại không
    IF NOT EXISTS (SELECT 1 FROM dbo.LoaiNhanVien WHERE loai_nv_id = @loai_nv_id)
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
        loai_nv_id = @loai_nv_id,
        ngay_vao_lam = @ngay_vao_lam,
        trang_thai = @trang_thai
    WHERE nv_id = @nv_id;
    
    -- Trả về số dòng được cập nhật
    SELECT @@ROWCOUNT AS rows_affected;
END
GO
