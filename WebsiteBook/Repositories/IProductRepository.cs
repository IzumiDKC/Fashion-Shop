﻿using FashionShopDemo.Models;
using System;

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
        Task<List<Product>> GetProductsOnSaleAsync();
        Task<List<Product>> GetHotProductsAsync();
        Task<IEnumerable<Product>> GetProductsByBrandNameAsync(string brandName);
        Task<IEnumerable<Product>> GetProductsByCategoryNamesAsync(List<string> categoryNames);
        Task<List<Product>> GetLatestProductsAsync(int count);
        Task<List<Product>> GetProductsByCategoryAsync(int categoryId);

    }
}
