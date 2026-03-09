using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Manager.Products
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;

        public IndexModel(IProductService productService)
        {
            _productService = productService;
        }

        public List<Product> Products { get; set; } = new();
        public List<string> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CategoryFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool ShowInactive { get; set; } = false;

        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is Manager or Admin
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager")
            {
                return RedirectToPage("/Login");
            }

            // Get all categories for filter dropdown
            Categories = await _productService.GetAllCategoriesAsync();

            // Get products
            var allProducts = ShowInactive
                ? await _productService.GetAllProductsAsync()
                : await _productService.GetActiveProductsAsync();

            // Apply filters
            Products = allProducts;

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                Products = Products
                    .Where(p => p.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                (p.SKU != null && p.SKU.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(CategoryFilter))
            {
                Products = Products
                    .Where(p => p.Category == CategoryFilter)
                    .ToList();
            }

            // Check for success message from TempData
            if (TempData["SuccessMessage"] != null)
            {
                SuccessMessage = TempData["SuccessMessage"]?.ToString();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Product deactivated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to deactivate product.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRestoreAsync(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product != null)
            {
                product.IsActive = true;
                await _productService.UpdateProductAsync(product);
                TempData["SuccessMessage"] = "Product restored successfully.";
            }

            return RedirectToPage(new { ShowInactive = true });
        }
    }
}
