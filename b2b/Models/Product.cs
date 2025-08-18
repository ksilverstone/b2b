using System.ComponentModel.DataAnnotations;

namespace b2b.Models
{
    public class Product
    {
        public int Id { get; set; }
        public int SellerCompanyId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public string? SKU { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        
        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
        
        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
        // Yeni alanlar
        [StringLength(100)]
        public string? Category { get; set; }
        
        [StringLength(100)]
        public string? Brand { get; set; }
        
        [StringLength(20)]
        public string Unit { get; set; } = "Adet";
        
        [Range(0, int.MaxValue)]
        public int MinStock { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(50)]
        public string Status { get; set; } = "Aktif";
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }

        // İlişki alanları
        public Company? SellerCompany { get; set; }
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    }
}