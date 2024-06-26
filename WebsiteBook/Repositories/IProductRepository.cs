using FashionShopDemo.Models;

namespace FashionShopDemo.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task FilterProductsAsync(string brandId, string categoryId);

    }
}
