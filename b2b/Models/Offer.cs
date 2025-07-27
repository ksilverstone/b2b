using System;

namespace b2b.Models
{
    public class Offer
    {
        public int Id { get; set; }
        public int SellerCompanyId { get; set; }
        public int BuyerCompanyId { get; set; }
        public int CreatedById { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
} 