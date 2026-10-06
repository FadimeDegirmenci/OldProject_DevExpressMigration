using System.Collections.ObjectModel;
using System.Windows.Input;
using SM.Core.Models;
using SM.MAUI.Services;

namespace SM.MAUI.ViewModels
{
    public class ProductLoadMoreViewModel : BaseViewModel
    {
        private const int PageSize = 20;
        private readonly ApiService _apiService;

        // Tabloya bağlanacak liste
        public ObservableCollection<Product> Products { get; } = new();

        // Grid'in alttaki "yükleniyor" göstergesi
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        // Daha fazla ürün var mı? Yoksa grid artık istek atmaz
        private bool _isLoadMoreEnabled = true;
        public bool IsLoadMoreEnabled
        {
            get => _isLoadMoreEnabled;
            set => SetProperty(ref _isLoadMoreEnabled, value);
        }
        // Sadece ilk açılıştaki yüklemede true: iskelet (shimmer) görünür
        private bool _isFirstLoading;
        public bool IsFirstLoading
        {
            get => _isFirstLoading;
            set => SetProperty(ref _isFirstLoading, value);
        }

        public ICommand LoadMoreCommand { get; }

        public ProductLoadMoreViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Ürünler (Sonsuz Kaydırma)";

            LoadMoreCommand = new Command(async () => await LoadMoreAsync());
        }

        // Sayfa açılınca ilk 20 ürünü yükler
        // Sayfa açılınca ilk 20 ürünü yükler, bu sırada iskelet gösterilir
        public async Task LoadFirstPageAsync()
        {
            if (Products.Count > 0) return;

            try
            {
                IsFirstLoading = true;
                await LoadMoreAsync();
            }
            finally
            {
                IsFirstLoading = false;
            }
        }

        // Sıradaki 20 ürünü getirip listenin sonuna ekler
        private async Task LoadMoreAsync()
        {
            if (IsBusy)
            {
                IsRefreshing = false;
                return;
            }

            try
            {
                SetBusy(true);
                ClearError();

                var newProducts = await _apiService.GetProductsPagedAsync(Products.Count, PageSize);

                foreach (var product in newProducts)
                {
                    Products.Add(product);
                }

                if (newProducts.Count < PageSize)
                {
                    IsLoadMoreEnabled = false;
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Ürünler yüklenirken hata oluştu: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
                IsRefreshing = false;
            }
        }
    }
}