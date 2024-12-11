using Azure.Core.Pipeline;
using Microsoft.EntityFrameworkCore;
using PhoneShopSharedLibrary.Models;

namespace PhoneShopServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext>options) : base(options)
        {

        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<ApplicationRole> ApplicationRole { get; set; }
        public DbSet<ApplicationUserRole> ApplicationUserRole { get; set; }
        public DbSet<ApplicationTokenInfo> ApplicationTokenInfo { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            modelBuilder.Entity<OrderItem>()
               .Property(u => u.TotalPrice)
               .HasComputedColumnSql("[UnitPrice] * [Quantity]");

            base.OnModelCreating(modelBuilder);

            
        }
    }
}
