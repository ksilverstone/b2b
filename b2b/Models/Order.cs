using System;

namespace b2b.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int SellerCompanyId { get; set; }
        public int BuyerCompanyId { get; set; }
        public int CreatedById { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
} 