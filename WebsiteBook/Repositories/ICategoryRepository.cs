using FashionShopDemo.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FashionShopDemo.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(int id);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
        Task<List<Category>> GetCategoryTreeAsync();
    }
}
