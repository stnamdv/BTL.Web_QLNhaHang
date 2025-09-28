USE [NhaHang]
GO
/****** Object:  StoredProcedure [dbo].[sp_ThongKe_NhaCungCapNguyenLieu_TongChi]    Script Date: 9/28/2025 3:42:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROC [dbo].[sp_ThongKe_NhaCungCapNguyenLieu_TongChi_TheoNgay](
	@TuNgay DATETIME = NULL,
    @DenNgay DATETIME = NULL
) AS
BEGIN
	SELECT  
		nl.nl_id, nl.ten ten_nguyen_lieu, nl.don_vi,
		SUM(ct.dinh_luong * oi.so_luong) AS tong_luong,
		SUM(ct.dinh_luong * oi.so_luong * nl.gia_nhap) AS tong_gia_tri -- Tổng tiền = tổng định lượng * số lượng món trong 1 order * giá nhập
	FROM [Order] o
	JOIN OrderItem oi ON o.order_id = oi.order_id
	JOIN dbo.CongThuc ct ON ct.mon_id = oi.mon_id
	JOIN dbo.NguyenLieu nl ON ct.nl_id = nl.nl_id
	WHERE 
		(o.thoi_diem_dat >= @TuNgay OR @TuNgay IS NULL)
		AND (o.thoi_diem_dat < @DenNgay + 1 OR @DenNgay IS NULL)
	GROUP BY nl.nl_id, nl.ten, nl.don_vi
	ORDER BY tong_gia_tri DESC;
END