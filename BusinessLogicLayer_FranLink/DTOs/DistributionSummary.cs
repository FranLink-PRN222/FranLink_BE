namespace BusinessLogicLayer_FranLink.DTOs
{
    public class DistributionSummary
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public int DeliveringOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalValue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal OrderFulfillmentRate { get; set; }
        public List<DistributionByDay> ByDay { get; set; } = new();
    }

    public class DistributionByStore
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string? StoreAddress { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalValue { get; set; }
        public int ItemCount { get; set; }
        public decimal FulfillmentRate { get; set; }
    }

    public class DistributionByProduct
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int TotalQuantityOrdered { get; set; }
        public int TotalQuantityDelivered { get; set; }
        public decimal TotalValue { get; set; }
        public int OrderCount { get; set; }
    }

    public class DistributionByDay
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class DeliveryPerformance
    {
        public decimal AverageDeliveryTimeHours { get; set; }
        public int TotalDeliveries { get; set; }
        public int CompletedDeliveries { get; set; }
        public int PendingDeliveries { get; set; }
    }
}
