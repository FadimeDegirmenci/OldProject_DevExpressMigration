using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SM.API.Data;
using SM.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace SM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly SMDbContext _context;

        public InventoryController(SMDbContext context)
        {
            _context = context;
        }

        // GET: api/inventory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetAllInventory()
        {
            try
            {
                var inventory = await _context.InventoryItems
                    .Include(i => i.Product)
                    .Include(i => i.Warehouse)
                    .ToListAsync();

                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Envanter listesi alınırken hata oluştu: {ex.Message}");
            }
        }

        // GET: api/inventory/product/5
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetProductInventory(int productId)
        {
            try
            {
                var inventory = await _context.InventoryItems
                    .Include(i => i.Product)
                    .Include(i => i.Warehouse)
                    .Where(i => i.ProductId == productId)
                    .ToListAsync();

                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ürün envanteri alınırken hata oluştu: {ex.Message}");
            }
        }

        // GET: api/inventory/warehouse/1
        [HttpGet("warehouse/{warehouseId}")]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetWarehouseInventory(int warehouseId)
        {
            try
            {
                var inventory = await _context.InventoryItems
                    .Include(i => i.Product)
                    .Include(i => i.Warehouse)
                    .Where(i => i.WarehouseId == warehouseId)
                    .ToListAsync();

                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Depo envanteri alınırken hata oluştu: {ex.Message}");
            }
        }

        // POST: api/inventory/stock-in
        [HttpPost("stock-in")]
        public async Task<ActionResult<StockOperationResult>> StockIn(StockOperationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Ürün ve depo kontrolleri
                var product = await _context.Products.FindAsync(request.ProductId);
                if (product == null)
                {
                    return NotFound($"ID'si {request.ProductId} olan ürün bulunamadı.");
                }

                var warehouse = await _context.Warehouses.FindAsync(request.WarehouseId);
                if (warehouse == null)
                {
                    return NotFound($"ID'si {request.WarehouseId} olan depo bulunamadı.");
                }

                // Mevcut stok kaydı var mı kontrol et
                var existingInventory = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == request.ProductId && i.WarehouseId == request.WarehouseId);

                int previousQuantity = 0;
                int newQuantity = request.Quantity;

                if (existingInventory != null)
                {
                    // Var olan stok kaydını güncelle
                    previousQuantity = existingInventory.Quantity;
                    existingInventory.Quantity += request.Quantity;
                    existingInventory.LastUpdated = DateTime.Now;
                    newQuantity = existingInventory.Quantity;
                }
                else
                {
                    // Yeni stok kaydı oluştur
                    var newInventoryItem = new InventoryItem
                    {
                        ProductId = request.ProductId,
                        WarehouseId = request.WarehouseId,
                        Quantity = request.Quantity,
                        LastUpdated = DateTime.Now
                    };
                    _context.InventoryItems.Add(newInventoryItem);
                }

                await _context.SaveChangesAsync();

                var result = new StockOperationResult
                {
                    Success = true,
                    Message = $"{product.Name} ürününden {request.Quantity} adet {warehouse.Name} deposuna eklendi.",
                    ProductName = product.Name,
                    WarehouseName = warehouse.Name,
                    OperationType = "Stok Girişi",
                    QuantityChanged = request.Quantity,
                    PreviousQuantity = previousQuantity,
                    NewQuantity = newQuantity,
                    OperationDate = DateTime.Now
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Stok girişi yapılırken hata oluştu: {ex.Message}");
            }
        }

        // POST: api/inventory/stock-out
        [HttpPost("stock-out")]
        public async Task<ActionResult<StockOperationResult>> StockOut(StockOperationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Ürün ve depo kontrolleri
                var product = await _context.Products.FindAsync(request.ProductId);
                if (product == null)
                {
                    return NotFound($"ID'si {request.ProductId} olan ürün bulunamadı.");
                }

                var warehouse = await _context.Warehouses.FindAsync(request.WarehouseId);
                if (warehouse == null)
                {
                    return NotFound($"ID'si {request.WarehouseId} olan depo bulunamadı.");
                }

                // Mevcut stok kaydını kontrol et
                var existingInventory = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == request.ProductId && i.WarehouseId == request.WarehouseId);

                if (existingInventory == null)
                {
                    return BadRequest($"{product.Name} ürünü {warehouse.Name} deposunda bulunamadı.");
                }

                // Yeterli stok var mı kontrol et
                if (existingInventory.Quantity < request.Quantity)
                {
                    return BadRequest($"Yetersiz stok! {warehouse.Name} deposunda {product.Name} ürününden sadece {existingInventory.Quantity} adet var. {request.Quantity} adet çıkış yapılamaz.");
                }

                int previousQuantity = existingInventory.Quantity;
                existingInventory.Quantity -= request.Quantity;
                existingInventory.LastUpdated = DateTime.Now;

                // Stok 0 olursa kaydı sil (isteğe bağlı)
                if (existingInventory.Quantity == 0)
                {
                    _context.InventoryItems.Remove(existingInventory);
                }

                await _context.SaveChangesAsync();

                var result = new StockOperationResult
                {
                    Success = true,
                    Message = $"{product.Name} ürününden {request.Quantity} adet {warehouse.Name} deposundan çıkarıldı.",
                    ProductName = product.Name,
                    WarehouseName = warehouse.Name,
                    OperationType = "Stok Çıkışı",
                    QuantityChanged = request.Quantity,
                    PreviousQuantity = previousQuantity,
                    NewQuantity = existingInventory.Quantity,
                    OperationDate = DateTime.Now
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Stok çıkışı yapılırken hata oluştu: {ex.Message}");
            }
        }
    }

    // DTO sınıfları
    public class StockOperationRequest
    {
        [Required(ErrorMessage = "Ürün ID zorunludur")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Depo ID zorunludur")]
        public int WarehouseId { get; set; }

        [Required(ErrorMessage = "Miktar zorunludur")]
        [Range(1, int.MaxValue, ErrorMessage = "Miktar 1'den büyük olmalıdır")]
        public int Quantity { get; set; }
    }

    public class StockOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public int QuantityChanged { get; set; }
        public int PreviousQuantity { get; set; }
        public int NewQuantity { get; set; }
        public DateTime OperationDate { get; set; }
    }
}