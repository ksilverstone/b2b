using System.ComponentModel.DataAnnotations;

namespace b2b.Models
{
    public class CustomerTransaction
    {
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        
        [StringLength(50)]
        public string? DocumentNo { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public decimal Debit { get; set; } = 0; // Borç
        
        public decimal Credit { get; set; } = 0; // Alacak
        
        public decimal? Balance { get; set; } // Bakiye
        
        [StringLength(50)]
        public string? TransactionType { get; set; } // Fatura, Tahsilat, vs.
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation property
        public virtual Customer? Customer { get; set; }
    }
} 