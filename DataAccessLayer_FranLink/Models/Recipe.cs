using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer_FranLink.Models
{
    /// <summary>
    /// Công thức sản xuất - mỗi sản phẩm đầu ra có thể có một công thức
    /// </summary>
    public class Recipe
    {
        [Key]
        public int RecipeId { get; set; }

        /// <summary>
        /// Sản phẩm đầu ra của công thức
        /// </summary>
        public int ProductId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Instructions { get; set; }  // Hướng dẫn thực hiện

        /// <summary>
        /// Số lượng sản phẩm đầu ra cho mỗi lần sản xuất theo công thức
        /// </summary>
        public int OutputQuantity { get; set; } = 1;

        /// <summary>
        /// Thời gian ước tính (phút)
        /// </summary>
        public decimal EstimatedTime { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Product Product { get; set; } = null!;
        public ICollection<RecipeItem> RecipeItems { get; set; } = new List<RecipeItem>();
    }
}
