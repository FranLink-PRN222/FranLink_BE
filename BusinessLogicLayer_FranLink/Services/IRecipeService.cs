using BusinessLogicLayer_FranLink.DTOs;
using DataAccessLayer_FranLink.Models;

namespace BusinessLogicLayer_FranLink.Services
{
    public interface IRecipeService
    {
        #region Recipe CRUD
        
        Task<List<Recipe>> GetAllRecipesAsync();
        Task<List<Recipe>> GetActiveRecipesAsync();
        Task<Recipe?> GetRecipeByIdAsync(int recipeId);
        Task<Recipe?> GetRecipeByProductIdAsync(int productId);
        Task<Recipe> CreateRecipeAsync(Recipe recipe);
        Task<Recipe?> UpdateRecipeAsync(Recipe recipe);
        Task<bool> DeleteRecipeAsync(int recipeId);

        #endregion

        #region Recipe Items
        
        Task<RecipeItem> AddRecipeItemAsync(RecipeItem item);
        Task<RecipeItem?> UpdateRecipeItemAsync(RecipeItem item);
        Task<bool> RemoveRecipeItemAsync(int recipeItemId);
        Task<List<RecipeItem>> GetRecipeItemsAsync(int recipeId);

        #endregion

        #region Calculations
        
        /// <summary>
        /// Tính toán lượng nguyên liệu cần dùng để sản xuất số lượng sản phẩm mong muốn
        /// </summary>
        /// <param name="recipeId">ID công thức</param>
        /// <param name="outputQuantity">Số lượng sản phẩm muốn sản xuất</param>
        /// <returns>Dictionary với key là ProductId của nguyên liệu, value là số lượng cần</returns>
        Task<Dictionary<int, decimal>> CalculateIngredientsNeededAsync(int recipeId, int outputQuantity);
        
        /// <summary>
        /// Kiểm tra nguyên liệu có đủ trong kho Central Kitchen không
        /// </summary>
        Task<bool> CheckIngredientsAvailabilityAsync(int recipeId, int outputQuantity, int centralKitchenId);
        
        /// <summary>
        /// Lấy chi tiết tình trạng nguyên liệu (đủ/thiếu bao nhiêu)
        /// </summary>
        Task<List<IngredientAvailability>> GetIngredientsAvailabilityDetailAsync(int recipeId, int outputQuantity, int centralKitchenId);

        #endregion
    }
}
