using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer_FranLink.Models
{
    public class InternalOrderItem
    {
        [Key]
        public int Id { get; set; }
        public int InternalOrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public InternalOrder InternalOrder { get; set; }
        public Product Product { get; set; }
    }
}

