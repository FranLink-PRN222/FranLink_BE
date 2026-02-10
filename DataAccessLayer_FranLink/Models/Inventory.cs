namespace DataAccessLayer_FranLink.Models
{
    public class Inventory
    {
        public Guid InventoryId { get; set; }
        public int FranchiseStoreId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public FranchiseStore FranchiseStore { get; set; }
        public Product Product { get; set; }
    }
}
