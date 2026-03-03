namespace BusinessLogicLayer_FranLink.DTOs
{
    /// <summary>
    /// DTO cho sản xuất đang tiến hành
    /// </summary>
    public class InProgressProductionItem
    {
        public int ProductionId { get; set; }
        public string RecipeName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int PlannedQuantity { get; set; }
        public DateTime StartTime { get; set; }
        public string KitchenName { get; set; } = string.Empty;
    }
}
