using FashionShopDemo.Models;

namespace FashionShopDemo.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(int id);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
        // 🔹 Thêm phương thức lấy danh mục theo dạng cây
        Task<List<Category>> GetCategoryTreeAsync();
    }
}
