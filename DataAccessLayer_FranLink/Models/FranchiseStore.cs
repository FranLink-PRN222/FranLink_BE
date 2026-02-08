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

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
