﻿using FashionShop.Models;
using FashionShopDemo.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FashionShopDemo.Repositories
{
    public interface IBrandRepository
    {
        Task<IEnumerable<Brand>> GetAllAsync();
        Task<Brand> GetByIdAsync(int id);
        Task AddAsync(Brand brand);
        Task UpdateAsync(Brand brand);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id); 

    }
}
