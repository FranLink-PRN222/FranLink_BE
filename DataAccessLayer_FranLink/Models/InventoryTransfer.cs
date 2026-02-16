using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer_FranLink.Models
{
    public class InventoryTransfer
    {
        [Key]
        public int Id { get; set; }

        // Source location (either CentralKitchen or FranchiseStore)
        public int? FromCentralKitchenId { get; set; }
        public int? FromStoreId { get; set; }

        // Destination location (either CentralKitchen or FranchiseStore)
        public int? ToCentralKitchenId { get; set; }
        public int? ToStoreId { get; set; }

        public int ProductId { get; set; }
        public int Quantity { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Completed

        [StringLength(500)]
        public string? Notes { get; set; }

        public Guid RequestedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        // Navigation properties
        public CentralKitchen? FromCentralKitchen { get; set; }
        public FranchiseStore? FromStore { get; set; }
        public CentralKitchen? ToCentralKitchen { get; set; }
        public FranchiseStore? ToStore { get; set; }
        public Product Product { get; set; }
        public User RequestedBy { get; set; }
        public User? ApprovedBy { get; set; }
    }
}
