USE [NhaHang]
GO
/****** Object:  Table [dbo].[BanAn]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BanAn](
	[ban_id] [int] IDENTITY(1,1) NOT NULL,
	[loai_ban_id] [int] NOT NULL,
	[so_hieu] [nvarchar](10) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ban_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[so_hieu] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BuocXuLy]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BuocXuLy](
	[buoc_id] [int] IDENTITY(1,1) NOT NULL,
	[ten_buoc] [nvarchar](100) NOT NULL,
	[mo_ta] [nvarchar](500) NULL,
	[thu_tu] [int] NOT NULL,
	[thoi_gian_du_kien] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[buoc_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UK_BuocXuLy_ThuTu] UNIQUE NONCLUSTERED 
(
	[thu_tu] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CongThuc]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CongThuc](
	[mon_id] [int] NOT NULL,
	[nl_id] [int] NOT NULL,
	[dinh_luong] [decimal](14, 3) NOT NULL,
 CONSTRAINT [PK_CongThuc] PRIMARY KEY CLUSTERED 
(
	[mon_id] ASC,
	[nl_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HoaDon]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HoaDon](
	[hd_id] [int] IDENTITY(1,1) NOT NULL,
	[order_id] [int] NOT NULL,
	[thoi_diem_tt] [datetime2](0) NOT NULL,
	[phuong_thuc] [nvarchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[hd_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[order_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[KhachHang]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KhachHang](
	[kh_id] [int] IDENTITY(1,1) NOT NULL,
	[ho_ten] [nvarchar](120) NULL,
	[so_dien_thoai] [nvarchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[kh_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Layout]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Layout](
	[layout_id] [int] IDENTITY(1,1) NOT NULL,
	[layout_name] [nvarchar](100) NOT NULL,
	[grid_size] [int] NOT NULL,
	[created_date] [datetime] NULL,
	[updated_date] [datetime] NULL,
	[is_active] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[layout_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LayoutDetail]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LayoutDetail](
	[layout_detail_id] [int] IDENTITY(1,1) NOT NULL,
	[layout_id] [int] NOT NULL,
	[ban_id] [int] NOT NULL,
	[position_x] [float] NOT NULL,
	[position_y] [float] NOT NULL,
	[created_date] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[layout_detail_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LichSuThucHien]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LichSuThucHien](
	[lich_su_id] [int] IDENTITY(1,1) NOT NULL,
	[order_item_id] [int] NOT NULL,
	[buoc_id] [int] NOT NULL,
	[nv_id] [int] NOT NULL,
	[trang_thai] [nvarchar](20) NOT NULL,
	[thoi_diem_bat_dau] [datetime2](0) NULL,
	[thoi_diem_ket_thuc] [datetime2](0) NULL,
	[ghi_chu] [nvarchar](500) NULL,
	[thoi_diem_tao] [datetime2](0) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[lich_su_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LoaiBan]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoaiBan](
	[loai_ban_id] [int] IDENTITY(1,1) NOT NULL,
	[suc_chua] [int] NOT NULL,
	[so_luong] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[loai_ban_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LoaiNhanVien]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoaiNhanVien](
	[loai_nv_id] [int] IDENTITY(1,1) NOT NULL,
	[loai_nv] [nvarchar](20) NOT NULL,
	[luong_co_ban] [decimal](12, 2) NOT NULL,
 CONSTRAINT [PK_LoaiNhanVien] PRIMARY KEY CLUSTERED 
(
	[loai_nv_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Mon]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Mon](
	[mon_id] [int] IDENTITY(1,1) NOT NULL,
	[ma_mon] [nvarchar](20) NOT NULL,
	[ten_mon] [nvarchar](120) NOT NULL,
	[loai_mon] [nvarchar](12) NOT NULL,
	[gia] [decimal](12, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[mon_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[ma_mon] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NguyenLieu]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NguyenLieu](
	[nl_id] [int] IDENTITY(1,1) NOT NULL,
	[ten] [nvarchar](160) NOT NULL,
	[don_vi] [nvarchar](20) NOT NULL,
	[nguon_goc] [nvarchar](max) NOT NULL,
	[ncc_id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[nl_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NhaCungCap]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NhaCungCap](
	[ncc_id] [int] IDENTITY(1,1) NOT NULL,
	[ten] [nvarchar](160) NOT NULL,
	[dia_chi] [nvarchar](max) NULL,
	[sdt] [nvarchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[ncc_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NhanVien]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NhanVien](
	[nv_id] [int] IDENTITY(1,1) NOT NULL,
	[loai_nv_id] [int] NOT NULL,
	[ma_nv] [varchar](20) NOT NULL,
	[ho_ten] [nvarchar](120) NOT NULL,
	[ngay_vao_lam] [date] NULL,
	[trang_thai] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK__NhanVien__CA1AF457904DD9A3] PRIMARY KEY CLUSTERED 
(
	[nv_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UK_NhanVien_ma_nv] UNIQUE NONCLUSTERED 
(
	[ma_nv] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Order]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Order](
	[order_id] [int] IDENTITY(1,1) NOT NULL,
	[kh_id] [int] NULL,
	[ban_id] [int] NULL,
	[la_mang_ve] [bit] NOT NULL,
	[so_khach] [int] NULL,
	[thoi_diem_dat] [datetime2](0) NOT NULL,
	[trang_thai] [nvarchar](20) NOT NULL,
	[tong_tien] [decimal](12, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[order_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderItem]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderItem](
	[order_item_id] [int] IDENTITY(1,1) NOT NULL,
	[order_id] [int] NOT NULL,
	[mon_id] [int] NOT NULL,
	[so_luong] [int] NOT NULL,
	[t_dat] [datetime2](0) NOT NULL,
	[t_hoan_thanh] [datetime2](0) NULL,
	[t_phuc_vu] [datetime2](0) NULL,
PRIMARY KEY CLUSTERED 
(
	[order_item_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PhanCongBuocXuLy]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PhanCongBuocXuLy](
	[phan_cong_buoc_id] [int] IDENTITY(1,1) NOT NULL,
	[buoc_id] [int] NOT NULL,
	[loai_nv_id] [int] NOT NULL,
	[vai_tro] [nvarchar](50) NULL,
	[trang_thai] [nvarchar](20) NOT NULL,
	[thoi_diem_tao] [datetime2](0) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[phan_cong_buoc_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UK_PhanCongBuocXuLy_Buoc_LoaiNv] UNIQUE NONCLUSTERED 
(
	[buoc_id] ASC,
	[loai_nv_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ThucDon]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ThucDon](
	[thuc_don_id] [int] IDENTITY(1,1) NOT NULL,
	[ten] [nvarchar](120) NOT NULL,
	[hieu_luc_tu] [date] NOT NULL,
	[hieu_luc_den] [date] NULL,
PRIMARY KEY CLUSTERED 
(
	[thuc_don_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ThucDon_Mon]    Script Date: 9/18/2025 6:07:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ThucDon_Mon](
	[thuc_don_id] [int] NOT NULL,
	[mon_id] [int] NOT NULL,
 CONSTRAINT [PK_ThucDon_Mon] PRIMARY KEY CLUSTERED 
(
	[thuc_don_id] ASC,
	[mon_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Layout] ADD  DEFAULT ((10)) FOR [grid_size]
GO
ALTER TABLE [dbo].[Layout] ADD  DEFAULT (getdate()) FOR [created_date]
GO
ALTER TABLE [dbo].[Layout] ADD  DEFAULT (getdate()) FOR [updated_date]
GO
ALTER TABLE [dbo].[Layout] ADD  DEFAULT ((1)) FOR [is_active]
GO
ALTER TABLE [dbo].[LayoutDetail] ADD  DEFAULT (getdate()) FOR [created_date]
GO
ALTER TABLE [dbo].[LichSuThucHien] ADD  DEFAULT ('CHUA_BAT_DAU') FOR [trang_thai]
GO
ALTER TABLE [dbo].[LichSuThucHien] ADD  DEFAULT (getdate()) FOR [thoi_diem_tao]
GO
ALTER TABLE [dbo].[NguyenLieu] ADD  DEFAULT ((1)) FOR [ncc_id]
GO
ALTER TABLE [dbo].[NhanVien] ADD  CONSTRAINT [DF__NhanVien__trang___5FB337D6]  DEFAULT (N'ACTIVE') FOR [trang_thai]
GO
ALTER TABLE [dbo].[Order] ADD  DEFAULT ((0)) FOR [la_mang_ve]
GO
ALTER TABLE [dbo].[Order] ADD  CONSTRAINT [DF_Order_trang_thai]  DEFAULT (N'pending') FOR [trang_thai]
GO
ALTER TABLE [dbo].[PhanCongBuocXuLy] ADD  DEFAULT ('ACTIVE') FOR [trang_thai]
GO
ALTER TABLE [dbo].[PhanCongBuocXuLy] ADD  DEFAULT (getdate()) FOR [thoi_diem_tao]
GO
ALTER TABLE [dbo].[BanAn]  WITH CHECK ADD FOREIGN KEY([loai_ban_id])
REFERENCES [dbo].[LoaiBan] ([loai_ban_id])
GO
ALTER TABLE [dbo].[CongThuc]  WITH CHECK ADD  CONSTRAINT [FK_CT_Mon] FOREIGN KEY([mon_id])
REFERENCES [dbo].[Mon] ([mon_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CongThuc] CHECK CONSTRAINT [FK_CT_Mon]
GO
ALTER TABLE [dbo].[CongThuc]  WITH CHECK ADD  CONSTRAINT [FK_CT_NL] FOREIGN KEY([nl_id])
REFERENCES [dbo].[NguyenLieu] ([nl_id])
GO
ALTER TABLE [dbo].[CongThuc] CHECK CONSTRAINT [FK_CT_NL]
GO
ALTER TABLE [dbo].[HoaDon]  WITH CHECK ADD FOREIGN KEY([order_id])
REFERENCES [dbo].[Order] ([order_id])
GO
ALTER TABLE [dbo].[LayoutDetail]  WITH CHECK ADD FOREIGN KEY([ban_id])
REFERENCES [dbo].[BanAn] ([ban_id])
GO
ALTER TABLE [dbo].[LayoutDetail]  WITH CHECK ADD FOREIGN KEY([layout_id])
REFERENCES [dbo].[Layout] ([layout_id])
GO
ALTER TABLE [dbo].[LichSuThucHien]  WITH CHECK ADD FOREIGN KEY([buoc_id])
REFERENCES [dbo].[BuocXuLy] ([buoc_id])
GO
ALTER TABLE [dbo].[LichSuThucHien]  WITH CHECK ADD FOREIGN KEY([nv_id])
REFERENCES [dbo].[NhanVien] ([nv_id])
GO
ALTER TABLE [dbo].[LichSuThucHien]  WITH CHECK ADD FOREIGN KEY([order_item_id])
REFERENCES [dbo].[OrderItem] ([order_item_id])
GO
ALTER TABLE [dbo].[NguyenLieu]  WITH CHECK ADD  CONSTRAINT [FK_NguyenLieu_NhaCungCap] FOREIGN KEY([ncc_id])
REFERENCES [dbo].[NhaCungCap] ([ncc_id])
GO
ALTER TABLE [dbo].[NguyenLieu] CHECK CONSTRAINT [FK_NguyenLieu_NhaCungCap]
GO
ALTER TABLE [dbo].[NhanVien]  WITH CHECK ADD  CONSTRAINT [FK_NhanVien_LoaiNhanVien] FOREIGN KEY([loai_nv_id])
REFERENCES [dbo].[LoaiNhanVien] ([loai_nv_id])
GO
ALTER TABLE [dbo].[NhanVien] CHECK CONSTRAINT [FK_NhanVien_LoaiNhanVien]
GO
ALTER TABLE [dbo].[Order]  WITH CHECK ADD FOREIGN KEY([ban_id])
REFERENCES [dbo].[BanAn] ([ban_id])
GO
ALTER TABLE [dbo].[Order]  WITH CHECK ADD FOREIGN KEY([kh_id])
REFERENCES [dbo].[KhachHang] ([kh_id])
GO
ALTER TABLE [dbo].[OrderItem]  WITH CHECK ADD FOREIGN KEY([mon_id])
REFERENCES [dbo].[Mon] ([mon_id])
GO
ALTER TABLE [dbo].[OrderItem]  WITH CHECK ADD FOREIGN KEY([order_id])
REFERENCES [dbo].[Order] ([order_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PhanCongBuocXuLy]  WITH CHECK ADD FOREIGN KEY([buoc_id])
REFERENCES [dbo].[BuocXuLy] ([buoc_id])
GO
ALTER TABLE [dbo].[PhanCongBuocXuLy]  WITH CHECK ADD FOREIGN KEY([loai_nv_id])
REFERENCES [dbo].[LoaiNhanVien] ([loai_nv_id])
GO
ALTER TABLE [dbo].[ThucDon_Mon]  WITH CHECK ADD  CONSTRAINT [FK_TDM_Mon] FOREIGN KEY([mon_id])
REFERENCES [dbo].[Mon] ([mon_id])
GO
ALTER TABLE [dbo].[ThucDon_Mon] CHECK CONSTRAINT [FK_TDM_Mon]
GO
ALTER TABLE [dbo].[ThucDon_Mon]  WITH CHECK ADD  CONSTRAINT [FK_TDM_ThucDon] FOREIGN KEY([thuc_don_id])
REFERENCES [dbo].[ThucDon] ([thuc_don_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ThucDon_Mon] CHECK CONSTRAINT [FK_TDM_ThucDon]
GO
ALTER TABLE [dbo].[BuocXuLy]  WITH CHECK ADD CHECK  (([thu_tu]>(0)))
GO
ALTER TABLE [dbo].[CongThuc]  WITH CHECK ADD CHECK  (([dinh_luong]>(0)))
GO
ALTER TABLE [dbo].[LichSuThucHien]  WITH CHECK ADD  CONSTRAINT [CK_LichSuThucHien_ThoiGian] CHECK  (([thoi_diem_ket_thuc] IS NULL OR [thoi_diem_bat_dau] IS NULL OR [thoi_diem_ket_thuc]>=[thoi_diem_bat_dau]))
GO
ALTER TABLE [dbo].[LichSuThucHien] CHECK CONSTRAINT [CK_LichSuThucHien_ThoiGian]
GO
ALTER TABLE [dbo].[LoaiBan]  WITH CHECK ADD CHECK  (([so_luong]>=(0)))
GO
ALTER TABLE [dbo].[LoaiBan]  WITH CHECK ADD CHECK  (([suc_chua]>(0)))
GO
ALTER TABLE [dbo].[LoaiNhanVien]  WITH CHECK ADD  CONSTRAINT [CK__LoaiNhanV__luong__5BE2A6F2] CHECK  (([luong_co_ban]>=(0)))
GO
ALTER TABLE [dbo].[LoaiNhanVien] CHECK CONSTRAINT [CK__LoaiNhanV__luong__5BE2A6F2]
GO
ALTER TABLE [dbo].[Mon]  WITH CHECK ADD CHECK  (([gia]>=(0)))
GO
ALTER TABLE [dbo].[Mon]  WITH CHECK ADD CHECK  (([loai_mon]=N'TRANG_MIENG' OR [loai_mon]=N'MON_CHINH' OR [loai_mon]=N'KHAI_VI'))
GO
ALTER TABLE [dbo].[Mon]  WITH CHECK ADD  CONSTRAINT [CK_Mon_Ma_Theo_Loai] CHECK  (([loai_mon]=N'KHAI_VI' AND [ma_mon] like N'KV-%' OR [loai_mon]=N'MON_CHINH' AND [ma_mon] like N'MC-%' OR [loai_mon]=N'TRANG_MIENG' AND [ma_mon] like N'TM-%'))
GO
ALTER TABLE [dbo].[Mon] CHECK CONSTRAINT [CK_Mon_Ma_Theo_Loai]
GO
ALTER TABLE [dbo].[Order]  WITH CHECK ADD CHECK  (([so_khach] IS NULL OR [so_khach]>(0)))
GO
ALTER TABLE [dbo].[Order]  WITH CHECK ADD  CONSTRAINT [CK_Order_DineIn_Takeaway] CHECK  (([la_mang_ve]=(1) AND [ban_id] IS NULL OR [la_mang_ve]=(0) AND [ban_id] IS NOT NULL AND [so_khach] IS NOT NULL))
GO
ALTER TABLE [dbo].[Order] CHECK CONSTRAINT [CK_Order_DineIn_Takeaway]
GO
ALTER TABLE [dbo].[OrderItem]  WITH CHECK ADD CHECK  (([so_luong]>(0)))
GO
ALTER TABLE [dbo].[OrderItem]  WITH CHECK ADD  CONSTRAINT [CK_OI_Time_Seq1] CHECK  (([t_hoan_thanh] IS NULL OR [t_hoan_thanh]>=[t_dat]))
GO
ALTER TABLE [dbo].[OrderItem] CHECK CONSTRAINT [CK_OI_Time_Seq1]
GO
ALTER TABLE [dbo].[OrderItem]  WITH CHECK ADD  CONSTRAINT [CK_OI_Time_Seq2] CHECK  (([t_phuc_vu] IS NULL OR [t_hoan_thanh] IS NULL OR [t_phuc_vu]>=[t_hoan_thanh]))
GO
ALTER TABLE [dbo].[OrderItem] CHECK CONSTRAINT [CK_OI_Time_Seq2]
GO
ALTER TABLE [dbo].[ThucDon]  WITH CHECK ADD  CONSTRAINT [CK_ThucDon_Date] CHECK  (([hieu_luc_den] IS NULL OR [hieu_luc_den]>=[hieu_luc_tu]))
GO
ALTER TABLE [dbo].[ThucDon] CHECK CONSTRAINT [CK_ThucDon_Date]
GO
