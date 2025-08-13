using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SM.API.Data;
using SM.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace SM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehousesController : ControllerBase
    {
        private readonly SMDbContext _context;

        public WarehousesController(SMDbContext context)
        {
            _context = context;
        }

        // GET: api/warehouses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Warehouse>>> GetWarehouses()
        {
            try
            {
                var warehouses = await _context.Warehouses
                    .Include(w => w.InventoryItems)
                    .ThenInclude(i => i.Product)
                    .ToListAsync();

                return Ok(warehouses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Depolar alınırken hata oluştu: {ex.Message}");
            }
        }

        // GET: api/warehouses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Warehouse>> GetWarehouse(int id)
        {
            try
            {
                var warehouse = await _context.Warehouses
                    .Include(w => w.InventoryItems)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(w => w.Id == id);

                if (warehouse == null)
                {
                    return NotFound($"ID'si {id} olan depo bulunamadı.");
                }

                return Ok(warehouse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Depo alınırken hata oluştu: {ex.Message}");
            }
        }

        // POST: api/warehouses
        [HttpPost]
        public async Task<ActionResult<Warehouse>> PostWarehouse(CreateWarehouseRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var warehouse = new Warehouse
                {
                    Name = request.Name,
                    Location = request.Location,
                    CreatedDate = DateTime.Now
                };

                _context.Warehouses.Add(warehouse);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetWarehouse), new { id = warehouse.Id }, warehouse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Depo eklenirken hata oluştu: {ex.Message}");
            }
        }

        // PUT: api/warehouses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWarehouse(int id, UpdateWarehouseRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var warehouse = await _context.Warehouses.FindAsync(id);
                if (warehouse == null)
                {
                    return NotFound($"ID'si {id} olan depo bulunamadı.");
                }

                warehouse.Name = request.Name;
                warehouse.Location = request.Location;

                await _context.SaveChangesAsync();

                return Ok(warehouse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Depo güncellenirken hata oluştu: {ex.Message}");
            }
        }

        // DELETE: api/warehouses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            try
            {
                var warehouse = await _context.Warehouses.FindAsync(id);
                if (warehouse == null)
                {
                    return NotFound($"ID'si {id} olan depo bulunamadı.");
                }

                // Depoda stok var mı kontrol et
                var hasInventory = await _context.InventoryItems.AnyAsync(i => i.WarehouseId == id);
                if (hasInventory)
                {
                    return BadRequest("Bu depoda stok bulunduğu için silinemez. Önce stokları başka depoya taşıyın.");
                }

                _context.Warehouses.Remove(warehouse);
                await _context.SaveChangesAsync();

                return Ok($"'{warehouse.Name}' deposu başarıyla silindi.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Depo silinirken hata oluştu: {ex.Message}");
            }
        }
    }

    // DTO sınıfları
    public class CreateWarehouseRequest
    {
        [Required(ErrorMessage = "Depo adı zorunludur")]
        [StringLength(100, ErrorMessage = "Depo adı en fazla 100 karakter olabilir")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lokasyon zorunludur")]
        [StringLength(200, ErrorMessage = "Lokasyon en fazla 200 karakter olabilir")]
        public string Location { get; set; } = string.Empty;
    }

    public class UpdateWarehouseRequest
    {
        [Required(ErrorMessage = "Depo adı zorunludur")]
        [StringLength(100, ErrorMessage = "Depo adı en fazla 100 karakter olabilir")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lokasyon zorunludur")]
        [StringLength(200, ErrorMessage = "Lokasyon en fazla 200 karakter olabilir")]
        public string Location { get; set; } = string.Empty;
    }
}