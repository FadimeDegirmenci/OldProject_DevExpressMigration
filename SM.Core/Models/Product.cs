using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace SM.Core.Models
{
    public class Product
    {
        public int Id { get; set; }

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

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property - bir ürünün birden fazla depoda stoku olabilir
        public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    }
}