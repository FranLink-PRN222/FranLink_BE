namespace DataAccessLayer_FranLink.Models
{
    public class QualityFeedback
    {
        public Guid QualityFeedbackId { get; set; }
        public Guid UserId { get; set; }
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public User User { get; set; }
        public Product Product { get; set; }
    }
}
