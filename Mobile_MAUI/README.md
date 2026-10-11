# Food Service App — bản chuyển sang C# / .NET MAUI

Bản chuyển đổi từ project Flutter sang **C# / .NET MAUI**, giữ nguyên toàn bộ
luồng màn hình phía **Khách hàng** đã có: Đăng nhập/Đăng ký → Trang chủ →
Thực đơn & Giỏ hàng → Thanh toán (kèm áp mã khuyến mãi) → Theo dõi đơn hàng
→ Đánh giá, cộng thêm 3 tab Hoạt động / Ví tiền / Tin nhắn và trang Tài khoản.

Đã bổ sung luồng **Gian hàng (đối tác)** với các trang: đăng ký tham gia nền
tảng (`VendorRegisterPage`, hồ sơ ở trạng thái chờ duyệt), đăng nhập
(`VendorLoginPage`), trang chủ/dashboard (`VendorHomePage` — bật/tắt tình
trạng kinh doanh, xem nhanh doanh thu và đơn chờ xác nhận), quản lý đơn hàng
đầy đủ vòng đời tiếp nhận → xác nhận/từ chối → chuẩn bị → bàn giao tài xế
(`VendorOrdersPage`), quản lý thực đơn — thêm/sửa/xóa món, bật tắt tình trạng
còn/hết hàng (`VendorMenuPage`, `VendorMenuItemEditPage`), theo dõi doanh thu
và món bán chạy (`VendorRevenuePage`), và hồ sơ cửa hàng (`VendorProfilePage`).
Vào từ trang đăng nhập khách hàng qua liên kết "🏪 Bạn là gian hàng đối tác?".

Đã bổ sung luồng **Quản lý quản trị** với các trang: đăng nhập quản trị viên
(`AdminLoginPage`), trang chủ tổng quan hệ thống — tổng gian hàng, tổng khách
hàng, tổng doanh thu (`AdminHomePage`), duyệt/từ chối hồ sơ gian hàng mới đăng
ký (`AdminVendorApprovalPage`), danh sách toàn bộ gian hàng kèm ngày tham gia,
doanh thu ước tính theo từng đối tác và khóa/mở tài khoản
(`AdminVendorListPage`), quản lý tài khoản khách hàng — khóa/mở
(`AdminCustomerListPage`), và tiếp nhận/xử lý các vấn đề, khiếu nại phát sinh
trong vận hành (`AdminIssuesPage`). Vào từ trang đăng nhập khách hàng qua liên
kết "🛡️ Quản trị viên hệ thống".

Đã bổ sung **module Quản lý khuyến mãi** — chương trình khuyến mãi/mã giảm
giá/voucher (`KhuyenMai`) có thể do gian hàng tự tạo (chỉ áp dụng cho gian
hàng đó) hoặc do quản trị viên tạo áp dụng **toàn hệ thống**:
- Phía gian hàng: `VendorPromotionsPage` (danh sách, bật/tắt, xóa) +
  `VendorPromotionEditPage` (tạo/sửa mã — giảm theo % hoặc số tiền cố định,
  đơn tối thiểu, giới hạn lượt dùng, thời hạn).
- Phía quản trị: `AdminPromotionsPage` (giám sát toàn bộ mã trên nền tảng,
  lọc theo phạm vi, bật/tắt/xóa) + `AdminPromotionEditPage` (tạo mã áp dụng
  toàn hệ thống).
- Phía khách hàng: `PaymentPage` giờ gọi thẳng `MockData.ApDungMaKhuyenMai(...)`
  — kiểm tra mã tồn tại, còn hạn, còn lượt, đúng phạm vi gian hàng, đủ điều
  kiện đơn tối thiểu — thay cho logic giả lập mã "GIAM10" cứng trước đây.

**Còn thiếu để hoàn thiện toàn bộ đề cương:** module Thống kê & báo cáo cấp hệ
thống (doanh thu/đơn hàng/món bán chạy/hiệu quả khuyến mãi tổng hợp toàn nền
tảng theo ngày/tháng/năm — khác với báo cáo doanh thu riêng từng gian hàng đã
có trong `VendorRevenuePage`).

## Vì sao không nộp sẵn file .csproj/.sln đầy đủ?

Sandbox tạo các file này không có .NET SDK và không truy cập được NuGet để
tải khung project MAUI chuẩn (Platforms/Android, Platforms/iOS, Info.plist,
AndroidManifest.xml...). Những file đó do `dotnet new maui` sinh ra và phụ
thuộc đúng phiên bản SDK bạn cài — viết tay dễ bị lỗi/lỗi thời. Vì vậy cách
an toàn nhất là bạn tự tạo khung project rỗng rồi copy các file **mã nguồn
dùng chung** (Models, Views, App, AppShell) đè vào.

## Các bước tích hợp

1. Cài .NET SDK 8 + workload MAUI (Visual Studio 2022 có sẵn, hoặc chạy
   `dotnet workload install maui`).
2. Tạo project rỗng:
   ```bash
   dotnet new maui -n FoodServiceApp.Maui
   cd FoodServiceApp.Maui
   ```
3. Copy đè các thư mục/file trong gói này vào project vừa tạo:
   - `Models/Models.cs`
   - `Converters/TrangThaiToTextConverter.cs`
   - `Resources/Styles/Colors.xaml`, `Resources/Styles/Styles.xaml`
   - `Views/*.xaml` và `*.xaml.cs` (10 trang)
   - `App.xaml`, `App.xaml.cs`, `AppShell.xaml`, `AppShell.xaml.cs`,
     `MauiProgram.cs` (đè lên file mặc định `dotnet new maui` đã tạo)
4. Thêm 5 icon cho tab bar vào `Resources/Images/` (đặt tên đúng
   `tab_home.png`, `tab_orders.png`, `tab_activity.png`, `tab_wallet.png`,
   `tab_messages.png`) — có thể dùng icon tạm từ Google Fonts Icons trong
   lúc chờ thiết kế chính thức.
5. Build & chạy:
   ```bash
   dotnet build -t:Run -f net8.0-android
   ```

## Trạng thái hiện tại

Toàn bộ dữ liệu vẫn là **mock data** trong `Models/Models.cs` — các điểm cần
nối API thật (ASP.NET Core Web API + SQL Server) đã đánh dấu `// TODO` trong
code (`/api/khach-hang/...`, `/api/don-hang`, `/api/thanh-toan`,
`/api/danh-gia`, `/api/gian-hang/...`, `/api/quan-tri/...`,
`/api/khuyen-mai/...`).

Đã bổ sung **module Thống kê & báo cáo toàn hệ thống** (`AdminStatisticsPage`,
vào từ trang chủ quản trị "📊 Thống kê & báo cáo"): lọc theo khoảng thời gian
(Hôm nay / 7 ngày / 30 ngày / Tất cả), tổng doanh thu — tổng đơn hoàn thành —
giá trị đơn trung bình — tỉ lệ đơn bị hủy, doanh số xếp hạng theo từng gian
hàng (dạng thanh tiến trình), top 5 món ăn bán chạy toàn nền tảng, và hiệu
quả từng chương trình khuyến mãi (số lượt đã dùng/giới hạn). Toàn bộ tính
trực tiếp từ `MockData.TatCaDonHangHeThong` (gộp mọi nguồn đơn hàng mock hiện
có) — ghi rõ TODO thay bằng API thống kê thật khi có backend + CSDL.

**Vậy là đã có đủ giao diện cho 6/6 nhóm yêu cầu chức năng trong đề cương**
(Mua hàng, Gian hàng, Đơn hàng & giao hàng, Khuyến mãi, Quản trị, Thống kê).

## Giới hạn còn lại (thuộc tầng hạ tầng, không phải thiếu chức năng)

- Toàn bộ vẫn chạy trên **dữ liệu mock**, chưa có API ASP.NET Core + CSDL
  SQL Server thật như đề cương yêu cầu (mục 7).
- Chưa build/chạy thử được trong môi trường tạo project này (không có
  .NET MAUI workload) — mới kiểm tra cú pháp C#/XAML bằng mắt và validate
  XML của toàn bộ file `.xaml`.
- Chưa làm phần **giao diện Website** (đề cương yêu cầu cả Website lẫn
  Mobile ở mục 6) — hiện chỉ có bản Mobile (.NET MAUI).
- Kết nối API thật, lưu token đăng nhập (nên dùng `SecureStorage` có sẵn
  trong MAUI thay vì tự viết).
- Luồng khuyến mãi chưa gắn `SoLuongDaDung` tăng tự động khi đơn hàng đặt
  thành công (hiện chỉ tính trên số liệu mock có sẵn) — cần nối khi có API.
