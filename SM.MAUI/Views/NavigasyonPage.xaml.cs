using DevExpress.Maui.Controls;

namespace SM.MAUI.Views;

public partial class NavigasyonPage : ContentPage
{
    private bool _animasyonDevam;

    public NavigasyonPage()
    {
        InitializeComponent();
    }

    // Sayfa ekrana gelince ok zıplamaya başlasın
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _animasyonDevam = true;

        while (_animasyonDevam)
        {
            await okIsareti.TranslateToAsync(0, -10, 500, Easing.SinInOut); // yukarı
            await okIsareti.TranslateToAsync(0, 0, 500, Easing.SinInOut);   // geri aşağı
        }
    }

    // Sayfadan çıkınca animasyonu durdur
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _animasyonDevam = false;
    }

    private void OnYukariKaydirildi(object sender, SwipedEventArgs e)
    {
        menuSheet.Show();
    }

    private void OnDokunuldu(object sender, TappedEventArgs e)
    {
        menuSheet.Show();
    }

    private void OnUrunlerClicked(object sender, EventArgs e)
    {
        Title = "Ürünler";
        menuSheet.State = BottomSheetState.Hidden;
    }

    private void OnHakkindaClicked(object sender, EventArgs e)
    {
        Title = "Hakkında";
        menuSheet.State = BottomSheetState.Hidden;
    }
}