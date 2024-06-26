using FashionShop.Models;
using FashionShopDemo.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FashionShopDemo.Repositories
{
    public class EFBrandRepository : IBrandRepository
    {
        private readonly ApplicationDbContext _context;

        public EFBrandRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Brand brand)
        {
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Brand>> GetAllAsync()
        {
            return await _context.Brands.ToListAsync();
        }

        public async Task<Brand> GetByIdAsync(int id)
        {
            return await _context.Brands.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task UpdateAsync(Brand brand)
        {
            _context.Brands.Update(brand); 
            await _context.SaveChangesAsync();
        }
    }
}
