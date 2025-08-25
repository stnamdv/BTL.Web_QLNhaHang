USE NhaHang;
GO

/* ===== XÓA DỮ LIỆU CŨ (đúng thứ tự FK) ===== */
DELETE FROM HoaDon;
DELETE FROM OrderItem;
DELETE FROM [Order];
DELETE FROM CongThuc;
DELETE FROM NL_NCC;
DELETE FROM ThucDon_Mon;
DELETE FROM ThucDon;
DELETE FROM Mon;
DELETE FROM NguyenLieu;
DELETE FROM NhaCungCap;
DELETE FROM BanAn;
DELETE FROM LoaiBan;
DELETE FROM NhanVien;
DELETE FROM LoaiNhanVien;
DELETE FROM KhachHang;
GO

/* ===== KHÁCH HÀNG (đã sửa tên cột) ===== */
INSERT INTO KhachHang (ho_ten, so_dien_thoai) VALUES
(N'Nguyễn Anh',  N'0901000001'),
(N'Trần Bình',   N'0901000002'),
(N'Lê Cường',    N'0901000003');

/* ===== LOẠI NHÂN VIÊN + LƯƠNG CƠ BẢN ===== */
INSERT INTO LoaiNhanVien (loai_nv, luong_co_ban) VALUES
(N'BEP',       8000000),
(N'PHUC_VU',   6000000),
(N'DICH_VU',   5500000),
(N'THU_NGAN',  7000000);

/* ===== NHÂN VIÊN ===== */
INSERT INTO NhanVien (ho_ten, loai_nv, ngay_vao_lam, trang_thai) VALUES
(N'Nguyễn Văn Bếp',  N'BEP',      '2024-01-10', N'ACTIVE'),
(N'Trần Thị Phục',   N'PHUC_VU',  '2024-03-05', N'ACTIVE'),
(N'Lê Văn Dịch',     N'DICH_VU',  '2024-05-22', N'ACTIVE'),
(N'Phạm Thị Thu',    N'THU_NGAN', '2024-02-15', N'ACTIVE');

/* ===== LOẠI BÀN (cần cả so_luong) ===== */
INSERT INTO LoaiBan (suc_chua, so_luong) VALUES
(2,  5),   -- 5 bàn loại 2 ghế
(4,  5),   -- 5 bàn loại 4 ghế
(6,  2);   -- 2 bàn loại 6 ghế

/* Lấy id loại bàn để tạo bàn cụ thể */
DECLARE @lb2 INT = (SELECT loai_ban_id FROM LoaiBan WHERE suc_chua=2);
DECLARE @lb4 INT = (SELECT loai_ban_id FROM LoaiBan WHERE suc_chua=4);
DECLARE @lb6 INT = (SELECT loai_ban_id FROM LoaiBan WHERE suc_chua=6);

/* ===== BÀN ĂN ===== */
INSERT INTO BanAn (loai_ban_id, so_hieu) VALUES
(@lb2, N'B2-01'), (@lb2, N'B2-02'),
(@lb4, N'B4-01'), (@lb4, N'B4-02'),
(@lb6, N'B6-01');

/* ===== NHÀ CUNG CẤP ===== */
INSERT INTO NhaCungCap (ten, dia_chi, sdt) VALUES
(N'Công ty Thực phẩm Sạch', N'Q1, TP.HCM', N'0281111111'),
(N'NCC Hải Sản Tươi',       N'Q4, TP.HCM', N'0282222222'),
(N'Nhà cung cấp Rau Củ',    N'Q7, TP.HCM', N'0283333333');

/* ===== NGUYÊN LIỆU ===== */
INSERT INTO NguyenLieu (ten, don_vi, nguon_goc) VALUES
(N'Tôm tươi',    N'Kg',  N'Phú Quốc'),
(N'Cá hồi',      N'Kg',  N'Na Uy'),
(N'Rau xà lách', N'Kg',  N'Lâm Đồng'),
(N'Nước mắm',    N'L',   N'Phan Thiết'),
(N'Bánh mì',     N'Cái', N'HCM');

/* ===== MÓN ĂN (ma_mon theo rule KV-/MC-/TM-) ===== */
INSERT INTO Mon (ma_mon, ten_mon, loai_mon, gia) VALUES
(N'KV-01', N'Salad tôm',       N'KHAI_VI',     120000),
(N'KV-02', N'Bánh mì bơ tỏi',  N'KHAI_VI',      50000),
(N'MC-01', N'Cá hồi nướng',    N'MON_CHINH',   250000),
(N'MC-02', N'Tôm hấp',         N'MON_CHINH',   180000),
(N'TM-01', N'Bánh flan',       N'TRANG_MIENG',  40000);

/* Lấy id món & NL để tạo công thức */
DECLARE @mon_kv1 INT = (SELECT mon_id FROM Mon WHERE ma_mon=N'KV-01');
DECLARE @mon_kv2 INT = (SELECT mon_id FROM Mon WHERE ma_mon=N'KV-02');
DECLARE @mon_mc1 INT = (SELECT mon_id FROM Mon WHERE ma_mon=N'MC-01');
DECLARE @mon_mc2 INT = (SELECT mon_id FROM Mon WHERE ma_mon=N'MC-02');
DECLARE @mon_tm1 INT = (SELECT mon_id FROM Mon WHERE ma_mon=N'TM-01');

DECLARE @nl_tom  INT = (SELECT nl_id FROM NguyenLieu WHERE ten=N'Tôm tươi');
DECLARE @nl_cahoi INT = (SELECT nl_id FROM NguyenLieu WHERE ten=N'Cá hồi');
DECLARE @nl_rau  INT = (SELECT nl_id FROM NguyenLieu WHERE ten=N'Rau xà lách');
DECLARE @nl_nuocmam INT = (SELECT nl_id FROM NguyenLieu WHERE ten=N'Nước mắm');
DECLARE @nl_banhmi INT = (SELECT nl_id FROM NguyenLieu WHERE ten=N'Bánh mì');

/* ===== CÔNG THỨC (MÓN ↔ NGUYÊN LIỆU) ===== */
INSERT INTO CongThuc (mon_id, nl_id, dinh_luong) VALUES
(@mon_kv1, @nl_tom,     0.20),
(@mon_kv1, @nl_rau,     0.10),
(@mon_kv1, @nl_nuocmam, 0.05),
(@mon_kv2, @nl_banhmi,  1.00),
(@mon_mc1, @nl_cahoi,   0.25),
(@mon_mc1, @nl_rau,     0.05),
(@mon_mc2, @nl_tom,     0.30),
(@mon_mc2, @nl_nuocmam, 0.05),
(@mon_tm1, @nl_banhmi,  0.50);

/* ===== THỰC ĐƠN (tùy chọn) ===== */
INSERT INTO ThucDon (ten, hieu_luc_tu, hieu_luc_den)
VALUES (N'Menu tháng 7/2025', '2025-07-01', '2025-07-31');

DECLARE @td7 INT = SCOPE_IDENTITY();

INSERT INTO ThucDon_Mon (thuc_don_id, mon_id)
SELECT @td7, mon_id FROM Mon;

/* ===== NHẬP HÀNG (NL_NCC) ===== */
DECLARE @ncc1 INT = (SELECT ncc_id FROM NhaCungCap WHERE ten=N'Công ty Thực phẩm Sạch');
DECLARE @ncc2 INT = (SELECT ncc_id FROM NhaCungCap WHERE ten=N'NCC Hải Sản Tươi');
DECLARE @ncc3 INT = (SELECT ncc_id FROM NhaCungCap WHERE ten=N'Nhà cung cấp Rau Củ');

INSERT INTO NL_NCC (nl_id, ncc_id, ngay_nhap, so_luong, don_gia) VALUES
(@nl_tom,    @ncc1, '2025-07-01', 10, 300000),
(@nl_cahoi,  @ncc2, '2025-07-01',  5, 500000),
(@nl_rau,    @ncc3, '2025-07-01', 20,  40000),
(@nl_nuocmam,@ncc1, '2025-07-01',  5, 100000),
(@nl_banhmi, @ncc3, '2025-07-01', 50,  10000),
(@nl_tom,    @ncc1, '2025-07-10', 15, 310000),
(@nl_cahoi,  @ncc2, '2025-07-10',  8, 520000),
(@nl_rau,    @ncc3, '2025-07-10', 25,  42000);

/* ===== ĐƠN HÀNG (không chèn identity tay) ===== */
DECLARE @kh1 INT = (SELECT TOP 1 kh_id FROM KhachHang WHERE ho_ten=N'Nguyễn Anh');
DECLARE @kh2 INT = (SELECT TOP 1 kh_id FROM KhachHang WHERE ho_ten=N'Trần Bình');
DECLARE @kh3 INT = (SELECT TOP 1 kh_id FROM KhachHang WHERE ho_ten=N'Lê Cường');

DECLARE @ban_B2_01 INT = (SELECT ban_id FROM BanAn WHERE so_hieu=N'B2-01');
DECLARE @ban_B4_01 INT = (SELECT ban_id FROM BanAn WHERE so_hieu=N'B4-01');

/* Order 1 (dine-in bàn B2-01, 2 khách) */
INSERT INTO [Order](kh_id, ban_id, la_mang_ve, so_khach, thoi_diem_dat)
VALUES (@kh1, @ban_B2_01, 0, 2, '2025-07-02T18:00:00');
DECLARE @order1 INT = SCOPE_IDENTITY();

INSERT INTO OrderItem(order_id, mon_id, so_luong, t_dat, t_hoan_thanh, t_phuc_vu)
VALUES
(@order1, @mon_kv1, 1, '2025-07-02T18:05:00', '2025-07-02T18:15:00', '2025-07-02T18:16:00'),
(@order1, @mon_mc1, 2, '2025-07-02T18:05:00', '2025-07-02T18:25:00', '2025-07-02T18:26:00');

INSERT INTO HoaDon(order_id, thoi_diem_tt, phuong_thuc)
VALUES (@order1, '2025-07-02T19:00:00', N'Tiền mặt');

/* Order 2 (mang về) */
INSERT INTO [Order](kh_id, ban_id, la_mang_ve, so_khach, thoi_diem_dat)
VALUES (@kh2, NULL, 1, NULL, '2025-07-05T11:30:00');
DECLARE @order2 INT = SCOPE_IDENTITY();

INSERT INTO OrderItem(order_id, mon_id, so_luong, t_dat)
VALUES
(@order2, @mon_kv2, 2, '2025-07-05T11:31:00'),
(@order2, @mon_tm1, 3, '2025-07-05T11:32:00');

INSERT INTO HoaDon(order_id, thoi_diem_tt, phuong_thuc)
VALUES (@order2, '2025-07-05T11:45:00', N'Chuyển khoản');

/* Order 3 (dine-in bàn B4-01, 4 khách) */
INSERT INTO [Order](kh_id, ban_id, la_mang_ve, so_khach, thoi_diem_dat)
VALUES (@kh3, @ban_B4_01, 0, 4, '2025-07-10T19:00:00');
DECLARE @order3 INT = SCOPE_IDENTITY();

INSERT INTO OrderItem(order_id, mon_id, so_luong, t_dat, t_hoan_thanh, t_phuc_vu)
VALUES
(@order3, @mon_mc2, 2, '2025-07-10T19:05:00', '2025-07-10T19:25:00', '2025-07-10T19:26:00'),
(@order3, @mon_kv1, 1, '2025-07-10T19:06:00', '2025-07-10T19:16:00', '2025-07-10T19:17:00');

INSERT INTO HoaDon(order_id, thoi_diem_tt, phuong_thuc)
VALUES (@order3, '2025-07-10T20:00:00', N'Thẻ tín dụng');
GO
