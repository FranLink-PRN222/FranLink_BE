using BusinessLogicLayer_FranLink.DTOs;
using DataAccessLayer_FranLink.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer_FranLink.Services
{
    public class ProductionService : IProductionService
    {
        private readonly FranLinkContext _context;

        public ProductionService(FranLinkContext context)
        {
            _context = context;
        }

        public async Task<ProductionRecord> StartProductionAsync(StartProductionDto dto, Guid userId)
        {
            var record = new ProductionRecord
            {
                CentralKitchenId = dto.CentralKitchenId,
                RecipeId = dto.RecipeId,
                ProducedByUserId = userId,
                PlannedQuantity = dto.PlannedQuantity,
                ActualQuantity = 0,
                StartTime = DateTime.UtcNow,
                Notes = dto.Notes,
                Status = "InProgress",
                CreatedAt = DateTime.UtcNow
            };

            _context.ProductionRecords.Add(record);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(record.ProductionId) ?? record;
        }

        public async Task<ProductionRecord?> CompleteProductionAsync(int productionId, CompleteProductionDto dto)
        {
            var record = await _context.ProductionRecords.FindAsync(productionId);
            if (record == null || record.Status != "InProgress")
                return null;

            record.ActualQuantity = dto.ActualQuantity;
            record.EndTime = DateTime.UtcNow;
            record.Status = "Completed";
            if (!string.IsNullOrEmpty(dto.Notes))
            {
                record.Notes = string.IsNullOrEmpty(record.Notes)
                    ? dto.Notes
                    : $"{record.Notes}\n{dto.Notes}";
            }

            // Update inventory - add produced quantity to Central Kitchen
            var recipe = await _context.Recipes.FindAsync(record.RecipeId);
            if (recipe != null)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.CentralKitchenId == record.CentralKitchenId
                                           && i.ProductId == recipe.ProductId);

                if (inventory != null)
                {
                    inventory.Quantity += dto.ActualQuantity;
                    inventory.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    // Create new inventory entry
                    _context.Inventories.Add(new Inventory
                    {
                        CentralKitchenId = record.CentralKitchenId,
                        ProductId = recipe.ProductId,
                        Quantity = dto.ActualQuantity,
                        LastUpdated = DateTime.UtcNow
                    });
                }

                // Deduct ingredients from inventory
                var recipeItems = await _context.RecipeItems
                    .Where(ri => ri.RecipeId == record.RecipeId)
                    .ToListAsync();

                foreach (var item in recipeItems)
                {
                    var ingredientInventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.CentralKitchenId == record.CentralKitchenId
                                               && i.ProductId == item.IngredientProductId);

                    if (ingredientInventory != null)
                    {
                        var usedQuantity = item.Quantity * dto.ActualQuantity / recipe.OutputQuantity;
                        ingredientInventory.Quantity -= (int)usedQuantity;
                        ingredientInventory.LastUpdated = DateTime.UtcNow;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(productionId);
        }

        public async Task<ProductionRecord?> CancelProductionAsync(int productionId, string reason)
        {
            var record = await _context.ProductionRecords.FindAsync(productionId);
            if (record == null || record.Status != "InProgress")
                return null;

            record.Status = "Cancelled";
            record.EndTime = DateTime.UtcNow;
            record.Notes = string.IsNullOrEmpty(record.Notes)
                ? $"Cancelled: {reason}"
                : $"{record.Notes}\nCancelled: {reason}";

            await _context.SaveChangesAsync();
            return await GetByIdAsync(productionId);
        }

        public async Task<ProductionRecord?> GetByIdAsync(int productionId)
        {
            return await _context.ProductionRecords
                .Include(pr => pr.CentralKitchen)
                .Include(pr => pr.Recipe)
                    .ThenInclude(r => r.Product)
                .Include(pr => pr.ProducedByUser)
                .FirstOrDefaultAsync(pr => pr.ProductionId == productionId);
        }

        public async Task<List<ProductionRecord>> GetProductionsByDateRangeAsync(
            DateTime from, DateTime to, int? centralKitchenId = null)
        {
            var query = _context.ProductionRecords
                .Include(pr => pr.CentralKitchen)
                .Include(pr => pr.Recipe)
                    .ThenInclude(r => r.Product)
                .Include(pr => pr.ProducedByUser)
                .Where(pr => pr.StartTime >= from && pr.StartTime <= to);

            if (centralKitchenId.HasValue)
                query = query.Where(pr => pr.CentralKitchenId == centralKitchenId.Value);

            return await query.OrderByDescending(pr => pr.StartTime).ToListAsync();
        }

        public async Task<List<ProductionRecord>> GetProductionsByStatusAsync(string status)
        {
            return await _context.ProductionRecords
                .Include(pr => pr.CentralKitchen)
                .Include(pr => pr.Recipe)
                    .ThenInclude(r => r.Product)
                .Include(pr => pr.ProducedByUser)
                .Where(pr => pr.Status == status)
                .OrderByDescending(pr => pr.StartTime)
                .ToListAsync();
        }

        public async Task<List<ProductionRecord>> GetInProgressProductionsAsync(int? centralKitchenId = null)
        {
            var query = _context.ProductionRecords
                .Include(pr => pr.CentralKitchen)
                .Include(pr => pr.Recipe)
                    .ThenInclude(r => r.Product)
                .Include(pr => pr.ProducedByUser)
                .Where(pr => pr.Status == "InProgress");

            if (centralKitchenId.HasValue)
                query = query.Where(pr => pr.CentralKitchenId == centralKitchenId.Value);

            return await query.OrderBy(pr => pr.StartTime).ToListAsync();
        }

        public async Task<ProductionSummary> GetProductionSummaryAsync(
            DateTime from, DateTime to, int? centralKitchenId = null)
        {
            var query = _context.ProductionRecords
                .Include(pr => pr.Recipe)
                    .ThenInclude(r => r.Product)
                .Where(pr => pr.StartTime >= from && pr.StartTime <= to && pr.Status == "Completed");

            if (centralKitchenId.HasValue)
                query = query.Where(pr => pr.CentralKitchenId == centralKitchenId.Value);

            var records = await query.ToListAsync();

            var summary = new ProductionSummary
            {
                TotalProductions = records.Count,
                TotalPlannedQuantity = records.Sum(r => r.PlannedQuantity),
                TotalActualQuantity = records.Sum(r => r.ActualQuantity),
                EfficiencyRate = records.Any() && records.Sum(r => r.PlannedQuantity) > 0
                    ? Math.Round((decimal)records.Sum(r => r.ActualQuantity) / records.Sum(r => r.PlannedQuantity) * 100, 2)
                    : 0,
                AverageProductionTimeMinutes = records.Any() && records.All(r => r.EndTime.HasValue)
                    ? Math.Round((decimal)records.Average(r => (r.EndTime!.Value - r.StartTime).TotalMinutes), 2)
                    : 0,
                ByProduct = records
                    .GroupBy(r => new { r.Recipe.ProductId, r.Recipe.Product.Name })
                    .Select(g => new ProductionByProduct
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.Name,
                        TotalPlanned = g.Sum(r => r.PlannedQuantity),
                        TotalActual = g.Sum(r => r.ActualQuantity),
                        EfficiencyRate = g.Sum(r => r.PlannedQuantity) > 0
                            ? Math.Round((decimal)g.Sum(r => r.ActualQuantity) / g.Sum(r => r.PlannedQuantity) * 100, 2)
                            : 0
                    })
                    .OrderByDescending(p => p.TotalActual)
                    .ToList(),
                ByDay = records
                    .GroupBy(r => r.StartTime.Date)
                    .Select(g => new ProductionByDay
                    {
                        Date = g.Key,
                        TotalQuantity = g.Sum(r => r.ActualQuantity),
                        ProductionCount = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList()
            };

            return summary;
        }

        public async Task<List<ProductionByProduct>> GetTopProductsAsync(DateTime from, DateTime to, int top = 10)
        {
            var records = await _context.ProductionRecords
                .Include(pr => pr.Recipe)
                    .ThenInclude(r => r.Product)
                .Where(pr => pr.StartTime >= from && pr.StartTime <= to && pr.Status == "Completed")
                .ToListAsync();

            return records
                .GroupBy(r => new { r.Recipe.ProductId, r.Recipe.Product.Name })
                .Select(g => new ProductionByProduct
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalPlanned = g.Sum(r => r.PlannedQuantity),
                    TotalActual = g.Sum(r => r.ActualQuantity),
                    EfficiencyRate = g.Sum(r => r.PlannedQuantity) > 0
                        ? Math.Round((decimal)g.Sum(r => r.ActualQuantity) / g.Sum(r => r.PlannedQuantity) * 100, 2)
                        : 0
                })
                .OrderByDescending(p => p.TotalActual)
                .Take(top)
                .ToList();
        }

        public async Task<List<CentralKitchen>> GetAllCentralKitchensAsync()
        {
            return await _context.CentralKitchens.OrderBy(ck => ck.Name).ToListAsync();
        }

        public async Task<List<Recipe>> GetActiveRecipesAsync()
        {
            return await _context.Recipes
                .Include(r => r.Product)
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }
    }
}
