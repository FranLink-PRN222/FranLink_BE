namespace DataAccessLayer_FranLink.Models
{
    public class Delivery
    {
        public Guid DeliveryId { get; set; }
        public int InternalOrderId { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string DeliveryStatus { get; set; }
        public InternalOrder InternalOrder { get; set; }
    }
}
