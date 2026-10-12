# Đối chiếu chức năng gian hàng

Cập nhật ngày 12/10/2026 theo danh sách yêu cầu người dùng cung cấp.

| Nhóm | Kết quả kiểm tra | Vị trí chính |
|---|---|---|
| Đăng ký | Đã có đăng ký gian hàng và trạng thái chờ duyệt | `AuthController.DangKy`, trang `Auth/DangKy` |
| Tài khoản | Đã có đăng nhập, đăng xuất, đổi mật khẩu; đã bổ sung quên mật khẩu xác minh tên đăng nhập + SĐT | `AuthController`, `HoSoController` |
| Hồ sơ quán | Đã có tên, địa chỉ, SĐT, email tài khoản, mô tả, hình ảnh | `HoSoController` |
| Kinh doanh | Đã có mở/đóng cửa, giờ mở/đóng, tự động nhận đơn, tạm dừng và lý do | `CauHinhGianHangController` |
| Danh mục | Đã có thêm, sửa, xóa, thứ tự, mô tả, ẩn/hiện | `DanhMucController` |
| Món ăn | Đã có thêm, sửa, xóa an toàn | `MonAnController` |
| Thông tin món | Đã có tên, ảnh, giá, mô tả và danh mục | `MonAnController`, `Views/MonAn` |
| Tình trạng món | Đã có còn hàng/hết hàng | Web và API vendor |
| Tùy chọn món | Đã bổ sung size, topping, mức cay, ghi chú riêng; lưu vào `ChiTietDonHang.TuyChonMon` | Script `04`, trang chi tiết món, API customer |
| Đơn hàng | Đã có đơn mới, đang xử lý, hoàn tất, hủy và chi tiết | `DonHangController` |
| Xử lý đơn | Đã có xác nhận, từ chối/hủy, chuẩn bị và sẵn sàng cho tài xế | `VendorOrderService`, Web/API |
| Tài xế | Đã có hàng chờ tài xế nhận và luồng giao hàng | `DriverDonHangController`, `DriverApiController` |
| Khuyến mãi | Đã có tạo, sửa, bật/tắt; xóa khi chưa được sử dụng | `KhuyenMaiController` |
| Voucher | Đã có mã, giảm %, giảm số tiền, điều kiện, thời gian và phạm vi món/cửa hàng | Entity `KhuyenMai`, form khuyến mãi |
| Doanh thu | Đã bổ sung bộ lọc hôm nay, tháng, năm và 30 ngày | `DoanhThuController` |
| Báo cáo | Đã có doanh thu theo ngày, đơn hoàn thành và món bán chạy | Trang `DoanhThu/Index` |
| Đánh giá | Đã bổ sung trang tổng hợp đánh giá cửa hàng và từng món | `DanhGiaGianHangController` |
| Thông báo | Đã bổ sung trang đơn mới, đơn hủy và đánh giá mới trong 30 ngày | `ThongBaoGianHangController` |
| Tài chính | Đã tách doanh thu không gồm phí giao hàng, phí nền tảng 10% và thực nhận | `DoanhThuController` |
| Chiến dịch quảng cáo | Đã có CRUD, trạng thái, vị trí, ngân sách và lượt tương tác | `ChienDichQuangCaoController`, API vendor/catalog |

## Ghi chú vận hành

- Phí nền tảng hiện được quy ước 10% doanh thu món trong phạm vi đồ án.
- Quên mật khẩu Web dùng tên đăng nhập và số điện thoại đăng ký. Khi triển khai thật nên thay bằng OTP SMS/email.
- Thông báo hiện được tổng hợp trực tiếp từ dữ liệu đơn hàng và đánh giá nên không cần bảng thông báo riêng.
- Sau khi cập nhật source hoặc chạy script schema, cần dừng và khởi động lại Web đang chạy để nạp DLL mới.
