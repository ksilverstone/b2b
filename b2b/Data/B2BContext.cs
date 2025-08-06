using Microsoft.EntityFrameworkCore;

namespace b2b.Models
{
    public class B2BContext : DbContext
    {
        public B2BContext(DbContextOptions<B2BContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        
        // Cari sistemi DbSet'leri
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerTransaction> CustomerTransactions { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add default roles
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
                
            // Cari sistemi konfigürasyonları
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Company)
                .WithMany()
                .HasForeignKey(c => c.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // ID auto increment konfigürasyonu
            modelBuilder.Entity<Customer>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();
                
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
                
            modelBuilder.Entity<CustomerOrder>()
                .HasOne(co => co.Company)
                .WithMany()
                .HasForeignKey(co => co.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}