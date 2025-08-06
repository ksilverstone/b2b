using System.ComponentModel.DataAnnotations;

namespace b2b.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int CompanyId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Email { get; set; }
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [StringLength(500)]
        public string? Address { get; set; }
        
        [StringLength(20)]
        public string? TaxNumber { get; set; }
        
        [StringLength(100)]
        public string? TaxOffice { get; set; }
        
        public decimal Balance { get; set; } = 0;
        
        [StringLength(50)]
        public string Group { get; set; } = "Müşteri"; // Müşteri/Tedarikçi
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(50)]
        public string CustomerGroup { get; set; } = "Müşteri"; // Müşteri/Tedarikçi
        
        [StringLength(20)]
        public string Status { get; set; } = "Aktif";
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedDate { get; set; }
        
        // Navigation properties
        public virtual Company? Company { get; set; }
        public virtual ICollection<CustomerTransaction> Transactions { get; set; } = new List<CustomerTransaction>();
        public virtual ICollection<CustomerOrder> Orders { get; set; } = new List<CustomerOrder>();
    }
} 