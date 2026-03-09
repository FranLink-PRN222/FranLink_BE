using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Manager.Products
{
    public class DetailsModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly IRecipeService _recipeService;
        private readonly IInventoryService _inventoryService;

        public DetailsModel(
            IProductService productService,
            IRecipeService recipeService,
            IInventoryService inventoryService)
        {
            _productService = productService;
            _recipeService = recipeService;
            _inventoryService = inventoryService;
        }

        public Product Product { get; set; } = null!;
        public Recipe? Recipe { get; set; }
        public int TotalInventoryQuantity { get; set; }
        public List<InventoryLocation> InventoryLocations { get; set; } = new();

        public class InventoryLocation
        {
            public string LocationName { get; set; } = string.Empty;
            public string LocationType { get; set; } = string.Empty;  // CentralKitchen or FranchiseStore
            public int Quantity { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Check if user is Manager or Admin
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager")
            {
                return RedirectToPage("/Login");
            }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            Product = product;

            // Get recipe for this product
            Recipe = await _recipeService.GetRecipeByProductIdAsync(id);

            // Get inventory information
            var allInventory = await _inventoryService.GetAllCentralKitchenInventoryAsync();
            var storeInventory = await _inventoryService.GetAllStoresInventoryAsync();
            
            var productInventory = allInventory
                .Where(i => i.ProductId == id)
                .ToList();
            
            productInventory.AddRange(storeInventory.Where(i => i.ProductId == id));

            TotalInventoryQuantity = productInventory.Sum(i => i.Quantity);

            // Group by location
            foreach (var inv in productInventory)
            {
                if (inv.CentralKitchenId.HasValue && inv.CentralKitchen != null)
                {
                    InventoryLocations.Add(new InventoryLocation
                    {
                        LocationName = inv.CentralKitchen.Name,
                        LocationType = "Central Kitchen",
                        Quantity = inv.Quantity
                    });
                }
                else if (inv.FranchiseStoreId.HasValue && inv.FranchiseStore != null)
                {
                    InventoryLocations.Add(new InventoryLocation
                    {
                        LocationName = inv.FranchiseStore.Name,
                        LocationType = "Franchise Store",
                        Quantity = inv.Quantity
                    });
                }
            }

            return Page();
        }
    }
}
