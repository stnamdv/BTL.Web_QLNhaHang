-- =============================================
-- Stored Procedures for BanAn Search and Pagination
-- =============================================

-- 1. Search BanAn with Pagination
-- =============================================
CREATE OR ALTER PROCEDURE sp_BanAn_SearchWithPagination
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(100) = NULL,
    @LoaiBanId INT = NULL,
    @Capacity INT = NULL,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Đếm tổng số records
    SELECT @TotalCount = COUNT(1)
    FROM dbo.BanAn ba
    INNER JOIN dbo.LoaiBan lb ON ba.loai_ban_id = lb.loai_ban_id
    WHERE (@SearchTerm IS NULL OR ba.so_hieu LIKE N'%' + @SearchTerm + N'%')
      AND (@LoaiBanId IS NULL OR ba.loai_ban_id = @LoaiBanId)
      AND (@Capacity IS NULL OR lb.suc_chua = @Capacity);
    
    -- Lấy dữ liệu phân trang
    SELECT 
        ba.ban_id,
        ba.so_hieu,
        ba.loai_ban_id,
        lb.suc_chua,
        lb.so_luong
    FROM dbo.BanAn ba
    INNER JOIN dbo.LoaiBan lb ON ba.loai_ban_id = lb.loai_ban_id
    WHERE (@SearchTerm IS NULL OR ba.so_hieu LIKE N'%' + @SearchTerm + N'%')
      AND (@LoaiBanId IS NULL OR ba.loai_ban_id = @LoaiBanId)
      AND (@Capacity IS NULL OR lb.suc_chua = @Capacity)
    ORDER BY ba.loai_ban_id, ba.ban_id
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- 2. Get Table Status Summary
-- =============================================
CREATE OR ALTER PROCEDURE sp_BanAn_GetTableStatus
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        lb.loai_ban_id,
        lb.suc_chua,
        lb.so_luong,
        COUNT(ba.ban_id) AS so_ban_hien_co,
        COUNT(CASE WHEN o.order_id IS NOT NULL AND o.trang_thai NOT IN ('HOAN_THANH', 'DA_HUY') THEN 1 END) AS so_ban_dang_su_dung,
        COUNT(ba.ban_id) - COUNT(CASE WHEN o.order_id IS NOT NULL AND o.trang_thai NOT IN ('HOAN_THANH', 'DA_HUY') THEN 1 END) AS so_ban_trong
    FROM dbo.LoaiBan lb
    LEFT JOIN dbo.BanAn ba ON lb.loai_ban_id = ba.loai_ban_id
    LEFT JOIN dbo.[Order] o ON ba.ban_id = o.ban_id 
        AND o.thoi_diem_dat >= CAST(GETDATE() AS DATE)
        AND o.trang_thai NOT IN ('HOAN_THANH', 'DA_HUY')
    GROUP BY lb.loai_ban_id, lb.suc_chua, lb.so_luong
    ORDER BY lb.loai_ban_id;
END
GO

-- 3. Get Available Tables
-- =============================================
CREATE OR ALTER PROCEDURE sp_BanAn_GetAvailableTables
    @Capacity INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ba.ban_id,
        ba.so_hieu,
        ba.loai_ban_id,
        lb.suc_chua,
        lb.so_luong
    FROM dbo.BanAn ba
    INNER JOIN dbo.LoaiBan lb ON ba.loai_ban_id = lb.loai_ban_id
    WHERE ba.ban_id NOT IN (
        SELECT DISTINCT o.ban_id 
        FROM dbo.[Order] o 
        WHERE o.ban_id IS NOT NULL
          AND o.thoi_diem_dat >= CAST(GETDATE() AS DATE)
          AND o.trang_thai NOT IN ('HOAN_THANH', 'DA_HUY')
    )
    AND (@Capacity IS NULL OR lb.suc_chua >= @Capacity)
    ORDER BY lb.loai_ban_id, ba.ban_id;
END
GO
