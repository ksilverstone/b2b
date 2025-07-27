using System;

namespace b2b.Models
{
    public class Campaign
    {
        public int Id { get; set; }
        public int SellerCompanyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
} 