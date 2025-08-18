using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace b2b.Models
{
    public class CustomerOrder
    {
        public int Id { get; set; }
        
        [StringLength(100)]
        public string? OrderNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string DocumentType { get; set; } = "Sipariş";
        
        [Required]
        public int CustomerId { get; set; }
        


        [Required]
        public int BuyerCompanyId { get; set; }

        public int? SellerCompanyId { get; set; }
        
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        public decimal TotalAmount { get; set; }
        public decimal? TotalNet { get; set; }
        public decimal? TotalTax { get; set; }
        public decimal? TotalGross { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Beklemede";
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int ItemCount { get; set; } = 0;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedDate { get; set; }

        // Anlık alanlar
        [StringLength(200)]
        public string? BuyerName { get; set; }

        [StringLength(40)]
        public string? BuyerTaxNumber { get; set; }

        [StringLength(1000)]
        public string? BillingAddress { get; set; }

        [StringLength(1000)]
        public string? ShippingAddress { get; set; }

        [StringLength(100)]
        public string? PaymentMethod { get; set; }

        [StringLength(100)]
        public string? ShipmentMethod { get; set; }

        [StringLength(10)]
        public string? CurrencyCode { get; set; }
        
        // İlişki alanları
        public virtual Customer? Customer { get; set; }
        public virtual Company? BuyerCompany { get; set; }
        public virtual Company? SellerCompany { get; set; }
        public virtual ICollection<CustomerOrderItem> Items { get; set; } = new List<CustomerOrderItem>();
        
        // Items için uyumluluk alias'ı
        public virtual ICollection<CustomerOrderItem> OrderItems => Items;
        
        // Uyumluluk için UpdatedAt alanı
        public DateTime? UpdatedAt => UpdatedDate;
    }
} 