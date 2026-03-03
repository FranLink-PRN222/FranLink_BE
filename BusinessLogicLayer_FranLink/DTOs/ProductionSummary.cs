namespace BusinessLogicLayer_FranLink.DTOs
{
    public class ProductionSummary
    {
        public int TotalProductions { get; set; }
        public int TotalPlannedQuantity { get; set; }
        public int TotalActualQuantity { get; set; }
        public decimal EfficiencyRate { get; set; }
        public decimal AverageProductionTimeMinutes { get; set; }
        public List<ProductionByProduct> ByProduct { get; set; } = new();
        public List<ProductionByDay> ByDay { get; set; } = new();
    }

    public class ProductionByProduct
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalPlanned { get; set; }
        public int TotalActual { get; set; }
        public decimal EfficiencyRate { get; set; }
    }

    public class ProductionByDay
    {
        public DateTime Date { get; set; }
        public int TotalQuantity { get; set; }
        public int ProductionCount { get; set; }
    }

    public class StartProductionDto
    {
        public int CentralKitchenId { get; set; }
        public int RecipeId { get; set; }
        public int PlannedQuantity { get; set; }
        public string? Notes { get; set; }
    }

    public class CompleteProductionDto
    {
        public int ActualQuantity { get; set; }
        public string? Notes { get; set; }
    }
}
