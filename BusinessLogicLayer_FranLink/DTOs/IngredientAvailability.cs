namespace BusinessLogicLayer_FranLink.DTOs
{
    /// <summary>
    /// DTO cho tình trạng nguyên liệu
    /// </summary>
    public class IngredientAvailability
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal RequiredQuantity { get; set; }
        public decimal AvailableQuantity { get; set; }
        public decimal ShortageQuantity => Math.Max(0, RequiredQuantity - AvailableQuantity);
        public bool IsAvailable => AvailableQuantity >= RequiredQuantity;
        public string? Unit { get; set; }
    }
}
