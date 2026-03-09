using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace PresentationLayer_FranLink.Pages.Manager.Products
{
    public class EditModel : PageModel
    {
        private readonly IProductService _productService;

        public EditModel(IProductService productService)
        {
            _productService = productService;
        }

        [BindProperty]
        public ProductInput Input { get; set; } = new();

        public List<string> ExistingCategories { get; set; } = new();

        public class ProductInput
        {
            public int Id { get; set; }

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

            public bool IsActive { get; set; }
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

            Input = new ProductInput
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                Category = product.Category,
                SKU = product.SKU,
                Unit = product.Unit,
                IsActive = product.IsActive
            };

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
                Id = Input.Id,
                Name = Input.Name,
                Price = Input.Price,
                Description = Input.Description,
                Category = Input.Category,
                SKU = Input.SKU,
                Unit = Input.Unit,
                IsActive = Input.IsActive
            };

            var result = await _productService.UpdateProductAsync(product);
            if (result == null)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = $"Product '{product.Name}' updated successfully.";
            return RedirectToPage("Index");
        }
    }
}
