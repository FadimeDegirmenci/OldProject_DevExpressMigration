using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SM.API.Data;

namespace SM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly SMDbContext _context;

        public DashboardController(SMDbContext context)
        {
            _context = context;
        }

        // GET: api/dashboard/summary
        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummary>> GetDashboardSummary()
        {
            try
            {
                // Toplam ürün sayısı
                var totalProducts = await _context.Products.CountAsync();

                // Toplam depo sayısı
                var totalWarehouses = await _context.Warehouses.CountAsync();

                // Toplam stok miktarı (tüm depolardaki tüm ürünlerin toplamı)
                var totalStockQuantity = await _context.InventoryItems.SumAsync(i => i.Quantity);

                // Toplam stok değeri (miktar × fiyat)
                var totalStockValue = await _context.InventoryItems
                    .Include(i => i.Product)
                    .SumAsync(i => i.Quantity * i.Product.Price);

                // En yüksek stoklu ürünler (Top 5)
                var topStockedProducts = await _context.InventoryItems
                    .Include(i => i.Product)
                    .Include(i => i.Warehouse)
                    .GroupBy(i => i.Product)
                    .Select(g => new TopStockedProduct
                    {
                        ProductName = g.Key.Name,
                        SKU = g.Key.SKU,
                        TotalQuantity = g.Sum(i => i.Quantity),
                        TotalValue = g.Sum(i => i.Quantity * i.Product.Price)
                    })
                    .OrderByDescending(p => p.TotalQuantity)
                    .Take(5)
                    .ToListAsync();

                // Depo bazında stok dağılımı - TÜM DEPOLARI DAHIL ET (stok olmasa bile)
                var allWarehouses = await _context.Warehouses.ToListAsync();

                var warehouseStockDistribution = new List<WarehouseStockInfo>();

                foreach (var warehouse in allWarehouses)
                {
                    var warehouseInventory = await _context.InventoryItems
                        .Include(i => i.Product)
                        .Where(i => i.WarehouseId == warehouse.Id)
                        .ToListAsync();

                    var warehouseStockInfo = new WarehouseStockInfo
                    {
                        WarehouseName = warehouse.Name,
                        Location = warehouse.Location,
                        TotalProducts = warehouseInventory.Count,
                        TotalQuantity = warehouseInventory.Sum(i => i.Quantity),
                        TotalValue = warehouseInventory.Sum(i => i.Quantity * i.Product.Price)
                    };

                    warehouseStockDistribution.Add(warehouseStockInfo);
                }

                // Depoları toplam değere göre sırala (en yüksek değer önce)
                warehouseStockDistribution = warehouseStockDistribution
                    .OrderByDescending(w => w.TotalValue)
                    .ToList();

                // Stok durumu kategorileri
                var stockCategories = await _context.InventoryItems
                    .Include(i => i.Product)
                    .GroupBy(i => i.Quantity == 0 ? "Stokta Yok" :
                                i.Quantity <= 10 ? "Düşük Stok" :
                                i.Quantity <= 50 ? "Orta Stok" : "Yüksek Stok")
                    .Select(g => new StockCategoryInfo
                    {
                        Category = g.Key,
                        ProductCount = g.Count(),
                        TotalQuantity = g.Sum(i => i.Quantity)
                    })
                    .ToListAsync();

                // Son 30 gündeki stok hareketleri sayısı
                var recentMovements = await _context.InventoryItems
                    .Where(i => i.LastUpdated >= DateTime.Now.AddDays(-30))
                    .CountAsync();

                var summary = new DashboardSummary
                {
                    TotalProducts = totalProducts,
                    TotalWarehouses = totalWarehouses,
                    TotalStockQuantity = totalStockQuantity,
                    TotalStockValue = totalStockValue,
                    TopStockedProducts = topStockedProducts,
                    WarehouseStockDistribution = warehouseStockDistribution,
                    StockCategories = stockCategories,
                    RecentMovements = recentMovements,
                    LastUpdated = DateTime.Now
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Dashboard verileri alınırken hata oluştu: {ex.Message}");
            }
        }

        // GET: api/dashboard/low-stock
        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<LowStockAlert>>> GetLowStockAlerts(int threshold = 10)
        {
            try
            {
                var lowStockItems = await _context.InventoryItems
                    .Include(i => i.Product)
                    .Include(i => i.Warehouse)
                    .Where(i => i.Quantity <= threshold)
                    .Select(i => new LowStockAlert
                    {
                        ProductName = i.Product.Name,
                        SKU = i.Product.SKU,
                        WarehouseName = i.Warehouse.Name,
                        CurrentQuantity = i.Quantity,
                        Threshold = threshold,
                        LastUpdated = i.LastUpdated
                    })
                    .OrderBy(l => l.CurrentQuantity)
                    .ToListAsync();

                return Ok(lowStockItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Düşük stok uyarıları alınırken hata oluştu: {ex.Message}");
            }
        }

        // GET: api/dashboard/warehouse-summary/{warehouseId}
        [HttpGet("warehouse-summary/{warehouseId}")]
        public async Task<ActionResult<WarehouseDetailSummary>> GetWarehouseSummary(int warehouseId)
        {
            try
            {
                var warehouse = await _context.Warehouses.FindAsync(warehouseId);
                if (warehouse == null)
                {
                    return NotFound($"ID'si {warehouseId} olan depo bulunamadı.");
                }

                var warehouseInventory = await _context.InventoryItems
                    .Include(i => i.Product)
                    .Where(i => i.WarehouseId == warehouseId)
                    .ToListAsync();

                var summary = new WarehouseDetailSummary
                {
                    WarehouseName = warehouse.Name,
                    Location = warehouse.Location,
                    TotalProducts = warehouseInventory.Count,
                    TotalQuantity = warehouseInventory.Sum(i => i.Quantity),
                    TotalValue = warehouseInventory.Sum(i => i.Quantity * i.Product.Price),
                    ProductDetails = warehouseInventory.Select(i => new ProductStockDetail
                    {
                        ProductName = i.Product.Name,
                        SKU = i.Product.SKU,
                        Quantity = i.Quantity,
                        UnitPrice = i.Product.Price,
                        TotalValue = i.Quantity * i.Product.Price,
                        LastUpdated = i.LastUpdated
                    }).ToList()
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Depo özeti alınırken hata oluştu: {ex.Message}");
            }
        }
    }

    // DTO sınıfları
    public class DashboardSummary
    {
        public int TotalProducts { get; set; }
        public int TotalWarehouses { get; set; }
        public int TotalStockQuantity { get; set; }
        public decimal TotalStockValue { get; set; }
        public List<TopStockedProduct> TopStockedProducts { get; set; } = new();
        public List<WarehouseStockInfo> WarehouseStockDistribution { get; set; } = new();
        public List<StockCategoryInfo> StockCategories { get; set; } = new();
        public int RecentMovements { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class TopStockedProduct
    {
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class WarehouseStockInfo
    {
        public string WarehouseName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class StockCategoryInfo
    {
        public string Category { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int TotalQuantity { get; set; }
    }

    public class LowStockAlert
    {
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public int CurrentQuantity { get; set; }
        public int Threshold { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class WarehouseDetailSummary
    {
        public string WarehouseName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
        public List<ProductStockDetail> ProductDetails { get; set; } = new();
    }

    public class ProductStockDetail
    {
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}