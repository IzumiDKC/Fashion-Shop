using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using FashionShopDemo.Models;
using FashionShopDemo.Repositories;

namespace FashionShopDemo.Controllers
{
    [Route("api/trees")]
    [ApiController]
    public class TreeViewApiController : ControllerBase 
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;

        public TreeViewApiController(ICategoryRepository categoryRepository, IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        [HttpGet("hot-products")]
        public async Task<ActionResult<List<Product>>> GetHotProducts()
        {
            var hotProducts = await _productRepository.GetHotProductsAsync();

            if (hotProducts == null || !hotProducts.Any())
            {
                return NotFound("Không có sản phẩm hot.");
            }

            return Ok(hotProducts);
        }


        [HttpGet("GetProductsByCategory/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory(int categoryId)
        {
            var products = await _productRepository.GetProductsByCategoryAsync(categoryId);
            return Ok(products);
        }

        [HttpGet("category-tree")]
        public async Task<IActionResult> GetCategoryTree()
        {
            var categories = await _categoryRepository.GetCategoryTreeAsync();
            var treeData = categories.Select(c => new
            {
                id = c.Id.ToString(),
                parent = c.ParentId.HasValue ? c.ParentId.ToString() : "#",
                text = c.Name
            }).ToList();
            return Ok(treeData);
        }
    }
}
