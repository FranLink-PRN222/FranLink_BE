using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer_FranLink.Models
{
    public class InventoryDisposal
    {
        [Key]
        public int Id { get; set; }

        public Guid InventoryId { get; set; }
        public int Quantity { get; set; }

        [Required]
        [StringLength(100)]
        public string Reason { get; set; } = string.Empty; // Expired, Damaged, QualityIssue

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public Guid DisposedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }

        public DateTime DisposalDate { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedDate { get; set; }

        // Navigation properties
        public Inventory Inventory { get; set; }
        public User DisposedBy { get; set; }
        public User? ApprovedBy { get; set; }
    }
}
