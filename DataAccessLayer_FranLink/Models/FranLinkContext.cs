using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer_FranLink.Models
{
    public class FranLinkContext : DbContext
    {
        public FranLinkContext(DbContextOptions<FranLinkContext> options) : base(options)
        {
        }

        public DbSet<FranchiseStore> FranchiseStores { get; set; }
        public DbSet<CentralKitchen> CentralKitchens { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; } 
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Product> Products { get; set; }    
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryTransfer> InventoryTransfers { get; set; }
        public DbSet<InventoryDisposal> InventoryDisposals { get; set; }
        public DbSet<InternalOrder> InternalOrders { get; set; }
        public DbSet<InternalOrderItem> InternalOrderItems { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<QualityFeedback> QualityFeedbacks { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeItem> RecipeItems { get; set; }
        public DbSet<ProductionRecord> ProductionRecords { get; set; }
        public DbSet<SystemConfig> SystemConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // UserRole composite key
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // User -> FranchiseStore (optional)
            modelBuilder.Entity<User>()
                .HasOne(u => u.FranchiseStore)
                .WithMany(s => s.Users)
                .HasForeignKey(u => u.FranchiseStoreId)
                .OnDelete(DeleteBehavior.SetNull);

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
                .HasForeignKey(i => i.FranchiseStoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.CentralKitchen)
                .WithMany(ck => ck.Inventories)
                .HasForeignKey(i => i.CentralKitchenId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Product)
                .WithMany(p => p.Inventories)
                .HasForeignKey(i => i.ProductId);

            // InventoryTransfer configuration
            modelBuilder.Entity<InventoryTransfer>()
                .HasOne(t => t.FromCentralKitchen)
                .WithMany()
                .HasForeignKey(t => t.FromCentralKitchenId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryTransfer>()
                .HasOne(t => t.FromStore)
                .WithMany()
                .HasForeignKey(t => t.FromStoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryTransfer>()
                .HasOne(t => t.ToCentralKitchen)
                .WithMany()
                .HasForeignKey(t => t.ToCentralKitchenId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryTransfer>()
                .HasOne(t => t.ToStore)
                .WithMany()
                .HasForeignKey(t => t.ToStoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryTransfer>()
                .HasOne(t => t.Product)
                .WithMany()
                .HasForeignKey(t => t.ProductId);

            modelBuilder.Entity<InventoryTransfer>()
                .HasOne(t => t.RequestedBy)
                .WithMany(u => u.RequestedTransfers)
                .HasForeignKey(t => t.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryTransfer>()
                .HasOne(t => t.ApprovedBy)
                .WithMany(u => u.ApprovedTransfers)
                .HasForeignKey(t => t.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // InventoryDisposal configuration
            modelBuilder.Entity<InventoryDisposal>()
                .HasOne(d => d.Inventory)
                .WithMany()
                .HasForeignKey(d => d.InventoryId);

            modelBuilder.Entity<InventoryDisposal>()
                .HasOne(d => d.DisposedBy)
                .WithMany(u => u.RequestedDisposals)
                .HasForeignKey(d => d.DisposedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryDisposal>()
                .HasOne(d => d.ApprovedBy)
                .WithMany(u => u.ApprovedDisposals)
                .HasForeignKey(d => d.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // QualityFeedback configuration
            modelBuilder.Entity<QualityFeedback>()
                .HasOne(q => q.User)
                .WithMany(u => u.QualityFeedbacks)
                .HasForeignKey(q => q.UserId);

            modelBuilder.Entity<QualityFeedback>()
                .HasOne(q => q.Product)
                .WithMany()
                .HasForeignKey(q => q.ProductId);

            // Recipe configuration
            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // RecipeItem configuration
            modelBuilder.Entity<RecipeItem>()
                .HasOne(ri => ri.Recipe)
                .WithMany(r => r.RecipeItems)
                .HasForeignKey(ri => ri.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecipeItem>()
                .HasOne(ri => ri.IngredientProduct)
                .WithMany(p => p.RecipeItems)
                .HasForeignKey(ri => ri.IngredientProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProductionRecord configuration
            modelBuilder.Entity<ProductionRecord>()
                .HasOne(pr => pr.CentralKitchen)
                .WithMany()
                .HasForeignKey(pr => pr.CentralKitchenId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductionRecord>()
                .HasOne(pr => pr.Recipe)
                .WithMany()
                .HasForeignKey(pr => pr.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductionRecord>()
                .HasOne(pr => pr.ProducedByUser)
                .WithMany()
                .HasForeignKey(pr => pr.ProducedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            // SystemConfig unique key on ConfigKey
            modelBuilder.Entity<SystemConfig>()
                .HasIndex(sc => sc.ConfigKey)
                .IsUnique();

            // Note: Roles (Admin, Manager, Supply Coordinator, Central Kitchen Staff, Franchise Store Staff)
            // are already seeded in the database. Do not use HasData() to avoid duplicate key conflicts.

            base.OnModelCreating(modelBuilder);
        }
    }
}

