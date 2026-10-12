# FoodServiceApp hoàn chỉnh

Project gồm hai ứng dụng dùng chung SQL Server:

- `FoodServiceApp.Web`: ASP.NET Core MVC + Web API, chạy trên .NET 8.
- `Mobile_MAUI`: ứng dụng .NET MAUI cho khách hàng, gian hàng và quản trị.

MAUI gọi API của `FoodServiceApp.Web`, vì vậy khi chạy mobile cần mở Web trước và giữ cửa sổ Web đang chạy.

## Chức năng hiện có

### Khách hàng

- Đăng ký, đăng nhập, quản lý hồ sơ và địa chỉ giao hàng.
- Xem gian hàng đã duyệt, thực đơn đang bán, size, topping, mức cay và ghi chú món.
- Giỏ hàng theo một gian hàng, báo giá lại tại server và tính phí giao hàng qua Goong Maps hoặc mức phí dự phòng.
- Áp dụng voucher tại checkout; theo dõi đơn, lịch sử đơn và đánh giá gian hàng/món ăn.

### Gian hàng

- Đăng ký và chờ quản trị viên xét duyệt; tài khoản bị khóa hoặc chưa duyệt không được truy cập nghiệp vụ.
- Quản lý hồ sơ, trạng thái mở/đóng, giờ hoạt động, tạm dừng nhận đơn và thời gian chuẩn bị.
- CRUD danh mục, sắp xếp/ẩn danh mục; CRUD món, ảnh, giá, trạng thái còn hàng và tùy chọn món.
- Quản lý đơn theo luồng `ChoXacNhan → DaXacNhan → DangChuanBi → SanSangGiao → TaiXeDaNhan → DangGiao → DaGiao`.
- Từ chối/hủy đơn phải có lý do; chuyển trạng thái được ghi vào `NhatKyTrangThaiDon`.
- Tạo voucher giảm phần trăm, tiền cố định hoặc phí vận chuyển; giới hạn theo đơn tối thiểu, lượt dùng, khách hàng, món, danh mục và gian hàng.
- Quản lý chiến dịch quảng cáo, xem đánh giá, phản hồi đánh giá, thông báo và báo cáo doanh thu/phí nền tảng/thực nhận.

### Tài xế và quản trị

- Tài xế xem đơn sẵn sàng giao, nhận đơn có kiểm tra cạnh tranh, bắt đầu giao, hoàn thành hoặc báo giao thất bại.
- Quản trị duyệt/từ chối/khóa gian hàng, quản lý khách hàng, theo dõi thống kê và xử lý sự cố giao hàng.

## Cách hệ thống hoạt động

- Razor MVC dùng Cookie Authentication; API dành cho MAUI dùng JWT và kiểm tra role ở backend.
- Web và MAUI cùng đọc/ghi SQL Server thông qua `FoodServiceApp.Web`; MAUI không dùng dữ liệu local làm nguồn nghiệp vụ chính.
- Khi checkout, server tải lại giá và trạng thái món, kiểm tra gian hàng, tính phí giao hàng và voucher rồi mới tạo đơn.
- `ChiTietDonHang` lưu snapshot tên món, đơn giá và tùy chọn tại thời điểm đặt.
- Voucher và tạo đơn chạy trong transaction để hạn chế vượt lượt hoặc ghi nhận trùng.
- Gian hàng chỉ được thao tác dữ liệu có `MaGianHang` thuộc tài khoản hiện tại; tài xế chỉ cập nhật đơn đã nhận.

## 1. Yêu cầu môi trường

- Windows 10/11.
- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0). Project đang ghim SDK `8.0.425` trong `global.json`.
- SQL Server LocalDB hoặc SQL Server Express.
- Visual Studio 2022 với các workload:
  - ASP.NET and web development.
  - .NET Multi-platform App UI development.
- Nếu chạy Android: Android SDK và Android Emulator từ Visual Studio Installer.

Kiểm tra môi trường bằng PowerShell:

```powershell
dotnet --version
dotnet workload list
sqllocaldb info MSSQLLocalDB
```

Nếu chưa có workload MAUI:

```powershell
dotnet workload install maui
```

## 2. Chuẩn bị database

Connection string mặc định nằm trong `FoodServiceApp.Web/appsettings.json`:

```text
Server=(localdb)\MSSQLLocalDB;Database=QuanLyDichVuAnUong;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;
```

`SQL/Script.sql` là file SQL chính, chứa toàn bộ lệnh tạo database, schema, quan hệ và dữ liệu mẫu. Chỉ cần chạy file này khi thiết lập project trên máy mới.

### Khởi tạo toàn bộ database

Mở `SQL/Script.sql` bằng SQL Server Management Studio và chọn **Execute**, hoặc chạy:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -i "SQL\Script.sql"
```

> `Script.sql` xóa database `QuanLyDichVuAnUong` nếu đã tồn tại rồi tạo lại từ đầu. Hãy sao lưu dữ liệu trước khi chạy trên database đang sử dụng.

`SQL/01_MotFileDuyNhat_TaoMoi.sql` được giữ làm bản nguồn tương đương. Hai file phải luôn có schema giống nhau.

### Nâng cấp database cũ và giữ dữ liệu

Không chạy `Script.sql` nếu cần giữ dữ liệu. Chạy các script nâng cấp chuyên biệt theo thứ tự:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -i "SQL\02_NangCap_CSDL_DBCu.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -i "SQL\03_BoSung_CuaHang_QuangCao_DanhMuc.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -i "SQL\04_BoSung_TuyChonMon.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -i "SQL\05_NangCap_KhuyenMai.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -i "SQL\06_NangCap_NghiepVu_DoiTac.sql"
```

Script `03` bổ sung cấu hình vận hành cửa hàng, chiến dịch quảng cáo và thuộc tính danh mục. Script `04` bổ sung size, topping, mức cay và lựa chọn được lưu theo chi tiết đơn. Các script nâng cấp có thể chạy lại mà không xóa dữ liệu.

Script `05` bổ sung voucher theo gian hàng/hệ thống, điều kiện áp dụng, giới hạn lượt và lịch sử sử dụng. Script `06` bổ sung snapshot tên món, lý do hủy, phản hồi đánh giá, trạng thái chuẩn bị/bàn giao và timeline đơn hàng.

Nếu dùng SQL Server Express, sửa `ConnectionStrings:DefaultConnection` thành ví dụ sau:

```text
Server=.\SQLEXPRESS;Database=QuanLyDichVuAnUong;Trusted_Connection=True;TrustServerCertificate=True;
```

## 3. Chạy Web và Web API

Tại thư mục gốc project, chạy:

```powershell
dotnet restore FoodServiceApp.Web\FoodServiceApp.Web.csproj
dotnet run --project FoodServiceApp.Web\FoodServiceApp.Web.csproj
```

Sau khi thấy dòng `Now listening on`, mở:

- Website: <https://localhost:5001>
- Website HTTP: <http://localhost:5000>
- Swagger API: <https://localhost:5001/swagger>

Nếu trình duyệt cảnh báo chứng chỉ HTTPS, chạy một lần:

```powershell
dotnet dev-certs https --trust
```

Sau đó đóng và chạy lại Web.

### Chạy Web bằng Visual Studio

1. Mở `FoodServiceApp.Web/FoodServiceApp.Web.csproj`.
2. Chọn profile **FoodServiceApp.Web**.
3. Nhấn `F5` hoặc `Ctrl+F5`.
4. Giữ Web đang chạy trong lúc sử dụng MAUI.

## 4. Chạy MAUI trên Windows

Mở một cửa sổ PowerShell thứ hai. Web phải đang chạy ở cửa sổ thứ nhất.

```powershell
dotnet restore Mobile_MAUI\FoodServiceApp.Maui.csproj
dotnet build Mobile_MAUI\FoodServiceApp.Maui.csproj -t:Run -f net8.0-windows10.0.19041.0
```

Trên Windows, MAUI mặc định gọi:

```text
https://localhost:5001/
```

Nếu app báo không kết nối được API:

1. Kiểm tra <https://localhost:5001/swagger> có mở được không.
2. Chạy `dotnet dev-certs https --trust`.
3. Tại màn hình đăng nhập MAUI, chọn **Cấu hình máy chủ** và nhập `https://localhost:5001/`.
4. Đăng nhập lại vì thay địa chỉ server sẽ xóa token cũ.

### Chạy MAUI Windows bằng Visual Studio

1. Mở `Mobile_MAUI/FoodServiceApp.Maui.csproj`.
2. Chọn target **Windows Machine**.
3. Nhấn `F5`.

## 5. Chạy MAUI trên Android Emulator

Khởi động Android Emulator trước, sau đó chạy:

```powershell
dotnet build Mobile_MAUI\FoodServiceApp.Maui.csproj -t:Run -f net8.0-android
```

Android Emulator không dùng `localhost` của máy Windows. App đã cấu hình mặc định:

```text
http://10.0.2.2:5000/
```

Đây là địa chỉ đặc biệt để emulator truy cập cổng `5000` trên máy phát triển. Web phải đang chạy bằng profile hiện tại với URL `http://localhost:5000`.

Trong Visual Studio:

1. Mở `Mobile_MAUI/FoodServiceApp.Maui.csproj`.
2. Chọn một Android Emulator trong danh sách thiết bị.
3. Nhấn `F5`.

## 6. Chạy MAUI trên điện thoại Android thật

Điện thoại và máy tính phải dùng cùng mạng Wi-Fi.

1. Xem IPv4 của máy tính:

   ```powershell
   ipconfig
   ```

2. Chạy Web để nhận kết nối từ mạng LAN:

   ```powershell
   dotnet run --project FoodServiceApp.Web\FoodServiceApp.Web.csproj --urls "http://0.0.0.0:5000"
   ```

3. Cho phép cổng `5000` qua Windows Firewall nếu Windows hỏi.
4. Trong màn hình đăng nhập MAUI, chọn **Cấu hình máy chủ**.
5. Nhập địa chỉ theo IPv4 của máy tính, ví dụ:

   ```text
   http://192.168.1.20:5000/
   ```

Không nhập `localhost` hoặc `10.0.2.2` khi chạy trên điện thoại thật.

## 7. Tài khoản mẫu

Các tài khoản sau có khi database được tạo bằng script `01`:

| Vai trò | Tên đăng nhập | Mật khẩu |
|---|---|---|
| Khách hàng | `khachhang01` | `123456` |
| Gian hàng | `gianhang01` | `123456` |
| Quản trị | `quantri01` | `admin123` |
| Tài xế | `taixe01` | `123456` |

Các tài khoản đánh số tiếp theo như `gianhang02`, `khachhang02` và `taixe02` cũng dùng mật khẩu `123456`.

Địa chỉ đăng nhập Web:

- Khách hàng: <https://localhost:5001/MuaHang/Auth/Login>
- Gian hàng: <https://localhost:5001/Auth/Login>
- Quản trị: <https://localhost:5001/AdminAuth/Login>
- Tài xế: <https://localhost:5001/DriverAuth/Login>

## 8. Thứ tự chạy nhanh để demo

Terminal 1:

```powershell
dotnet run --project FoodServiceApp.Web\FoodServiceApp.Web.csproj
```

Terminal 2, chọn một nền tảng:

```powershell
# Windows
dotnet build Mobile_MAUI\FoodServiceApp.Maui.csproj -t:Run -f net8.0-windows10.0.19041.0

# Hoặc Android Emulator
dotnet build Mobile_MAUI\FoodServiceApp.Maui.csproj -t:Run -f net8.0-android
```

## 9. Build và kiểm thử

```powershell
dotnet build FoodServiceApp.Web\FoodServiceApp.Web.csproj
dotnet build Mobile_MAUI\FoodServiceApp.Maui.csproj -f net8.0-windows10.0.19041.0
dotnet build Mobile_MAUI\FoodServiceApp.Maui.csproj -f net8.0-android
powershell -ExecutionPolicy Bypass -File tests\vendor-integration.ps1
```

Bộ kiểm thử tích hợp tạo database tạm có tên ngẫu nhiên và tự xóa sau khi chạy; dữ liệu trong `QuanLyDichVuAnUong` không bị thay đổi.

## 10. Lỗi thường gặp

### `Không kết nối được API`

- Web chưa chạy hoặc sai cổng.
- Windows MAUI cần `https://localhost:5001/`.
- Android Emulator cần `http://10.0.2.2:5000/`.
- Điện thoại thật cần `http://<IPv4-của-máy>:5000/`.
- Firewall đang chặn cổng `5000`.

### `Login failed`, `Cannot open database` hoặc lỗi LocalDB

Kiểm tra LocalDB:

```powershell
sqllocaldb start MSSQLLocalDB
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -Q "SELECT DB_NAME()"
```

Kiểm tra lại connection string và tên database `QuanLyDichVuAnUong`.

### `Invalid column name` hoặc `Invalid object name`

Database cũ chưa được nâng cấp. Chạy lại script `02`, `03` và `04` theo mục 2.

### MAUI báo thiếu workload

```powershell
dotnet workload restore Mobile_MAUI\FoodServiceApp.Maui.csproj
```

Sau đó khởi động lại Visual Studio hoặc terminal.

## 11. Cấu hình tích hợp ngoài

Trong `FoodServiceApp.Web/appsettings.json`:

- `GoongMaps:ApiKey`: dùng tính khoảng cách và phí giao hàng. Khi chưa cấu hình, hệ thống dùng phí mặc định.
- `VnPay:TmnCode` và `VnPay:HashSecret`: cần điền thông tin sandbox/thật để thanh toán VNPay hoạt động.
- `Jwt:Key`: phải đổi sang khóa bí mật riêng trước khi triển khai thật.

Tài liệu bàn giao chi tiết của phần gian hàng nằm tại `docs/NHAT_BAN_GIAO.md`.
