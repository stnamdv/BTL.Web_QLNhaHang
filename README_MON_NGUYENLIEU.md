# Hệ thống quản lý món ăn và nguyên liệu

## Tổng quan

Hệ thống quản lý món ăn và nguyên liệu được phát triển theo yêu cầu nghiệp vụ sau:

- **Món ăn/uống** được chia thành 3 loại chính: khai vị, món chính, tráng miệng
- **Mỗi món ăn** có thể được chế biến từ một hoặc nhiều nguyên liệu
- **Một nguyên liệu** có thể được dùng để chế biến nhiều món ăn
- **Mã món ăn** phải chứa thông tin về loại món ăn (KV-, MC-, TM-)
- **Thông tin nguyên liệu** cho mỗi món ăn được thể hiện trong CSDL qua bảng CôngThuc
- **Các nguyên liệu** đều cần có thông tin nguồn gốc rõ ràng

## Kiến trúc hệ thống

### 1. Database Layer

- **Stored Procedures**: Tất cả logic nghiệp vụ và validation được xử lý trong stored procedures
- **Validation**: Kiểm tra ràng buộc nghiệp vụ, format mã món, nguồn gốc nguyên liệu
- **Transaction**: Đảm bảo tính toàn vẹn dữ liệu

### 2. Repository Layer

- **IMonRepository / MonRepository**: Quản lý dữ liệu món ăn
- **INguyenLieuRepository / NguyenLieuRepository**: Quản lý dữ liệu nguyên liệu
- **Dapper**: Sử dụng Dapper để thực thi stored procedures

### 3. Service Layer

- **IMonService / MonService**: Logic nghiệp vụ món ăn
- **INguyenLieuService / NguyenLieuService**: Logic nghiệp vụ nguyên liệu
- **Validation**: Kiểm tra dữ liệu đầu vào, tạo mã món tự động

### 4. Controller Layer

- **MonController**: Xử lý HTTP requests cho món ăn
- **NguyenLieuController**: Xử lý HTTP requests cho nguyên liệu
- **AJAX**: Hỗ trợ tạo mã món tự động, validate dữ liệu

### 5. View Layer

- **Razor Views**: Giao diện người dùng responsive
- **Bootstrap**: Thiết kế hiện đại, thân thiện
- **JavaScript**: Tương tác động, validation client-side

## Các tính năng chính

### Quản lý Món ăn

#### 1. CRUD Operations

- **Create**: Tạo món ăn mới với công thức
- **Read**: Xem danh sách, chi tiết món ăn
- **Update**: Cập nhật thông tin món ăn và công thức
- **Delete**: Xóa món ăn (có kiểm tra ràng buộc)

#### 2. Validation nghiệp vụ

- **Mã món**: Phải bắt đầu theo loại (KV-, MC-, TM-)
- **Loại món**: Chỉ chấp nhận 3 loại quy định
- **Giá**: Không được âm
- **Công thức**: Định lượng phải > 0

#### 3. Tính năng đặc biệt

- **Tự động tạo mã món**: Theo format và số thứ tự
- **Quản lý công thức**: Thêm/sửa/xóa nguyên liệu trong món
- **Thống kê**: Phân tích theo loại món, giá cả
- **Tìm kiếm**: Theo tên, mã món, loại món

### Quản lý Nguyên liệu

#### 1. CRUD Operations

- **Create**: Tạo nguyên liệu mới
- **Read**: Xem danh sách, chi tiết nguyên liệu
- **Update**: Cập nhật thông tin nguyên liệu
- **Delete**: Xóa nguyên liệu (có kiểm tra ràng buộc)

#### 2. Validation nghiệp vụ

- **Tên nguyên liệu**: Không trùng lặp, tối đa 160 ký tự
- **Đơn vị**: Bắt buộc, tối đa 20 ký tự
- **Nguồn gốc**: Bắt buộc, ít nhất 10 ký tự, phải rõ ràng

#### 3. Tính năng đặc biệt

- **Thống kê sử dụng**: Số món sử dụng, lần nhập kho
- **Tìm kiếm nâng cao**: Theo tên, đơn vị, nguồn gốc
- **Validation nguồn gốc**: Kiểm tra tính hợp lệ

## Cấu trúc Database

### Bảng Mon

```sql
CREATE TABLE dbo.Mon (
  mon_id   INT IDENTITY(1,1) PRIMARY KEY,
  ma_mon   NVARCHAR(20)  NOT NULL UNIQUE,
  ten_mon  NVARCHAR(120) NOT NULL,
  loai_mon NVARCHAR(12)  NOT NULL CHECK (loai_mon IN (N'KHAI_VI', N'MON_CHINH', N'TRANG_MIENG')),
  gia      DECIMAL(12,2) NOT NULL CHECK (gia >= 0),
  CONSTRAINT CK_Mon_Ma_Theo_Loai CHECK (
    (loai_mon = N'KHAI_VI'     AND ma_mon LIKE N'KV-%') OR
    (loai_mon = N'MON_CHINH'   AND ma_mon LIKE N'MC-%') OR
    (loai_mon = N'TRANG_MIENG' AND ma_mon LIKE N'TM-%')
  )
);
```

### Bảng NguyenLieu

```sql
CREATE TABLE dbo.NguyenLieu (
  nl_id     INT IDENTITY(1,1) PRIMARY KEY,
  ten       NVARCHAR(160) NOT NULL,
  don_vi    NVARCHAR(20)  NOT NULL,
  nguon_goc NVARCHAR(MAX) NOT NULL
);
```

### Bảng CongThuc

```sql
CREATE TABLE dbo.CongThuc (
  mon_id     INT NOT NULL,
  nl_id      INT NOT NULL,
  dinh_luong DECIMAL(14,3) NOT NULL CHECK (dinh_luong > 0),
  CONSTRAINT PK_CongThuc PRIMARY KEY (mon_id, nl_id),
  CONSTRAINT FK_CT_Mon FOREIGN KEY (mon_id) REFERENCES dbo.Mon(mon_id) ON DELETE CASCADE,
  CONSTRAINT FK_CT_NL  FOREIGN KEY (nl_id)  REFERENCES dbo.NguyenLieu(nl_id) ON DELETE NO ACTION
);
```

## Stored Procedures

### Món ăn

- **SP_GetAllMon**: Lấy danh sách món ăn có phân trang và tìm kiếm
- **SP_GetMonById**: Lấy thông tin món ăn theo ID
- **SP_CreateMon**: Tạo món ăn mới với validation
- **SP_UpdateMon**: Cập nhật món ăn với validation
- **SP_DeleteMon**: Xóa món ăn với kiểm tra ràng buộc
- **SP_GetMonWithCongThuc**: Lấy món ăn kèm công thức chi tiết

### Nguyên liệu

- **SP_GetAllNguyenLieu**: Lấy danh sách nguyên liệu có phân trang
- **SP_GetNguyenLieuById**: Lấy thông tin nguyên liệu theo ID
- **SP_CreateNguyenLieu**: Tạo nguyên liệu mới với validation
- **SP_UpdateNguyenLieu**: Cập nhật nguyên liệu với validation
- **SP_DeleteNguyenLieu**: Xóa nguyên liệu với kiểm tra ràng buộc
- **SP_GetNguyenLieuStats**: Thống kê sử dụng nguyên liệu
- **SP_SearchNguyenLieu**: Tìm kiếm nguyên liệu theo nhiều tiêu chí
- **SP_ValidateNguonGoc**: Kiểm tra tính hợp lệ của nguồn gốc

## Cách sử dụng

### 1. Thiết lập Database

```sql
-- Chạy script.sql để tạo database và bảng
-- Chạy StoredProcedures_Mon.sql để tạo stored procedures cho món ăn
-- Chạy StoredProcedures_NguyenLieu.sql để tạo stored procedures cho nguyên liệu
```

### 2. Cấu hình Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=NhaHang;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### 3. Chạy ứng dụng

```bash
dotnet run
```

### 4. Truy cập các chức năng

- **Món ăn**: `/Mon`
- **Nguyên liệu**: `/NguyenLieu`

## Validation Rules

### Món ăn

1. **Mã món**:

   - Không được trùng lặp
   - Phải bắt đầu theo loại: KV- (khai vị), MC- (món chính), TM- (tráng miệng)
   - Tối đa 20 ký tự

2. **Tên món**:

   - Bắt buộc, tối đa 120 ký tự

3. **Loại món**:

   - Chỉ chấp nhận: KHAI_VI, MON_CHINH, TRANG_MIENG

4. **Giá**:

   - Không được âm

5. **Công thức**:
   - Định lượng phải > 0
   - Nguyên liệu phải tồn tại

### Nguyên liệu

1. **Tên nguyên liệu**:

   - Không được trùng lặp
   - Bắt buộc, tối đa 160 ký tự

2. **Đơn vị**:

   - Bắt buộc, tối đa 20 ký tự

3. **Nguồn gốc**:
   - Bắt buộc, ít nhất 10 ký tự
   - Phải chứa ít nhất một từ có ý nghĩa
   - Không được chứa ký tự đặc biệt: < > { }

## Tính năng nâng cao

### 1. Tự động tạo mã món

- Hệ thống tự động tạo mã món theo format: [PREFIX]-[NUMBER]
- Prefix: KV (khai vị), MC (món chính), TM (tráng miệng)
- Number: 3 chữ số, tự động tăng

### 2. Quản lý công thức động

- Thêm/sửa/xóa nguyên liệu trong món ăn
- Hiển thị đơn vị tự động khi chọn nguyên liệu
- Validation định lượng real-time

### 3. Thống kê và báo cáo

- Thống kê món ăn theo loại
- Thống kê sử dụng nguyên liệu
- Biểu đồ trực quan với Chart.js

### 4. Tìm kiếm nâng cao

- Tìm kiếm món ăn theo nhiều tiêu chí
- Tìm kiếm nguyên liệu theo nguồn gốc
- Phân trang hiệu quả

## Bảo mật và Performance

### 1. Bảo mật

- **SQL Injection**: Sử dụng stored procedures và parameterized queries
- **XSS**: Validation và encoding dữ liệu đầu vào
- **CSRF**: Anti-forgery tokens

### 2. Performance

- **Pagination**: Phân trang cho danh sách lớn
- **Indexing**: Chỉ mục database cho tìm kiếm
- **Caching**: Có thể mở rộng với Redis

## Mở rộng trong tương lai

1. **Quản lý nhà cung cấp**: Tích hợp với bảng NhaCungCap
2. **Quản lý nhập kho**: Tích hợp với bảng NL_NCC
3. **Báo cáo chi phí**: Tính toán giá thành món ăn
4. **API**: RESTful API cho mobile app
5. **Real-time**: SignalR cho cập nhật real-time

## Kết luận

Hệ thống quản lý món ăn và nguyên liệu đã được phát triển hoàn chỉnh với:

- ✅ **Đầy đủ chức năng CRUD** cho món ăn và nguyên liệu
- ✅ **Validation nghiệp vụ** toàn diện trong stored procedures
- ✅ **Giao diện hiện đại** và thân thiện người dùng
- ✅ **Kiến trúc clean** với separation of concerns
- ✅ **Performance tốt** với pagination và indexing
- ✅ **Bảo mật** với parameterized queries và validation

Hệ thống sẵn sàng để triển khai và sử dụng trong môi trường production.
