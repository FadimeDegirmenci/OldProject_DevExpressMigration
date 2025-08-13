using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SM.Core.Models;
using SM.MAUI.Services;
using SM.MAUI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Android.Icu.Text.CaseMap;
#if ANDROID
using static Android.Icu.Text.CaseMap;
#endif
using System.Windows.Input;

namespace SM.MAUI.ViewModels
{
    public partial class ProductListViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Product> _products = new();

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand ProductTappedCommand { get; }

        public ProductListViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Ürün Listesi";

            RefreshCommand = new Command(async () => await LoadProductsAsync());
            AddProductCommand = new Command(async () => await AddProduct());
            DeleteProductCommand = new Command<Product>(async (product) => await DeleteProduct(product));
            ProductTappedCommand = new Command<Product>(async (product) => await OnProductTapped(product));
        }

        public async Task LoadProductsAsync()
        {
            if (IsBusy) return;

            try
            {
                SetBusy(true);
                ClearError();

                var productList = await _apiService.GetProductsAsync();
                Products.Clear();

                foreach (var product in productList)
                {
                    Products.Add(product);
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Ürünler yüklenirken hata oluştu: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task AddProduct()
        {
            await Shell.Current.GoToAsync(nameof(AddProductPage));
        }

        private async Task DeleteProduct(Product product)
        {
            if (product == null) return;

            try
            {
                // Onay al
                bool shouldDelete = await Shell.Current.DisplayAlert(
                    "Ürün Sil",
                    $"'{product.Name}' ürününü silmek istediğinizden emin misiniz?\n\nBu işlem geri alınamaz!",
                    "Evet, Sil",
                    "İptal");

                if (!shouldDelete) return;

                SetBusy(true);
                ClearError();

                bool deleted = await _apiService.DeleteProductAsync(product.Id);

                if (deleted)
                {
                    // Listeden kaldır
                    Products.Remove(product);
                    await Shell.Current.DisplayAlert("Başarılı", $"'{product.Name}' ürünü başarıyla silindi!", "Tamam");
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Ürün silinirken hata oluştu: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task OnProductTapped(Product product)
        {
            if (product == null) return;

            try
            {
                var parameters = new Dictionary<string, object>
                {
                    ["ProductId"] = product.Id,
                    ["ProductName"] = product.Name
                };

                await Shell.Current.GoToAsync("ProductDetailPage", parameters);
            }
            catch (Exception ex)
            {
                await ShowError($"Ürün detayına giderken hata: {ex.Message}");
            }
        }

        // Sayfa yüklendiğinde çalışacak
        public async Task OnAppearing()
        {
            await LoadProductsAsync();
        }
    }
}