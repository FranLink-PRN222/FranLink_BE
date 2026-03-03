using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer_FranLink.Models
{
    /// <summary>
    /// Chi tiết nguyên liệu trong công thức - định mức nguyên liệu
    /// </summary>
    public class RecipeItem
    {
        [Key]
        public int RecipeItemId { get; set; }

        public int RecipeId { get; set; }

        /// <summary>
        /// Nguyên liệu (cũng là Product trong hệ thống)
        /// </summary>
        public int IngredientProductId { get; set; }

        /// <summary>
        /// Định mức số lượng nguyên liệu cần dùng
        /// </summary>
        public decimal Quantity { get; set; }

        [StringLength(50)]
        public string? Unit { get; set; }  // Đơn vị tính: gram, ml, cái, etc.

        [StringLength(200)]
        public string? Notes { get; set; }  // Ghi chú thêm

        // Navigation
        public Recipe Recipe { get; set; } = null!;
        public Product IngredientProduct { get; set; } = null!;
    }
}
