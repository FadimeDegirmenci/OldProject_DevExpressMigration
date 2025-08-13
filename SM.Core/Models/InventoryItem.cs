using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace SM.Core.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün ID zorunludur")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Depo ID zorunludur")]
        public int WarehouseId { get; set; }

        [Required(ErrorMessage = "Miktar zorunludur")]
        [Range(0, int.MaxValue, ErrorMessage = "Miktar 0 veya pozitif olmalıdır")]
        public int Quantity { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Navigation properties - ilişkili tablolara erişim
        public virtual Product Product { get; set; } = null!;
        public virtual Warehouse Warehouse { get; set; } = null!;
    }
}