using FashionShopDemo.Models;
using FashionShopDemo.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FashionShopDemo.Controllers
{
    public class ProductController : Controller
    {

        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;

        public async Task DropdownListsAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            var brands = await _brandRepository.GetAllAsync();
            ViewBag.Brands = new SelectList(brands, "Id", "Name");
        }
        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, IBrandRepository brandRepository)

        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;

        }
        public async Task<IActionResult> Index()
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var products = await _productRepository.GetAllAsync();

            return View(products);
        }
        public async Task<IActionResult> Add()
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            var brands = await _brandRepository.GetAllAsync();
            ViewBag.Brands = new SelectList(brands, "Id", "Name");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Add(Product product, IFormFile imageUrl)
        {
            if (ModelState.IsValid)
            {

                if (imageUrl != null)
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }
                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            var brands = await _brandRepository.GetAllAsync();
            ViewBag.Brands = new SelectList(brands, "Id", "Name"); return View(product);
        }
        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName);

            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName;

        }
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        public async Task<IActionResult> Update(int id)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            var brands = await _brandRepository.GetAllAsync();
            ViewBag.Brands = new SelectList(brands, "Id", "Name");
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, Product product, IFormFile imageUrl)

        {
            ModelState.Remove("ImageUrl");

            if (id != product.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var existingProduct = await

                _productRepository.GetByIdAsync(id);



                if (imageUrl == null)
                {
                    product.ImageUrl = existingProduct.ImageUrl;
                }
                else
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.PromotionPrice = product.PromotionPrice;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.BrandId = product.BrandId;
                existingProduct.SeoTitle = product.SeoTitle;
                existingProduct.Quantity = product.Quantity;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.IsHot = product.IsHot;

                if (existingProduct.IsHot)
                {
                    existingProduct.HotStartDate = product.HotStartDate;
                    existingProduct.HotEndDate = product.HotEndDate;
                }
                else
                {
                    existingProduct.HotStartDate = null;
                    existingProduct.HotEndDate = null;
                }

                await _productRepository.UpdateAsync(existingProduct);
                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            var brands = await _brandRepository.GetAllAsync();
            ViewBag.Brands = new SelectList(brands, "Id", "Name"); return View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet("onsale")]
        public async Task<ActionResult<List<Product>>> GetProductsOnSale()
        {
            var productsOnSale = await _productRepository.GetProductsOnSaleAsync();

            if (productsOnSale == null || !productsOnSale.Any())
            {
                return NotFound("Không có sản phẩm giảm giá.");
            }

            return Ok(productsOnSale);
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
        [HttpGet("hot-products-time-remaining")]
        public async Task<ActionResult<List<Product>>> GetTimeRemainingForHotProducts()
        {
            var hotProducts = await _productRepository.GetHotProductsAsync();

            if (hotProducts == null || !hotProducts.Any())
            {
                return NotFound("Không có sản phẩm hot.");
            }

            // Lọc các sản phẩm có thời gian > 0
            var filteredProducts = hotProducts.Where(p =>
                p.HotEndDate.HasValue && (p.HotEndDate.Value - DateTime.UtcNow).TotalSeconds > 0
            ).ToList();

            if (!filteredProducts.Any())
            {
                return NotFound("Tất cả các sản phẩm hot đã hết thời gian.");
            }

            return Ok(filteredProducts);
        }

        private string CalculateTimeRemaining(DateTime? hotEndDate)
        {
            if (!hotEndDate.HasValue)
            {
                return string.Empty;
            }

            var timeRemaining = hotEndDate.Value - DateTime.UtcNow;
            if (timeRemaining.TotalSeconds <= 0)
            {
                return "Thời gian sale đã kết thúc";
            }

            var days = Math.Floor(timeRemaining.TotalDays);
            var hours = Math.Floor(timeRemaining.TotalHours % 24);
            var minutes = Math.Floor(timeRemaining.TotalMinutes % 60);
            var seconds = Math.Floor(timeRemaining.TotalSeconds % 60);

            return $"{days}d {hours}h {minutes}m {seconds}s";
        }
        [HttpGet("christmas-collection")]
        public async Task<IActionResult> GetProductsByBrandNoel()
        {
            var products = await _productRepository.GetProductsByBrandNameAsync("Noel");

            if (products == null || !products.Any())
            {
                return NotFound("Không có sản phẩm của thương hiệu Noel.");
            }

            return Ok(products);
        }
        [HttpGet("accessories")]
        public async Task<IActionResult> GetAccessories()
        {
            var categoryNames = new List<string> { "Tất", "Cà vạt", "Kẹp cà vạt", "Phụ kiện" };
            var accessories = await _productRepository.GetProductsByCategoryNamesAsync(categoryNames);

            if (accessories == null || !accessories.Any())
            {
                return NotFound("Không có sản phẩm nào thuộc các danh mục này.");
            }

            return Ok(accessories);
        }


    }

}
