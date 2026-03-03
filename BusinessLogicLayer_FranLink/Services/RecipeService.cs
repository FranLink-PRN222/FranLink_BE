using BusinessLogicLayer_FranLink.DTOs;
using DataAccessLayer_FranLink.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer_FranLink.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly FranLinkContext _context;

        public RecipeService(FranLinkContext context)
        {
            _context = context;
        }

        #region Recipe CRUD

        public async Task<List<Recipe>> GetAllRecipesAsync()
        {
            return await _context.Recipes
                .Include(r => r.Product)
                .Include(r => r.RecipeItems)
                    .ThenInclude(ri => ri.IngredientProduct)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<List<Recipe>> GetActiveRecipesAsync()
        {
            return await _context.Recipes
                .Include(r => r.Product)
                .Include(r => r.RecipeItems)
                    .ThenInclude(ri => ri.IngredientProduct)
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<Recipe?> GetRecipeByIdAsync(int recipeId)
        {
            return await _context.Recipes
                .Include(r => r.Product)
                .Include(r => r.RecipeItems)
                    .ThenInclude(ri => ri.IngredientProduct)
                .FirstOrDefaultAsync(r => r.RecipeId == recipeId);
        }

        public async Task<Recipe?> GetRecipeByProductIdAsync(int productId)
        {
            return await _context.Recipes
                .Include(r => r.Product)
                .Include(r => r.RecipeItems)
                    .ThenInclude(ri => ri.IngredientProduct)
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.IsActive);
        }

        public async Task<Recipe> CreateRecipeAsync(Recipe recipe)
        {
            // Check if product already has a recipe
            var existingRecipe = await _context.Recipes
                .FirstOrDefaultAsync(r => r.ProductId == recipe.ProductId && r.IsActive);
            
            if (existingRecipe != null)
            {
                throw new InvalidOperationException($"Product already has an active recipe: {existingRecipe.Name}");
            }

            recipe.CreatedAt = DateTime.UtcNow;
            recipe.IsActive = true;

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            return recipe;
        }

        public async Task<Recipe?> UpdateRecipeAsync(Recipe recipe)
        {
            var existing = await _context.Recipes.FindAsync(recipe.RecipeId);
            if (existing == null)
            {
                return null;
            }

            existing.Name = recipe.Name;
            existing.ProductId = recipe.ProductId;
            existing.Instructions = recipe.Instructions;
            existing.OutputQuantity = recipe.OutputQuantity;
            existing.EstimatedTime = recipe.EstimatedTime;
            existing.IsActive = recipe.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteRecipeAsync(int recipeId)
        {
            var recipe = await _context.Recipes.FindAsync(recipeId);
            if (recipe == null)
            {
                return false;
            }

            // Soft delete
            recipe.IsActive = false;
            recipe.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Recipe Items

        public async Task<List<RecipeItem>> GetRecipeItemsAsync(int recipeId)
        {
            return await _context.RecipeItems
                .Include(ri => ri.IngredientProduct)
                .Where(ri => ri.RecipeId == recipeId)
                .ToListAsync();
        }

        public async Task<RecipeItem> AddRecipeItemAsync(RecipeItem item)
        {
            // Validate recipe exists
            var recipe = await _context.Recipes.FindAsync(item.RecipeId);
            if (recipe == null)
            {
                throw new InvalidOperationException("Recipe not found.");
            }

            // Validate ingredient product exists
            var product = await _context.Products.FindAsync(item.IngredientProductId);
            if (product == null)
            {
                throw new InvalidOperationException("Ingredient product not found.");
            }

            // Check if ingredient already exists in recipe
            var existing = await _context.RecipeItems
                .FirstOrDefaultAsync(ri => ri.RecipeId == item.RecipeId && ri.IngredientProductId == item.IngredientProductId);
            
            if (existing != null)
            {
                throw new InvalidOperationException("This ingredient already exists in the recipe. Update quantity instead.");
            }

            _context.RecipeItems.Add(item);
            await _context.SaveChangesAsync();

            return item;
        }

        public async Task<RecipeItem?> UpdateRecipeItemAsync(RecipeItem item)
        {
            var existing = await _context.RecipeItems.FindAsync(item.RecipeItemId);
            if (existing == null)
            {
                return null;
            }

            existing.Quantity = item.Quantity;
            existing.Unit = item.Unit;
            existing.Notes = item.Notes;

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> RemoveRecipeItemAsync(int recipeItemId)
        {
            var item = await _context.RecipeItems.FindAsync(recipeItemId);
            if (item == null)
            {
                return false;
            }

            _context.RecipeItems.Remove(item);
            await _context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Calculations

        public async Task<Dictionary<int, decimal>> CalculateIngredientsNeededAsync(int recipeId, int outputQuantity)
        {
            var recipe = await GetRecipeByIdAsync(recipeId);
            if (recipe == null)
            {
                throw new InvalidOperationException("Recipe not found.");
            }

            // Calculate multiplier based on output quantity vs recipe output quantity
            decimal multiplier = (decimal)outputQuantity / recipe.OutputQuantity;

            var result = new Dictionary<int, decimal>();
            foreach (var item in recipe.RecipeItems)
            {
                result[item.IngredientProductId] = item.Quantity * multiplier;
            }

            return result;
        }

        public async Task<bool> CheckIngredientsAvailabilityAsync(int recipeId, int outputQuantity, int centralKitchenId)
        {
            var ingredientsNeeded = await CalculateIngredientsNeededAsync(recipeId, outputQuantity);

            foreach (var (productId, requiredQty) in ingredientsNeeded)
            {
                var availableQty = await _context.Inventories
                    .Where(i => i.CentralKitchenId == centralKitchenId && i.ProductId == productId)
                    .SumAsync(i => i.Quantity);

                if (availableQty < requiredQty)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<List<IngredientAvailability>> GetIngredientsAvailabilityDetailAsync(int recipeId, int outputQuantity, int centralKitchenId)
        {
            var recipe = await GetRecipeByIdAsync(recipeId);
            if (recipe == null)
            {
                throw new InvalidOperationException("Recipe not found.");
            }

            var ingredientsNeeded = await CalculateIngredientsNeededAsync(recipeId, outputQuantity);
            var result = new List<IngredientAvailability>();

            foreach (var item in recipe.RecipeItems)
            {
                var requiredQty = ingredientsNeeded[item.IngredientProductId];
                
                var availableQty = await _context.Inventories
                    .Where(i => i.CentralKitchenId == centralKitchenId && i.ProductId == item.IngredientProductId)
                    .SumAsync(i => i.Quantity);

                result.Add(new IngredientAvailability
                {
                    ProductId = item.IngredientProductId,
                    ProductName = item.IngredientProduct?.Name ?? "Unknown",
                    RequiredQuantity = requiredQty,
                    AvailableQuantity = availableQty,
                    Unit = item.Unit
                });
            }

            return result;
        }

        #endregion
    }
}
