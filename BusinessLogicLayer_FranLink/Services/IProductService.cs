using DataAccessLayer_FranLink.Models;

namespace BusinessLogicLayer_FranLink.Services
{
    public interface IProductService
    {
        // Queries
        Task<List<Product>> GetAllProductsAsync();
        Task<List<Product>> GetActiveProductsAsync();
        Task<List<Product>> GetProductsByCategoryAsync(string category);
        Task<Product?> GetProductByIdAsync(int id);
        Task<List<string>> GetAllCategoriesAsync();

        // Commands
        Task<Product> CreateProductAsync(Product product);
        Task<Product?> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);  // Soft delete - set IsActive = false
        Task<bool> HardDeleteProductAsync(int id);  // Permanent delete
    }
}
