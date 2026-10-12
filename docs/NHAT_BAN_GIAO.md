# Bàn giao module Gian hàng và Đối tác

Ngày cập nhật: 12/10/2026. Phạm vi: các chức năng và tài liệu thuộc phần phân công của Nguyễn Hoàng Minh Nhật trong tracker 12 tuần.

## Kết quả triển khai

Module đối tác trên MAUI đã chuyển từ dữ liệu mock sang Web API và SQL Server: đăng ký, đăng nhập, hồ sơ, mở/đóng cửa, danh mục, món, tình trạng còn hàng, đơn hàng, khuyến mãi và thống kê từ đơn thật. Token từng vai trò được lưu bằng SecureStorage, không lưu mật khẩu. Lỗi mạng hoặc xác thực hiển thị thông báo và không thay bằng dữ liệu demo.

Web bổ sung trang `/DanhMuc/Index` để thêm, đổi tên, xóa danh mục; kiểm tra quyền sở hữu danh mục khi thêm/sửa món. API quản trị duyệt/khóa gian hàng yêu cầu JWT vai trò QuanTri. API kiểm tra trạng thái tài khoản trong Database mỗi request; gian hàng chưa duyệt hoặc bị từ chối không được vào nghiệp vụ. Web kiểm tra lại trạng thái duyệt/khóa thay vì chỉ dựa trên claim cũ.

Phần phối hợp với khách hàng đã có đăng ký/đăng nhập thật, đọc gian hàng/món từ Database, tính tiền tại server, đặt đơn tiền mặt và theo dõi trạng thái cùng đơn đó. Server kiểm tra giá, món còn hàng, gian hàng đang mở và khuyến mãi trước khi lưu. Khách phải xác nhận tổng tiền; nếu giá/phí thay đổi, API trả 409 để tính lại.

Gian hàng chỉ xác nhận hoặc hủy đơn chờ xử lý; trạng thái đang giao do tài xế nhận đơn. Hủy đơn đã thanh toán giữ nguyên trạng thái thanh toán để đối soát, không tự ghi “đã hoàn tiền”. Báo cáo hiện dùng tổng giá trị đơn đã giao, bao gồm phí giao hàng như quy ước sẵn có của web; đây không phải lợi nhuận ròng của cửa hàng.

## Chạy với Database hiện có

Không cần xóa Database hoặc chạy lại script tạo mới. Các thay đổi dùng schema hiện tại, không thêm cột.

Tại thư mục gốc project:

```powershell
dotnet run --project FoodServiceApp.Web/FoodServiceApp.Web.csproj
```

MAUI Windows: mở project `Mobile_MAUI/FoodServiceApp.Maui.csproj`, chọn Windows Machine. Màn hình đăng nhập đối tác/quản trị có nút **Cấu hình máy chủ**, mặc định `https://localhost:5001/`. Chứng chỉ HTTPS trên máy phải được tin cậy. Nếu cần thiết lập chứng chỉ phát triển, chạy `dotnet dev-certs https --trust` trong terminal của người dùng.

Đăng nhập bằng **tên đăng nhập và mật khẩu trong Database**, không dùng tài khoản mock. Hồ sơ đăng ký đối tác từ mobile được lưu trạng thái chờ duyệt và xuất hiện ở màn hình quản trị. Khách hàng có thể tạo tài khoản thật ngay trên mobile.

Với Android Emulator, địa chỉ mặc định là `http://10.0.2.2:5000/`. Chạy server chỉ HTTP trong phiên phát triển để tránh chuyển hướng sang chứng chỉ localhost mà emulator không tin cậy:

```powershell
dotnet run --project FoodServiceApp.Web/FoodServiceApp.Web.csproj --no-launch-profile -- --urls http://localhost:5000 --environment Development
```

Network policy Android chỉ cho HTTP tới các địa chỉ loopback/emulator nêu trên; thiết bị thật hoặc máy chủ LAN cần URL HTTPS với chứng chỉ hợp lệ và cấu hình qua nút Cấu hình máy chủ. Không tắt kiểm tra chứng chỉ. Chưa kiểm thử thao tác trên thiết bị thật.

Ở thời điểm đối chiếu, Database người dùng có một gian hàng đã duyệt nhưng chưa có quản trị. Đã bổ sung lệnh tạo quản trị đầu tiên, chạy tương tác trên máy chủ:

```powershell
dotnet run --project FoodServiceApp.Web/FoodServiceApp.Web.csproj -- --bootstrap-admin
```

Lệnh yêu cầu nhập tên đăng nhập, mật khẩu và xác nhận; mật khẩu không hiện trên màn hình và lưu bằng BCrypt. Lệnh từ chối nếu Database đã có tài khoản quản trị. Tính năng này chưa được chạy lên Database người dùng trong quá trình sửa mã.

Biến `FOOD_DB_CONNECTION` có thể chỉ định Database khác. Nếu biến này được đặt mà kết nối lỗi, ứng dụng dừng thay vì âm thầm chuyển sang Database khác. Khi chạy test, luôn dùng Database riêng.

## API đối tác

Các route `/api/vendor/*` yêu cầu `Authorization: Bearer <token>` vai trò GianHang và gian hàng đã duyệt, ngoại trừ đăng ký. `GET` trả JSON, cập nhật/xóa phần quản lý thường trả 204. Dữ liệu không hợp lệ trả 400; chưa đăng nhập/khóa trả 401; sai quyền/chưa duyệt trả 403; bản ghi không thuộc gian hàng trả 404 hoặc lỗi danh mục 400; trạng thái xung đột trả 409.

| Method | Route | Nội dung |
|---|---|---|
| POST | `/api/vendor/register` | Tạo tài khoản và gian hàng chờ duyệt trong transaction |
| POST | `/api/auth/login/GianHang` | Đăng nhập lấy JWT |
| GET, PUT | `/api/vendor/profile` | Đọc, lưu hồ sơ và trạng thái mở bán |
| PUT | `/api/vendor/business-status` | Đặt trạng thái mở bán theo giá trị, tránh toggle lặp |
| GET | `/api/vendor/home` | Thống kê hôm nay |
| GET, POST | `/api/vendor/categories` | Danh sách, tạo danh mục |
| PUT, DELETE | `/api/vendor/categories/{id}` | Đổi tên, xóa danh mục trống |
| GET, POST | `/api/vendor/items` | Danh sách, tạo món |
| PUT, DELETE | `/api/vendor/items/{id}` | Sửa/xóa món; món có đơn hoặc khuyến mãi phải chuyển hết hàng |
| PUT | `/api/vendor/items/{id}/availability` | Còn/hết hàng |
| GET | `/api/vendor/menu` | Endpoint danh sách món cũ, giữ tương thích |
| GET | `/api/vendor/orders?trangThai=...` | Đơn hàng thuộc gian hàng, gồm chi tiết và phí giao |
| POST | `/api/vendor/orders/{id}/status` | Xác nhận hoặc hủy hợp lệ |
| GET, POST | `/api/vendor/promotions` | Danh sách, tạo khuyến mãi |
| PUT, DELETE | `/api/vendor/promotions/{id}` | Sửa/xóa; mã từng dùng chỉ được tắt |
| PUT | `/api/vendor/promotions/{id}/status` | Bật/tắt mã |

Route cũ `POST /api/vendor/toggle-mo-cua` vẫn tồn tại cho client cũ; MAUI mới dùng PUT theo giá trị.

Ví dụ đăng nhập:

```json
{"tenDangNhap":"ten_dang_nhap_cua_ban","matKhau":"mat_khau_cua_ban"}
```

Response có `token`, `vaiTro`, `maTK`, `hoTenHienThi`. Không đưa token hoặc mật khẩu thật vào Git.

Ví dụ tạo/sửa món (ID danh mục lấy từ GET categories):

```json
{"maDanhMuc":1,"tenMon":"Cơm sườn","gia":50000,"moTa":"Cơm và sườn nướng","hinhAnh":"/uploads/monan/anh.png","conHang":true}
```

Ví dụ cập nhật đơn:

```json
{"trangThaiMoi":"DaXacNhan"}
```

Ví dụ khuyến mãi:

```json
{"maCode":"COM10","moTa":"Giảm 10 phần trăm","maMonAn":null,"laPhanTram":true,"giaTriGiam":10,"ngayBatDau":"2026-10-12T00:00:00","ngayKetThuc":"2026-10-31T23:59:59","trangThai":true}
```

Mã chỉ gồm chữ, số, `_`, `-`, tối đa 30 ký tự; không đổi mã đã tạo. `maMonAn` rỗng nghĩa là áp dụng cả đơn, có ID nghĩa là chỉ áp dụng món thuộc gian hàng. Form mobile đã khớp schema: giảm theo phần trăm/số tiền, món áp dụng, ngày và trạng thái; không còn các trường giới hạn lượt/trần giảm demo mà backend không lưu.

## API phối hợp khách hàng và quản trị

| Method | Route | Quyền |
|---|---|---|
| GET | `/api/catalog/stores` | Công khai, chỉ gian hàng đã duyệt/đang mở/tài khoản hoạt động |
| GET | `/api/catalog/stores/{id}/items` | Công khai, chỉ món còn hàng |
| POST | `/api/customer/register` | Công khai |
| POST | `/api/auth/login/KhachHang` | Công khai |
| POST | `/api/customer/quote` | KhachHang; tính tiền theo Database |
| POST | `/api/customer/orders` | KhachHang; đặt đơn tiền mặt |
| GET | `/api/customer/orders` | KhachHang; chỉ đơn của khách đăng nhập |
| GET | `/api/admin/vendors/pending`, `/by-status` | QuanTri |
| POST | `/api/admin/vendors/{id}/approve`, `/reject`, `/toggle-lock` | QuanTri |

Body quote gồm `maGianHang`, `diaChiGiaoHang`, `ghiChu`, `maKhuyenMai`, `items: [{maMonAn, soLuong}]`. Quote trả `tamTinh`, `giamGia`, `phiGiaoHang`, `tongTien`. Khi đặt, gửi thêm `tongTienXacNhan` bằng số tiền đã xác nhận. Server không nhận giá món từ client làm giá chính thức.

## Kiểm thử và bằng chứng

Kết quả cuối ngày 12/10/2026:

| Kiểm tra | Kết quả |
|---|---|
| Web net8.0 | Build thành công, 0 lỗi, 0 cảnh báo |
| MAUI Windows net8.0-windows10.0.19041.0 | Build thành công, 0 lỗi, 0 cảnh báo |
| MAUI Android net8.0-android | Build thành công, 0 lỗi, 0 cảnh báo |
| API + SQL Server trên Database test riêng | 67 kiểm tra qua |
| Thao tác UI trên thiết bị thật | Chưa kiểm thử |

Script: `tests/vendor-integration.ps1`. Kết quả lần chạy mới nhất: `tests/vendor-test-results.txt`.

```powershell
dotnet build FoodServiceApp.Web/FoodServiceApp.Web.csproj --no-restore
powershell -NoProfile -ExecutionPolicy Bypass -File tests/vendor-integration.ps1
```

Script tạo Database `FoodServiceVendorTest_<GUID>` trên LocalDB, khởi động Web API ở cổng 5188, chạy kiểm tra và dừng tiến trình/dọn Database test trong finally. Dữ liệu test bao gồm hai gian hàng, quản trị, khách và tài xế. Không dùng Database `QuanLyDichVuAnUong` để viết fixtures. Không chạy đồng thời nhiều bản script ở cùng cổng.

67 kiểm tra đã qua: đăng ký/duyệt; 401/403; hồ sơ lưu thật; sở hữu danh mục/món/đơn; cập nhật còn hàng phản ánh trong catalog; giới hạn giảm phần trăm; giá và khuyến mãi do server tính; từ chối tổng tiền sai; khách đặt đơn xuất hiện ở gian hàng; xác nhận phản ánh phía khách; tài xế nhận/giao thành công; gian hàng không được hủy sau khi tài xế nhận; doanh thu loại đơn hủy; không ghi hoàn tiền giả; khóa/từ chối có hiệu lực trên token đã cấp.

Các test này kiểm tra HTTP API và SQL Server, không thay thế kiểm thử thao tác thực tế trên màn hình hoặc thiết bị Android. Xem log build mới nhất để biết kết quả từng nền tảng.

## Kịch bản demo

1. Khởi động web, cấu hình địa chỉ API trong mobile, chuẩn bị tài khoản quản trị nếu Database chưa có.
2. Đăng ký đối tác trên mobile, chứng minh hồ sơ xuất hiện trong danh sách chờ duyệt; đăng nhập trước duyệt bị chặn.
3. Quản trị duyệt; đối tác đăng nhập, sửa hồ sơ, tạo danh mục và hai món. Mở web cùng gian hàng để xác nhận dữ liệu đã lưu.
4. Tạo mã giảm 10%, còn hạn. Khách đăng ký/đăng nhập, thấy món và đặt hai phần bằng tiền mặt sau khi xác nhận tổng tiền.
5. Đối tác tải lại đơn, xác nhận; khách tải lại trang theo dõi để thấy trạng thái mới.
6. Tài xế nhận và giao bằng web hoặc API hiện có. Đối tác mở báo cáo, đối chiếu đơn đã giao; khách thấy hoàn thành.
7. Tạo đơn thứ hai rồi hủy; chứng minh đơn hủy không được tính vào thống kê hoàn thành.
8. Thử khóa tài khoản đối tác từ quản trị, gọi lại màn hình nghiệp vụ và chứng minh server chặn.

## Phần cần thành viên xác nhận ngoài mã nguồn

Tài liệu thiết kế được bổ sung tại `docs/NHAT_THIET_KE.md`. Biên bản khảo sát thật, người tham gia khảo sát, ảnh chụp thao tác demo, nhận xét GVHD, số buổi tham gia và chữ ký người kiểm tra phải do nhóm cung cấp/xác nhận; không thể suy ra từ source code. Không tự đánh dấu toàn bộ tracker DONE.

Mobile tài xế, các màn hình quản trị ngoài phạm vi gian hàng và các tab demo khác chưa được chuyển toàn bộ sang API trong đợt này. Kịch bản tích hợp dùng API/web tài xế. Thanh toán điện tử và hoàn tiền tự động không nằm trong luồng đặt tiền mặt mới; không tuyên bố đã nghiệm thu cổng thanh toán. Kiểm thử tải, cạnh tranh nhiều thiết bị và thao tác UI thực tế cần được thực hiện trước triển khai công khai.
