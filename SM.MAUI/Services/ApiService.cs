using SM.Core.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace SM.MAUI.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService()
        {
            // HTTPS sertifika doğrulamasını geliştirme için bypass et
            var handler = new HttpClientHandler();
#if DEBUG
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

            _httpClient = new HttpClient(handler);

            // Android emülatörde localhost için
#if ANDROID
            // Android emülatörde 10.0.2.2 = host makinenin localhost'u
            _httpClient.BaseAddress = new Uri("http://10.0.2.2:5000/");
#else
            // Windows/Desktop için
            _httpClient.BaseAddress = new Uri("http://localhost:5000/");
#endif

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        #region Product Operations

        public async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/products");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var products = JsonSerializer.Deserialize<List<Product>>(jsonContent, _jsonOptions);
                    return products ?? new List<Product>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ağ hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Beklenmeyen hata: {ex.Message}");
            }
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/products/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var product = JsonSerializer.Deserialize<Product>(jsonContent, _jsonOptions);
                    return product;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ağ hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Beklenmeyen hata: {ex.Message}");
            }
        }

        public async Task<Product?> GetProductBySkuAsync(string sku)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/products/by-sku/{sku}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var product = JsonSerializer.Deserialize<Product>(jsonContent, _jsonOptions);
                    return product;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ağ hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Beklenmeyen hata: {ex.Message}");
            }
        }

        public async Task<Product> CreateProductAsync(CreateProductDto productDto)
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(productDto, _jsonOptions);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/products", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var product = JsonSerializer.Deserialize<Product>(responseJson, _jsonOptions);
                    return product ?? throw new Exception("Ürün oluşturuldu ama veri alınamadı.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ağ hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Beklenmeyen hata: {ex.Message}");
            }
        }

        public async Task<Product> UpdateProductAsync(int productId, UpdateProductDto productDto)
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(productDto, _jsonOptions);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/products/{productId}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var product = JsonSerializer.Deserialize<Product>(responseJson, _jsonOptions);
                    return product ?? throw new Exception("Ürün güncellendi ama veri alınamadı.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ağ hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Beklenmeyen hata: {ex.Message}");
            }
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/products/{productId}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ağ hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Beklenmeyen hata: {ex.Message}");
            }
        }

        #endregion

        #region Connection Test

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/products");
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetBaseUrl()
        {
            return _httpClient.BaseAddress?.ToString() ?? "Unknown";
        }

        #endregion

        #region Warehouse Operations

        public async Task<List<Warehouse>> GetWarehousesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/warehouses");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var warehouses = JsonSerializer.Deserialize<List<Warehouse>>(jsonContent, _jsonOptions);
                    return warehouses ?? new List<Warehouse>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ağ hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Beklenmeyen hata: {ex.Message}");
            }
        }

        public async Task<Warehouse?> GetWarehouseByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/warehouses/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var warehouse = JsonSerializer.Deserialize<Warehouse>(jsonContent, _jsonOptions);
                    return warehouse;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ağ hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Beklenmeyen hata: {ex.Message}");
            }
        }

        public async Task<Warehouse> CreateWarehouseAsync(CreateWarehouseDto warehouseDto)
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(warehouseDto, _jsonOptions);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/warehouses", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var warehouse = JsonSerializer.Deserialize<Warehouse>(responseJson, _jsonOptions);
                    return warehouse ?? throw new Exception("Depo oluşturuldu ama veri alınamadı.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ağ hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Beklenmeyen hata: {ex.Message}");
            }
        }

        public async Task<Warehouse> UpdateWarehouseAsync(int warehouseId, UpdateWarehouseDto warehouseDto)
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(warehouseDto, _jsonOptions);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/warehouses/{warehouseId}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var warehouse = JsonSerializer.Deserialize<Warehouse>(responseJson, _jsonOptions);
                    return warehouse ?? throw new Exception("Depo güncellendi ama veri alınamadı.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ağ hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Beklenmeyen hata: {ex.Message}");
            }
        }

        public async Task<bool> DeleteWarehouseAsync(int warehouseId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/warehouses/{warehouseId}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ağ hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Beklenmeyen hata: {ex.Message}");
            }
        }

        #endregion

        #region Inventory Operations

        public async Task<StockOperationResult> StockInAsync(StockOperationDto stockDto)
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(stockDto, _jsonOptions);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/inventory/stock-in", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<StockOperationResult>(responseJson, _jsonOptions);
                    return result ?? throw new Exception("Stok girişi yapıldı ama sonuç alınamadı.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ağ hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Beklenmeyen hata: {ex.Message}");
            }
        }

        public async Task<StockOperationResult> StockOutAsync(StockOperationDto stockDto)
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(stockDto, _jsonOptions);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/inventory/stock-out", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<StockOperationResult>(responseJson, _jsonOptions);
                    return result ?? throw new Exception("Stok çıkışı yapıldı ama sonuç alınamadı.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ağ hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Beklenmeyen hata: {ex.Message}");
            }
        }

        // YENİ EKLENEN METODLAR
        public async Task<List<InventoryItem>> GetProductInventoryAsync(int productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/inventory/product/{productId}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var inventory = JsonSerializer.Deserialize<List<InventoryItem>>(jsonContent, _jsonOptions);
                    return inventory ?? new List<InventoryItem>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new List<InventoryItem>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ağ hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Beklenmeyen hata: {ex.Message}");
            }
        }

        public async Task<List<InventoryItem>> GetWarehouseInventoryAsync(int warehouseId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/inventory/warehouse/{warehouseId}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var inventory = JsonSerializer.Deserialize<List<InventoryItem>>(jsonContent, _jsonOptions);
                    return inventory ?? new List<InventoryItem>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new List<InventoryItem>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ağ hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Beklenmeyen hata: {ex.Message}");
            }
        }

        public async Task<List<InventoryItem>> GetAllInventoryAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/inventory");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var inventory = JsonSerializer.Deserialize<List<InventoryItem>>(jsonContent, _jsonOptions);
                    return inventory ?? new List<InventoryItem>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ağ hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Beklenmeyen hata: {ex.Message}");
            }
        }

        #endregion

        #region Dashboard Operations

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/dashboard/summary");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var apiSummary = JsonSerializer.Deserialize<ApiDashboardSummary>(jsonContent, _jsonOptions);

                    if (apiSummary == null)
                        throw new Exception("Dashboard özeti alınamadı.");

                    var wpfSummary = new DashboardSummaryDto
                    {
                        TotalProducts = apiSummary.TotalProducts,
                        TotalWarehouses = apiSummary.TotalWarehouses,
                        TotalStockQuantity = apiSummary.TotalStockQuantity,
                        TotalStockValue = apiSummary.TotalStockValue,
                        TopProducts = apiSummary.TopStockedProducts.Select(p => new TopProductDto
                        {
                            ProductName = p.ProductName,
                            TotalQuantity = p.TotalQuantity,
                            TotalValue = p.TotalValue
                        }).ToList(),
                        WarehouseStocks = apiSummary.WarehouseStockDistribution.Select(w => new WarehouseStockDto
                        {
                            WarehouseName = w.WarehouseName,
                            ProductCount = w.TotalProducts,
                            TotalQuantity = w.TotalQuantity,
                            TotalValue = w.TotalValue
                        }).ToList()
                    };

                    return wpfSummary;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Hatası: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ağ hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Beklenmeyen hata: {ex.Message}");
            }
        }

        #endregion
    }

    #region DTOs (Data Transfer Objects)

    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }

    public class UpdateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }

    public class CreateWarehouseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    public class UpdateWarehouseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    public class StockOperationDto
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int Quantity { get; set; }
    }

    public class StockOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int NewQuantity { get; set; }
        public int QuantityChanged { get; set; }
    }

    public class DashboardSummaryDto
    {
        public int TotalProducts { get; set; }
        public int TotalWarehouses { get; set; }
        public decimal TotalStockValue { get; set; }
        public int TotalStockQuantity { get; set; }
        public List<TopProductDto> TopProducts { get; set; } = new List<TopProductDto>();
        public List<WarehouseStockDto> WarehouseStocks { get; set; } = new List<WarehouseStockDto>();
    }

    public class TopProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class WarehouseStockDto
    {
        public string WarehouseName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class ApiDashboardSummary
    {
        public int TotalProducts { get; set; }
        public int TotalWarehouses { get; set; }
        public int TotalStockQuantity { get; set; }
        public decimal TotalStockValue { get; set; }
        public List<ApiTopStockedProduct> TopStockedProducts { get; set; } = new();
        public List<ApiWarehouseStockInfo> WarehouseStockDistribution { get; set; } = new();
    }

    public class ApiTopStockedProduct
    {
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class ApiWarehouseStockInfo
    {
        public string WarehouseName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }

    #endregion
}