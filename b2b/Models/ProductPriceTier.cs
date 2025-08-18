using System.ComponentModel.DataAnnotations;

namespace b2b.Models
{
    public class ProductPriceTier
    {
        public int Id { get; set; }
        [Required]
        public int ProductId { get; set; }
        [Range(1, int.MaxValue)]
        public int MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        public Product? Product { get; set; }
    }
}


