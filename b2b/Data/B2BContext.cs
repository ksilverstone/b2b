using Microsoft.EntityFrameworkCore;

namespace b2b.Models
{
    // Veritabanı tabloları ve ilişkileri
    public class B2BContext : DbContext
    {
        public B2BContext(DbContextOptions<B2BContext> options) : base(options) { }

        // Kullanıcı tabloları
        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        
        // Ürün tabloları
        public DbSet<Product> Products { get; set; }

        public DbSet<Notification> Notifications { get; set; }
        
        // Cari sistemi DbSet'leri
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerTransaction> CustomerTransactions { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }

        // Ürünler modülü DbSet'leri
        public DbSet<ProductRequest> ProductRequests { get; set; }

        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<CustomerOrderItem> CustomerOrderItems { get; set; }
        public DbSet<CustomerOrderStatusHistory> CustomerOrderStatusHistories { get; set; }
        public DbSet<ProductPriceTier> ProductPriceTiers { get; set; }
        
        // Fatura sistemi DbSet'leri
        public DbSet<Invoice> Invoices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Varsayılan rolleri ekle
            modelBuilder.Entity<Role>().HasData(new Role
            {
                Id = 1,
                RoleName = "Admin"
            }, new Role
            {
                Id = 2,
                RoleName = "Satıcı"
            }, new Role
            {
                Id = 3,
                RoleName = "Alıcı"
            });


            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<User>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Cari sistemi ayarları
                
            // ID otomatik artış ayarı
            modelBuilder.Entity<Customer>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Company)
                .WithMany(comp => comp.Customers)
                .HasForeignKey(c => c.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<CustomerTransaction>()
                .HasOne(ct => ct.Customer)
                .WithMany(c => c.Transactions)
                .HasForeignKey(ct => ct.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<CustomerOrder>()
                .HasOne(co => co.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(co => co.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // CompanyId foreign key kaldırıldı

            modelBuilder.Entity<CustomerOrder>()
                .HasOne(co => co.BuyerCompany)
                .WithMany(c => c.BuyerOrders)
                .HasForeignKey(co => co.BuyerCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerOrder>()
                .HasOne(co => co.SellerCompany)
                .WithMany(c => c.SellerOrders)
                .HasForeignKey(co => co.SellerCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerOrder>()
                .HasIndex(co => new { co.DocumentType, co.OrderNumber })
                .IsUnique()
                .HasFilter("[OrderNumber] IS NOT NULL");

            // Ürünler modülü konfigürasyonları
            modelBuilder.Entity<Product>()
                .HasOne(p => p.SellerCompany)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.SellerCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductRequest>()
                .HasOne(pr => pr.Customer)
                .WithMany()
                .HasForeignKey(pr => pr.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductRequest>()
                .HasOne(pr => pr.Product)
                .WithMany()
                .HasForeignKey(pr => pr.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProductRequest>()
                .HasOne(pr => pr.BuyerCompany)
                .WithMany(comp => comp.BuyerRequests)
                .HasForeignKey(pr => pr.BuyerCompanyId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductPriceTier>()
                .HasOne(t => t.Product)
                .WithMany()
                .HasForeignKey(t => t.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductPriceTier>()
                .HasIndex(t => new { t.ProductId, t.MinQuantity })
                .IsUnique();

            // Sepet minimal ayarları
            modelBuilder.Entity<Cart>().ToTable("Carts");
            modelBuilder.Entity<CartItem>().ToTable("CartItems");

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.SellerCompany)
                .WithMany(comp => comp.Carts)
                .HasForeignKey(c => c.SellerCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.BuyerCompany)
                .WithMany(comp => comp.BuyerCarts)
                .HasForeignKey(c => c.BuyerCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cart>()
                .HasIndex(c => new { c.CustomerId, c.SellerCompanyId })
                .IsUnique()
                .HasFilter("[Status] = N'Active' AND [SellerCompanyId] IS NOT NULL");

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerOrderItem>()
                .HasOne(i => i.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerOrderItem>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CustomerOrderItem>()
                .HasIndex(i => new { i.OrderId, i.LineNo })
                .IsUnique();

            // CustomerOrder - Items ilişkisi
            modelBuilder.Entity<CustomerOrder>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerOrderStatusHistory>()
                .HasOne(h => h.Order)
                .WithMany()
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerOrderStatusHistory>()
                .HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}