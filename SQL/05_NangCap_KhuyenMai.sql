USE QuanLyDichVuAnUong;
GO
/* Nâng cấp voucher/khuyến mãi. Có thể chạy lặp lại an toàn. */
IF COL_LENGTH('KhuyenMai', 'MaDanhMuc') IS NULL
    ALTER TABLE KhuyenMai ADD MaDanhMuc INT NULL;
IF COL_LENGTH('KhuyenMai', 'MaGianHangApDung') IS NULL
    ALTER TABLE KhuyenMai ADD MaGianHangApDung INT NULL;
IF COL_LENGTH('KhuyenMai', 'GiamPhiVanChuyen') IS NULL
    ALTER TABLE KhuyenMai ADD GiamPhiVanChuyen BIT NOT NULL CONSTRAINT DF_KhuyenMai_GiamPhiVanChuyen DEFAULT 0;
IF COL_LENGTH('KhuyenMai', 'GiamPhiVanChuyenToiDa') IS NULL
    ALTER TABLE KhuyenMai ADD GiamPhiVanChuyenToiDa DECIMAL(12,0) NULL;
IF COL_LENGTH('KhuyenMai', 'GiaTriDonToiThieu') IS NULL
    ALTER TABLE KhuyenMai ADD GiaTriDonToiThieu DECIMAL(12,0) NOT NULL CONSTRAINT DF_KhuyenMai_GiaTriDonToiThieu DEFAULT 0;
IF COL_LENGTH('KhuyenMai', 'SoLuongToiDa') IS NULL
    ALTER TABLE KhuyenMai ADD SoLuongToiDa INT NULL;
IF COL_LENGTH('KhuyenMai', 'SoLuongDaDung') IS NULL
    ALTER TABLE KhuyenMai ADD SoLuongDaDung INT NOT NULL CONSTRAINT DF_KhuyenMai_SoLuongDaDung DEFAULT 0;
IF COL_LENGTH('KhuyenMai', 'MoiKhachMotLan') IS NULL
    ALTER TABLE KhuyenMai ADD MoiKhachMotLan BIT NOT NULL CONSTRAINT DF_KhuyenMai_MoiKhachMotLan DEFAULT 0;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_KhuyenMai_DanhMuc')
    ALTER TABLE KhuyenMai ADD CONSTRAINT FK_KhuyenMai_DanhMuc FOREIGN KEY (MaDanhMuc) REFERENCES DanhMuc(MaDanhMuc);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_KhuyenMai_GianHangApDung')
    ALTER TABLE KhuyenMai ADD CONSTRAINT FK_KhuyenMai_GianHangApDung FOREIGN KEY (MaGianHangApDung) REFERENCES GianHang(MaGianHang);
GO

IF OBJECT_ID('SuDungKhuyenMai', 'U') IS NULL
BEGIN
    CREATE TABLE SuDungKhuyenMai (
        MaSuDung INT IDENTITY(1,1) PRIMARY KEY,
        MaKhuyenMai INT NOT NULL,
        MaKH INT NOT NULL,
        MaDonHang INT NOT NULL,
        NgaySuDung DATETIME NOT NULL CONSTRAINT DF_SuDungKhuyenMai_NgaySuDung DEFAULT GETDATE(),
        CONSTRAINT FK_SuDungKhuyenMai_KhuyenMai FOREIGN KEY (MaKhuyenMai) REFERENCES KhuyenMai(MaKhuyenMai),
        CONSTRAINT FK_SuDungKhuyenMai_KhachHang FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH),
        CONSTRAINT FK_SuDungKhuyenMai_DonHang FOREIGN KEY (MaDonHang) REFERENCES DonHang(MaDonHang),
        CONSTRAINT UQ_SuDungKhuyenMai_DonHang UNIQUE (MaDonHang)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('SuDungKhuyenMai') AND name = 'IX_SuDungKhuyenMai_MaKhuyenMai_MaKH')
    CREATE INDEX IX_SuDungKhuyenMai_MaKhuyenMai_MaKH ON SuDungKhuyenMai(MaKhuyenMai, MaKH);
GO

