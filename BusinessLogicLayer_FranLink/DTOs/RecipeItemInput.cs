namespace BusinessLogicLayer_FranLink.DTOs
{
    /// <summary>
    /// DTO cho input nguyên liệu công thức
    /// </summary>
    public class RecipeItemInput
    {
        public int RecipeItemId { get; set; }
        public int IngredientProductId { get; set; }
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public string? Notes { get; set; }
    }
}
