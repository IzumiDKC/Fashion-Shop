using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FashionShopDemo.Models;
using FashionShopDemo.Repositories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FashionShopDemo.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, IProductRepository productRepository)
        {
            _context = context;
            _logger = logger;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index()
        {
            var brands = _context.Brands.ToList();
            var categories = _context.Categories.ToList();

            ViewBag.Brands = brands;
            ViewBag.Categories = categories; 
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

     /*   public ActionResult Search(string query)
        {
            var searchResults = _context.Products
                .Where(p => p.Name.Contains(query))
                .Select(p => new { name = p.Name })
                .ToList();
            return Json(searchResults);
        } */
        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("Query không hợp lệ.");
            }

            var products = await _context.Products
                .Where(p => EF.Functions.Like(p.Name, $"%{query}%")) 
                .Select(p => new { Name = p.Name }) 
                .Take(10) 
                .ToListAsync();

            return Json(products); 
        }
        public IActionResult SearchResult(int categoryId)
        {
            ViewBag.Category = categoryId;
            var searchResults = _context.Products.Where(p => p.CategoryId == categoryId).ToList();
            return View(searchResults);
        }
        [HttpGet]
        public IActionResult Introduce()
        {
            return View();
        }
        public IActionResult AccessDeny()
        {
            return View();
        }

        [HttpPost]
        public IActionResult FilterProducts(string productName, int? brandId, string priceRange, int? categoryId)
        {
            var filteredProducts = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(productName))
            {
                filteredProducts = filteredProducts.Where(p => p.Name.Contains(productName));
            }

            if (brandId.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.BrandId == brandId);
            }

            if (!string.IsNullOrEmpty(priceRange))
            {
                var priceRangeParts = priceRange.Split('-');
                if (priceRangeParts.Length == 2 && int.TryParse(priceRangeParts[0], out int minPrice) && int.TryParse(priceRangeParts[1], out int maxPrice))
                {
                    filteredProducts = filteredProducts.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
                }
            }

            if (categoryId.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.CategoryId == categoryId);
            }

            var productList = filteredProducts.ToList();

            if (productList.Count == 0)
            {
                ViewData["Message"] = "Không có sản phẩm nào phù hợp với yêu cầu lọc của bạn.";
            }

            return View("Index", productList);
        }
        [HttpGet]
        public IActionResult AutoComplete(string term)
        {
            var suggestions = _context.Products
                .Where(p => p.Name.Contains(term))
                .Select(p => new { name = p.Name })
                .ToList();

            return Json(suggestions);
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult ErrorPage()
        {
            Console.WriteLine("✅ Đã gọi đến trang ErrorPage!");
            return View("ErrorPage");
        }


    }
}
