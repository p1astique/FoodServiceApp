USE QuanLyDichVuAnUong;
GO
IF COL_LENGTH('MonAn', 'KichCo') IS NULL ALTER TABLE MonAn ADD KichCo NVARCHAR(255) NULL;
IF COL_LENGTH('MonAn', 'Topping') IS NULL ALTER TABLE MonAn ADD Topping NVARCHAR(500) NULL;
IF COL_LENGTH('MonAn', 'MucCay') IS NULL ALTER TABLE MonAn ADD MucCay NVARCHAR(255) NULL;
IF COL_LENGTH('ChiTietDonHang', 'TuyChonMon') IS NULL ALTER TABLE ChiTietDonHang ADD TuyChonMon NVARCHAR(500) NULL;
GO
PRINT N'Đã bổ sung cấu hình và lựa chọn size, topping, mức cay, ghi chú món.';
