namespace DataAccessLayer_FranLink.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(50)]
        public string? SKU { get; set; }  // Stock Keeping Unit

        [StringLength(50)]
        public string? Unit { get; set; }  // kg, cái, hộp, etc.

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<InternalOrderItem> InternalOrderItems { get; set; } = new List<InternalOrderItem>();
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public ICollection<RecipeItem> RecipeItems { get; set; } = new List<RecipeItem>();
    }
}
