/* =========================================================
   HỆ CSDL QUẢN LÝ NHÀ HÀNG — SCHEMA CORE (SQL Server)
   ========================================================= */
CREATE DATABASE NhaHang
GO

USE NhaHang
GO

-- 1) Loại bàn & Bàn ăn
CREATE TABLE dbo.LoaiBan (
  loai_ban_id INT IDENTITY(1,1) PRIMARY KEY,
  suc_chua    INT NOT NULL CHECK (suc_chua > 0),
  so_luong    INT NOT NULL CHECK (so_luong >= 0)   -- số bàn hữu hạn theo loại
);
GO

CREATE TABLE dbo.BanAn (
  ban_id      INT IDENTITY(1,1) PRIMARY KEY,
  loai_ban_id INT NOT NULL FOREIGN KEY REFERENCES dbo.LoaiBan(loai_ban_id),
  so_hieu     NVARCHAR(10) NOT NULL UNIQUE
);
GO

-- 2) Khách hàng
CREATE TABLE dbo.KhachHang (
  kh_id         INT IDENTITY(1,1) PRIMARY KEY,
  ho_ten        NVARCHAR(120) NULL,
  so_dien_thoai NVARCHAR(20)  NULL
);
GO

-- 3) Món ăn/đồ uống & Thực đơn
CREATE TABLE dbo.Mon (
  mon_id   INT IDENTITY(1,1) PRIMARY KEY,
  ma_mon   NVARCHAR(20)  NOT NULL UNIQUE,
  ten_mon  NVARCHAR(120) NOT NULL,
  loai_mon NVARCHAR(12)  NOT NULL CHECK (loai_mon IN (N'KHAI_VI', N'MON_CHINH', N'TRANG_MIENG')),
  gia      DECIMAL(12,2) NOT NULL CHECK (gia >= 0),
  -- Mã món phải encode loại: KV- / MC- / TM-
  CONSTRAINT CK_Mon_Ma_Theo_Loai CHECK (
    (loai_mon = N'KHAI_VI'     AND ma_mon LIKE N'KV-%') OR
    (loai_mon = N'MON_CHINH'   AND ma_mon LIKE N'MC-%') OR
    (loai_mon = N'TRANG_MIENG' AND ma_mon LIKE N'TM-%')
  )
);
GO

CREATE TABLE dbo.ThucDon (
  thuc_don_id  INT IDENTITY(1,1) PRIMARY KEY,
  ten          NVARCHAR(120) NOT NULL,
  hieu_luc_tu  DATE NOT NULL,
  hieu_luc_den DATE NULL,
  CONSTRAINT CK_ThucDon_Date CHECK (hieu_luc_den IS NULL OR hieu_luc_den >= hieu_luc_tu)
);
GO

CREATE TABLE dbo.ThucDon_Mon (
  thuc_don_id INT NOT NULL,
  mon_id      INT NOT NULL,
  CONSTRAINT PK_ThucDon_Mon PRIMARY KEY (thuc_don_id, mon_id),
  CONSTRAINT FK_TDM_ThucDon FOREIGN KEY (thuc_don_id) REFERENCES dbo.ThucDon(thuc_don_id) ON DELETE CASCADE,
  CONSTRAINT FK_TDM_Mon     FOREIGN KEY (mon_id)      REFERENCES dbo.Mon(mon_id)           ON DELETE NO ACTION
);
GO

-- 4) Nhà cung cấp, Nguyên liệu, Nhập kho, Công thức món
CREATE TABLE dbo.NhaCungCap (
  ncc_id INT IDENTITY(1,1) PRIMARY KEY,
  ten    NVARCHAR(160) NOT NULL,
  dia_chi NVARCHAR(MAX) NULL,
  sdt     NVARCHAR(20)  NULL
);
GO

CREATE TABLE dbo.NguyenLieu (
  nl_id     INT IDENTITY(1,1) PRIMARY KEY,
  ten       NVARCHAR(160) NOT NULL,
  don_vi    NVARCHAR(20)  NOT NULL,  -- kg, g, l, ml, cái...
  nguon_goc NVARCHAR(MAX) NOT NULL   -- thông tin nguồn gốc rõ ràng
);
GO

-- Công thức: món dùng các nguyên liệu nào (định lượng cho 1 suất)
CREATE TABLE dbo.CongThuc (
  mon_id     INT NOT NULL,
  nl_id      INT NOT NULL,
  dinh_luong DECIMAL(14,3) NOT NULL CHECK (dinh_luong > 0),
  CONSTRAINT PK_CongThuc PRIMARY KEY (mon_id, nl_id),
  CONSTRAINT FK_CT_Mon FOREIGN KEY (mon_id) REFERENCES dbo.Mon(mon_id)               ON DELETE CASCADE,
  CONSTRAINT FK_CT_NL  FOREIGN KEY (nl_id)  REFERENCES dbo.NguyenLieu(nl_id)         ON DELETE NO ACTION
);
GO

-- 5) Nhân viên & loại nhân viên (ISA)
CREATE TABLE dbo.LoaiNhanVien (
  loai_nv      NVARCHAR(20) PRIMARY KEY,   -- 'DAU_BEP','PHUC_VU','DICH_VU','THU_NGAN'
  luong_co_ban DECIMAL(12,2) NOT NULL CHECK (luong_co_ban >= 0)
);
GO

CREATE TABLE dbo.NhanVien (
  nv_id        INT IDENTITY(1,1) PRIMARY KEY,
  ho_ten       NVARCHAR(120) NOT NULL,
  loai_nv      NVARCHAR(20)  NOT NULL FOREIGN KEY REFERENCES dbo.LoaiNhanVien(loai_nv),
  ngay_vao_lam DATE NULL,
  trang_thai   NVARCHAR(20) NOT NULL DEFAULT N'ACTIVE'
);
GO

-- 6) Đơn gọi (Order), các món trong đơn & Hoá đơn
-- Lưu ý: ORDER là từ khóa nên dùng [Order]
CREATE TABLE dbo.[Order] (
  order_id      INT IDENTITY(1,1) PRIMARY KEY,
  kh_id         INT NULL FOREIGN KEY REFERENCES dbo.KhachHang(kh_id),
  ban_id        INT NULL FOREIGN KEY REFERENCES dbo.BanAn(ban_id),     -- NULL nếu mang về
  la_mang_ve    BIT NOT NULL DEFAULT 0,
  trang_thai    NVARCHAR(20) NOT NULL DEFAULT N'pending',
  so_khach      INT NULL CHECK (so_khach IS NULL OR so_khach > 0),
  thoi_diem_dat DATETIME2(0) NOT NULL,
  -- Nếu mang về thì không có bàn; nếu ăn tại chỗ thì phải có bàn & so_khach
  CONSTRAINT CK_Order_DineIn_Takeaway CHECK (
      (la_mang_ve = 1 AND ban_id IS NULL)
   OR (la_mang_ve = 0 AND ban_id IS NOT NULL AND so_khach IS NOT NULL)
  )
);
GO

CREATE TABLE dbo.OrderItem (
  order_item_id INT IDENTITY(1,1) PRIMARY KEY,
  order_id      INT NOT NULL FOREIGN KEY REFERENCES dbo.[Order](order_id) ON DELETE CASCADE,
  mon_id        INT NOT NULL FOREIGN KEY REFERENCES dbo.Mon(mon_id),
  so_luong      INT NOT NULL CHECK (so_luong > 0),
  t_dat         DATETIME2(0) NOT NULL,   -- cần trigger để bảo đảm >= thoi_diem_dat của order
  t_hoan_thanh  DATETIME2(0) NULL,
  t_phuc_vu     DATETIME2(0) NULL,
  CONSTRAINT CK_OI_Time_Seq1 CHECK (t_hoan_thanh IS NULL OR t_hoan_thanh >= t_dat),
  CONSTRAINT CK_OI_Time_Seq2 CHECK (t_phuc_vu    IS NULL OR t_hoan_thanh IS NULL OR t_phuc_vu >= t_hoan_thanh)
);
GO

CREATE TABLE dbo.HoaDon (
  hd_id        INT IDENTITY(1,1) PRIMARY KEY,
  order_id     INT NOT NULL UNIQUE FOREIGN KEY REFERENCES dbo.[Order](order_id),
  thoi_diem_tt DATETIME2(0) NOT NULL,
  phuong_thuc  NVARCHAR(20) NULL
  -- NOTE: Ràng buộc "thanh toán >= max(t_phuc_vu)" phải dùng TRIGGER (CHECK không tham chiếu bảng khác).
);
GO

/* =========================================================
   (Tùy chọn) Chỉ mục gợi ý cho hiệu năng
   ========================================================= */
-- CREATE INDEX IX_Order_thoidiemdat   ON dbo.[Order](thoi_diem_dat);
-- CREATE INDEX IX_HoaDon_thoidiemtt   ON dbo.HoaDon(thoi_diem_tt);
-- CREATE INDEX IX_OrderItem_mon       ON dbo.OrderItem(mon_id);
-- CREATE INDEX IX_OrderItem_order     ON dbo.OrderItem(order_id);
-- CREATE INDEX IX_CongThuc_mon        ON dbo.CongThuc(mon_id);


/* ================================================
   TRG 1: Check sức chứa & chống overbook bàn (dine-in)
   ================================================ */
DROP TRIGGER IF EXISTS dbo.TR_Order_CheckCapacityAndOpen;
GO
CREATE TRIGGER dbo.TR_Order_CheckCapacityAndOpen
ON dbo.[Order]
AFTER INSERT, UPDATE
AS
BEGIN
  SET NOCOUNT ON;

  /* 1) Không vượt sức chứa: so_khach <= LoaiBan.suc_chua (chỉ áp dụng dine-in) */
  IF EXISTS (
    SELECT 1
    FROM inserted i
    JOIN dbo.BanAn b  ON b.ban_id = i.ban_id
    JOIN dbo.LoaiBan lb ON lb.loai_ban_id = b.loai_ban_id
    WHERE i.la_mang_ve = 0
      AND (i.so_khach IS NULL OR i.so_khach > lb.suc_chua)
  )
  BEGIN
    RAISERROR (N'Vượt sức chứa bàn: so_khach phải <= suc_chua của LoaiBan.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
  END

  /* 2) Chống overbook: mỗi bàn chỉ có 1 order "đang mở" (chưa có hóa đơn) tại một thời điểm
        => Không cho tồn tại order khác cùng bàn chưa thanh toán */
  IF EXISTS (
    SELECT 1
    FROM inserted i
    WHERE i.la_mang_ve = 0
      AND EXISTS (
        SELECT 1
        FROM dbo.[Order] o2
        LEFT JOIN dbo.HoaDon h2 ON h2.order_id = o2.order_id
        WHERE o2.ban_id = i.ban_id
          AND o2.order_id <> i.order_id
          AND h2.order_id IS NULL     -- order kia chưa thanh toán
      )
  )
  BEGIN
    RAISERROR (N'Overbook: Bàn đang có order khác chưa thanh toán.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
  END
END
GO


/* ================================================
   TRG 2: Check thời điểm món >= thời điểm đặt order
   ================================================ */
DROP TRIGGER IF EXISTS dbo.TR_OrderItem_CheckTimes;
GO
CREATE TRIGGER dbo.TR_OrderItem_CheckTimes
ON dbo.OrderItem
AFTER INSERT, UPDATE
AS
BEGIN
  SET NOCOUNT ON;

  /* t_dat >= Order.thoi_diem_dat */
  IF EXISTS (
    SELECT 1
    FROM inserted i
    JOIN dbo.[Order] o ON o.order_id = i.order_id
    WHERE i.t_dat < o.thoi_diem_dat
  )
  BEGIN
    RAISERROR (N'Thời điểm gọi món (t_dat) phải >= thời điểm đặt đơn (thoi_diem_dat).', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
  END

  /* Đã có CHECK để đảm bảo: t_hoan_thanh >= t_dat và t_phuc_vu >= t_hoan_thanh (nếu có).
     Không cần lặp lại tại trigger. */
END
GO


/* ================================================
   TRG 3: Check thời điểm thanh toán >= max(t_phuc_vu)
          (nếu chưa phục vụ món nào, dùng thoi_diem_dat)
   ================================================ */
DROP TRIGGER IF EXISTS dbo.TR_HoaDon_CheckAfterServe;
GO
CREATE TRIGGER dbo.TR_HoaDon_CheckAfterServe
ON dbo.HoaDon
AFTER INSERT, UPDATE
AS
BEGIN
  SET NOCOUNT ON;

  /* Bất kỳ hóa đơn nào vi phạm sẽ bị chặn */
  IF EXISTS (
    SELECT 1
    FROM inserted h
    JOIN dbo.[Order] o ON o.order_id = h.order_id
    OUTER APPLY (
      SELECT MAX(oi.t_phuc_vu) AS max_phuc_vu
      FROM dbo.OrderItem oi
      WHERE oi.order_id = h.order_id
    ) mx
    WHERE h.thoi_diem_tt < COALESCE(mx.max_phuc_vu, o.thoi_diem_dat)
  )
  BEGIN
    RAISERROR (N'Thời điểm thanh toán phải >= thời điểm phục vụ cuối cùng (hoặc >= thời điểm đặt đơn nếu chưa phục vụ).', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
  END
END
GO
