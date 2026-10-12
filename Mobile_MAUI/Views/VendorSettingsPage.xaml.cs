using FoodServiceApp.Maui.Models;
using FoodServiceApp.Maui.Services;
namespace FoodServiceApp.Maui.Views;
public partial class VendorSettingsPage : ContentPage
{
    public VendorSettingsPage() => InitializeComponent();
    protected override async void OnAppearing() { base.OnAppearing(); await ApiUi.Run(this, async () => { var x = await ApiClient.GetAsync<CauHinhGianHang>("api/vendor/settings"); GioMo.Time=x.GioMoCua; GioDong.Time=x.GioDongCua; TuDong.IsToggled=x.TuDongDongMo; TamDung.IsToggled=x.TamDungNhanDon; LyDo.Text=x.LyDoTamDung; ThongBao.Text=x.ThongBaoCuaHang; ThoiGian.Text=x.ThoiGianChuanBiPhut.ToString(); }); }
    private async void OnSave(object sender, EventArgs e)
    {
        if (!int.TryParse(ThoiGian.Text, out var prep) || prep is < 5 or > 180) { await DisplayAlert("Dữ liệu chưa đúng", "Thời gian chuẩn bị phải từ 5 đến 180 phút.", "OK"); return; }
        if (TamDung.IsToggled && string.IsNullOrWhiteSpace(LyDo.Text)) { await DisplayAlert("Thiếu lý do", "Hãy nhập lý do tạm dừng nhận đơn.", "OK"); return; }
        await ApiUi.Run(this, async () => { await ApiClient.SendAsync(HttpMethod.Put, "api/vendor/settings", new { GioMoCua=GioMo.Time, GioDongCua=GioDong.Time, TuDongDongMo=TuDong.IsToggled, TamDungNhanDon=TamDung.IsToggled, LyDoTamDung=LyDo.Text, ThongBaoCuaHang=ThongBao.Text, ThoiGianChuanBiPhut=prep }); await DisplayAlert("Đã lưu", "Cấu hình vận hành đã được cập nhật.", "OK"); });
    }
}
