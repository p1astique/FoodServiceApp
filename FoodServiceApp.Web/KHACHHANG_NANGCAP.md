# Nâng cấp module Khách hàng

Ngày: 01/10/2026

## Đã bổ sung

1. **Tìm kiếm gian hàng nâng cao**
   - Từ khóa theo tên gian hàng, tên món và địa chỉ.
   - Lọc theo khu vực.
   - Lọc theo mức sao tối thiểu.
   - Sắp xếp: phù hợp, đánh giá cao, tên A-Z, tên Z-A.
   - Phân trang 12 gian hàng/trang.
   - Tối ưu truy vấn đánh giá, bỏ cách tải từng gian hàng một lần (giảm N+1 query).

2. **Giỏ hàng an toàn hơn**
   - Giới hạn số lượng 1-50.
   - Khi cập nhật/checkout, kiểm tra lại món còn hàng.
   - Tự đồng bộ tên, giá và ảnh món từ CSDL.
   - Chặn đặt hàng khi gian hàng đã đóng/tạm ngưng.
   - Món không còn tồn tại/hết hàng được tự động loại khỏi giỏ.

3. **Checkout**
   - Giảm giá được giới hạn tối đa bằng tiền món trong giỏ.
   - Kiểm tra lại dữ liệu giỏ hàng trên server trước khi tạo đơn.
   - Chỉ cho phép phương thức đang hỗ trợ: Tiền mặt và VNPay.
   - MoMo hiển thị trạng thái sắp tích hợp, không gửi giá trị không hợp lệ lên server.

4. **Theo dõi đơn**
   - Trang chi tiết đơn đang xử lý tự tải lại mỗi 15 giây để khách dễ theo dõi trạng thái giao hàng.

## Các tệp chính đã sửa

- `Controllers/CustomerHomeController.cs`
- `Controllers/CustomerCartController.cs`
- `Services/CartService.cs`
- `Models/ViewModels/CustomerViewModels.cs`
- `Views/CustomerHome/Index.cshtml`
- `Views/CustomerCart/Checkout.cshtml`
- `Views/CustomerOrder/ChiTiet.cshtml`
- `wwwroot/css/site.css`

## Cách kiểm tra nhanh trên máy có .NET 8

```powershell
cd "FoodServiceApp.Web"
dotnet restore
dotnet build
dotnet run
```

Sau đó kiểm tra các luồng:

- `/MuaHang`
- Tìm `món ăn` + `khu vực` + `sao`.
- Mở gian hàng → thêm món → giỏ hàng → checkout.
- Đổi số lượng/giá hoặc trạng thái món trong DB rồi mở lại checkout để xác nhận giỏ được đồng bộ.
- Đặt đơn bằng tiền mặt/VNPay.
- Vào `/MuaHang/DonHang/{id}` để xem trạng thái và thử tự cập nhật.
