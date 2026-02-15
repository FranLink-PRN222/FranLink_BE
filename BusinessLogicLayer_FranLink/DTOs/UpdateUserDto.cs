using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer_FranLink.DTOs
{
    public class UpdateUserDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        public int? FranchiseStoreId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
