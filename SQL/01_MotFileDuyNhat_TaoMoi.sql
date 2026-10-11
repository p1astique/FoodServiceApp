/* ============================================================
   CƠ SỞ DỮ LIỆU: QuanLyDichVuAnUong (FILE DUY NHẤT - TRỌN BỘ)
   Đề tài: CNTT-KLCN069 - Xây dựng ứng dụng quản lý dịch vụ ăn uống
   Hệ quản trị: SQL Server

   File này gộp toàn bộ, chạy 1 lần là xong, không cần file phụ nào khác:
     PHẦN 1: Tạo DB + 16 bảng (KhachHang/GianHang/QuanTri + Shipper)
     PHẦN 2-5: Dữ liệu mẫu nhỏ (8 gian hàng, 5 khách hàng, 4 tài xế,
               2 quản trị viên, khuyến mãi, đơn hàng mẫu)
     PHẦN 6: Đặt mật khẩu demo = 123456 (quản trị = admin123)
     PHẦN 7: Sinh thêm dữ liệu lớn — 100 khách hàng, 30 quán ăn
             (kèm thực đơn), 40 tài xế — mật khẩu cũng = 123456
   ============================================================ */

IF DB_ID('QuanLyDichVuAnUong') IS NOT NULL
    DROP DATABASE QuanLyDichVuAnUong;
GO

CREATE DATABASE QuanLyDichVuAnUong;
GO

USE QuanLyDichVuAnUong;
GO

/* ============================================================
   PHẦN 1: TẠO BẢNG
   ============================================================ */

CREATE TABLE TaiKhoan (
    MaTK            INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap     NVARCHAR(50)  NOT NULL UNIQUE,
    MatKhau         NVARCHAR(255) NOT NULL,
    VaiTro          NVARCHAR(20)  NOT NULL
        CHECK (VaiTro IN (N'KhachHang', N'GianHang', N'QuanTri', N'TaiXe')),
    NgayTao         DATETIME      NOT NULL DEFAULT GETDATE(),
    TrangThai       BIT           NOT NULL DEFAULT 1
);
GO

CREATE TABLE KhachHang (
    MaKH            INT IDENTITY(1,1) PRIMARY KEY,
    MaTK            INT NOT NULL UNIQUE REFERENCES TaiKhoan(MaTK),
    HoTen           NVARCHAR(100) NOT NULL,
    SDT             VARCHAR(15)   NOT NULL,
    Email           VARCHAR(100)  NULL,
    DiaChiGiaoHang  NVARCHAR(255) NULL
);
GO

CREATE TABLE GianHang (
    MaGianHang        INT IDENTITY(1,1) PRIMARY KEY,
    MaTK              INT NOT NULL UNIQUE REFERENCES TaiKhoan(MaTK),
    TenCuaHang        NVARCHAR(150) NOT NULL,
    DiaChi            NVARCHAR(255) NOT NULL,
    SDT               VARCHAR(15)   NOT NULL,
    MoTa              NVARCHAR(500) NULL,
    HinhAnh           NVARCHAR(500) NULL,
    TrangThaiKinhDoanh NVARCHAR(20) NOT NULL DEFAULT N'DangMo'
        CHECK (TrangThaiKinhDoanh IN (N'DangMo', N'DongCua', N'TamNgung')),
    TrangThaiDuyet    NVARCHAR(20) NOT NULL DEFAULT N'ChoDuyet'
        CHECK (TrangThaiDuyet IN (N'ChoDuyet', N'DaDuyet', N'TuChoi')),
    NgayDangKy        DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE QuanTri (
    MaQuanTri       INT IDENTITY(1,1) PRIMARY KEY,
    MaTK            INT NOT NULL UNIQUE REFERENCES TaiKhoan(MaTK),
    HoTen           NVARCHAR(100) NOT NULL,
    SDT             VARCHAR(15)   NOT NULL,
    Email           VARCHAR(100)  NULL,
    CapQuyenHan     NVARCHAR(30)  NOT NULL DEFAULT N'NhanVien'
        CHECK (CapQuyenHan IN (N'SuperAdmin', N'QuanLy', N'NhanVien')),
    PhongBanPhuTrach NVARCHAR(100) NULL
);
GO

CREATE TABLE TaiXe (
    MaTaiXe         INT IDENTITY(1,1) PRIMARY KEY,
    MaTK            INT NOT NULL UNIQUE REFERENCES TaiKhoan(MaTK),
    HoTen           NVARCHAR(100) NOT NULL,
    SDT             VARCHAR(15)   NOT NULL,
    PhuongTien      NVARCHAR(50)  NULL,
    BienSo          VARCHAR(20)   NULL,
    KhuVucHoatDong  NVARCHAR(150) NULL,
    TrangThai       NVARCHAR(20)  NOT NULL DEFAULT N'SanSang'
        CHECK (TrangThai IN (N'SanSang', N'DangGiao', N'NgoaiTuyen')),
    TrangThaiDuyet  NVARCHAR(20)  NOT NULL DEFAULT N'DaDuyet'
        CHECK (TrangThaiDuyet IN (N'ChoDuyet', N'DaDuyet', N'TuChoi')),
    NgayDangKy      DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE DanhMuc (
    MaDanhMuc       INT IDENTITY(1,1) PRIMARY KEY,
    MaGianHang      INT NOT NULL REFERENCES GianHang(MaGianHang),
    TenDanhMuc      NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE MonAn (
    MaMonAn         INT IDENTITY(1,1) PRIMARY KEY,
    MaDanhMuc       INT NOT NULL REFERENCES DanhMuc(MaDanhMuc),
    TenMon          NVARCHAR(150) NOT NULL,
    Gia             DECIMAL(12,0) NOT NULL CHECK (Gia >= 0),
    MoTa            NVARCHAR(500) NULL,
    HinhAnh         NVARCHAR(255) NULL,
    TinhTrang       NVARCHAR(20) NOT NULL DEFAULT N'ConHang'
        CHECK (TinhTrang IN (N'ConHang', N'HetHang'))
);
GO

CREATE TABLE DonHang (
    MaDonHang       INT IDENTITY(1,1) PRIMARY KEY,
    MaKH            INT NOT NULL REFERENCES KhachHang(MaKH),
    MaGianHang      INT NOT NULL REFERENCES GianHang(MaGianHang),
    MaTaiXe         INT NULL REFERENCES TaiXe(MaTaiXe),
    NgayDat         DATETIME NOT NULL DEFAULT GETDATE(),
    TrangThai       NVARCHAR(30) NOT NULL DEFAULT N'ChoXacNhan'
        CHECK (TrangThai IN (N'ChoXacNhan', N'DaXacNhan', N'DangGiao', N'DaGiao', N'DaHuy')),
    TongTien        DECIMAL(12,0) NOT NULL DEFAULT 0,
    PhiGiaoHang     DECIMAL(12,0) NOT NULL DEFAULT 0, -- đã gồm trong TongTien; là thu nhập tài xế
    DiaChiGiaoHang  NVARCHAR(255) NOT NULL,
    GhiChu          NVARCHAR(255) NULL,
    MaKhuyenMaiApDung INT NULL -- FK thêm bằng ALTER TABLE bên dưới, sau khi có bảng KhuyenMai
);
GO

CREATE TABLE ChiTietDonHang (
    MaCTDH          INT IDENTITY(1,1) PRIMARY KEY,
    MaDonHang       INT NOT NULL REFERENCES DonHang(MaDonHang),
    MaMonAn         INT NOT NULL REFERENCES MonAn(MaMonAn),
    SoLuong         INT NOT NULL CHECK (SoLuong > 0),
    DonGia          DECIMAL(12,0) NOT NULL CHECK (DonGia >= 0)
);
GO

CREATE TABLE ThanhToan (
    MaThanhToan     INT IDENTITY(1,1) PRIMARY KEY,
    MaDonHang       INT NOT NULL UNIQUE REFERENCES DonHang(MaDonHang),
    HinhThuc        NVARCHAR(30) NOT NULL
        CHECK (HinhThuc IN (N'TienMat', N'VNPay', N'Momo')),
    TrangThai       NVARCHAR(20) NOT NULL DEFAULT N'ChoThanhToan'
        CHECK (TrangThai IN (N'ChoThanhToan', N'DaThanhToan', N'ThatBai', N'DaHoanTien')),
    MaGiaoDich      VARCHAR(100) NULL,
    NgayThanhToan   DATETIME NULL
);
GO

CREATE TABLE GiaoHang (
    MaGiaoHang      INT IDENTITY(1,1) PRIMARY KEY,
    MaDonHang       INT NOT NULL UNIQUE REFERENCES DonHang(MaDonHang),
    MaTaiXe         INT NOT NULL REFERENCES TaiXe(MaTaiXe),
    TrangThai       NVARCHAR(20) NOT NULL DEFAULT N'DaNhan'
        CHECK (TrangThai IN (N'DaNhan', N'DangGiao', N'GiaoThanhCong', N'GiaoThatBai')),
    ThoiGianNhan    DATETIME NULL,
    ThoiGianGiao    DATETIME NULL
    -- Không có cột PhiGiaoHang ở đây: phí giao hàng (= thu nhập tài xế) được
    -- tính và lưu thẳng vào DonHang.PhiGiaoHang ngay lúc khách đặt hàng
    -- (CustomerCartController), không phải lúc tài xế nhận đơn.
);
GO

CREATE TABLE KhuyenMai (
    MaKhuyenMai     INT IDENTITY(1,1) PRIMARY KEY,
    MaGianHang      INT NULL REFERENCES GianHang(MaGianHang),
    MaMonAn         INT NULL REFERENCES MonAn(MaMonAn),   -- NULL = giảm cả đơn; có giá trị = chỉ giảm trên món này
    MaCode          VARCHAR(30) NOT NULL UNIQUE,
    MoTa            NVARCHAR(255) NULL,
    PhanTramGiam    DECIMAL(5,2) NULL CHECK (PhanTramGiam BETWEEN 0 AND 100),
    SoTienGiam      DECIMAL(12,0) NULL,
    DieuKienApDung  NVARCHAR(255) NULL,
    NgayBatDau      DATETIME NOT NULL,
    NgayKetThuc     DATETIME NOT NULL,
    TrangThai       BIT NOT NULL DEFAULT 1
);
GO

ALTER TABLE DonHang ADD CONSTRAINT FK_DonHang_KhuyenMai
    FOREIGN KEY (MaKhuyenMaiApDung) REFERENCES KhuyenMai(MaKhuyenMai);
GO

CREATE TABLE DanhGia (
    MaDanhGia       INT IDENTITY(1,1) PRIMARY KEY,
    MaKH            INT NOT NULL REFERENCES KhachHang(MaKH),
    MaGianHang      INT NOT NULL REFERENCES GianHang(MaGianHang),
    MaDonHang       INT NOT NULL REFERENCES DonHang(MaDonHang),
    SoSao           INT NOT NULL CHECK (SoSao BETWEEN 1 AND 5),
    NoiDung         NVARCHAR(500) NULL,
    NgayDanhGia     DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE DanhGiaMon (
    MaDanhGiaMon    INT IDENTITY(1,1) PRIMARY KEY,
    MaKH            INT NOT NULL REFERENCES KhachHang(MaKH),
    MaMonAn         INT NOT NULL REFERENCES MonAn(MaMonAn),
    MaDonHang       INT NOT NULL REFERENCES DonHang(MaDonHang),
    SoSao           INT NOT NULL CHECK (SoSao BETWEEN 1 AND 5),
    NoiDung         NVARCHAR(500) NULL,
    NgayDanhGia     DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_DanhGiaMon UNIQUE (MaDonHang, MaMonAn)
);
GO

CREATE TABLE DiaChiKhachHang (
    MaDiaChi        INT IDENTITY(1,1) PRIMARY KEY,
    MaKH            INT NOT NULL REFERENCES KhachHang(MaKH),
    Nhan            NVARCHAR(50)  NOT NULL,
    DiaChi          NVARCHAR(255) NOT NULL,
    MacDinh         BIT NOT NULL DEFAULT 0
);
GO

CREATE TABLE PhanHoi (
    MaPhanHoi       INT IDENTITY(1,1) PRIMARY KEY,
    MaKH            INT NOT NULL REFERENCES KhachHang(MaKH),
    MaDonHang       INT NULL REFERENCES DonHang(MaDonHang),
    MaGianHang      INT NULL REFERENCES GianHang(MaGianHang),
    Loai            NVARCHAR(20) NOT NULL DEFAULT N'GopY'
        CHECK (Loai IN (N'GopY', N'KhieuNai', N'Khac')),
    TieuDe          NVARCHAR(150) NOT NULL,
    NoiDung         NVARCHAR(1000) NOT NULL,
    TrangThai       NVARCHAR(20) NOT NULL DEFAULT N'ChuaXuLy'
        CHECK (TrangThai IN (N'ChuaXuLy', N'DaXuLy')),
    TraLoi          NVARCHAR(1000) NULL,
    NgayGui         DATETIME NOT NULL DEFAULT GETDATE(),
    NgayXuLy        DATETIME NULL
);
GO

/* ---- Tài xế từ chối đơn giao (ẩn đơn khỏi danh sách của riêng tài xế đó) ---- */
IF OBJECT_ID('TaiXeTuChoiDon', 'U') IS NULL
CREATE TABLE TaiXeTuChoiDon (
    MaTuChoi    INT IDENTITY(1,1) PRIMARY KEY,
    MaTaiXe     INT NOT NULL REFERENCES TaiXe(MaTaiXe),
    MaDonHang   INT NOT NULL REFERENCES DonHang(MaDonHang),
    NgayTuChoi  DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_TaiXeTuChoiDon UNIQUE (MaTaiXe, MaDonHang)
);
GO


/* ============================================================
   PHẦN 2: NGUỒN DỮ LIỆU LỚN - TÀI KHOẢN
   5 khách hàng, 8 gian hàng, 2 quản trị viên, 4 tài xế
   ============================================================ */

INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro) VALUES
(N'khachhang01', N'$2a$hash', N'KhachHang'),
(N'khachhang02', N'$2a$hash', N'KhachHang'),
(N'khachhang03', N'$2a$hash', N'KhachHang'),
(N'khachhang04', N'$2a$hash', N'KhachHang'),
(N'khachhang05', N'$2a$hash', N'KhachHang'),
(N'gianhang01', N'$2a$hash', N'GianHang'),
(N'gianhang02', N'$2a$hash', N'GianHang'),
(N'gianhang03', N'$2a$hash', N'GianHang'),
(N'gianhang04', N'$2a$hash', N'GianHang'),
(N'gianhang05', N'$2a$hash', N'GianHang'),
(N'gianhang06', N'$2a$hash', N'GianHang'),
(N'gianhang07', N'$2a$hash', N'GianHang'),
(N'gianhang08', N'$2a$hash', N'GianHang'),
(N'quantri01', N'$2a$hash', N'QuanTri'),
(N'quantri02', N'$2a$hash', N'QuanTri'),
(N'taixe01', N'$2a$hash', N'TaiXe'),
(N'taixe02', N'$2a$hash', N'TaiXe'),
(N'taixe03', N'$2a$hash', N'TaiXe'),
(N'taixe04', N'$2a$hash', N'TaiXe');
GO

INSERT INTO QuanTri (MaTK, HoTen, SDT, Email, CapQuyenHan, PhongBanPhuTrach)
SELECT MaTK, HoTen, SDT, Email, CapQuyen, PhongBan FROM (VALUES
    (N'quantri01', N'Nguyễn Thị Quản Lý', '0920000001', 'quanly@huit.edu.vn', N'SuperAdmin', N'Điều hành hệ thống'),
    (N'quantri02', N'Trần Văn Giám Sát', '0921000002', 'giamsat@huit.edu.vn', N'NhanVien', N'Giám sát đối tác & khuyến mãi')
) AS d(TenDN, HoTen, SDT, Email, CapQuyen, PhongBan)
JOIN TaiKhoan t ON t.TenDangNhap = d.TenDN;
GO

INSERT INTO KhachHang (MaTK, HoTen, SDT, Email, DiaChiGiaoHang)
SELECT MaTK, HoTen, SDT, Email, DiaChi FROM (VALUES
    (N'khachhang01', N'Nguyễn Văn A', '0901000001', 'a@gmail.com', N'123 Lê Lợi, Q.1, TP.HCM'),
    (N'khachhang02', N'Phạm Thị D', '0907000007', 'd@gmail.com', N'56 Lý Thường Kiệt, Q.10, TP.HCM'),
    (N'khachhang03', N'Trần Văn E', '0908000008', 'e@gmail.com', N'89 Nguyễn Thị Minh Khai, Q.3, TP.HCM'),
    (N'khachhang04', N'Lê Thị F', '0909000009', 'f@gmail.com', N'12 Điện Biên Phủ, Bình Thạnh, TP.HCM'),
    (N'khachhang05', N'Hoàng Văn G', '0910000010', 'g@gmail.com', N'34 Cộng Hòa, Tân Bình, TP.HCM')
) AS d(TenDN, HoTen, SDT, Email, DiaChi)
JOIN TaiKhoan t ON t.TenDangNhap = d.TenDN;
GO

/* ---------- 8 gian hàng thuộc nhiều nhóm ẩm thực khác nhau ---------- */
INSERT INTO GianHang (MaTK, TenCuaHang, DiaChi, SDT, MoTa, TrangThaiKinhDoanh, TrangThaiDuyet)
SELECT MaTK, TenCuaHang, DiaChi, SDT, MoTa, N'DangMo', N'DaDuyet' FROM (VALUES
    (N'gianhang01', N'Quán Cơm Tấm Cô Ba', N'45 Nguyễn Trãi, Q.5, TP.HCM', '0902000002', N'Cơm tấm sườn bì chả truyền thống'),
    (N'gianhang02', N'Bún Bò Huế Mệ Ba', N'12 Trần Hưng Đạo, Q.1, TP.HCM', '0904000004', N'Bún bò chuẩn vị Huế, mở cửa từ 6h sáng'),
    (N'gianhang03', N'Trà Sữa Mộc Trà', N'88 Cách Mạng Tháng 8, Q.3, TP.HCM', '0905000005', N'Trà sữa, trà trái cây tươi mỗi ngày'),
    (N'gianhang04', N'Phở Hà Nội Xưa', N'20 Hai Bà Trưng, Q.1, TP.HCM', '0911000011', N'Phở bò/gà theo công thức Hà Nội gốc'),
    (N'gianhang05', N'Bánh Mì Bà Huynh', N'5 Nguyễn Đình Chiểu, Q.3, TP.HCM', '0912000012', N'Bánh mì thịt nướng, pate nhà làm'),
    (N'gianhang06', N'Gà Rán Cô Út', N'150 Ba Tháng Hai, Q.10, TP.HCM', '0913000013', N'Gà rán giòn, combo học sinh sinh viên'),
    (N'gianhang07', N'Hủ Tiếu Nam Vang Sáu Sài Gòn', N'77 Trần Phú, Q.5, TP.HCM', '0914000014', N'Hủ tiếu Nam Vang, hủ tiếu khô'),
    (N'gianhang08', N'Chè Ba Miền', N'22 Sư Vạn Hạnh, Q.10, TP.HCM', '0915000015', N'Chè Bắc - Trung - Nam đủ loại')
) AS d(TenDN, TenCuaHang, DiaChi, SDT, MoTa)
JOIN TaiKhoan t ON t.TenDangNhap = d.TenDN;
GO

INSERT INTO TaiXe (MaTK, HoTen, SDT, PhuongTien, KhuVucHoatDong)
SELECT MaTK, HoTen, SDT, PhuongTien, KhuVuc FROM (VALUES
    (N'taixe01', N'Trần Văn B', '0903000003', N'Xe máy', N'Quận 1, Quận 5'),
    (N'taixe02', N'Lê Thị C', '0906000006', N'Xe máy', N'Quận 3, Quận 10'),
    (N'taixe03', N'Đỗ Văn H', '0916000016', N'Xe máy', N'Bình Thạnh, Quận 1'),
    (N'taixe04', N'Ngô Thị K', '0917000017', N'Xe đạp điện', N'Quận 5, Quận 10')
) AS d(TenDN, HoTen, SDT, PhuongTien, KhuVuc)
JOIN TaiKhoan t ON t.TenDangNhap = d.TenDN;
GO

/* ============================================================
   PHẦN 3: DANH MỤC + MÓN ĂN CHO TỪNG GIAN HÀNG (~45 món)
   ============================================================ */

DECLARE @gh1 INT = (SELECT MaGianHang FROM GianHang WHERE TenCuaHang = N'Quán Cơm Tấm Cô Ba');
DECLARE @gh2 INT = (SELECT MaGianHang FROM GianHang WHERE TenCuaHang = N'Bún Bò Huế Mệ Ba');
DECLARE @gh3 INT = (SELECT MaGianHang FROM GianHang WHERE TenCuaHang = N'Trà Sữa Mộc Trà');
DECLARE @gh4 INT = (SELECT MaGianHang FROM GianHang WHERE TenCuaHang = N'Phở Hà Nội Xưa');
DECLARE @gh5 INT = (SELECT MaGianHang FROM GianHang WHERE TenCuaHang = N'Bánh Mì Bà Huynh');
DECLARE @gh6 INT = (SELECT MaGianHang FROM GianHang WHERE TenCuaHang = N'Gà Rán Cô Út');
DECLARE @gh7 INT = (SELECT MaGianHang FROM GianHang WHERE TenCuaHang = N'Hủ Tiếu Nam Vang Sáu Sài Gòn');
DECLARE @gh8 INT = (SELECT MaGianHang FROM GianHang WHERE TenCuaHang = N'Chè Ba Miền');

-- Cơm Tấm Cô Ba
INSERT INTO DanhMuc (MaGianHang, TenDanhMuc) VALUES (@gh1, N'Cơm phần'), (@gh1, N'Món thêm'), (@gh1, N'Nước uống');
DECLARE @gh1_com INT = (SELECT TOP 1 MaDanhMuc FROM DanhMuc WHERE MaGianHang=@gh1 AND TenDanhMuc=N'Cơm phần');
DECLARE @gh1_them INT = (SELECT TOP 1 MaDanhMuc FROM DanhMuc WHERE MaGianHang=@gh1 AND TenDanhMuc=N'Món thêm');
DECLARE @gh1_nuoc INT = (SELECT TOP 1 MaDanhMuc FROM DanhMuc WHERE MaGianHang=@gh1 AND TenDanhMuc=N'Nước uống');
INSERT INTO MonAn (MaDanhMuc, TenMon, Gia, MoTa) VALUES
(@gh1_com, N'Cơm sườn bì chả', 45000, N'Sườn nướng, bì, chả trứng'),
(@gh1_com, N'Cơm sườn trứng', 40000, NULL),
(@gh1_com, N'Cơm sườn ốp la', 42000, NULL),
(@gh1_com, N'Cơm gà nướng', 48000, NULL),
(@gh1_them, N'Trứng ốp la', 8000, N'Trứng chiên lòng đào'),
(@gh1_them, N'Chả trứng hấp', 15000, NULL),
(@gh1_nuoc, N'Trà đá', 5000, NULL),
(@gh1_nuoc, N'Nước ngọt lon', 12000, NULL);

-- Bún Bò Huế Mệ Ba
INSERT INTO DanhMuc (MaGianHang, TenDanhMuc) VALUES (@gh2, N'Món chính'), (@gh2, N'Nước uống');
DECLARE @gh2_chinh INT = (SELECT TOP 1 MaDanhMuc FROM DanhMuc WHERE MaGianHang=@gh2 AND TenDanhMuc=N'Món chính');
DECLARE @gh2_nuoc INT = (SELECT TOP 1 MaDanhMuc FROM DanhMuc WHERE MaGianHang=@gh2 AND TenDanhMuc=N'Nước uống');
INSERT INTO MonAn (MaDanhMuc, TenMon, Gia, MoTa) VALUES
(@gh2_chinh, N'Bún bò giò heo', 55000, N'Tô đầy đủ giò, chả, huyết'),
(@gh2_chinh, N'Bún bò đặc biệt', 65000, N'Thêm giò heo và chả cua'),
(@gh2_chinh, N'Bún chả cua', 50000, NULL),
(@gh2_chinh, N'Bún giò heo', 48000, NULL),
(@gh2_nuoc, N'Trà đá', 5000, NULL),
(@gh2_nuoc, N'Nước sâm', 10000, NULL);

-- Trà Sữa Mộc Trà
INSERT INTO DanhMuc (MaGianHang, TenDanhMuc) VALUES (@gh3, N'Trà sữa'), (@gh3, N'Trà trái cây');
DECLARE @gh3_ts INT = (SELECT TOP 1 MaDanhMuc FROM DanhMuc WHERE MaGianHang=@gh3 AND TenDanhMuc=N'Trà sữa');
DECLARE @gh3_tc INT = (SELECT TOP 1 MaDanhMuc FROM DanhMuc WHERE MaGianHang=@gh3 AND TenDanhMuc=N'Trà trái cây');
INSERT INTO MonAn (MaDanhMuc, TenMon, Gia, MoTa) VALUES
(@gh3_ts, N'Trà sữa truyền thống', 35000, NULL),
(@gh3_ts, N'Trà sữa trân châu đường đen', 42000, NULL),
(@gh3_ts, N'Trà sữa matcha', 40000, NULL),
(@gh3_tc, N'Trà đào cam sả', 39000, NULL),
(@gh3_tc, N'Trà vải', 39000, NULL);

-- Phở Hà Nội Xưa
INSERT INTO DanhMuc (MaGianHang, TenDanhMuc) VALUES (@gh4, N'Phở'), (@gh4, N'Nước uống');
DECLARE @gh4_pho INT = (SELECT TOP 1 MaDanhMuc FROM DanhMuc WHERE MaGianHang=@gh4 AND TenDanhMuc=N'Phở');
DECLARE @gh4_nuoc INT = (SELECT TOP 1 MaDanhMuc FROM DanhMuc WHERE MaGianHang=@gh4 AND TenDanhMuc=N'Nước uống');
INSERT INTO MonAn (MaDanhMuc, TenMon, Gia, MoTa) VALUES
(@gh4_pho, N'Phở bò tái', 50000, NULL),
(@gh4_pho, N'Phở bò chín', 50000, NULL),
(@gh4_pho, N'Phở gà', 45000, NULL),
(@gh4_pho, N'Phở đặc biệt', 65000, N'Tái, nạm, gân, bò viên'),
(@gh4_nuoc, N'Trà đá', 5000, NULL),
(@gh4_nuoc, N'Nước ngọt lon', 12000, NULL);

-- Bánh Mì Bà Huynh
INSERT INTO DanhMuc (MaGianHang, TenDanhMuc) VALUES (@gh5, N'Bánh mì'), (@gh5, N'Nước uống');
DECLARE @gh5_bm INT = (SELECT TOP 1 MaDanhMuc FROM DanhMuc WHERE MaGianHang=@gh5 AND TenDanhMuc=N'Bánh mì');
DECLARE @gh5_nuoc INT = (SELECT TOP 1 MaDanhMuc FROM DanhMuc WHERE MaGianHang=@gh5 AND TenDanhMuc=N'Nước uống');
INSERT INTO MonAn (MaDanhMuc, TenMon, Gia, MoTa) VALUES
(@gh5_bm, N'Bánh mì thịt nướng', 25000, NULL),
(@gh5_bm, N'Bánh mì pate chả', 20000, NULL),
(@gh5_bm, N'Bánh mì trứng ốp la', 18000, NULL),
(@gh5_bm, N'Bánh mì xíu mại', 22000, NULL),
(@gh5_nuoc, N'Sữa đậu nành', 10000, NULL);

-- Gà Rán Cô Út
INSERT INTO DanhMuc (MaGianHang, TenDanhMuc) VALUES (@gh6, N'Gà rán'), (@gh6, N'Món ăn kèm'), (@gh6, N'Nước ngọt');
DECLARE @gh6_ga INT = (SELECT TOP 1 MaDanhMuc FROM DanhMuc WHERE MaGianHang=@gh6 AND TenDanhMuc=N'Gà rán');
DECLARE @gh6_kem INT = (SELECT TOP 1 MaDanhMuc FROM DanhMuc WHERE MaGianHang=@gh6 AND TenDanhMuc=N'Món ăn kèm');
DECLARE @gh6_nuoc INT = (SELECT TOP 1 MaDanhMuc FROM DanhMuc WHERE MaGianHang=@gh6 AND TenDanhMuc=N'Nước ngọt');
INSERT INTO MonAn (MaDanhMuc, TenMon, Gia, MoTa) VALUES
(@gh6_ga, N'Gà rán miếng đùi', 35000, NULL),
(@gh6_ga, N'Combo 2 miếng gà + cơm', 55000, NULL),
(@gh6_kem, N'Khoai tây chiên', 20000, NULL),
(@gh6_kem, N'Salad trộn', 15000, NULL),
(@gh6_nuoc, N'Pepsi lon', 12000, NULL);

-- Hủ Tiếu Nam Vang
INSERT INTO DanhMuc (MaGianHang, TenDanhMuc) VALUES (@gh7, N'Hủ tiếu'), (@gh7, N'Nước uống');
DECLARE @gh7_ht INT = (SELECT TOP 1 MaDanhMuc FROM DanhMuc WHERE MaGianHang=@gh7 AND TenDanhMuc=N'Hủ tiếu');
DECLARE @gh7_nuoc INT = (SELECT TOP 1 MaDanhMuc FROM DanhMuc WHERE MaGianHang=@gh7 AND TenDanhMuc=N'Nước uống');
INSERT INTO MonAn (MaDanhMuc, TenMon, Gia, MoTa) VALUES
(@gh7_ht, N'Hủ tiếu Nam Vang', 45000, NULL),
(@gh7_ht, N'Hủ tiếu khô', 45000, NULL),
(@gh7_ht, N'Hủ tiếu đặc biệt', 55000, N'Tôm, thịt bằm, gan, trứng cút'),
(@gh7_nuoc, N'Trà đá', 5000, NULL);

-- Chè Ba Miền
INSERT INTO DanhMuc (MaGianHang, TenDanhMuc) VALUES (@gh8, N'Chè'), (@gh8, N'Sữa chua');
DECLARE @gh8_che INT = (SELECT TOP 1 MaDanhMuc FROM DanhMuc WHERE MaGianHang=@gh8 AND TenDanhMuc=N'Chè');
DECLARE @gh8_sc INT = (SELECT TOP 1 MaDanhMuc FROM DanhMuc WHERE MaGianHang=@gh8 AND TenDanhMuc=N'Sữa chua');
INSERT INTO MonAn (MaDanhMuc, TenMon, Gia, MoTa) VALUES
(@gh8_che, N'Chè đậu xanh', 18000, NULL),
(@gh8_che, N'Chè bà ba', 20000, NULL),
(@gh8_che, N'Chè thái', 25000, NULL),
(@gh8_che, N'Chè khúc bạch', 25000, NULL),
(@gh8_sc, N'Sữa chua nếp cẩm', 20000, NULL),
(@gh8_sc, N'Sữa chua trân châu', 20000, NULL);
GO

/* ============================================================
   PHẦN 4: KHUYẾN MÃI MẪU
   ============================================================ */
DECLARE @gh_bunbo INT = (SELECT MaGianHang FROM GianHang WHERE TenCuaHang = N'Bún Bò Huế Mệ Ba');
DECLARE @gh_traSua INT = (SELECT MaGianHang FROM GianHang WHERE TenCuaHang = N'Trà Sữa Mộc Trà');

INSERT INTO KhuyenMai (MaGianHang, MaCode, MoTa, PhanTramGiam, SoTienGiam, DieuKienApDung, NgayBatDau, NgayKetThuc) VALUES
(NULL, N'CHAODON10', N'Giảm 10% cho đơn hàng đầu tiên toàn hệ thống', 10, NULL, N'Áp dụng 1 lần/tài khoản', '2026-01-01', '2026-12-31'),
(NULL, N'FREESHIP', N'Miễn phí giao hàng cho đơn từ 150.000đ', NULL, 15000, N'Đơn tối thiểu 150.000đ', '2026-01-01', '2026-12-31'),
(@gh_bunbo, N'BUNBO20K', N'Giảm 20.000đ cho đơn từ 100.000đ tại Bún Bò Huế Mệ Ba', NULL, 20000, N'Đơn tối thiểu 100.000đ', '2026-01-01', '2026-12-31'),
(@gh_traSua, N'TRASUA15', N'Giảm 15% cho mọi đơn trà sữa', 15, NULL, N'Không giới hạn giá trị đơn', '2026-01-01', '2026-12-31');
GO

/* ============================================================
   PHẦN 5: ĐƠN HÀNG MẪU (kèm chi tiết, thanh toán, giao hàng, đánh giá)
   ============================================================ */
DECLARE @kh1 INT = (SELECT MaKH FROM KhachHang WHERE SDT = '0901000001');
DECLARE @kh2 INT = (SELECT MaKH FROM KhachHang WHERE SDT = '0907000007');
DECLARE @tx1 INT = (SELECT MaTaiXe FROM TaiXe WHERE SDT = '0903000003');
DECLARE @tx2 INT = (SELECT MaTaiXe FROM TaiXe WHERE SDT = '0906000006');
DECLARE @gh1_id INT = (SELECT MaGianHang FROM GianHang WHERE TenCuaHang = N'Quán Cơm Tấm Cô Ba');
DECLARE @gh2_id INT = (SELECT MaGianHang FROM GianHang WHERE TenCuaHang = N'Bún Bò Huế Mệ Ba');
DECLARE @mon_com1 INT = (SELECT MaMonAn FROM MonAn WHERE TenMon = N'Cơm sườn bì chả');
DECLARE @mon_bunbo1 INT = (SELECT MaMonAn FROM MonAn WHERE TenMon = N'Bún bò giò heo');

-- Đơn 1: đã giao thành công
INSERT INTO DonHang (MaKH, MaGianHang, MaTaiXe, TrangThai, TongTien, PhiGiaoHang, DiaChiGiaoHang)
VALUES (@kh1, @gh1_id, @tx1, N'DaGiao', 45000, 15000, N'123 Lê Lợi, Q.1, TP.HCM');
DECLARE @dh1 INT = SCOPE_IDENTITY();
INSERT INTO ChiTietDonHang (MaDonHang, MaMonAn, SoLuong, DonGia) VALUES (@dh1, @mon_com1, 1, 45000);
INSERT INTO ThanhToan (MaDonHang, HinhThuc, TrangThai, NgayThanhToan) VALUES (@dh1, N'Momo', N'DaThanhToan', GETDATE());
INSERT INTO GiaoHang (MaDonHang, MaTaiXe, TrangThai, ThoiGianNhan, ThoiGianGiao)
VALUES (@dh1, @tx1, N'GiaoThanhCong', DATEADD(MINUTE,-40,GETDATE()), DATEADD(MINUTE,-10,GETDATE()));
INSERT INTO DanhGia (MaKH, MaGianHang, MaDonHang, SoSao, NoiDung) VALUES (@kh1, @gh1_id, @dh1, 5, N'Cơm ngon, giao nhanh');

-- Đơn 2: đang giao
INSERT INTO DonHang (MaKH, MaGianHang, MaTaiXe, TrangThai, TongTien, PhiGiaoHang, DiaChiGiaoHang)
VALUES (@kh2, @gh2_id, @tx2, N'DangGiao', 55000, 20000, N'56 Lý Thường Kiệt, Q.10, TP.HCM');
DECLARE @dh2 INT = SCOPE_IDENTITY();
INSERT INTO ChiTietDonHang (MaDonHang, MaMonAn, SoLuong, DonGia) VALUES (@dh2, @mon_bunbo1, 1, 55000);
INSERT INTO ThanhToan (MaDonHang, HinhThuc, TrangThai) VALUES (@dh2, N'VNPay', N'DaThanhToan');
INSERT INTO GiaoHang (MaDonHang, MaTaiXe, TrangThai, ThoiGianNhan) VALUES (@dh2, @tx2, N'DangGiao', DATEADD(MINUTE,-10,GETDATE()));

-- Đơn 3: chờ xác nhận (chưa gán tài xế)
INSERT INTO DonHang (MaKH, MaGianHang, TrangThai, TongTien, DiaChiGiaoHang)
VALUES (@kh1, @gh2_id, N'ChoXacNhan', 65000, N'123 Lê Lợi, Q.1, TP.HCM');
DECLARE @dh3 INT = SCOPE_IDENTITY();
INSERT INTO ChiTietDonHang (MaDonHang, MaMonAn, SoLuong, DonGia)
SELECT @dh3, MaMonAn, 1, Gia FROM MonAn WHERE TenMon = N'Bún bò đặc biệt';
INSERT INTO ThanhToan (MaDonHang, HinhThuc, TrangThai) VALUES (@dh3, N'TienMat', N'ChoThanhToan');
GO

PRINT N'Hoàn tất: CSDL QuanLyDichVuAnUong với 17 bảng (đúng đề cương: KhachHang/GianHang/QuanTri + Shipper hỗ trợ giao hàng), 8 gian hàng, ~45 món ăn, 5 khách hàng, 2 quản trị viên, 4 tài xế, 4 khuyến mãi, 3 đơn hàng mẫu.';

/* ============================================================
   PHẦN 6: CẬP NHẬT MẬT KHẨU DEMO CHO TÀI KHOẢN MẪU BAN ĐẦU
   (8 gian hàng, 5 khách hàng, 4 tài xế, 2 quản trị viên ở PHẦN 2-4 ở trên)
   Mật khẩu plaintext để đăng nhập thử — xem ghi chú bảo mật trong README.
   ============================================================ */

-- Khách hàng: mật khẩu = 123456
UPDATE TaiKhoan SET MatKhau = '123456' WHERE VaiTro = N'KhachHang';

-- Gian hàng (quán ăn / đối tác): mật khẩu = 123456
UPDATE TaiKhoan SET MatKhau = '123456' WHERE VaiTro = N'GianHang';

-- Tài xế: mật khẩu = 123456
UPDATE TaiKhoan SET MatKhau = '123456' WHERE VaiTro = N'TaiXe';

-- Quản trị: mật khẩu = admin123 (đổi riêng cho dễ phân biệt khi demo)
UPDATE TaiKhoan SET MatKhau = 'admin123' WHERE VaiTro = N'QuanTri';
GO

-- Xem lại danh sách tài khoản vừa cập nhật để đối chiếu khi đăng nhập thử
SELECT
    t.MaTK,
    t.TenDangNhap,
    t.MatKhau,
    t.VaiTro,
    COALESCE(kh.HoTen, gh.TenCuaHang, tx.HoTen, qt.HoTen) AS TenHienThi
FROM TaiKhoan t
LEFT JOIN KhachHang kh ON kh.MaTK = t.MaTK
LEFT JOIN GianHang gh ON gh.MaTK = t.MaTK
LEFT JOIN TaiXe tx ON tx.MaTK = t.MaTK
LEFT JOIN QuanTri qt ON qt.MaTK = t.MaTK
ORDER BY t.VaiTro, t.MaTK;
GO

/* ============================================================
   PHẦN 7: SINH DỮ LIỆU LỚN — 100 KHÁCH HÀNG, 30 QUÁN ĂN, 40 TÀI XẾ
   Mật khẩu tất cả tài khoản sinh ra = 123456 (plaintext, chỉ để demo).
   ============================================================ */

-- ---------- Bảng tạm chứa các thành phần tên tiếng Việt để ghép ngẫu nhiên ----------
DECLARE @Ho TABLE (STT INT IDENTITY(0,1), Ten NVARCHAR(20));
INSERT INTO @Ho (Ten) VALUES
(N'Nguyễn'),(N'Trần'),(N'Lê'),(N'Phạm'),(N'Hoàng'),(N'Huỳnh'),(N'Phan'),(N'Vũ'),
(N'Võ'),(N'Đặng'),(N'Bùi'),(N'Đỗ'),(N'Hồ'),(N'Ngô'),(N'Dương'),(N'Lý');

DECLARE @TenDemNam TABLE (STT INT IDENTITY(0,1), Ten NVARCHAR(20));
INSERT INTO @TenDemNam (Ten) VALUES (N'Văn'),(N'Hữu'),(N'Đức'),(N'Công'),(N'Quang'),(N'Minh'),(N'Thành'),(N'Anh');

DECLARE @TenDemNu TABLE (STT INT IDENTITY(0,1), Ten NVARCHAR(20));
INSERT INTO @TenDemNu (Ten) VALUES (N'Thị'),(N'Ngọc'),(N'Thu'),(N'Kim'),(N'Hồng'),(N'Bích'),(N'Diễm'),(N'Ánh');

DECLARE @TenNam TABLE (STT INT IDENTITY(0,1), Ten NVARCHAR(20));
INSERT INTO @TenNam (Ten) VALUES
(N'An'),(N'Bình'),(N'Cường'),(N'Dũng'),(N'Đạt'),(N'Hải'),(N'Hùng'),(N'Khang'),
(N'Long'),(N'Minh'),(N'Nam'),(N'Phong'),(N'Quân'),(N'Sơn'),(N'Tài'),(N'Thắng'),
(N'Tuấn'),(N'Việt'),(N'Vinh'),(N'Đăng');

DECLARE @TenNu TABLE (STT INT IDENTITY(0,1), Ten NVARCHAR(20));
INSERT INTO @TenNu (Ten) VALUES
(N'Anh'),(N'Chi'),(N'Diệp'),(N'Giang'),(N'Hà'),(N'Hoa'),(N'Huyền'),(N'Lan'),
(N'Linh'),(N'Mai'),(N'My'),(N'Ngân'),(N'Nhi'),(N'Phương'),(N'Quyên'),(N'Thảo'),
(N'Thư'),(N'Trang'),(N'Vy'),(N'Yến');

DECLARE @QuanHuyen TABLE (STT INT IDENTITY(0,1), Ten NVARCHAR(50));
INSERT INTO @QuanHuyen (Ten) VALUES
(N'Quận 1'),(N'Quận 3'),(N'Quận 4'),(N'Quận 5'),(N'Quận 6'),(N'Quận 7'),
(N'Quận 8'),(N'Quận 10'),(N'Quận 11'),(N'Quận 12'),(N'Bình Thạnh'),(N'Tân Bình'),
(N'Tân Phú'),(N'Phú Nhuận'),(N'Gò Vấp'),(N'Bình Tân'),(N'Thủ Đức');

DECLARE @TenDuong TABLE (STT INT IDENTITY(0,1), Ten NVARCHAR(50));
INSERT INTO @TenDuong (Ten) VALUES
(N'Lê Lợi'),(N'Nguyễn Trãi'),(N'Trần Hưng Đạo'),(N'Cách Mạng Tháng 8'),
(N'Nguyễn Thị Minh Khai'),(N'Điện Biên Phủ'),(N'Hai Bà Trưng'),(N'Ba Tháng Hai'),
(N'Lý Thường Kiệt'),(N'Nguyễn Văn Cừ'),(N'Sư Vạn Hạnh'),(N'Cộng Hòa'),
(N'Phan Xích Long'),(N'Quang Trung'),(N'Nguyễn Oanh');

-- ---------- Loại hình quán ăn để đặt tên gian hàng ----------
DECLARE @LoaiQuan TABLE (STT INT IDENTITY(0,1), Ten NVARCHAR(50));
INSERT INTO @LoaiQuan (Ten) VALUES
(N'Cơm Tấm'),(N'Bún Bò Huế'),(N'Phở'),(N'Bánh Mì'),(N'Hủ Tiếu'),(N'Bún Riêu'),
(N'Mì Quảng'),(N'Cơm Gà'),(N'Trà Sữa'),(N'Chè'),(N'Gà Rán'),(N'Pizza'),
(N'Bún Chả'),(N'Bánh Xèo'),(N'Lẩu'),(N'Nướng BBQ'),(N'Sushi'),(N'Bún Đậu'),
(N'Xôi'),(N'Ốc'),(N'Trà Chanh'),(N'Bánh Canh'),(N'Cháo'),(N'Cơm Niêu'),
(N'Bò Kho'),(N'Nem Nướng'),(N'Bánh Cuốn'),(N'Cà Phê'),(N'Sinh Tố'),(N'Bún Thịt Nướng');

DECLARE @TenChuQuan TABLE (STT INT IDENTITY(0,1), Ten NVARCHAR(30));
INSERT INTO @TenChuQuan (Ten) VALUES
(N'Cô Ba'),(N'Cô Út'),(N'Cô Sáu'),(N'Cô Tư'),(N'Chú Hai'),(N'Chú Bảy'),
(N'Mẹ Ba'),(N'Bà Năm'),(N'Anh Tuấn'),(N'Chị Hoa'),(N'Sài Gòn'),(N'Hà Nội'),
(N'Miền Tây'),(N'Ba Miền'),(N'Xưa'),(N'136'),(N'168'),(N'Number One'),
(N'Gia Truyền'),(N'Truyền Thống'),(N'Ngon'),(N'Đệ Nhất'),(N'Phố'),(N'Quê'),
(N'Nhà Làm'),(N'Việt'),(N'Sài Thành'),(N'Ông Địa'),(N'Cây Bàng'),(N'Xóm Nhỏ');

-- ==============================================================
-- 100 TÀI KHOẢN KHÁCH HÀNG
-- ==============================================================
DECLARE @i INT = 1;
WHILE @i <= 100
BEGIN
    DECLARE @tenDangNhap NVARCHAR(50) = N'khachhang' + RIGHT('000' + CAST(@i AS VARCHAR), 3);
    DECLARE @laNam BIT = @i % 2;
    DECLARE @ho NVARCHAR(20) = (SELECT Ten FROM @Ho WHERE STT = @i % 16);
    DECLARE @tenDem NVARCHAR(20) = CASE WHEN @laNam = 1
        THEN (SELECT Ten FROM @TenDemNam WHERE STT = @i % 8)
        ELSE (SELECT Ten FROM @TenDemNu WHERE STT = @i % 8) END;
    DECLARE @ten NVARCHAR(20) = CASE WHEN @laNam = 1
        THEN (SELECT Ten FROM @TenNam WHERE STT = @i % 20)
        ELSE (SELECT Ten FROM @TenNu WHERE STT = @i % 20) END;
    DECLARE @hoTen NVARCHAR(100) = @ho + N' ' + @tenDem + N' ' + @ten;
    DECLARE @sdt VARCHAR(15) = '09' + RIGHT('00000000' + CAST(30000000 + @i AS VARCHAR), 8);
    DECLARE @diaChi NVARCHAR(255) =
        CAST(10 + (@i % 90) AS VARCHAR) + N' Đường ' +
        (SELECT Ten FROM @TenDuong WHERE STT = @i % 15) + N', ' +
        (SELECT Ten FROM @QuanHuyen WHERE STT = @i % 17) + N', TP.HCM';

    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro) VALUES (@tenDangNhap, '123456', N'KhachHang');
    DECLARE @maTK INT = SCOPE_IDENTITY();

    INSERT INTO KhachHang (MaTK, HoTen, SDT, Email, DiaChiGiaoHang)
    VALUES (@maTK, @hoTen, @sdt, LOWER(REPLACE(@tenDangNhap, N' ', '')) + '@gmail.com', @diaChi);

    SET @i += 1;
END
GO

-- ==============================================================
-- 30 TÀI KHOẢN QUÁN ĂN (GIAN HÀNG)
-- ==============================================================
DECLARE @Ho TABLE (STT INT IDENTITY(0,1), Ten NVARCHAR(20));
INSERT INTO @Ho (Ten) VALUES
(N'Nguyễn'),(N'Trần'),(N'Lê'),(N'Phạm'),(N'Hoàng'),(N'Huỳnh'),(N'Phan'),(N'Vũ'),
(N'Võ'),(N'Đặng'),(N'Bùi'),(N'Đỗ'),(N'Hồ'),(N'Ngô'),(N'Dương'),(N'Lý');

DECLARE @QuanHuyen TABLE (STT INT IDENTITY(0,1), Ten NVARCHAR(50));
INSERT INTO @QuanHuyen (Ten) VALUES
(N'Quận 1'),(N'Quận 3'),(N'Quận 4'),(N'Quận 5'),(N'Quận 6'),(N'Quận 7'),
(N'Quận 8'),(N'Quận 10'),(N'Quận 11'),(N'Quận 12'),(N'Bình Thạnh'),(N'Tân Bình'),
(N'Tân Phú'),(N'Phú Nhuận'),(N'Gò Vấp'),(N'Bình Tân'),(N'Thủ Đức');

DECLARE @TenDuong TABLE (STT INT IDENTITY(0,1), Ten NVARCHAR(50));
INSERT INTO @TenDuong (Ten) VALUES
(N'Lê Lợi'),(N'Nguyễn Trãi'),(N'Trần Hưng Đạo'),(N'Cách Mạng Tháng 8'),
(N'Nguyễn Thị Minh Khai'),(N'Điện Biên Phủ'),(N'Hai Bà Trưng'),(N'Ba Tháng Hai'),
(N'Lý Thường Kiệt'),(N'Nguyễn Văn Cừ'),(N'Sư Vạn Hạnh'),(N'Cộng Hòa'),
(N'Phan Xích Long'),(N'Quang Trung'),(N'Nguyễn Oanh');

DECLARE @LoaiQuan TABLE (STT INT IDENTITY(0,1), Ten NVARCHAR(50));
INSERT INTO @LoaiQuan (Ten) VALUES
(N'Cơm Tấm'),(N'Bún Bò Huế'),(N'Phở'),(N'Bánh Mì'),(N'Hủ Tiếu'),(N'Bún Riêu'),
(N'Mì Quảng'),(N'Cơm Gà'),(N'Trà Sữa'),(N'Chè'),(N'Gà Rán'),(N'Pizza'),
(N'Bún Chả'),(N'Bánh Xèo'),(N'Lẩu'),(N'Nướng BBQ'),(N'Sushi'),(N'Bún Đậu'),
(N'Xôi'),(N'Ốc'),(N'Trà Chanh'),(N'Bánh Canh'),(N'Cháo'),(N'Cơm Niêu'),
(N'Bò Kho'),(N'Nem Nướng'),(N'Bánh Cuốn'),(N'Cà Phê'),(N'Sinh Tố'),(N'Bún Thịt Nướng');

DECLARE @TenChuQuan TABLE (STT INT IDENTITY(0,1), Ten NVARCHAR(30));
INSERT INTO @TenChuQuan (Ten) VALUES
(N'Cô Ba'),(N'Cô Út'),(N'Cô Sáu'),(N'Cô Tư'),(N'Chú Hai'),(N'Chú Bảy'),
(N'Mẹ Ba'),(N'Bà Năm'),(N'Anh Tuấn'),(N'Chị Hoa'),(N'Sài Gòn'),(N'Hà Nội'),
(N'Miền Tây'),(N'Ba Miền'),(N'Xưa'),(N'136'),(N'168'),(N'Number One'),
(N'Gia Truyền'),(N'Truyền Thống'),(N'Ngon'),(N'Đệ Nhất'),(N'Phố'),(N'Quê'),
(N'Nhà Làm'),(N'Việt'),(N'Sài Thành'),(N'Ông Địa'),(N'Cây Bàng'),(N'Xóm Nhỏ');

DECLARE @j INT = 1;
WHILE @j <= 30
BEGIN
    DECLARE @tenDN NVARCHAR(50) = N'gianhang' + RIGHT('000' + CAST(@j AS VARCHAR), 3);
    DECLARE @tenCuaHang NVARCHAR(150) =
        (SELECT Ten FROM @LoaiQuan WHERE STT = (@j - 1) % 30) + N' ' +
        (SELECT Ten FROM @TenChuQuan WHERE STT = @j % 30);
    DECLARE @diaChiQ NVARCHAR(255) =
        CAST(5 + (@j % 200) AS VARCHAR) + N' ' +
        (SELECT Ten FROM @TenDuong WHERE STT = @j % 15) + N', ' +
        (SELECT Ten FROM @QuanHuyen WHERE STT = @j % 17) + N', TP.HCM';
    DECLARE @sdtQ VARCHAR(15) = '09' + RIGHT('00000000' + CAST(40000000 + @j AS VARCHAR), 8);

    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro) VALUES (@tenDN, '123456', N'GianHang');
    DECLARE @maTKQ INT = SCOPE_IDENTITY();

    INSERT INTO GianHang (MaTK, TenCuaHang, DiaChi, SDT, MoTa, TrangThaiKinhDoanh, TrangThaiDuyet)
    VALUES (@maTKQ, @tenCuaHang, @diaChiQ, @sdtQ, N'Quán ăn phục vụ tận tâm, nguyên liệu tươi mỗi ngày',
            CASE WHEN @j % 10 = 0 THEN N'TamNgung' ELSE N'DangMo' END, N'DaDuyet');

    -- Mỗi gian hàng có sẵn 1 danh mục "Món chính" + 3 món ăn cơ bản để không bị trống thực đơn
    DECLARE @maGH INT = SCOPE_IDENTITY();
    INSERT INTO DanhMuc (MaGianHang, TenDanhMuc) VALUES (@maGH, N'Món chính');
    DECLARE @maDM INT = SCOPE_IDENTITY();

    INSERT INTO MonAn (MaDanhMuc, TenMon, Gia, TinhTrang) VALUES
    (@maDM, (SELECT Ten FROM @LoaiQuan WHERE STT = (@j - 1) % 30) + N' đặc biệt', 45000 + (@j % 6) * 5000, N'ConHang'),
    (@maDM, (SELECT Ten FROM @LoaiQuan WHERE STT = (@j - 1) % 30) + N' thường', 35000 + (@j % 5) * 3000, N'ConHang'),
    (@maDM, N'Nước ngọt', 12000, N'ConHang');

    SET @j += 1;
END
GO

-- ==============================================================
-- 40 TÀI KHOẢN TÀI XẾ
-- ==============================================================
DECLARE @Ho2 TABLE (STT INT IDENTITY(0,1), Ten NVARCHAR(20));
INSERT INTO @Ho2 (Ten) VALUES
(N'Nguyễn'),(N'Trần'),(N'Lê'),(N'Phạm'),(N'Hoàng'),(N'Huỳnh'),(N'Phan'),(N'Vũ'),
(N'Võ'),(N'Đặng'),(N'Bùi'),(N'Đỗ'),(N'Hồ'),(N'Ngô'),(N'Dương'),(N'Lý');

DECLARE @TenDemNam2 TABLE (STT INT IDENTITY(0,1), Ten NVARCHAR(20));
INSERT INTO @TenDemNam2 (Ten) VALUES (N'Văn'),(N'Hữu'),(N'Đức'),(N'Công'),(N'Quang'),(N'Minh'),(N'Thành'),(N'Anh');

DECLARE @TenNam2 TABLE (STT INT IDENTITY(0,1), Ten NVARCHAR(20));
INSERT INTO @TenNam2 (Ten) VALUES
(N'An'),(N'Bình'),(N'Cường'),(N'Dũng'),(N'Đạt'),(N'Hải'),(N'Hùng'),(N'Khang'),
(N'Long'),(N'Minh'),(N'Nam'),(N'Phong'),(N'Quân'),(N'Sơn'),(N'Tài'),(N'Thắng'),
(N'Tuấn'),(N'Việt'),(N'Vinh'),(N'Đăng');

DECLARE @QuanHuyen2 TABLE (STT INT IDENTITY(0,1), Ten NVARCHAR(50));
INSERT INTO @QuanHuyen2 (Ten) VALUES
(N'Quận 1'),(N'Quận 3'),(N'Quận 4'),(N'Quận 5'),(N'Quận 6'),(N'Quận 7'),
(N'Quận 8'),(N'Quận 10'),(N'Quận 11'),(N'Quận 12'),(N'Bình Thạnh'),(N'Tân Bình'),
(N'Tân Phú'),(N'Phú Nhuận'),(N'Gò Vấp'),(N'Bình Tân'),(N'Thủ Đức');

DECLARE @k INT = 1;
WHILE @k <= 40
BEGIN
    DECLARE @tenDNTX NVARCHAR(50) = N'taixe' + RIGHT('000' + CAST(@k AS VARCHAR), 3);
    DECLARE @hoTX NVARCHAR(20) = (SELECT Ten FROM @Ho2 WHERE STT = @k % 16);
    DECLARE @tenDemTX NVARCHAR(20) = (SELECT Ten FROM @TenDemNam2 WHERE STT = @k % 8);
    DECLARE @tenTX NVARCHAR(20) = (SELECT Ten FROM @TenNam2 WHERE STT = @k % 20);
    DECLARE @hoTenTX NVARCHAR(100) = @hoTX + N' ' + @tenDemTX + N' ' + @tenTX;
    DECLARE @sdtTX VARCHAR(15) = '09' + RIGHT('00000000' + CAST(50000000 + @k AS VARCHAR), 8);
    DECLARE @khuVuc NVARCHAR(150) =
        (SELECT Ten FROM @QuanHuyen2 WHERE STT = @k % 17) + N', ' +
        (SELECT Ten FROM @QuanHuyen2 WHERE STT = (@k + 3) % 17);
    DECLARE @phuongTien NVARCHAR(50) = CASE WHEN @k % 5 = 0 THEN N'Xe đạp điện' ELSE N'Xe máy' END;
    DECLARE @bienSo VARCHAR(20) = N'59-' + CHAR(65 + (@k % 26)) + N'1-' +
        RIGHT('00000' + CAST(10000 + @k AS VARCHAR), 5);

    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro) VALUES (@tenDNTX, '123456', N'TaiXe');
    DECLARE @maTKTX INT = SCOPE_IDENTITY();

    INSERT INTO TaiXe (MaTK, HoTen, SDT, PhuongTien, BienSo, KhuVucHoatDong, TrangThai)
    VALUES (@maTKTX, @hoTenTX, @sdtTX, @phuongTien, @bienSo, @khuVuc,
            CASE WHEN @k % 7 = 0 THEN N'NgoaiTuyen' ELSE N'SanSang' END);

    SET @k += 1;
END
GO

PRINT N'Đã sinh xong: 100 khách hàng, 30 gian hàng (kèm 3 món/gian hàng), 40 tài xế. Mật khẩu tất cả = 123456.';

SELECT VaiTro, COUNT(*) AS SoLuong FROM TaiKhoan
WHERE TenDangNhap LIKE 'khachhang0%' OR TenDangNhap LIKE 'gianhang0%' OR TenDangNhap LIKE 'taixe0%'
GROUP BY VaiTro;
GO

/* ============================================================
   PHẦN 8: DỮ LIỆU MẪU CHO CÁC CHỨC NĂNG MỚI (sổ địa chỉ, đánh giá món...)
   ============================================================ */

-- Sổ địa chỉ: mỗi khách mẫu có 1 địa chỉ mặc định từ hồ sơ
INSERT INTO DiaChiKhachHang (MaKH, Nhan, DiaChi, MacDinh)
SELECT MaKH, N'Nhà', DiaChiGiaoHang, 1 FROM KhachHang WHERE DiaChiGiaoHang IS NOT NULL;
GO

-- Đánh giá món ăn mẫu (cho các đơn đã giao có chi tiết món)
INSERT INTO DanhGiaMon (MaKH, MaMonAn, MaDonHang, SoSao, NoiDung)
SELECT TOP 5 d.MaKH, c.MaMonAn, d.MaDonHang, 5, N'Món ngon, đúng vị'
FROM DonHang d JOIN ChiTietDonHang c ON c.MaDonHang = d.MaDonHang
WHERE d.TrangThai = N'DaGiao';
GO

-- Phản hồi mẫu
INSERT INTO PhanHoi (MaKH, Loai, TieuDe, NoiDung)
SELECT TOP 1 MaKH, N'GopY', N'Mong có thêm nhiều quán trong khu vực', N'Ứng dụng dùng rất tiện, mong sớm có thêm quán ở khu vực của mình.'
FROM KhachHang ORDER BY MaKH;
GO

/* ============================================================
   TỔNG KẾT TOÀN BỘ
   ============================================================ */
PRINT N'====== HOÀN TẤT TOÀN BỘ FILE ======';

SELECT VaiTro, COUNT(*) AS TongSoTaiKhoan FROM TaiKhoan GROUP BY VaiTro;
SELECT COUNT(*) AS TongSoGianHang FROM GianHang;
SELECT COUNT(*) AS TongSoMonAn FROM MonAn;
SELECT COUNT(*) AS TongSoDonHangMau FROM DonHang;
GO

/* ============================================================
   GHI CHÚ: nếu bạn ĐÃ có CSDL QuanLyDichVuAnUong chạy từ trước (tạo bằng
   bản cũ hơn của file này) và không muốn DROP/tạo lại từ đầu, KHÔNG chạy
   các lệnh ALTER thủ công ở đây — hãy chạy file NangCap_CSDL.sql đi kèm,
   đã gộp đầy đủ mọi cột/bảng còn thiếu và an toàn để chạy lại nhiều lần.
   ============================================================ */

/* ============================================================
   PHẦN 9: GÁN HÌNH ẢNH MINH HOẠ cho Gian hàng / Món ăn
   Ảnh SVG tự vẽ (không vi phạm bản quyền) đặt sẵn ở wwwroot/uploads/ —
   khớp theo TÊN (an toàn dù ID tự tăng khác nhau giữa các lần tạo DB).
   ============================================================ */

-- 1) Ảnh đại diện Gian hàng
UPDATE GianHang SET HinhAnh = N'/uploads/gh1_comtam_coba.svg'     WHERE TenCuaHang = N'Quán Cơm Tấm Cô Ba';
UPDATE GianHang SET HinhAnh = N'/uploads/gh2_bunbo_meba.svg'      WHERE TenCuaHang = N'Bún Bò Huế Mệ Ba';
UPDATE GianHang SET HinhAnh = N'/uploads/gh3_trasua_moctra.svg'   WHERE TenCuaHang = N'Trà Sữa Mộc Trà';
UPDATE GianHang SET HinhAnh = N'/uploads/gh4_pho_hanoi.svg'       WHERE TenCuaHang = N'Phở Hà Nội Xưa';
UPDATE GianHang SET HinhAnh = N'/uploads/gh5_banhmi_bahuynh.svg'  WHERE TenCuaHang = N'Bánh Mì Bà Huynh';
UPDATE GianHang SET HinhAnh = N'/uploads/gh6_garan_cout.svg'      WHERE TenCuaHang = N'Gà Rán Cô Út';
UPDATE GianHang SET HinhAnh = N'/uploads/gh7_hutieu_namvang.svg'  WHERE TenCuaHang = N'Hủ Tiếu Nam Vang Sáu Sài Gòn';
UPDATE GianHang SET HinhAnh = N'/uploads/gh8_che_bamien.svg'      WHERE TenCuaHang = N'Chè Ba Miền';
GO

-- 2) Ảnh món ăn tiêu biểu (1 món/gian hàng — có thể tự thêm ảnh cho các món còn lại theo mẫu này)
UPDATE MonAn SET HinhAnh = N'/uploads/mon1_com_suon_bi_cha.svg'              WHERE TenMon = N'Cơm sườn bì chả';
UPDATE MonAn SET HinhAnh = N'/uploads/mon2_bun_bo_gio_heo.svg'               WHERE TenMon = N'Bún bò giò heo';
UPDATE MonAn SET HinhAnh = N'/uploads/mon3_trasua_tranchau_duongden.svg'     WHERE TenMon = N'Trà sữa trân châu đường đen';
UPDATE MonAn SET HinhAnh = N'/uploads/mon4_pho_dac_biet.svg'                 WHERE TenMon = N'Phở đặc biệt';
UPDATE MonAn SET HinhAnh = N'/uploads/mon5_banhmi_thitnuong.svg'             WHERE TenMon = N'Bánh mì thịt nướng';
UPDATE MonAn SET HinhAnh = N'/uploads/mon6_combo_ga_com.svg'                 WHERE TenMon = N'Combo 2 miếng gà + cơm';
UPDATE MonAn SET HinhAnh = N'/uploads/mon7_hutieu_dac_biet.svg'              WHERE TenMon = N'Hủ tiếu đặc biệt';
UPDATE MonAn SET HinhAnh = N'/uploads/mon8_che_thai.svg'                     WHERE TenMon = N'Chè thái';
GO

-- Kiểm tra nhanh
SELECT TenCuaHang, HinhAnh FROM GianHang;
SELECT TenMon, HinhAnh FROM MonAn WHERE HinhAnh IS NOT NULL;
