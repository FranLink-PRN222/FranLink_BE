namespace DataAccessLayer_FranLink.Models
{
    public class InternalOrder
    {
        public int Id { get; set; }
        public int FranchiseStoreId { get; set; }
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";
        public FranchiseStore FranchiseStore { get; set; }
        public User User { get; set; }
        public ICollection<InternalOrderItem> Items { get; set; }
        // Delivery relationship if needed, assuming Delivery exists or I leave it. 
        // The existing code had `public Delivery Delivery { get; set; }`. I'll keep it but check if Delivery needs update if I change Id.
        // Assuming Delivery key is independent.
        public Delivery Delivery { get; set; }
    }
}
