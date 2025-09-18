/* =========================================================
   STORED PROCEDURES CHO QUẢN LÝ LỊCH SỬ THỰC HIỆN
   Quản lý lịch sử thực hiện các bước xử lý đơn hàng
   ========================================================= */

USE NhaHang
GO

-- =========================================================
-- 1. sp_LichSuThucHien_GetAll - Lấy tất cả lịch sử
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_LichSuThucHien_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Đếm tổng số records
    SELECT @TotalCount = COUNT(1)
    FROM dbo.LichSuThucHien lsh
    JOIN dbo.OrderItem oi ON oi.order_item_id = lsh.order_item_id
    JOIN dbo.[Order] o ON o.order_id = oi.order_id;
    
    -- Lấy dữ liệu phân trang
    SELECT 
        lsh.lich_su_id,
        lsh.order_item_id,
        oi.order_id,
        o.ban_id,
        ba.so_hieu as ban_so_hieu,
        oi.mon_id,
        m.ten_mon,
        lsh.buoc_id,
        bx.ten_buoc,
        bx.thu_tu,
        lsh.nv_id,
        nv.ho_ten as nv_ho_ten,
        lnv.loai_nv,
        lsh.trang_thai,
        lsh.thoi_diem_bat_dau,
        lsh.thoi_diem_ket_thuc,
        lsh.ghi_chu,
        lsh.thoi_diem_tao
    FROM dbo.LichSuThucHien lsh
    JOIN dbo.OrderItem oi ON oi.order_item_id = lsh.order_item_id
    JOIN dbo.[Order] o ON o.order_id = oi.order_id
    LEFT JOIN dbo.BanAn ba ON ba.ban_id = o.ban_id
    JOIN dbo.Mon m ON m.mon_id = oi.mon_id
    JOIN dbo.BuocXuLy bx ON bx.buoc_id = lsh.buoc_id
    JOIN dbo.NhanVien nv ON nv.nv_id = lsh.nv_id
    JOIN dbo.LoaiNhanVien lnv ON lnv.loai_nv_id = nv.loai_nv_id
    ORDER BY lsh.thoi_diem_tao DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =========================================================
-- 2. sp_LichSuThucHien_GetByOrderItem - Lấy lịch sử theo order item
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_LichSuThucHien_GetByOrderItem
    @OrderItemId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        lsh.lich_su_id,
        lsh.order_item_id,
        lsh.buoc_id,
        bx.ten_buoc,
        bx.thu_tu,
        lsh.nv_id,
        nv.ho_ten as nv_ho_ten,
        lnv.loai_nv,
        lsh.trang_thai,
        lsh.thoi_diem_bat_dau,
        lsh.thoi_diem_ket_thuc,
        lsh.ghi_chu,
        lsh.thoi_diem_tao
    FROM dbo.LichSuThucHien lsh
    JOIN dbo.BuocXuLy bx ON bx.buoc_id = lsh.buoc_id
    JOIN dbo.NhanVien nv ON nv.nv_id = lsh.nv_id
    JOIN dbo.LoaiNhanVien lnv ON lnv.loai_nv_id = nv.loai_nv_id
    WHERE lsh.order_item_id = @OrderItemId
    ORDER BY bx.thu_tu;
END
GO

-- =========================================================
-- 3. sp_LichSuThucHien_GetByBuoc - Lấy lịch sử theo bước
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_LichSuThucHien_GetByBuoc
    @BuocId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        lsh.lich_su_id,
        lsh.order_item_id,
        oi.order_id,
        o.ban_id,
        ba.so_hieu as ban_so_hieu,
        oi.mon_id,
        m.ten_mon,
        lsh.buoc_id,
        bx.ten_buoc,
        bx.thu_tu,
        lsh.nv_id,
        nv.ho_ten as nv_ho_ten,
        lnv.loai_nv,
        lsh.trang_thai,
        lsh.thoi_diem_bat_dau,
        lsh.thoi_diem_ket_thuc,
        lsh.ghi_chu,
        lsh.thoi_diem_tao
    FROM dbo.LichSuThucHien lsh
    JOIN dbo.OrderItem oi ON oi.order_item_id = lsh.order_item_id
    JOIN dbo.[Order] o ON o.order_id = oi.order_id
    LEFT JOIN dbo.BanAn ba ON ba.ban_id = o.ban_id
    JOIN dbo.Mon m ON m.mon_id = oi.mon_id
    JOIN dbo.BuocXuLy bx ON bx.buoc_id = lsh.buoc_id
    JOIN dbo.NhanVien nv ON nv.nv_id = lsh.nv_id
    JOIN dbo.LoaiNhanVien lnv ON lnv.loai_nv_id = nv.loai_nv_id
    WHERE lsh.buoc_id = @BuocId
    ORDER BY lsh.thoi_diem_tao DESC;
END
GO

-- =========================================================
-- 4. sp_LichSuThucHien_GetByNhanVien - Lấy lịch sử theo nhân viên
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_LichSuThucHien_GetByNhanVien
    @NvId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        lsh.lich_su_id,
        lsh.order_item_id,
        oi.order_id,
        o.ban_id,
        ba.so_hieu as ban_so_hieu,
        oi.mon_id,
        m.ten_mon,
        lsh.buoc_id,
        bx.ten_buoc,
        bx.thu_tu,
        lsh.nv_id,
        nv.ho_ten as nv_ho_ten,
        lnv.loai_nv,
        lsh.trang_thai,
        lsh.thoi_diem_bat_dau,
        lsh.thoi_diem_ket_thuc,
        lsh.ghi_chu,
        lsh.thoi_diem_tao
    FROM dbo.LichSuThucHien lsh
    JOIN dbo.OrderItem oi ON oi.order_item_id = lsh.order_item_id
    JOIN dbo.[Order] o ON o.order_id = oi.order_id
    LEFT JOIN dbo.BanAn ba ON ba.ban_id = o.ban_id
    JOIN dbo.Mon m ON m.mon_id = oi.mon_id
    JOIN dbo.BuocXuLy bx ON bx.buoc_id = lsh.buoc_id
    JOIN dbo.NhanVien nv ON nv.nv_id = lsh.nv_id
    JOIN dbo.LoaiNhanVien lnv ON lnv.loai_nv_id = nv.loai_nv_id
    WHERE lsh.nv_id = @NvId
    ORDER BY lsh.thoi_diem_tao DESC;
END
GO

-- =========================================================
-- 5. sp_LichSuThucHien_Create - Tạo lịch sử mới
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_LichSuThucHien_Create
    @OrderItemId INT,
    @BuocId INT,
    @NvId INT,
    @GhiChu NVARCHAR(500) = NULL,
    @LichSuId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validation: Kiểm tra order item có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.OrderItem WHERE order_item_id = @OrderItemId)
        BEGIN
            RAISERROR(N'Order item không tồn tại.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation: Kiểm tra bước có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.BuocXuLy WHERE buoc_id = @BuocId)
        BEGIN
            RAISERROR(N'Bước xử lý không tồn tại.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation: Kiểm tra nhân viên có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien WHERE nv_id = @NvId AND trang_thai = 'ACTIVE')
        BEGIN
            RAISERROR(N'Nhân viên không tồn tại hoặc không hoạt động.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation: Kiểm tra nhân viên có đúng loại cho bước này không
        IF NOT EXISTS (
            SELECT 1 FROM dbo.PhanCongBuocXuLy pcbx
            JOIN dbo.NhanVien nv ON nv.loai_nv_id = pcbx.loai_nv_id
            WHERE pcbx.buoc_id = @BuocId AND nv.nv_id = @NvId AND pcbx.trang_thai = 'ACTIVE'
        )
        BEGIN
            RAISERROR(N'Nhân viên không được phân công cho bước này.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation: Kiểm tra đã có lịch sử cho bước này chưa
        IF EXISTS (SELECT 1 FROM dbo.LichSuThucHien WHERE order_item_id = @OrderItemId AND buoc_id = @BuocId)
        BEGIN
            RAISERROR(N'Đã có lịch sử thực hiện cho bước này.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Tạo lịch sử mới
        INSERT INTO dbo.LichSuThucHien (order_item_id, buoc_id, nv_id, trang_thai, ghi_chu)
        VALUES (@OrderItemId, @BuocId, @NvId, 'CHUA_BAT_DAU', @GhiChu);
        
        SET @LichSuId = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =========================================================
-- 6. sp_LichSuThucHien_BatDau - Bắt đầu thực hiện bước
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_LichSuThucHien_BatDau
    @LichSuId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validation: Kiểm tra lịch sử có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.LichSuThucHien WHERE lich_su_id = @LichSuId)
        BEGIN
            RAISERROR(N'Lịch sử thực hiện không tồn tại.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation: Kiểm tra trạng thái có hợp lệ không
        IF NOT EXISTS (SELECT 1 FROM dbo.LichSuThucHien WHERE lich_su_id = @LichSuId AND trang_thai = 'CHUA_BAT_DAU')
        BEGIN
            RAISERROR(N'Chỉ có thể bắt đầu bước chưa được thực hiện.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Cập nhật trạng thái bắt đầu
        UPDATE dbo.LichSuThucHien
        SET trang_thai = 'DANG_THUC_HIEN',
            thoi_diem_bat_dau = GETDATE()
        WHERE lich_su_id = @LichSuId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =========================================================
-- 7. sp_LichSuThucHien_HoanThanh - Hoàn thành bước
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_LichSuThucHien_HoanThanh
    @LichSuId INT,
    @GhiChu NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validation: Kiểm tra lịch sử có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM dbo.LichSuThucHien WHERE lich_su_id = @LichSuId)
        BEGIN
            RAISERROR(N'Lịch sử thực hiện không tồn tại.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validation: Kiểm tra trạng thái có hợp lệ không
        IF NOT EXISTS (SELECT 1 FROM dbo.LichSuThucHien WHERE lich_su_id = @LichSuId AND trang_thai = 'DANG_THUC_HIEN')
        BEGIN
            RAISERROR(N'Chỉ có thể hoàn thành bước đang thực hiện.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Cập nhật trạng thái hoàn thành
        UPDATE dbo.LichSuThucHien
        SET trang_thai = 'HOAN_THANH',
            thoi_diem_ket_thuc = GETDATE(),
            ghi_chu = ISNULL(@GhiChu, ghi_chu)
        WHERE lich_su_id = @LichSuId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =========================================================
-- 8. sp_LichSuThucHien_GetTrangThai - Lấy trạng thái hiện tại của order item
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_LichSuThucHien_GetTrangThai
    @OrderItemId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        lsh.lich_su_id,
        lsh.buoc_id,
        bx.ten_buoc,
        bx.thu_tu,
        lsh.trang_thai,
        lsh.thoi_diem_bat_dau,
        lsh.thoi_diem_ket_thuc,
        CASE 
            WHEN lsh.thoi_diem_bat_dau IS NOT NULL AND lsh.thoi_diem_ket_thuc IS NOT NULL 
            THEN DATEDIFF(MINUTE, lsh.thoi_diem_bat_dau, lsh.thoi_diem_ket_thuc)
            ELSE NULL
        END as thoi_gian_thuc_hien_phut
    FROM dbo.LichSuThucHien lsh
    JOIN dbo.BuocXuLy bx ON bx.buoc_id = lsh.buoc_id
    WHERE lsh.order_item_id = @OrderItemId
    ORDER BY bx.thu_tu;
END
GO

-- =========================================================
-- 9. sp_LichSuThucHien_GetThongKe - Thống kê hiệu suất nhân viên
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.sp_LichSuThucHien_GetThongKe
    @NvId INT = NULL,
    @BuocId INT = NULL,
    @TuNgay DATETIME2(0) = NULL,
    @DenNgay DATETIME2(0) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        lsh.nv_id,
        nv.ho_ten as nv_ho_ten,
        lnv.loai_nv,
        lsh.buoc_id,
        bx.ten_buoc,
        COUNT(*) as so_luong_thuc_hien,
        AVG(DATEDIFF(MINUTE, lsh.thoi_diem_bat_dau, lsh.thoi_diem_ket_thuc)) as thoi_gian_trung_binh_phut,
        MIN(DATEDIFF(MINUTE, lsh.thoi_diem_bat_dau, lsh.thoi_diem_ket_thuc)) as thoi_gian_nhanh_nhat_phut,
        MAX(DATEDIFF(MINUTE, lsh.thoi_diem_bat_dau, lsh.thoi_diem_ket_thuc)) as thoi_gian_cham_nhat_phut
    FROM dbo.LichSuThucHien lsh
    JOIN dbo.NhanVien nv ON nv.nv_id = lsh.nv_id
    JOIN dbo.LoaiNhanVien lnv ON lnv.loai_nv_id = nv.loai_nv_id
    JOIN dbo.BuocXuLy bx ON bx.buoc_id = lsh.buoc_id
    WHERE lsh.trang_thai = 'HOAN_THANH'
      AND lsh.thoi_diem_bat_dau IS NOT NULL
      AND lsh.thoi_diem_ket_thuc IS NOT NULL
      AND (@NvId IS NULL OR lsh.nv_id = @NvId)
      AND (@BuocId IS NULL OR lsh.buoc_id = @BuocId)
      AND (@TuNgay IS NULL OR lsh.thoi_diem_tao >= @TuNgay)
      AND (@DenNgay IS NULL OR lsh.thoi_diem_tao <= @DenNgay)
    GROUP BY lsh.nv_id, nv.ho_ten, lnv.loai_nv, lsh.buoc_id, bx.ten_buoc
    ORDER BY lnv.loai_nv, nv.ho_ten;
END
GO
