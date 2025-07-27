using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace b2b.Models
{
    public class ProductViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string SKU { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
        public IFormFile Image { get; set; }
        public string ExistingImageUrl { get; set; } // Düzenleme için mevcut resim
    }
} 