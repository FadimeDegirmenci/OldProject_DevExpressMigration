namespace SM.MAUI.Views;

public partial class DevExpressPage : ContentPage
{
    public DevExpressPage()
    {
        InitializeComponent();
    }

    private async void OnTestButtonClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Test", "DevExpress DXButton çalışıyor! 🎉", "Tamam");
    }
}