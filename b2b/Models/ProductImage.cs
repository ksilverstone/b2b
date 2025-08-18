using System.ComponentModel.DataAnnotations;

namespace b2b.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; } = null!;

        public bool IsPrimary { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;

        public Product? Product { get; set; }
    }
}

