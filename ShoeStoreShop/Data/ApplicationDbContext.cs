using Microsoft.EntityFrameworkCore;
using ShoeStore.Models;
namespace ShoeStore.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Shoe> Shoes { get; set; }
        public DbSet<ShoeImage> ShoeImages { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Shoe>()
                .HasOne(s => s.Category)
                .WithMany(c => c.Shoe)
                .HasForeignKey(s => s.Category_Id)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Shoe>()
                .HasOne(s => s.Brand)
                .WithMany(b => b.Shoes)
                .HasForeignKey(s => s.Brand_Id)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ShoeImage>()
                .HasOne(img => img.Shoe)
                .WithMany(s => s.ShoeImages)
                .HasForeignKey(img => img.Shoe_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Shoe>()
                .HasOne(s => s.Color)
                .WithMany(c => c.Shoes)
                .HasForeignKey(s => s.ColorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrderDetail>()
               .HasOne(od => od.Order) 
               .WithMany(o => o.OrderDetails) 
               .HasForeignKey(od => od.OrderId)
               .HasPrincipalKey(o => o.OrderId);
        }
        public static void EnableSensitiveLogging(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        public void EnsureSingleEntityTracking(Shoe shoe)
        {
            var trackedShoe = this.ChangeTracker.Entries<Shoe>()
                .FirstOrDefault(e => e.Entity.Id == shoe.Id);

            if (trackedShoe != null)
            {
                trackedShoe.State = EntityState.Detached;
            }

            this.Shoes.Update(shoe);
        }
    }
}
