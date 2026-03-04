namespace BusinessLogicLayer_FranLink.DTOs
{
    public class LossSummary
    {
        public decimal TotalLossValue { get; set; }
        public int TotalLossItems { get; set; }
        public int TotalDisposals { get; set; }
        public decimal ExpiredLossValue { get; set; }
        public decimal DamagedLossValue { get; set; }
        public decimal QualityIssueLossValue { get; set; }
        public decimal OtherLossValue { get; set; }
        public List<LossByDay> ByDay { get; set; } = new();
    }

    public class LossByReason
    {
        public string Reason { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public int DisposalCount { get; set; }
        public decimal TotalValue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class LossByProduct
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int LostQuantity { get; set; }
        public decimal LostValue { get; set; }
        public string TopReason { get; set; } = string.Empty;
        public int DisposalCount { get; set; }
    }

    public class LossByLocation
    {
        public int? CentralKitchenId { get; set; }
        public int? FranchiseStoreId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string LocationType { get; set; } = string.Empty;
        public int DisposalCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class LossByDay
    {
        public DateTime Date { get; set; }
        public int DisposalCount { get; set; }
        public int Quantity { get; set; }
        public decimal Value { get; set; }
    }
}
