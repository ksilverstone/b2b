using System.ComponentModel.DataAnnotations;

namespace b2b.Models
{
    public class ProductRequest
    {
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        public int? ProductId { get; set; }
        
        public int? BuyerCompanyId { get; set; }
        
        [StringLength(50)]
        public string RequestType { get; set; } = "Price"; // Fiyat, Stok, Genel
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Beklemede"; // Beklemede, Yanıtlandı, İptal
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        
        // İlişki alanları
        public Customer? Customer { get; set; }
        public Product? Product { get; set; }
        public Company? BuyerCompany { get; set; }
    }
} 