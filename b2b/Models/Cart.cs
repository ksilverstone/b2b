using System.ComponentModel.DataAnnotations;

namespace b2b.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        [StringLength(20)] public string Status { get; set; } = "Active"; // Aktif, Sipariş, İptal
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }

        public int? SellerCompanyId { get; set; }
        public int? BuyerCompanyId { get; set; }

        // İlişki alanları
        public Customer? Customer { get; set; }
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
        public Company? SellerCompany { get; set; }
        public Company? BuyerCompany { get; set; }
    }

    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // İlişki alanları
        public Cart? Cart { get; set; }
        public Product? Product { get; set; }
    }
}

