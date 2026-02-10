using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer_FranLink.Models
{
    public class FranchiseStore
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        // public bool IsCentralKitchen { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<Inventory> Inventories { get; set; }
        public ICollection<InternalOrder> InternalOrders { get; set; }
    }
}
