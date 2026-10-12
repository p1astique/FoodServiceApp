> Cập nhật sau triển khai ngày 12/10/2026: các nhận xét bên dưới là ảnh chụp hiện trạng trước khi sửa. Đã bổ sung API, tích hợp MAUI, quản lý danh mục web, phân quyền, đặc tả và kiểm thử. Xem [bản bàn giao hiện tại](docs/NHAT_BAN_GIAO.md), [đặc tả](docs/NHAT_THIET_KE.md) và [kết quả test](tests/vendor-test-results.txt). Không dùng các mục “chưa làm” bên dưới để đánh giá bản mã đã cập nhật.
# Đối chiếu công việc của Nhật với FoodServiceApp_HoanChinh

Ngày kiểm tra: 12/10/2026. Thành viên: Nguyễn Hoàng Minh Nhật. Phạm vi phân công: Gian hàng / Đối tác, 12 dòng tương ứng tuần 1–12 trong `Master_Tracker_Do_An_12_Tuan_Nhom_3_Nguoi (1).docx`.

**Kết luận:** project đã có phần lớn chức năng web của đối tác và kết nối được SQL Server. API đối tác mới có một phần; mobile đối tác có giao diện và xử lý demo nhưng chưa nối các nghiệp vụ chính vào API. Cần bổ sung tích hợp, kiểm thử và bằng chứng bàn giao trước khi xác nhận hoàn thành toàn bộ phần của Nhật.

Đánh giá này xác định chức năng thuộc phạm vi Nhật đã hiện diện trong project; không khẳng định mọi dòng mã đều do Nhật tự viết. Lịch sử Git kiểm tra được chỉ có commit `1b329a0` của `MinhNhat`, nội dung nhập toàn bộ source ban đầu, chưa đủ tách đóng góp theo từng tuần. Tài liệu khảo sát hoặc thiết kế nằm ngoài thư mục project có thể đã tồn tại nhưng chưa được cung cấp.

## 1. Kết quả kiểm tra Database và build

Đã kết nối trực tiếp bằng Windows Authentication đến `(localdb)\MSSQLLocalDB`, Database `QuanLyDichVuAnUong`, đúng connection string mặc định trong `FoodServiceApp.Web/appsettings.json`. Kiểm tra chỉ đọc cấu trúc và số lượng bản ghi; không thêm, sửa hoặc xóa dữ liệu.

| Nội dung | Kết quả tại thời điểm kiểm tra |
|---|---|
| Số bảng người dùng | 17 |
| Tài khoản | 1 |
| Gian hàng | 1, trạng thái `DaDuyet`, `DangMo` |
| Danh mục, món ăn | 0, 0 |
| Đơn hàng, chi tiết đơn, thanh toán | 0, 0, 0 |
| Khuyến mãi | 0 |
| Khách hàng, quản trị, tài xế | 0, 0, 0 |
| Các bảng còn lại | Chưa có bản ghi |
| Cột mở rộng đã hiện diện | `GianHang.TrangThaiDuyet`, `NgayDangKy`; `DonHang.PhiGiaoHang`, `MaKhuyenMaiApDung`; `KhuyenMai.MaMonAn` |
| Build web | Thành công, 0 lỗi, 1 cảnh báo CS1998 ở `Controllers/Api/PaymentController.cs:42` |
| Build MAUI Windows | Thành công, 0 lỗi, 1135 cảnh báo; đầu ra có CA1416 và CA1418 về nền tảng |

Lệnh đã chạy:

```powershell
dotnet build FoodServiceApp.Web/FoodServiceApp.Web.csproj --no-restore --nologo -v minimal
dotnet build Mobile_MAUI/FoodServiceApp.Maui.csproj -f net8.0-windows10.0.19041.0 --no-restore --nologo -v minimal
```

Build thành công không thay thế kiểm thử chức năng. Chưa chạy kiểm thử Android, giao diện hoặc chu trình đặt hàng → đối tác nhận → tài xế giao trên dữ liệu thật. Database hiện chưa có dữ liệu để chứng minh luồng đơn hàng và doanh thu hoạt động xuyên suốt.

## 2. Đối chiếu từng tuần

“Có mã nguồn” nghĩa là đã tìm thấy triển khai, chưa mặc nhiên đạt nghiệm thu. “Chưa thấy bằng chứng” không có nghĩa thành viên chắc chắn chưa làm ở nơi khác.

| Tuần / STT trong DOCX | Phân công cho Nhật | Hiện trạng đối chiếu | Phần cần bổ sung |
|---|---|---|---|
| 1 / 3 | Khảo sát đăng ký/khởi tạo gian hàng, quản lý món, nhận và xử lý đơn | Chưa thấy biên bản/phiếu khảo sát trong project | Bổ sung biên bản khảo sát thực tế, quy trình, kết luận; nếu đã có thì đính kèm đường dẫn |
| 2 / 6 | Phân tích Actor, Use Case và luồng nghiệp vụ đối tác | Chưa thấy bộ đặc tả riêng; mã nguồn thể hiện actor và một phần luồng | Danh sách Use Case, điều kiện trước/sau, luồng chính và ngoại lệ |
| 3 / 9 | Use Case/Activity/Sequence cho gian hàng, món và đơn | Chưa thấy file sơ đồ thiết kế trong project | Bổ sung ba loại sơ đồ phù hợp với trạng thái backend hiện tại |
| 4 / 12 | UI thông tin gian hàng, món, đơn, khuyến mãi | Có Razor views và các trang MAUI Vendor; chưa thấy bộ wireframe/mockup bàn giao riêng | Chụp giao diện thực tế, đính kèm thiết kế hoặc ghi rõ chuyển từ mockup sang giao diện triển khai |
| 5 / 15 | API gian hàng, món, đơn, khuyến mãi; request/response và test | Có đăng nhập JWT và 5 endpoint `/api/vendor`; chưa đủ API ghi dữ liệu món, danh mục, hồ sơ, khuyến mãi | Hoàn thiện hợp đồng API và endpoint thiếu, bộ request/response mẫu, kết quả test |
| 6 / 18 | Đăng nhập/xác thực, hồ sơ và quyền truy cập | Web có cookie, role, đăng ký, chờ duyệt, sửa hồ sơ, đổi mật khẩu; API có JWT; mobile vẫn đăng nhập mock | Tích hợp đăng nhập mobile, kiểm tra duyệt/khóa phía API, kiểm thử phân quyền |
| 7 / 21 | Web tạo/cập nhật gian hàng, danh mục/món | Có đăng ký, sửa hồ sơ, CRUD món, tải ảnh; có tạo danh mục trong form món | Bổ sung sửa/xóa danh mục trên web và kiểm tra quyền sở hữu danh mục khi ghi món |
| 8 / 24 | Web tiếp nhận/xử lý đơn, trạng thái món/đơn, khuyến mãi | Có danh sách/chi tiết đơn, xác nhận/hủy; còn/hết món; tạo/sửa/bật tắt khuyến mãi | Kiểm thử dữ liệu thật, đồng nhất hủy đơn giữa web/API và trạng thái giao nhận giữa mobile/backend |
| 9 / 27 | Thống kê đơn, doanh thu, món bán, dữ liệu quản trị | Có dashboard, doanh thu 30 ngày, số đơn hoàn thành, top 5 món trên web; mobile tính từ mock | Bổ sung dữ liệu test và đối soát kết quả; API báo cáo nếu mobile sử dụng |
| 10 / 30 | Phối hợp API và dữ liệu để mobile customer hiển thị gian hàng/món/đơn | Mobile customer vẫn đọc `MockData`; chưa thấy tích hợp API catalog/đơn khách hàng hoàn chỉnh trong project | Phối hợp người phụ trách khách hàng để thống nhất API, ID, trạng thái và kiểm thử đồng bộ |
| 11 / 33 | Mobile partner/store quản lý gian hàng, món, đơn; tích hợp API; test | Có các màn hình Vendor và xử lý demo/local; chưa tích hợp API nghiệp vụ đối tác | Thay nguồn dữ liệu mock bằng API cho đăng ký, đăng nhập, hồ sơ, món, danh mục, đơn và các màn hình bổ sung |
| 12 / 36 | Hoàn thiện, kiểm thử tổng thể, báo cáo và demo | Đã build được web và MAUI Windows trong lần kiểm tra này; chưa thấy bộ kết quả test, báo cáo riêng và demo script | Sửa các thiếu sót, test xuyên suốt, lưu bằng chứng, viết báo cáo và kịch bản demo |

Trong DOCX, tuần 1–4 của Nhật ghi 100% nhưng trạng thái vẫn `TODO`; tuần 5–12 để trống tiến độ. Cả 12 dòng chưa ghi công việc thực tế, bằng chứng, người kiểm tra và vẫn “Chưa xác nhận”. Vì vậy không nên chuyển toàn bộ sang DONE chỉ dựa trên số lượng màn hình hoặc build thành công.

## 3. Bằng chứng chức năng đã có

Các đường dẫn dưới đây tương đối so với thư mục gốc project.

| Chức năng | Mã nguồn đối chiếu |
|---|---|
| Đăng ký, đăng nhập, chờ duyệt | `FoodServiceApp.Web/Controllers/AuthController.cs`; `Controllers/GianHangBaseController.cs`; `Views/Auth/`; `Views/Dashboard/ChoDuyet.cshtml` |
| Hồ sơ, ảnh gian hàng, đổi mật khẩu | `FoodServiceApp.Web/Controllers/HoSoController.cs`; `Views/HoSo/Index.cshtml` |
| Món ăn, tạo danh mục kèm món, ảnh, tình trạng | `FoodServiceApp.Web/Controllers/MonAnController.cs`; `Views/MonAn/` |
| Đơn hàng | `FoodServiceApp.Web/Controllers/DonHangController.cs`; `Views/DonHang/` |
| Khuyến mãi của gian hàng | `FoodServiceApp.Web/Controllers/KhuyenMaiController.cs`; `Views/KhuyenMai/` |
| Dashboard và doanh thu | `FoodServiceApp.Web/Controllers/DashboardController.cs`; `Controllers/DoanhThuController.cs`; `Views/DoanhThu/Index.cshtml` |
| API đối tác hiện có | `FoodServiceApp.Web/Controllers/Api/VendorApiController.cs`; `ApiAuthController.cs`; `ApiBaseController.cs` |
| Giao diện mobile đối tác | `Mobile_MAUI/Views/Vendor*.xaml` và code-behind tương ứng |
| Dữ liệu demo và lưu cục bộ | `Mobile_MAUI/Models/Models.cs`; `Mobile_MAUI/Services/VendorLocalStore.cs` |

API đối tác hiện có:

| Method | Route | Chức năng |
|---|---|---|
| POST | `/api/auth/login/GianHang` | Đăng nhập lấy JWT |
| GET | `/api/vendor/home` | Thống kê nhanh |
| POST | `/api/vendor/toggle-mo-cua` | Đổi trạng thái kinh doanh |
| GET | `/api/vendor/orders?trangThai=...` | Danh sách đơn của gian hàng |
| POST | `/api/vendor/orders/{id}/status` | Xác nhận hoặc hủy theo luồng cho phép |
| GET | `/api/vendor/menu` | Đọc danh sách món |

API tải ảnh dùng chung đã có tại `Controllers/Api/UploadController.cs`. Các action MVC tạo/sửa món hoặc hồ sơ không tự thay thế hợp đồng JSON API cho mobile.

### Phần mobile đã nối API cần ghi nhận riêng

`Mobile_MAUI/Services/VendorRegistrationApiClient.cs` thực tế là client cho **quản trị duyệt/quản lý gian hàng**, mặc dù tên file có chữ Registration. Client gọi `/api/admin/vendors/...`, được sử dụng tại `AdminVendorApprovalPage` và `AdminVendorListPage` để tải danh sách, duyệt/từ chối, khóa/mở tài khoản. Đây là mã tích hợp API thật đã hiện diện.

Tuy nhiên, client này không gửi đăng ký gian hàng và không thay thế API cho các màn hình Vendor. `VendorRegisterPage` vẫn thêm vào `MockData.DanhSachGianHang`, nên hồ sơ đăng ký trên mobile chưa được ghi vào Database để danh sách duyệt lấy qua API nhìn thấy. Client hiện đặt cứng `https://localhost:5001/`, cần cấu hình địa chỉ phù hợp môi trường khi triển khai trên thiết bị khác.

## 4. Các thiếu sót cần ưu tiên

### P0 — Quyền truy cập và tính đúng của dữ liệu

1. **API quản trị gian hàng chưa yêu cầu xác thực.** `Controllers/Api/AdminVendorController.cs:10` kế thừa `ControllerBase`, không có `[Authorize]`; cấu hình hiện tại không có fallback policy bắt đăng nhập. Các action approve/reject/toggle-lock chưa được bảo vệ tương ứng vai trò quản trị. Cần dùng cơ chế JWT/role quản trị và đồng thời tích hợp đăng nhập, gửi token ở client quản trị. Không chỉ thêm attribute rồi để mobile ngừng hoạt động.
2. **API đối tác chưa kiểm tra trạng thái duyệt trong nghiệp vụ.** `ApiAuthController.Login` kiểm tra tài khoản hoạt động nhưng chưa chặn đối tác bị từ chối/chưa duyệt; `VendorApiBaseController` chỉ kiểm tra role. Cần quy định API nào cho phép hồ sơ chờ duyệt, API nào chỉ cho gian hàng đã duyệt, đồng thời xử lý tài khoản bị khóa sau khi cấp token. Web đang dựa vào claim trạng thái lúc đăng nhập nên cũng cần kiểm tra hành vi sau thay đổi trạng thái.
3. **Ghi món chưa xác minh danh mục thuộc gian hàng hiện tại.** POST Create/Edit trong `MonAnController.cs:43` và `:104` nhận `MaDanhMuc` rồi gán trực tiếp. Danh sách chọn trên giao diện có lọc nhưng request gửi thủ công có thể dùng ID khác. Cần kiểm tra quyền sở hữu ở server trước khi ghi và chỉ tạo danh mục sau khi các dữ liệu cần thiết hợp lệ.
4. **Trạng thái đơn trên mobile khác backend.** `VendorOrdersPage.OnSanSangGiaoClicked` tự đặt `DangGiao`; backend chỉ cho gian hàng xác nhận/hủy, tài xế mới chuyển sang giao. Cần ánh xạ `DangLam ↔ DaXacNhan`, `HoanThanh ↔ DaGiao` và bỏ hoặc điều chỉnh thao tác bàn giao cho đúng luồng chung.
5. **Hủy đơn web/API chưa đồng nhất.** Web đổi `ThanhToan.TrangThai` khi hủy đơn đã thanh toán; API chỉ đổi trạng thái đơn. Đổi trạng thái Database sang `DaHoanTien` không chứng minh tiền đã được hoàn qua cổng thanh toán. Cần thống nhất quy trình, tránh mobile thông báo “hoàn tiền tự động” khi chưa có xử lý tương ứng.

### P1 — Hoàn tất phần phân công

6. Bổ sung API đăng ký đối tác, đọc/sửa hồ sơ, CRUD danh mục, CRUD món và trạng thái còn hàng, khuyến mãi, báo cáo phục vụ mobile. Mọi thao tác cần lấy gian hàng từ danh tính đăng nhập và kiểm tra quyền sở hữu.
7. Tạo service API dùng chung và quản lý phiên đăng nhập cho MAUI, nối các màn hình Vendor hiện có, xử lý đang tải, lỗi mạng, token hết hạn và dữ liệu rỗng. Không tự chuyển sang mock khi gọi API lỗi.
8. Phối hợp module khách hàng để đọc gian hàng/món thật và theo dõi cùng ID đơn trong Database. Đây là phần phối hợp tuần 10, không quy toàn bộ việc phát triển mobile customer cho Nhật.
9. Hoàn thiện quản lý danh mục web: hiện có tạo trong form món, chưa thấy luồng đổi tên/xóa danh mục. Chốt cách xử lý danh mục còn món trước khi xóa.
10. Chuẩn bị dữ liệu kiểm thử: ít nhất hai gian hàng để kiểm tra cách ly quyền, khách hàng, tài xế, quản trị, món, khuyến mãi và các trạng thái đơn. Database hiện tại chưa có bộ dữ liệu này.

### P2 — Bằng chứng và bàn giao

11. Bổ sung tài liệu tuần 1–3 và bằng chứng UI tuần 4; không tạo biên bản khảo sát giả hoặc tự xác nhận thay người kiểm tra.
12. Lưu hợp đồng API, kết quả test, ảnh demo và commit tương ứng. README hiện có nội dung cũ như “chưa có web”, “chưa có csproj” hoặc chưa build; cần cập nhật theo project thực tế.
13. Viết báo cáo module đối tác và kịch bản demo; cập nhật tracker bằng kết quả thực tế, chưa điền phần trăm hoàn thành nếu chưa có tiêu chí và xác nhận.

## 5. Checklist nghiệm thu đề xuất

Các trường hợp dưới đây là **test cần thực hiện**, không phải kết quả đã pass.

| Mã | Trường hợp | Kết quả mong đợi |
|---|---|---|
| DT01 | Đăng ký từ mobile | Hồ sơ được lưu DB ở trạng thái chờ duyệt, xuất hiện trong danh sách quản trị |
| DT02 | Chưa duyệt, bị từ chối hoặc bị khóa | Server chặn đúng nghiệp vụ; gọi trực tiếp API cũng không vượt được |
| DT03 | Đăng nhập web/mobile | Truy cập đúng gian hàng; token hết hạn được xử lý |
| DT04 | Sửa hồ sơ, tạo danh mục và món từ mobile | Web và DB thấy dữ liệu sau tải lại; không mất khi đóng app |
| DT05 | Gửi ID danh mục/món/đơn của gian hàng khác | Server từ chối, không đổi dữ liệu |
| DT06 | Còn/hết món và mở/đóng gian hàng | Khách hàng đọc được trạng thái cập nhật từ server |
| DT07 | Khách đặt → đối tác xác nhận → tài xế giao | Ba vai trò xem cùng đơn, chuyển trạng thái hợp lệ, không giao trước khi tài xế nhận |
| DT08 | Hủy đơn tiền mặt và đơn đã trả online | Trạng thái đơn/thanh toán thống nhất giữa web/API; không báo hoàn tiền thành công khi chưa hoàn |
| DT09 | Khuyến mãi sai phạm vi, hết hạn, dữ liệu không hợp lệ | Server kiểm tra nhất quán; mã hợp lệ lưu và áp dụng được |
| DT10 | Doanh thu và top món | Đối soát với đơn đã giao, không tính đơn hủy; thống nhất kỳ thống kê |
| DT11 | Đóng/mở lại app và lỗi mạng | Dữ liệu được tải lại đúng; không hiện thành công khi server chưa lưu |
| DT12 | API quản trị không token hoặc token đối tác | Không được xem/duyệt/khóa thay quản trị |

## 6. Gợi ý điền cột công việc thực tế trong tracker

Đây là nội dung dự thảo dựa trên mã nguồn, cần người phụ trách kiểm tra và bổ sung bằng chứng trước khi xác nhận.

| Tuần | Nội dung có thể bổ sung |
|---|---|
| 1–3 | Chưa xác minh tài liệu khảo sát, phân tích và sơ đồ trong project; cần bổ sung đường dẫn tài liệu thực tế |
| 4 | Đã xây dựng giao diện web và MAUI cho hồ sơ, món, đơn, khuyến mãi; cần bổ sung ảnh chạy thực tế/wireframe |
| 5 | Đã có API đăng nhập JWT, dashboard, trạng thái kinh doanh, danh sách/cập nhật trạng thái đơn và danh sách món; còn thiếu API quản lý đầy đủ và test |
| 6 | Đã có đăng ký/đăng nhập, hồ sơ và phân quyền web; API JWT cơ bản; còn tích hợp auth mobile và kiểm tra duyệt/khóa |
| 7 | Đã có web tạo/cập nhật gian hàng, CRUD món, tải ảnh, tạo danh mục trong form món; còn hoàn thiện danh mục và kiểm tra quyền sở hữu |
| 8 | Đã có web xác nhận/hủy đơn, tình trạng món và quản lý khuyến mãi; cần test và đồng nhất với API/mobile |
| 9 | Đã có dashboard, doanh thu 30 ngày, số đơn và top món; chưa có dữ liệu đơn để nghiệm thu báo cáo |
| 10 | Chưa xác nhận tích hợp API đối tác với mobile customer; các màn hình khách hàng đang đọc mock |
| 11 | Đã có UI và xử lý demo mobile đối tác; còn nối API và test với Database |
| 12 | Web và MAUI Windows build thành công ngày 12/10/2026; còn cảnh báo, kiểm thử tổng thể, báo cáo và demo script |

Bản đối chiếu này không sửa file DOCX gốc, mã nguồn nghiệp vụ hoặc dữ liệu Database. Các mục P0–P2 là danh sách bổ sung sau kiểm tra, chưa được triển khai trong lần rà soát này.
