USE QuanLyDichVuAnUong;
GO

/* Chạy an toàn nhiều lần trên database đã có. */
IF COL_LENGTH('DanhMuc', 'MoTa') IS NULL ALTER TABLE DanhMuc ADD MoTa NVARCHAR(255) NULL;
IF COL_LENGTH('DanhMuc', 'ThuTuHienThi') IS NULL ALTER TABLE DanhMuc ADD ThuTuHienThi INT NOT NULL CONSTRAINT DF_DanhMuc_ThuTu DEFAULT 0;
IF COL_LENGTH('DanhMuc', 'TrangThai') IS NULL ALTER TABLE DanhMuc ADD TrangThai BIT NOT NULL CONSTRAINT DF_DanhMuc_TrangThai DEFAULT 1;
GO

IF OBJECT_ID('CauHinhGianHang', 'U') IS NULL
CREATE TABLE CauHinhGianHang (
    MaGianHang             INT NOT NULL PRIMARY KEY REFERENCES GianHang(MaGianHang) ON DELETE CASCADE,
    GioMoCua               TIME NOT NULL DEFAULT '07:00',
    GioDongCua             TIME NOT NULL DEFAULT '22:00',
    TuDongDongMo           BIT NOT NULL DEFAULT 0,
    TamDungNhanDon         BIT NOT NULL DEFAULT 0,
    LyDoTamDung            NVARCHAR(255) NULL,
    ThongBaoCuaHang        NVARCHAR(300) NULL,
    ThoiGianChuanBiPhut    INT NOT NULL DEFAULT 20,
    CONSTRAINT CK_CauHinh_ThoiGianChuanBi CHECK (ThoiGianChuanBiPhut BETWEEN 5 AND 180)
);
GO

IF OBJECT_ID('ChienDichQuangCao', 'U') IS NULL
CREATE TABLE ChienDichQuangCao (
    MaChienDich       INT IDENTITY(1,1) PRIMARY KEY,
    MaGianHang        INT NOT NULL REFERENCES GianHang(MaGianHang) ON DELETE CASCADE,
    TenChienDich      NVARCHAR(150) NOT NULL,
    LoaiViTri         NVARCHAR(20) NOT NULL DEFAULT N'NoiBat',
    TieuDe            NVARCHAR(150) NOT NULL,
    NoiDung           NVARCHAR(500) NULL,
    HinhAnh           NVARCHAR(500) NULL,
    NgayBatDau        DATETIME2 NOT NULL,
    NgayKetThuc       DATETIME2 NOT NULL,
    NganSach          DECIMAL(12,0) NOT NULL DEFAULT 0,
    TrangThai         NVARCHAR(20) NOT NULL DEFAULT N'BanNhap',
    LuotHienThi       INT NOT NULL DEFAULT 0,
    LuotTuongTac      INT NOT NULL DEFAULT 0,
    NgayTao           DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT CK_ChienDich_LoaiViTri CHECK (LoaiViTri IN (N'Banner', N'NoiBat', N'GoiY')),
    CONSTRAINT CK_ChienDich_TrangThai CHECK (TrangThai IN (N'BanNhap', N'DangChay', N'TamDung')),
    CONSTRAINT CK_ChienDich_ThoiGian CHECK (NgayKetThuc > NgayBatDau),
    CONSTRAINT CK_ChienDich_NganSach CHECK (NganSach >= 0)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DanhMuc_MaGianHang_ThuTuHienThi')
    CREATE INDEX IX_DanhMuc_MaGianHang_ThuTuHienThi ON DanhMuc(MaGianHang, ThuTuHienThi);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChienDichQuangCao_MaGianHang_TrangThai_NgayBatDau_NgayKetThuc')
    CREATE INDEX IX_ChienDichQuangCao_MaGianHang_TrangThai_NgayBatDau_NgayKetThuc
        ON ChienDichQuangCao(MaGianHang, TrangThai, NgayBatDau, NgayKetThuc);
GO

PRINT N'Đã bổ sung cấu hình cửa hàng, chiến dịch quảng cáo và thuộc tính danh mục.';
GO
