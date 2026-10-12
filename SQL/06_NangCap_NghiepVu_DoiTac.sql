USE QuanLyDichVuAnUong;
GO
/* Snapshot món, lý do hủy và timeline trạng thái đơn. Có thể chạy lặp lại. */
IF COL_LENGTH('ChiTietDonHang', 'TenMon') IS NULL
BEGIN
    ALTER TABLE ChiTietDonHang ADD TenMon NVARCHAR(150) NOT NULL CONSTRAINT DF_ChiTietDonHang_TenMon DEFAULT N'';
    EXEC(N'UPDATE c SET TenMon = m.TenMon FROM ChiTietDonHang c JOIN MonAn m ON m.MaMonAn = c.MaMonAn WHERE c.TenMon = N''''');
END;
IF COL_LENGTH('ChiTietDonHang', 'TenMon') IS NOT NULL
    EXEC(N'UPDATE c SET TenMon = m.TenMon FROM ChiTietDonHang c JOIN MonAn m ON m.MaMonAn = c.MaMonAn WHERE c.TenMon = N''''');
IF COL_LENGTH('DonHang', 'LyDoHuy') IS NULL ALTER TABLE DonHang ADD LyDoHuy NVARCHAR(500) NULL;
IF COL_LENGTH('DanhGia', 'PhanHoiGianHang') IS NULL ALTER TABLE DanhGia ADD PhanHoiGianHang NVARCHAR(500) NULL;
IF COL_LENGTH('DanhGia', 'NgayPhanHoi') IS NULL ALTER TABLE DanhGia ADD NgayPhanHoi DATETIME NULL;
GO

DECLARE @ck sysname;
SELECT TOP 1 @ck = cc.name FROM sys.check_constraints cc
JOIN sys.columns c ON c.object_id = cc.parent_object_id AND c.column_id = cc.parent_column_id
WHERE cc.parent_object_id = OBJECT_ID('DonHang') AND c.name = 'TrangThai';
IF @ck IS NOT NULL
BEGIN
    DECLARE @sqlDrop NVARCHAR(MAX) = N'ALTER TABLE DonHang DROP CONSTRAINT ' + QUOTENAME(@ck);
    EXEC sp_executesql @sqlDrop;
END;
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID('DonHang') AND name = 'CK_DonHang_TrangThai')
    ALTER TABLE DonHang ADD CONSTRAINT CK_DonHang_TrangThai CHECK (TrangThai IN
        (N'ChoXacNhan',N'DaXacNhan',N'DangChuanBi',N'SanSangGiao',N'TaiXeDaNhan',N'DangGiao',N'DaGiao',N'DaHuy'));
GO

IF OBJECT_ID('NhatKyTrangThaiDon', 'U') IS NULL
BEGIN
    CREATE TABLE NhatKyTrangThaiDon (
        MaNhatKy INT IDENTITY(1,1) PRIMARY KEY,
        MaDonHang INT NOT NULL,
        TrangThaiCu NVARCHAR(30) NULL,
        TrangThaiMoi NVARCHAR(30) NOT NULL,
        LoaiNguoiThucHien NVARCHAR(30) NOT NULL CONSTRAINT DF_NhatKyTrangThaiDon_Loai DEFAULT N'HeThong',
        MaNguoiThucHien INT NULL,
        LyDo NVARCHAR(500) NULL,
        ThoiGian DATETIME NOT NULL CONSTRAINT DF_NhatKyTrangThaiDon_ThoiGian DEFAULT GETDATE(),
        CONSTRAINT FK_NhatKyTrangThaiDon_DonHang FOREIGN KEY (MaDonHang) REFERENCES DonHang(MaDonHang)
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('NhatKyTrangThaiDon') AND name = 'IX_NhatKyTrangThaiDon_MaDonHang_ThoiGian')
    CREATE INDEX IX_NhatKyTrangThaiDon_MaDonHang_ThoiGian ON NhatKyTrangThaiDon(MaDonHang, ThoiGian);
GO

