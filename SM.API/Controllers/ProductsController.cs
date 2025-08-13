using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SM.API.Data;
using SM.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace SM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly SMDbContext _context;

        public ProductsController(SMDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.InventoryItems)
                    .ThenInclude(i => i.Warehouse)
                    .ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ürünler alınırken hata oluştu: {ex.Message}");
            }
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.InventoryItems)
                    .ThenInclude(i => i.Warehouse)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return NotFound($"ID'si {id} olan ürün bulunamadı.");
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ürün alınırken hata oluştu: {ex.Message}");
            }
        }

        // GET: api/products/by-sku/ABC123
        [HttpGet("by-sku/{sku}")]
        public async Task<ActionResult<Product>> GetProductBySku(string sku)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.InventoryItems)
                    .ThenInclude(i => i.Warehouse)
                    .FirstOrDefaultAsync(p => p.SKU == sku);

                if (product == null)
                {
                    return NotFound($"SKU'su {sku} olan ürün bulunamadı.");
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ürün alınırken hata oluştu: {ex.Message}");
            }
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(CreateProductRequest request)
        {
            try
            {
                // Model validation kontrolü
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // SKU benzersizlik kontrolü
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.SKU == request.SKU);

                if (existingProduct != null)
                {
                    return BadRequest($"SKU '{request.SKU}' zaten kullanılıyor.");
                }

                var product = new Product
                {
                    Name = request.Name,
                    SKU = request.SKU,
                    Description = request.Description,
                    Price = request.Price,
                    CreatedDate = DateTime.Now
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ürün eklenirken hata oluştu: {ex.Message}");
            }
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, UpdateProductRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound($"ID'si {id} olan ürün bulunamadı.");
                }

                // SKU benzersizlik kontrolü (kendisi hariç)
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.SKU == request.SKU && p.Id != id);

                if (existingProduct != null)
                {
                    return BadRequest($"SKU '{request.SKU}' zaten kullanılıyor.");
                }

                product.Name = request.Name;
                product.SKU = request.SKU;
                product.Description = request.Description;
                product.Price = request.Price;

                await _context.SaveChangesAsync();

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ürün güncellenirken hata oluştu: {ex.Message}");
            }
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound($"ID'si {id} olan ürün bulunamadı.");
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Ok($"'{product.Name}' ürünü başarıyla silindi.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ürün silinirken hata oluştu: {ex.Message}");
            }
        }
    }

    // DTO sınıfları (Data Transfer Objects)
    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Ürün adı zorunludur")]
        [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU zorunludur")]
        [StringLength(50, ErrorMessage = "SKU en fazla 50 karakter olabilir")]
        public string SKU { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Fiyat zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        public decimal Price { get; set; }
    }

    public class UpdateProductRequest
    {
        [Required(ErrorMessage = "Ürün adı zorunludur")]
        [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU zorunludur")]
        [StringLength(50, ErrorMessage = "SKU en fazla 50 karakter olabilir")]
        public string SKU { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Fiyat zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        public decimal Price { get; set; }
    }
}