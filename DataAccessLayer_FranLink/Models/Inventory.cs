using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer_FranLink.Models
{
    public class Inventory
    {
        [Key]
        public Guid InventoryId { get; set; }
        
        // Location - either CentralKitchen OR FranchiseStore (one must be set)
        public int? CentralKitchenId { get; set; }
        public int? FranchiseStoreId { get; set; }
        
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        
        // Batch tracking
        [StringLength(50)]
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        
        // Thresholds for alerts
        public int MinThreshold { get; set; } = 10; // Default minimum
        public int MaxThreshold { get; set; } = 1000; // Default maximum
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public CentralKitchen? CentralKitchen { get; set; }
        public FranchiseStore? FranchiseStore { get; set; }
        public Product Product { get; set; }
    }
}
