using FashionShopDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace FashionShopDemo.Repositories
{
    public class EFProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;


        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.Include(p => p.Category).ToListAsync();
        }
        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Product>> FilterProductsAsync(string brandId, string categoryId)
        {
            // Chuyển đổi brandId và categoryId sang kiểu dữ liệu phù hợp (ví dụ: int)
            int parsedBrandId = int.Parse(brandId);
            int parsedCategoryId = int.Parse(categoryId);

            // Lọc sản phẩm từ cơ sở dữ liệu dựa trên brandId và categoryId
            var filteredProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.BrandId == parsedBrandId && p.CategoryId == parsedCategoryId)
                .ToListAsync();

            return filteredProducts;
        }

        Task IProductRepository.FilterProductsAsync(string brandId, string categoryId)
        {
            throw new NotImplementedException();
        }
        public async Task<List<Product>> GetProductsOnSaleAsync()
        {
            return await _context.Products
                .Where(p => p.PromotionPrice > 0)
                .ToListAsync();
        }
        public async Task<List<Product>> GetHotProductsAsync()
        {
            return await _context.Products
                .Where(p => p.IsHot)
                .ToListAsync();
        }
        public async Task<IEnumerable<Product>> GetProductsByBrandNameAsync(string brandName)
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Where(p => p.Brand.Name == brandName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryNamesAsync(List<string> categoryNames)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => categoryNames.Contains(p.Category.Name))
                .ToListAsync();
        }


    }
}
