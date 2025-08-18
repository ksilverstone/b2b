using System;
using System.ComponentModel.DataAnnotations;

namespace b2b.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string InvoiceNumber { get; set; }
        
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // İlişki alanları
        public virtual CustomerOrder Order { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
