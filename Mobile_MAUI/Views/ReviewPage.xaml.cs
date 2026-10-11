using FoodServiceApp.Maui.Models;

namespace FoodServiceApp.Maui.Views;

[QueryProperty(nameof(DonHang), "DonHang")]
public partial class ReviewPage : ContentPage
{
    public DonHang DonHang { get; set; } = null!;

    private int _soSaoMonAn = 0;
    private int _soSaoGiaoHang = 0;
    private readonly HashSet<string> _theDaChon = new();
    private readonly string[] _dsThe = { "Món ngon", "Đóng gói đẹp", "Giao đúng giờ", "Đúng số lượng", "Nhân viên thân thiện" };

    public ReviewPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (DonHang is not null)
        {
            LblTenGianHang.Text = DonHang.TenGianHang;
            LblMaDon.Text = $"Đơn #{DonHang.Id}";
        }
        VeSao(SaoMonAn, _soSaoMonAn, v => { _soSaoMonAn = v; VeSao(SaoMonAn, _soSaoMonAn, null!); });
        VeSao(SaoGiaoHang, _soSaoGiaoHang, v => { _soSaoGiaoHang = v; VeSao(SaoGiaoHang, _soSaoGiaoHang, null!); });
        VeDsThe();
    }

    private void VeSao(HorizontalStackLayout khung, int soSao, Action<int>? onChonLai)
    {
        khung.Children.Clear();
        for (int i = 1; i <= 5; i++)
        {
            var idx = i;
            var lbl = new Label
            {
                Text = i <= soSao ? "★" : "☆",
                FontSize = 30,
                TextColor = i <= soSao ? (Color)Application.Current!.Resources["Accent"] : (Color)Application.Current!.Resources["BorderColor"],
            };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) => onChonLai?.Invoke(idx);
            lbl.GestureRecognizers.Add(tap);
            khung.Children.Add(lbl);
        }
    }

    private void VeDsThe()
    {
        DsThe.Children.Clear();
        foreach (var the in _dsThe)
        {
            var chon = _theDaChon.Contains(the);
            var frame = new Frame
            {
                Padding = new Thickness(12, 6),
                Margin = new Thickness(4),
                CornerRadius = 16,
                HasShadow = false,
                BackgroundColor = chon ? (Color)Application.Current!.Resources["Primary"] : Colors.White,
                BorderColor = chon ? (Color)Application.Current!.Resources["Primary"] : (Color)Application.Current!.Resources["BorderColor"],
                Content = new Label
                {
                    Text = the,
                    FontSize = 12.5,
                    TextColor = chon ? Colors.White : (Color)Application.Current!.Resources["TextDark"],
                }
            };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
            {
                if (!_theDaChon.Add(the)) _theDaChon.Remove(the);
                VeDsThe();
            };
            frame.GestureRecognizers.Add(tap);
            DsThe.Children.Add(frame);
        }
    }

    private async void OnGuiDanhGiaClicked(object sender, EventArgs e)
    {
        if (_soSaoMonAn == 0)
        {
            await DisplayAlert("Thiếu thông tin", "Vui lòng chọn số sao cho món ăn", "Đóng");
            return;
        }

        // TODO: gọi POST /api/danh-gia { don_hang_id, gian_hang_id, so_sao_mon_an,
        //   so_sao_giao_hang, the_nhan_xet, noi_dung }
        await Task.Delay(400);

        KhungForm.IsVisible = false;
        KhungCamOn.IsVisible = true;
    }

    private async void OnQuayLaiClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
