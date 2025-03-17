using Microsoft.EntityFrameworkCore;
using FashionShopDemo.Models;

namespace FashionShopDemo.Repositories
{
    public class EFCategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public EFCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Include(c => c.SubCategories) // danh mục con
                .ToListAsync();
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.SubCategories) // danh mục con
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Category>> GetCategoryTreeAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            return categories
                .Where(c => c.ParentId == null) //  danh mục gốc
                .Select(c => BuildCategoryTree(c, categories)) // cây danh mục
                .ToList();
        }

        private Category BuildCategoryTree(Category category, List<Category> allCategories)
        {
            category.SubCategories = allCategories
                .Where(c => c.ParentId == category.Id)
                .Select(c => BuildCategoryTree(c, allCategories))
                .ToList();
            return category;
        }

    }
}
