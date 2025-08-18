using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace b2b.Models
{
    public class CustomerOrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }

        [Required]
        public int LineNo { get; set; } = 1;

        public int? ProductId { get; set; }

        [Required]
        [StringLength(500)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? SKU { get; set; }

        [Required]
        [StringLength(40)]
        public string Unit { get; set; } = "Adet";

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        public decimal DiscountRate { get; set; } = 0;

        [Range(0, 100)]
        public decimal TaxRate { get; set; } = 20;

        public decimal? NetAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        
        // Uyumluluk için hesaplanan alan
        public decimal TotalPrice => TotalAmount ?? (UnitPrice * Quantity);

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }

        // İlişki alanları
        public CustomerOrder? Order { get; set; }
        public Product? Product { get; set; }
    }
}


