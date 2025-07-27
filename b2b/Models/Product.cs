namespace b2b.Models
{
    public class Product
    {
        public int Id { get; set; }
        public int SellerCompanyId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? SKU { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }

        public Company? SellerCompany { get; set; }
    }
}