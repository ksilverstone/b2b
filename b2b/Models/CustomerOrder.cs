using System.ComponentModel.DataAnnotations;

namespace b2b.Models
{
    public class CustomerOrder
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        public int CompanyId { get; set; }
        
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        public decimal TotalAmount { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Beklemede";
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int ItemCount { get; set; } = 0;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedDate { get; set; }
        
        // Navigation properties
        public virtual Customer? Customer { get; set; }
        public virtual Company? Company { get; set; }
    }
} 