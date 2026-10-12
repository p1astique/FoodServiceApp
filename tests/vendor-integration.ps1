$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http
$root = Split-Path $PSScriptRoot -Parent
$database = 'FoodServiceVendorTest_' + [Guid]::NewGuid().ToString('N')
$base = 'http://127.0.0.1:5188/'
$log = Join-Path $PSScriptRoot 'vendor-test-server.log'
$errLog = Join-Path $PSScriptRoot 'vendor-test-server-errors.log'
$results = [Collections.Generic.List[string]]::new()
$client = [Net.Http.HttpClient]::new()
$client.Timeout = [TimeSpan]::FromSeconds(30)
$process = $null
$oldConnection = $env:FOOD_DB_CONNECTION
$oldEnvironment = $env:ASPNETCORE_ENVIRONMENT
$oldGoong = $env:GoongMaps__ApiKey
$webDll = if ($env:FOOD_TEST_WEB_DLL) { $env:FOOD_TEST_WEB_DLL } else { 'bin/Debug/net8.0/FoodServiceApp.Web.dll' }
function Query([string]$sql) {
    $cn = [Data.SqlClient.SqlConnection]::new("Server=(localdb)\MSSQLLocalDB;Database=$database;Integrated Security=True;TrustServerCertificate=True")
    try { $cn.Open(); $cmd = $cn.CreateCommand(); $cmd.CommandText = $sql; return $cmd.ExecuteScalar() }
    finally { $cn.Dispose() }
}
function Call([string]$method, [string]$path, $body, [string]$token, [int]$expected = 200) {
    $req = [Net.Http.HttpRequestMessage]::new([Net.Http.HttpMethod]::new($method), $base + $path)
    if ($token) { $req.Headers.Authorization = [Net.Http.Headers.AuthenticationHeaderValue]::new('Bearer', $token) }
    if ($null -ne $body) { $req.Content = [Net.Http.StringContent]::new(($body | ConvertTo-Json -Depth 15 -Compress), [Text.Encoding]::UTF8, 'application/json') }
    try {
        $res = $client.SendAsync($req).GetAwaiter().GetResult()
        try {
            $text = $res.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            if ([int]$res.StatusCode -ne $expected) { throw "$method $path expected $expected, got $([int]$res.StatusCode): $text" }
            $results.Add("PASS $method $path -> $expected")
            if ($text -and $expected -lt 400) { foreach ($value in (ConvertFrom-Json $text)) { Write-Output $value } }
        } finally { $res.Dispose() }
    } finally { $req.Dispose() }
}
function Check([bool]$condition, [string]$name) {
    if (!$condition) { throw "FAIL $name" }; $results.Add("PASS $name")
}
try {
    $create = [Data.SqlClient.SqlConnection]::new('Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=True;TrustServerCertificate=True')
    try {
        $create.Open(); $cmd = $create.CreateCommand(); $cmd.CommandText = "CREATE DATABASE [$database]"; $cmd.ExecuteNonQuery() | Out-Null
    } finally { $create.Dispose() }
    $env:FOOD_DB_CONNECTION = "Server=(localdb)\MSSQLLocalDB;Database=$database;Integrated Security=True;TrustServerCertificate=True"
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:GoongMaps__ApiKey = ''
    $process = Start-Process dotnet -ArgumentList @($webDll, '--urls', $base) -WorkingDirectory (Join-Path $root 'FoodServiceApp.Web') -WindowStyle Hidden -PassThru -RedirectStandardOutput $log -RedirectStandardError $errLog
    $ready = $false
    for ($i=0; $i -lt 45; $i++) {
        try {
            $ping = $client.GetAsync($base + 'api/catalog/stores').GetAwaiter().GetResult()
            $ready = $ping.IsSuccessStatusCode; $ping.Dispose()
            if ($ready) { break }
        } catch { }
        if ($process.HasExited) { throw "Test server exited. Inspect $log and $errLog" }
        Start-Sleep -Milliseconds 500
    }
    if (!$ready) { throw 'Test server not ready.' }
    # All fixtures are written only to the randomly named test database.
    Query @'
INSERT INTO TaiKhoan(TenDangNhap,MatKhau,VaiTro,NgayTao,TrangThai) VALUES
('test-admin','test123456','QuanTri',GETDATE(),1),
('test-vendor','test123456','GianHang',GETDATE(),1),
('test-other','test123456','GianHang',GETDATE(),1);
INSERT INTO TaiKhoan(TenDangNhap,MatKhau,VaiTro,NgayTao,TrangThai) VALUES ('test-driver','test123456','TaiXe',GETDATE(),1);
INSERT INTO TaiXe(MaTK,HoTen,SDT,TrangThai,TrangThaiDuyet,NgayDangKy) SELECT MaTK,'Test Driver','0900000000','SanSang','DaDuyet',GETDATE() FROM TaiKhoan WHERE TenDangNhap='test-driver';
INSERT INTO QuanTri(MaTK,HoTen,SDT,CapQuyenHan) SELECT MaTK,N'Test Admin','0900000000','SuperAdmin' FROM TaiKhoan WHERE TenDangNhap='test-admin';
INSERT INTO GianHang(MaTK,TenCuaHang,DiaChi,SDT,TrangThaiDuyet,TrangThaiKinhDoanh,NgayDangKy)
SELECT MaTK,TenDangNhap,N'Test address','0900000000','DaDuyet','DangMo',GETDATE() FROM TaiKhoan WHERE VaiTro='GianHang';
'@ | Out-Null
    Call GET 'api/admin/vendors/pending' $null '' 401 | Out-Null
    Call GET 'api/vendor/profile' $null '' 401 | Out-Null
    $admin = (Call POST 'api/auth/login/QuanTri' @{tenDangNhap='test-admin';matKhau='test123456'} '').token
    $vendor = (Call POST 'api/auth/login/GianHang' @{tenDangNhap='test-vendor';matKhau='test123456'} '').token
    $other = (Call POST 'api/auth/login/GianHang' @{tenDangNhap='test-other';matKhau='test123456'} '').token
    Call POST 'api/admin/vendors/1/approve' $null $vendor 403 | Out-Null
    $profile = Call GET 'api/vendor/profile' $null $vendor
    $storeId = [int]$profile.id
    $registered = Call POST 'api/vendor/register' @{tenDangNhap='new-vendor';matKhau='test123456';tenCuaHang='New';diaChi='Address';sdt='0901234567'} ''
    Call POST 'api/auth/login/GianHang' @{tenDangNhap='new-vendor';matKhau='test123456'} '' 403 | Out-Null
    $pending = @(Call GET 'api/admin/vendors/pending' $null $admin)
    Check ($pending.Count -eq 1) 'Registration is visible to admin'
    Call POST "api/admin/vendors/$($registered.maGianHang)/approve" $null $admin | Out-Null
    Call POST 'api/auth/login/GianHang' @{tenDangNhap='new-vendor';matKhau='test123456'} '' | Out-Null
    Call PUT 'api/vendor/profile' @{tenCuaHang='Updated vendor';diaChi='Address';sdt='0901234567';dangMoCua=$true} $vendor 204 | Out-Null
    Check ((Call GET 'api/vendor/profile' $null $vendor).tenGianHang -eq 'Updated vendor') 'Profile persisted'
    $category = Call POST 'api/vendor/categories' @{tenDanhMuc='Main'} $vendor
    $foreign = Call POST 'api/vendor/categories' @{tenDanhMuc='Other'} $other
    $itemBody = @{maDanhMuc=$foreign.maDanhMuc;tenMon='Rice';gia=50000;conHang=$true;kichCo='Small,Large';topping='Egg';mucCay='Mild,Hot'}
    Call POST 'api/vendor/items' $itemBody $vendor 400 | Out-Null
    $itemBody.maDanhMuc = $category.maDanhMuc
    $item = Call POST 'api/vendor/items' $itemBody $vendor
    Call PUT "api/vendor/categories/$($category.maDanhMuc)" @{tenDanhMuc='Renamed'} $vendor 204 | Out-Null
    Call DELETE "api/vendor/categories/$($category.maDanhMuc)" $null $vendor 409 | Out-Null
    Call PUT "api/vendor/items/$($item.maMonAn)" $itemBody $other 400 | Out-Null
    Call DELETE "api/vendor/items/$($item.maMonAn)" $null $other 404 | Out-Null
    $catalog = @(Call GET "api/catalog/stores/$storeId/items" $null '')
    Check ($catalog.Count -eq 1 -and $catalog[0].gia -eq 50000) 'Customer sees vendor item'
    Call PUT "api/vendor/items/$($item.maMonAn)/availability" @{conHang=$false} $vendor 204 | Out-Null
    Check (@(Call GET "api/catalog/stores/$storeId/items" $null '').Count -eq 0) 'Unavailable item hidden from customer'
    Call PUT "api/vendor/items/$($item.maMonAn)/availability" @{conHang=$true} $vendor 204 | Out-Null
    Call PUT 'api/vendor/settings' @{gioMoCua='00:00:00';gioDongCua='23:59:00';tuDongDongMo=$true;tamDungNhanDon=$false;thongBaoCuaHang='Test notice';thoiGianChuanBiPhut=25} $vendor 204 | Out-Null
    $settings = Call GET 'api/vendor/settings' $null $vendor
    Check ($settings.thoiGianChuanBiPhut -eq 25 -and $settings.thongBaoCuaHang -eq 'Test notice') 'Store settings persisted'
    $campaignBody = @{tenChienDich='Lunch campaign';loaiViTri='Banner';tieuDe='Lunch special';noiDung='Test';ngayBatDau=(Get-Date).AddDays(-1).ToString('s');ngayKetThuc=(Get-Date).AddDays(2).ToString('s');nganSach=500000;kichHoat=$true}
    $campaign = Call POST 'api/vendor/campaigns' $campaignBody $vendor
    Call DELETE "api/vendor/campaigns/$($campaign.maChienDich)" $null $other 404 | Out-Null
    $publicCampaigns = @(Call GET 'api/catalog/campaigns?position=Banner' $null '')
    Check ($publicCampaigns.Count -eq 1 -and $publicCampaigns[0].maGianHang -eq $storeId) 'Active campaign appears in catalog'
    Call POST "api/catalog/campaigns/$($campaign.maChienDich)/interaction" $null '' 204 | Out-Null
    $campaigns = @(Call GET 'api/vendor/campaigns' $null $vendor)
    Check ($campaigns[0].luotTuongTac -eq 1) 'Campaign interaction is counted'
    Call PUT "api/vendor/categories/$($category.maDanhMuc)" @{tenDanhMuc='Renamed';moTa='Hidden test';thuTuHienThi=2;trangThai=$false} $vendor 204 | Out-Null
    Check (@(Call GET "api/catalog/stores/$storeId/items" $null '').Count -eq 0) 'Hidden category hides its menu items'
    Call PUT "api/vendor/categories/$($category.maDanhMuc)" @{tenDanhMuc='Renamed';moTa='Visible';thuTuHienThi=1;trangThai=$true} $vendor 204 | Out-Null
    $promoBody = @{maCode='SAVE10';moTa='Test discount';maDanhMuc=$category.maDanhMuc;laPhanTram=$true;giaTriGiam=101;giaTriDonToiThieu=50000;soLuongToiDa=2;moiKhachMotLan=$true;ngayBatDau=(Get-Date).AddDays(-1).ToString('s');ngayKetThuc=(Get-Date).AddDays(2).ToString('s');trangThai=$true}
    Call POST 'api/vendor/promotions' $promoBody $vendor 400 | Out-Null
    $promoBody.giaTriGiam=10
    $promo = Call POST 'api/vendor/promotions' $promoBody $vendor
    Call POST 'api/customer/register' @{tenDangNhap='test-customer';matKhau='test123456';hoTen='Customer';sdt='0901234567'} '' | Out-Null
    $customer = (Call POST 'api/auth/login/KhachHang' @{tenDangNhap='test-customer';matKhau='test123456'} '').token
    $orderBody = @{maGianHang=$storeId;diaChiGiaoHang='Delivery address';maKhuyenMai='SAVE10';items=@(@{maMonAn=$item.maMonAn;soLuong=2;tuyChonMon='Large | Egg | Hot'})}
    $quote = Call POST 'api/customer/quote' $orderBody $customer
    Check ($quote.tamTinh -eq 100000 -and $quote.giamGia -eq 10000) 'Server computes prices and promotion'
    $orderBody.tongTienXacNhan=1
    Call POST 'api/customer/orders' $orderBody $customer 409 | Out-Null
    $orderBody.tongTienXacNhan=$quote.tongTien
    $order = Call POST 'api/customer/orders' $orderBody $customer
    Check ($order.tuyChonMon -eq 'Large | Egg | Hot') "API receives menu options (actual: '$($order.tuyChonMon)')"
    Call POST 'api/customer/quote' $orderBody $customer 400 | Out-Null
    Check ((Query "SELECT COUNT(*) FROM SuDungKhuyenMai WHERE MaDonHang=$($order.maDonHang)") -eq 1) 'Voucher usage is recorded and one-use-per-customer is enforced'
    $shipBody = @{maCode='FREESHIP';moTa='Shipping voucher';laPhanTram=$false;giaTriGiam=0;giamPhiVanChuyen=$true;giamPhiVanChuyenToiDa=10000;giaTriDonToiThieu=50000;ngayBatDau=(Get-Date).AddDays(-1).ToString('s');ngayKetThuc=(Get-Date).AddDays(2).ToString('s');trangThai=$true}
    Call POST 'api/vendor/promotions' $shipBody $vendor | Out-Null
    $orderBody.maKhuyenMai='FREESHIP'; $orderBody.Remove('tongTienXacNhan')
    $shipQuote = Call POST 'api/customer/quote' $orderBody $customer
    Check ($shipQuote.giamPhiGiaoHang -eq 10000 -and $shipQuote.tongTien -eq ($shipQuote.tamTinh + $shipQuote.phiGiaoHang - 10000)) 'Shipping voucher is validated and applied at checkout'
    $savedOption = [string](Query "SELECT TuyChonMon FROM ChiTietDonHang WHERE MaDonHang=$($order.maDonHang)")
    Check ($savedOption.Trim() -eq 'Large | Egg | Hot') "Menu options persisted with order detail (actual: '$savedOption')"
    $savedName = [string](Query "SELECT TenMon FROM ChiTietDonHang WHERE MaDonHang=$($order.maDonHang)")
    Check ($savedName.Trim() -eq 'Rice') "Menu item name snapshot persisted (actual: '$savedName')"
    $vendorOrders = @(Call GET 'api/vendor/orders' $null $vendor)
    Check ($vendorOrders.Count -eq 1 -and $vendorOrders[0].maDonHang -eq $order.maDonHang) 'Customer order appears in vendor inbox'
    Call POST "api/vendor/orders/$($order.maDonHang)/status" @{trangThaiMoi='DaXacNhan'} $other 404 | Out-Null
    Call POST "api/vendor/orders/$($order.maDonHang)/status" @{trangThaiMoi='DangGiao'} $vendor 409 | Out-Null
    Call POST "api/vendor/orders/$($order.maDonHang)/status" @{trangThaiMoi='DaXacNhan'} $vendor | Out-Null
    $customerOrders = @(Call GET 'api/customer/orders' $null $customer)
    Check ($customerOrders[0].trangThai -eq 'DaXacNhan') 'Customer sees vendor confirmation'
    $oid = [int]$order.maDonHang
    Query "UPDATE ThanhToan SET TrangThai='DaThanhToan' WHERE MaDonHang=$oid" | Out-Null
    Call POST "api/vendor/orders/$oid/status" @{trangThaiMoi='DaHuy';lyDo='Khách yêu cầu hủy'} $vendor | Out-Null
    Check ((Query "SELECT TrangThai FROM ThanhToan WHERE MaDonHang=$oid") -eq 'DaThanhToan') 'Cancel does not claim a completed refund'
    Call DELETE "api/vendor/promotions/$($promo.maKhuyenMai)" $null $vendor 409 | Out-Null
    Call DELETE "api/vendor/items/$($item.maMonAn)" $null $vendor 409 | Out-Null
    Call PUT "api/vendor/promotions/$($promo.maKhuyenMai)/status" @{trangThai=$false} $vendor 204 | Out-Null
    $orderBody.Remove('maKhuyenMai')
    $quote = Call POST 'api/customer/quote' $orderBody $customer
    $orderBody.tongTienXacNhan = $quote.tongTien
    $delivery = Call POST 'api/customer/orders' $orderBody $customer
    Call POST "api/vendor/orders/$($delivery.maDonHang)/status" @{trangThaiMoi='DaXacNhan'} $vendor | Out-Null
    Call POST "api/vendor/orders/$($delivery.maDonHang)/status" @{trangThaiMoi='DangChuanBi'} $vendor | Out-Null
    Call POST "api/vendor/orders/$($delivery.maDonHang)/status" @{trangThaiMoi='SanSangGiao'} $vendor | Out-Null
    Check ((Query "SELECT COUNT(*) FROM NhatKyTrangThaiDon WHERE MaDonHang=$($delivery.maDonHang)") -eq 3) 'Vendor order status timeline is recorded'
    $driver = (Call POST 'api/auth/login/TaiXe' @{tenDangNhap='test-driver';matKhau='test123456'} '').token
    Call POST "api/driver/orders/$($delivery.maDonHang)/accept" $null $driver | Out-Null
    Call POST "api/vendor/orders/$($delivery.maDonHang)/status" @{trangThaiMoi='DaHuy';lyDo='Too late'} $vendor 409 | Out-Null
    Call POST "api/driver/orders/$($delivery.maDonHang)/start" $null $driver | Out-Null
    Call POST "api/driver/orders/$($delivery.maDonHang)/complete" $null $driver | Out-Null
    $vendorSummary = Call GET 'api/vendor/home' $null $vendor
    Check ($vendorSummary.soDonHomNay -eq 1 -and $vendorSummary.doanhThuHomNay -eq $quote.tongTien) 'Delivered order counted once, canceled order excluded'
    $customerOrders = @(Call GET 'api/customer/orders' $null $customer)
    Check (@($customerOrders | Where-Object { $_.maDonHang -eq $delivery.maDonHang -and $_.trangThai -eq 'DaGiao' }).Count -eq 1) 'Customer sees driver completion'
    Call POST "api/admin/vendors/$storeId/toggle-lock" $null $admin | Out-Null
    Call GET 'api/vendor/profile' $null $vendor 401 | Out-Null
    Call POST "api/admin/vendors/$storeId/toggle-lock" $null $admin | Out-Null
    Call POST "api/admin/vendors/$storeId/reject" $null $admin | Out-Null
    Call GET 'api/vendor/profile' $null $vendor 403 | Out-Null
    $results.Add("SUCCESS: $($results.Count) checks, isolated SQL Server database, no production records changed.")
} catch {
    $results.Add("FAIL: $($_.Exception.Message)")
    throw
} finally {
    $client.Dispose()
    if ($process -and !$process.HasExited) { Stop-Process -Id $process.Id -Force; $process.WaitForExit() }
    $env:FOOD_DB_CONNECTION = $oldConnection
    $env:ASPNETCORE_ENVIRONMENT = $oldEnvironment
    $env:GoongMaps__ApiKey = $oldGoong
    if ($database -notmatch '^FoodServiceVendorTest_[a-f0-9]{32}$') { throw 'Invalid cleanup target' }
    $master = [Data.SqlClient.SqlConnection]::new('Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=True;TrustServerCertificate=True')
    try {
        $master.Open(); $cmd = $master.CreateCommand()
        $cmd.CommandText = "IF DB_ID(N'$database') IS NOT NULL BEGIN ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$database]; END"
        $cmd.ExecuteNonQuery() | Out-Null
    } finally { $master.Dispose() }
    $results | Set-Content (Join-Path $PSScriptRoot 'vendor-test-results.txt') -Encoding UTF8
    $results | Write-Output
}
