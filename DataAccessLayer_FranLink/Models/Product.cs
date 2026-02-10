namespace DataAccessLayer_FranLink.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public ICollection<InternalOrderItem> InternalOrderItems { get; set; }
        public ICollection<Inventory> Inventories { get; set; }
    }
}
