using SM.Core.Models;
using SM.MAUI.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SM.MAUI.ViewModels
{
    public class WarehouseListViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        #region Properties

        private ObservableCollection<Warehouse> _warehouses = new();
        public ObservableCollection<Warehouse> Warehouses
        {
            get => _warehouses;
            set => SetProperty(ref _warehouses, value);
        }

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand AddWarehouseCommand { get; }
        public ICommand WarehouseTappedCommand { get; }

        #endregion

        public WarehouseListViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Depo Listesi";

            RefreshCommand = new Command(async () => await LoadWarehousesAsync());
            AddWarehouseCommand = new Command(async () => await AddWarehouse());
            WarehouseTappedCommand = new Command<Warehouse>(async (warehouse) => await OnWarehouseTapped(warehouse));
        }

        #region Methods

        public async Task LoadWarehousesAsync()
        {
            try
            {
                SetBusy(true);
                ClearError();

                var warehouseList = await _apiService.GetWarehousesAsync();
                Warehouses.Clear();

                foreach (var warehouse in warehouseList)
                {
                    Warehouses.Add(warehouse);
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Depolar yüklenirken hata oluştu: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task AddWarehouse()
        {
            await Shell.Current.GoToAsync("AddWarehousePage");
        }

        private async Task OnWarehouseTapped(Warehouse warehouse)
        {
            if (warehouse == null) return;

            try
            {
                var parameters = new Dictionary<string, object>
                {
                    ["WarehouseId"] = warehouse.Id,
                    ["WarehouseName"] = warehouse.Name
                };

                await Shell.Current.GoToAsync("WarehouseDetailPage", parameters);
            }
            catch (Exception ex)
            {
                await ShowError($"Depo detayına giderken hata: {ex.Message}");
            }
        }

        // Sayfa yüklendiğinde çalışacak
        public async Task OnAppearing()
        {
            await LoadWarehousesAsync();
        }

        #endregion
    }
}