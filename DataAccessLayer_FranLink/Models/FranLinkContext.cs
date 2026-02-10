using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer_FranLink.Models
{
    public class FranLinkContext : DbContext
    {
        public FranLinkContext(DbContextOptions<FranLinkContext> options) : base(options)
        {
        }

        public DbSet<FranchiseStore> FranchiseStores { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; } 
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Product> Products { get; set; }    
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InternalOrder> InternalOrders { get; set; }
        public DbSet<InternalOrderItem> InternalOrderItems { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<QualityFeedback> QualityFeedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // UserRole composite key
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // InternalOrder configuration
            modelBuilder.Entity<InternalOrder>()
                .HasOne(o => o.FranchiseStore)
                .WithMany(s => s.InternalOrders)
                .HasForeignKey(o => o.FranchiseStoreId);

            modelBuilder.Entity<InternalOrder>()
                .HasOne(o => o.User)
                .WithMany(u => u.InternalOrders)
                .HasForeignKey(o => o.UserId);

            // InternalOrderItem configuration
            modelBuilder.Entity<InternalOrderItem>()
                .HasOne(i => i.InternalOrder)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.InternalOrderId);

            modelBuilder.Entity<InternalOrderItem>()
                .HasOne(i => i.Product)
                .WithMany(p => p.InternalOrderItems)
                .HasForeignKey(i => i.ProductId);
            modelBuilder.Entity<InternalOrder>()
                .HasOne(o => o.Delivery)
                .WithOne(d => d.InternalOrder)
                .HasForeignKey<Delivery>(d => d.InternalOrderId);

            // Inventory configuration
            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.FranchiseStore)
                .WithMany(s => s.Inventories)
                .HasForeignKey(i => i.FranchiseStoreId);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Product)
                .WithMany(p => p.Inventories)
                .HasForeignKey(i => i.ProductId);

            // QualityFeedback configuration
            modelBuilder.Entity<QualityFeedback>()
                .HasOne(q => q.User)
                .WithMany(u => u.QualityFeedbacks)
                .HasForeignKey(q => q.UserId);

            modelBuilder.Entity<QualityFeedback>()
                .HasOne(q => q.Product)
                .WithMany() // Product does not have QualityFeedbacks collection, or I should check Product.cs again. 
                            // Product.cs has ICollection<Inventory> but NOT QualityFeedbacks. 
                            // So WithMany() is correct if navigation is one-way, but usually we'd add it to Product if bidirectional.
                            // User.cs has QualityFeedbacks. 
                            // Let's check Product.cs content again.
                .HasForeignKey(q => q.ProductId);


            base.OnModelCreating(modelBuilder);
        }
    }
}
