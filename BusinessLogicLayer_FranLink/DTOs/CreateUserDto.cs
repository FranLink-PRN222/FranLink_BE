using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer_FranLink.DTOs
{
    public class CreateUserDto
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        public int? FranchiseStoreId { get; set; }

        [Required]
        public int RoleId { get; set; }
    }
}
