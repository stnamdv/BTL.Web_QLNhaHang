CREATE OR ALTER PROC sp_ThongKe_NhaCungCapNguyenLieu (
	@Thang INT = NULL,
    @Nam INT = NULL,
    @NccId INT = NULL
) AS
BEGIN
	IF @Thang < 1 OR @Thang > 12
    BEGIN
        RAISERROR(N'Tháng phải từ 1 đến 12.', 16, 1);
        RETURN;
    END

	IF @Thang IS NULL SET @Thang = MONTH(GETDATE());
	IF @Nam IS NULL SET @Nam = YEAR(GETDATE());
	
	SELECT  
		ncc.ncc_id, ncc.ten, nl.don_vi, 
		SUM(ct.dinh_luong) AS tong_su_dung
	FROM [Order] o
	LEFT JOIN OrderItem oi ON o.order_id = oi.order_id
	JOIN dbo.CongThuc ct ON ct.mon_id = oi.mon_id
	JOIN dbo.NguyenLieu nl ON ct.nl_id = nl.nl_id
	JOIN dbo.NhaCungCap ncc ON ncc.ncc_id = nl.ncc_id
	WHERE 
		YEAR(o.thoi_diem_dat) = @Nam 
		AND MONTH(o.thoi_diem_dat) = @Thang
		AND (@NccId IS NULL OR ncc.ncc_id = @NccId)
	GROUP BY nl.don_vi, ncc.ten, ncc.ncc_id
	ORDER BY tong_su_dung DESC;
END