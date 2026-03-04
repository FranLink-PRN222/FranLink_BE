using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer_FranLink.Models
{
    public class ProductionRecord
    {
        [Key]
        public int ProductionId { get; set; }

        public int CentralKitchenId { get; set; }
        public int RecipeId { get; set; }
        public Guid ProducedByUserId { get; set; }

        public int PlannedQuantity { get; set; }
        public int ActualQuantity { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "InProgress"; // InProgress, Completed, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public CentralKitchen CentralKitchen { get; set; } = null!;
        public Recipe Recipe { get; set; } = null!;
        public User ProducedByUser { get; set; } = null!;
    }
}
