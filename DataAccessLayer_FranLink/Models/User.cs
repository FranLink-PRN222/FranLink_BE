using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer_FranLink.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<InternalOrder> InternalOrders { get; set; }
        public ICollection<QualityFeedback> QualityFeedbacks { get; set; }
    }
}
