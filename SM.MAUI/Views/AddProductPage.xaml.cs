using SM.MAUI.ViewModels;

namespace SM.MAUI.Views
{
    public partial class AddProductPage : ContentPage
    {
        private readonly AddProductViewModel _viewModel;

        public AddProductPage(AddProductViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            dataForm.Commit();              // 1) kutulardaki deðerleri ProductForm'a aktar

            if (!dataForm.Validate())       // 2) kurallara uymayan alan var mý?
                return;                     //    varsa hatalarý göster, kaydetme

            await _viewModel.SaveProductAsync();   // 3) geçerliyse kaydet
        }
    }
}