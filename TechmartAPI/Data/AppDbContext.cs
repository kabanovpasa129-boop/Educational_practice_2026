using Microsoft.EntityFrameworkCore;
using TechmartAPI.Models;


namespace TechmartAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ViewHistory> ViewHistories { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }

        public DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne<Order>()
                .WithMany()
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne<Product>()
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ViewHistory>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(vh => vh.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ViewHistory>()
                .HasOne<Product>()
                .WithMany()
                .HasForeignKey(vh => vh.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Recommendation>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Recommendation>()
                .HasOne<Product>()
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}