# Quy ước cập nhật database

- Khi thay đổi code có liên quan tới schema hoặc dữ liệu nền của database, phải cập nhật đồng thời file SQL chuyên biệt tương ứng trong thư mục `SQL`.
- Đồng thời phải chép chính phần SQL mới vào `SQL/Script.sql`. Không chỉ thêm lệnh `:r` hoặc tham chiếu tới file khác; `Script.sql` phải chạy độc lập.
- `SQL/Script.sql` là script nâng cấp tổng hợp cho database `QuanLyDichVuAnUong` đã tồn tại. Script không được `DROP DATABASE`, không xóa dữ liệu nghiệp vụ và nên chạy lặp lại an toàn bằng `IF COL_LENGTH`, `IF OBJECT_ID`, hoặc kiểm tra tương đương.
- Nếu thay đổi schema cũng áp dụng khi tạo database mới, cập nhật thêm `SQL/01_MotFileDuyNhat_TaoMoi.sql` để schema tạo mới khớp code.
- Sau mỗi thay đổi SQL, kiểm tra script trên SQL Server/LocalDB và build lại project liên quan.
