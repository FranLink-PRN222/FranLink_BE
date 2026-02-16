namespace BusinessLogicLayer_FranLink.DTOs
{
    public class CentralKitchenSummary
    {
        public int CentralKitchenId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public int TotalQuantity { get; set; }
        public int LowStockItems { get; set; }
        public int ExpiringItems { get; set; }
        public int ExpiredItems { get; set; }
        public decimal TotalValue { get; set; }
    }
}
