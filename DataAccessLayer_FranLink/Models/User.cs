using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer_FranLink.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int? FranchiseStoreId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public FranchiseStore FranchiseStore { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<InternalOrder> InternalOrders { get; set; }
        public ICollection<QualityFeedback> QualityFeedbacks { get; set; }
        
        // Inventory management
        public ICollection<InventoryTransfer> RequestedTransfers { get; set; }
        public ICollection<InventoryTransfer> ApprovedTransfers { get; set; }
        public ICollection<InventoryDisposal> RequestedDisposals { get; set; }
        public ICollection<InventoryDisposal> ApprovedDisposals { get; set; }
    }
}
