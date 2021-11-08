using Microsoft.EntityFrameworkCore;

namespace RawSql.Tests
{
    public class TestContext:DbContext
    {
        public TestContext() : base()
        {
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=RawSqlTest;Username=postgres;");
        }

        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>(v =>
            {
                v.HasKey(x => x.Id);
            });
            
            modelBuilder.Entity<OrderItem>(v =>
            {
                v.HasKey(x => x.Id);
            });
            
            modelBuilder.Entity<Product>(v =>
            {
                v.HasKey(x => x.Id);
            });
            
            modelBuilder.Entity<Customer>(v =>
            {
                v.HasKey(x => x.Id);
            });
        }
    }
}