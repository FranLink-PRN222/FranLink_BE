using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer_FranLink.DTOs
{
    public class CreateStoreDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        public bool IsCentralKitchen { get; set; } = false;
    }
}
