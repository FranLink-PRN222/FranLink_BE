using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer_FranLink.Models
{
    public class FranLinkContext : DbContext
    {
        public FranLinkContext(DbContextOptions<FranLinkContext> options) : base(options)
        {
        }

        public DbSet<FranchiseStore> FranchiseStores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
