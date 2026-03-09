using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace PresentationLayer_FranLink.Pages.Manager.Products
{
    public class CreateModel : PageModel
    {
        private readonly IProductService _productService;

        public CreateModel(IProductService productService)
        {
            _productService = productService;
        }

        [BindProperty]
        public ProductInput Input { get; set; } = new();

        public List<string> ExistingCategories { get; set; } = new();

        public class ProductInput
        {
            [Required(ErrorMessage = "Product name is required")]
            [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Price is required")]
            [Range(0, double.MaxValue, ErrorMessage = "Price must be positive")]
            public decimal Price { get; set; }

            [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
            public string? Description { get; set; }

            [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
            public string? Category { get; set; }

            [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
            public string? SKU { get; set; }

            [StringLength(50, ErrorMessage = "Unit cannot exceed 50 characters")]
            public string? Unit { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is Manager or Admin
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager")
            {
                return RedirectToPage("/Login");
            }

            ExistingCategories = await _productService.GetAllCategoriesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ExistingCategories = await _productService.GetAllCategoriesAsync();
                return Page();
            }

            var product = new Product
            {
                Name = Input.Name,
                Price = Input.Price,
                Description = Input.Description,
                Category = Input.Category,
                SKU = Input.SKU,
                Unit = Input.Unit
            };

            await _productService.CreateProductAsync(product);

            TempData["SuccessMessage"] = $"Product '{product.Name}' created successfully.";
            return RedirectToPage("Index");
        }
    }
}
