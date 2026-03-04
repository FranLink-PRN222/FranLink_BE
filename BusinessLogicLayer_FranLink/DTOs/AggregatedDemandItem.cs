namespace BusinessLogicLayer_FranLink.DTOs
{
    /// <summary>
    /// Tổng hợp nhu cầu sản phẩm từ các đơn hàng chờ xử lý
    /// </summary>
    public class AggregatedDemandItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int TotalQuantityDemanded { get; set; }
        public int OrderCount { get; set; }
        public int AvailableInInventory { get; set; }
        public int ShortageQuantity => System.Math.Max(0, TotalQuantityDemanded - AvailableInInventory);
    }
}
