using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodServiceApp.Web.Data;
using FoodServiceApp.Web.Models.Entities;
using FoodServiceApp.Web.Models.ViewModels;
using FoodServiceApp.Web.Services;
using FoodServiceApp.Web.Services.Maps;
using FoodServiceApp.Web.Services.Payment;

namespace FoodServiceApp.Web.Controllers
{
    [Route("MuaHang/GioHang")]
    public class CustomerCartController : Controller
    {
        private readonly AppDbContext _db;
        private readonly CartService _cart;
        private readonly IVnPayService _vnPayService;
        private readonly IGoongMapsService _mapsService;

        // VNĐ/km — trùng công thức với Controllers/Api/MapController để 2 nơi luôn tính ra cùng 1 kết quả.
        private const decimal PhiMoiKm = 5000m;
        private const decimal PhiGiaoMacDinh = 15000m; // dùng khi không xác định được tọa độ (vd. chưa cấu hình API key Goong)

        public CustomerCartController(AppDbContext db, CartService cart, IVnPayService vnPayService, IGoongMapsService mapsService)
        {
            _db = db;
            _cart = cart;
            _vnPayService = vnPayService;
            _mapsService = mapsService;
        }

        /// <summary>Tính phí giao hàng theo khoảng cách thật; nếu không geocode được thì trả về phí mặc định thay vì chặn luồng đặt hàng.</summary>
        private async Task<decimal> TinhPhiGiaoHangAsync(string diaChiGianHang, string diaChiGiao)
        {
            try
            {
                var diemGian = await _mapsService.GeocodeAsync(diaChiGianHang);
                var diemKhach = await _mapsService.GeocodeAsync(diaChiGiao);
                if (diemGian == null || diemKhach == null) return PhiGiaoMacDinh;

                var ketQua = await _mapsService.GetDistanceAsync(diemGian, diemKhach);
                if (ketQua == null) return PhiGiaoMacDinh;

                return Math.Ceiling((decimal)ketQua.DistanceKm) * PhiMoiKm;
            }
            catch
            {
                // Goong Maps lỗi/chưa cấu hình API key -> không để việc này chặn khách đặt hàng.
                return PhiGiaoMacDinh;
            }
        }

        [HttpGet("")]
        public IActionResult Index() => View(_cart.LayGioHang());

        [HttpPost("cap-nhat")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhat(int maMonAn, int soLuong)
        {
            var item = await _db.MonAns.Include(m => m.DanhMuc).FirstOrDefaultAsync(m => m.MaMonAn == maMonAn);
            if (item == null || item.DanhMuc == null)
            {
                _cart.CapNhatSoLuong(maMonAn, 0);
                TempData["LoiThongBao"] = "Món trong giỏ không còn tồn tại và đã được xoá.";
                return RedirectToAction("Index");
            }

            if (item.TinhTrang != "ConHang")
            {
                _cart.CapNhatSoLuong(maMonAn, 0);
                TempData["LoiThongBao"] = $"Món '{item.TenMon}' hiện đã hết hàng và đã được xoá khỏi giỏ.";
                return RedirectToAction("Index");
            }

            _cart.CapNhatSoLuong(maMonAn, Math.Clamp(soLuong, 1, 50));
            _cart.CapNhatThongTinMon(item.MaMonAn, item.TenMon, item.Gia, item.HinhAnh);
            return RedirectToAction("Index");
        }

        [HttpPost("xoa-mon")]
        [ValidateAntiForgeryToken]
        public IActionResult XoaMon(int maMonAn)
        {
            _cart.CapNhatSoLuong(maMonAn, 0);
            return RedirectToAction("Index");
        }

        [HttpPost("xoa-tat-ca")]
        [ValidateAntiForgeryToken]
        public IActionResult XoaTatCa()
        {
            _cart.XoaGioHang();
            return RedirectToAction("Index", "CustomerHome");
        }

        // Nạp sổ địa chỉ của khách để trang Checkout cho chọn nhanh
        private async Task NapDiaChiAsync()
        {
            var maKH = int.Parse(User.FindFirst("MaKH")!.Value);
            ViewBag.DiaChis = await _db.DiaChiKhachHangs
                .Where(d => d.MaKH == maKH)
                .OrderByDescending(d => d.MacDinh).ThenBy(d => d.MaDiaChi)
                .ToListAsync();
        }

        /// <summary>Đồng bộ giỏ hàng với CSDL trước khi checkout/đặt đơn: giá, tên, ảnh và tình trạng món.</summary>
        private async Task<string?> DongBoGioHangAsync()
        {
            var gioHang = _cart.LayGioHang();
            if (!gioHang.Items.Any()) return "Giỏ hàng đang trống.";

            var gianHang = await _db.GianHangs.FirstOrDefaultAsync(g => g.MaGianHang == gioHang.MaGianHang);
            if (gianHang == null)
            {
                _cart.XoaGioHang();
                return "Gian hàng của giỏ hàng không còn tồn tại.";
            }

            if (gianHang.TrangThaiKinhDoanh != "DangMo")
                return $"Gian hàng \"{gianHang.TenCuaHang}\" hiện đã đóng hoặc tạm ngưng nhận đơn.";

            var settings = await _db.CauHinhGianHangs.FindAsync(gioHang.MaGianHang);
            if (settings?.TamDungNhanDon == true)
                return settings.LyDoTamDung ?? "Gian hàng đang tạm dừng nhận đơn.";
            if (settings?.TuDongDongMo == true)
            {
                var time = DateTime.Now.TimeOfDay;
                var open = settings.GioMoCua < settings.GioDongCua
                    ? time >= settings.GioMoCua && time < settings.GioDongCua
                    : time >= settings.GioMoCua || time < settings.GioDongCua;
                if (!open) return $"Gian hàng chỉ nhận đơn từ {settings.GioMoCua:hh\\:mm} đến {settings.GioDongCua:hh\\:mm}.";
            }

            var ids = gioHang.Items.Select(i => i.MaMonAn).Distinct().ToList();
            var mons = await _db.MonAns.Include(m => m.DanhMuc)
                .Where(m => ids.Contains(m.MaMonAn))
                .ToDictionaryAsync(m => m.MaMonAn);

            var daXoa = new List<string>();
            foreach (var item in gioHang.Items.ToList())
            {
                if (!mons.TryGetValue(item.MaMonAn, out var mon) || mon.DanhMuc?.MaGianHang != gioHang.MaGianHang || !mon.DanhMuc.TrangThai)
                {
                    _cart.CapNhatSoLuong(item.MaMonAn, 0);
                    daXoa.Add(item.TenMon);
                    continue;
                }

                if (mon.TinhTrang != "ConHang")
                {
                    _cart.CapNhatSoLuong(item.MaMonAn, 0);
                    daXoa.Add(mon.TenMon);
                    continue;
                }

                _cart.CapNhatThongTinMon(mon.MaMonAn, mon.TenMon, mon.Gia, mon.HinhAnh);
            }

            if (daXoa.Count > 0)
                return "Một số món đã hết hàng hoặc thay đổi và được tự động cập nhật: " + string.Join(", ", daXoa);

            return null;
        }

        [HttpGet("thanh-toan")]
        [Authorize(Roles = "KhachHang")]
        public async Task<IActionResult> Checkout()
        {
            var loiDongBo = await DongBoGioHangAsync();
            var gioHang = _cart.LayGioHang();
            if (!gioHang.Items.Any())
            {
                if (!string.IsNullOrWhiteSpace(loiDongBo)) TempData["LoiThongBao"] = loiDongBo;
                return RedirectToAction("Index", "CustomerCart");
            }
            if (!string.IsNullOrWhiteSpace(loiDongBo)) TempData["LoiThongBao"] = loiDongBo;

            var maKH = int.Parse(User.FindFirst("MaKH")!.Value);
            var khachHang = await _db.KhachHangs.FirstAsync(k => k.MaKH == maKH);
            await NapDiaChiAsync();

            // Ưu tiên địa chỉ mặc định trong sổ địa chỉ; nếu chưa có thì dùng địa chỉ trong hồ sơ
            var macDinh = await _db.DiaChiKhachHangs.FirstOrDefaultAsync(d => d.MaKH == maKH && d.MacDinh);

            return View(new CheckoutViewModel
            {
                GioHang = gioHang,
                DiaChiGiaoHang = macDinh?.DiaChi ?? khachHang.DiaChiGiaoHang ?? "",
            });
        }

        // AJAX: khách gõ/đổi địa chỉ giao ở trang Checkout -> gọi endpoint này để hiện phí ước tính ngay,
        // trước khi bấm Đặt hàng. Giá trị cuối cùng vẫn được TÍNH LẠI Ở SERVER trong DatHang() bên dưới,
        // không tin tưởng số phí do client gửi lên, tránh khách sửa phí qua DevTools.
        [HttpPost("uoc-tinh-phi")]
        [Authorize(Roles = "KhachHang")]
        public async Task<IActionResult> UocTinhPhi([FromBody] string diaChiGiao)
        {
            var gioHang = _cart.LayGioHang();
            if (!gioHang.Items.Any() || string.IsNullOrWhiteSpace(diaChiGiao))
                return Ok(new { phiGiaoHang = 0m });

            var gianHang = await _db.GianHangs.FirstOrDefaultAsync(g => g.MaGianHang == gioHang.MaGianHang);
            if (gianHang == null) return Ok(new { phiGiaoHang = PhiGiaoMacDinh });

            var phi = await TinhPhiGiaoHangAsync(gianHang.DiaChi, diaChiGiao);
            return Ok(new { phiGiaoHang = phi });
        }

        [HttpPost("ap-dung-khuyen-mai")]
        [Authorize(Roles = "KhachHang")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApDungKhuyenMai(CheckoutViewModel model)
        {
            var loiDongBo = await DongBoGioHangAsync();
            model.GioHang = _cart.LayGioHang();
            if (!model.GioHang.Items.Any())
            {
                TempData["LoiThongBao"] = loiDongBo ?? "Giỏ hàng đang trống.";
                return RedirectToAction("Index", "CustomerCart");
            }
            if (!string.IsNullOrWhiteSpace(loiDongBo)) model.LoiThongBao = loiDongBo;

            if (!string.IsNullOrWhiteSpace(model.DiaChiGiaoHang))
            {
                var gianHangChoPhi = await _db.GianHangs.FirstOrDefaultAsync(g => g.MaGianHang == model.GioHang.MaGianHang);
                if (gianHangChoPhi != null)
                    model.PhiGiaoHangUocTinh = await TinhPhiGiaoHangAsync(gianHangChoPhi.DiaChi, model.DiaChiGiaoHang);
            }

            if (!string.IsNullOrWhiteSpace(model.MaKhuyenMai))
            {
                var km = await _db.KhuyenMais.FirstOrDefaultAsync(k => k.MaCode == model.MaKhuyenMai.Trim().ToUpper());

                if (km == null)
                {
                    model.LoiThongBao = "Mã khuyến mãi không tồn tại.";
                    model.SoTienGiam = 0;
                }
                else
                {
                    var result = await KhuyenMaiHelper.KiemTraAsync(_db, km, model.GioHang, model.GioHang.MaGianHang,
                        int.Parse(User.FindFirst("MaKH")!.Value), model.PhiGiaoHangUocTinh);
                    model.SoTienGiam = result.GiamTienMon; model.GiamPhiGiaoHang = result.GiamPhiGiaoHang;
                    if (!result.HopLe) model.LoiThongBao = result.Loi;
                }
            }
            else
            {
                model.SoTienGiam = 0;
            }

            await NapDiaChiAsync();
            return View("Checkout", model);
        }

        [HttpPost("dat-hang")]
        [Authorize(Roles = "KhachHang")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatHang(CheckoutViewModel model)
        {
            var loiDongBo = await DongBoGioHangAsync();
            var gioHang = _cart.LayGioHang();
            if (!gioHang.Items.Any())
            {
                TempData["LoiThongBao"] = loiDongBo ?? "Giỏ hàng đang trống.";
                return RedirectToAction("Index", "CustomerCart");
            }
            if (!string.IsNullOrWhiteSpace(loiDongBo))
            {
                model.GioHang = gioHang;
                model.LoiThongBao = loiDongBo;
                await NapDiaChiAsync();
                return View("Checkout", model);
            }

            if (string.IsNullOrWhiteSpace(model.DiaChiGiaoHang))
            {
                model.GioHang = gioHang;
                model.LoiThongBao = "Vui lòng nhập địa chỉ giao hàng.";
                await NapDiaChiAsync();
                return View("Checkout", model);
            }

            if (model.HinhThucThanhToan != "TienMat" && model.HinhThucThanhToan != "VNPay")
            {
                model.GioHang = gioHang;
                model.LoiThongBao = "Phương thức thanh toán không hợp lệ. Vui lòng chọn Tiền mặt hoặc VNPay.";
                await NapDiaChiAsync();
                return View("Checkout", model);
            }

            var maKH = int.Parse(User.FindFirst("MaKH")!.Value);

            var gianHang = await _db.GianHangs.FirstAsync(g => g.MaGianHang == gioHang.MaGianHang);
            var phiGiaoHang = await TinhPhiGiaoHangAsync(gianHang.DiaChi, model.DiaChiGiaoHang);
            await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            decimal soTienGiam = 0, giamPhiGiao = 0; KhuyenMai? km = null;
            if (!string.IsNullOrWhiteSpace(model.MaKhuyenMai))
            {
                km = await _db.KhuyenMais.FirstOrDefaultAsync(x => x.MaCode == model.MaKhuyenMai.Trim().ToUpper());
                if (km == null) { model.GioHang = gioHang; model.LoiThongBao = "Mã khuyến mãi không tồn tại."; await NapDiaChiAsync(); return View("Checkout", model); }
                var result = await KhuyenMaiHelper.KiemTraAsync(_db, km, gioHang, gioHang.MaGianHang, maKH, phiGiaoHang);
                if (!result.HopLe) { model.GioHang = gioHang; model.LoiThongBao = result.Loi; await NapDiaChiAsync(); return View("Checkout", model); }
                soTienGiam = result.GiamTienMon; giamPhiGiao = result.GiamPhiGiaoHang;
            }
            var tongTien = Math.Max(0, gioHang.TongTien - soTienGiam) + Math.Max(0, phiGiaoHang - giamPhiGiao);

            var donHang = new DonHang
            {
                MaKH = maKH,
                MaGianHang = gioHang.MaGianHang,
                TrangThai = "ChoXacNhan",
                TongTien = tongTien,
                PhiGiaoHang = phiGiaoHang,
                DiaChiGiaoHang = model.DiaChiGiaoHang,
                GhiChu = model.GhiChu,
                MaKhuyenMaiApDung = km?.MaKhuyenMai,
            };
            _db.DonHangs.Add(donHang);
            await _db.SaveChangesAsync();

            foreach (var item in gioHang.Items)
            {
                _db.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    MaDonHang = donHang.MaDonHang,
                    MaMonAn = item.MaMonAn,
                    TenMon = item.TenMon,
                    SoLuong = item.SoLuong,
                    DonGia = item.Gia,
                    TuyChonMon = item.TuyChonMon,
                });
            }

            _db.ThanhToans.Add(new ThanhToan
            {
                MaDonHang = donHang.MaDonHang,
                HinhThuc = model.HinhThucThanhToan,
                TrangThai = model.HinhThucThanhToan == "TienMat" ? "ChoThanhToan" : "ChoThanhToan",
            });

            if (km != null)
            {
                km.SoLuongDaDung++;
                _db.SuDungKhuyenMais.Add(new SuDungKhuyenMai { MaKhuyenMai = km.MaKhuyenMai, MaKH = maKH, MaDonHang = donHang.MaDonHang });
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            _cart.XoaGioHang();

            if (model.HinhThucThanhToan == "VNPay")
            {
                var returnUrl = Url.Action("VnPayReturn", "CustomerCart", null, Request.Scheme)!;
                var paymentUrl = _vnPayService.CreatePaymentUrl(new VnPayCreateRequest(
                    OrderId: donHang.MaDonHang,
                    AmountVnd: (long)tongTien,
                    OrderInfo: $"Thanh toan don hang #{donHang.MaDonHang} - FoodServiceApp",
                    IpAddress: HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    ReturnUrl: returnUrl));

                return Redirect(paymentUrl);
            }

            // Thanh toán tiền mặt khi nhận hàng -> không cần cổng thanh toán, vào thẳng trang theo dõi đơn.
            TempData["ThongBao"] = $"Đặt hàng thành công! Mã đơn #{donHang.MaDonHang}.";
            return RedirectToAction("ChiTiet", "CustomerOrder", new { id = donHang.MaDonHang });
        }

        // VNPay chuyển trình duyệt người dùng về đây sau khi thanh toán (thành công hoặc thất bại).
        // Cập nhật CSDL luôn tại đây (không chỉ chờ IPN) để demo vẫn hoạt động đúng ngay cả khi máy
        // chạy local không có địa chỉ public cho VNPay gọi IPN tới được. Có kiểm tra idempotent
        // (bỏ qua nếu đã "DaThanhToan") nên nếu IPN đã cập nhật trước đó thì không bị ghi đè sai.
        [HttpGet("vnpay-return")]
        [Authorize(Roles = "KhachHang")]
        public async Task<IActionResult> VnPayReturn()
        {
            var result = _vnPayService.ValidateCallback(Request.Query);

            if (!result.IsValidSignature)
            {
                TempData["LoiThongBao"] = "Không xác thực được kết quả thanh toán từ VNPay (chữ ký không hợp lệ).";
                return RedirectToAction("ChiTiet", "CustomerOrder", new { id = result.OrderId });
            }

            var thanhToan = await _db.ThanhToans.FirstOrDefaultAsync(t => t.MaDonHang == result.OrderId);
            if (thanhToan != null && thanhToan.TrangThai != "DaThanhToan")
            {
                if (result.IsSuccess)
                {
                    thanhToan.TrangThai = "DaThanhToan";
                    thanhToan.MaGiaoDich = result.TransactionId;
                    thanhToan.NgayThanhToan = DateTime.Now;

                    var donHang = await _db.DonHangs.FirstOrDefaultAsync(d => d.MaDonHang == result.OrderId);
                    if (donHang != null && donHang.TrangThai == "ChoXacNhan")
                        donHang.TrangThai = "DaXacNhan";
                }
                else
                {
                    thanhToan.TrangThai = "ThatBai";
                }
                await _db.SaveChangesAsync();
            }

            if (!result.IsSuccess && thanhToan?.TrangThai != "DaThanhToan")
            {
                TempData["LoiThongBao"] = $"Thanh toán VNPay không thành công. Bạn có thể thử lại hoặc đổi sang tiền mặt cho đơn #{result.OrderId}.";
            }
            else
            {
                TempData["ThongBao"] = $"Thanh toán VNPay thành công! Mã đơn #{result.OrderId}.";
            }
            return RedirectToAction("ChiTiet", "CustomerOrder", new { id = result.OrderId });
        }
    }
}
