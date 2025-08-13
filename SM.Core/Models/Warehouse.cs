using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace SM.Core.Models
{
    public class Warehouse
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Depo adı zorunludur")]
        [StringLength(100, ErrorMessage = "Depo adı en fazla 100 karakter olabilir")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lokasyon zorunludur")]
        [StringLength(200, ErrorMessage = "Lokasyon en fazla 200 karakter olabilir")]
        public string Location { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property - bir depoda birden fazla ürün stoku olabilir
        public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    }
}