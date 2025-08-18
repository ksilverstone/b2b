namespace b2b.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsSeller { get; set; }
        public bool IsBuyer { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<CustomerOrder> BuyerOrders { get; set; } = new List<CustomerOrder>();
        public ICollection<CustomerOrder> SellerOrders { get; set; } = new List<CustomerOrder>();
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public ICollection<Cart> BuyerCarts { get; set; } = new List<Cart>();
        public ICollection<ProductRequest> BuyerRequests { get; set; } = new List<ProductRequest>();
    }
}