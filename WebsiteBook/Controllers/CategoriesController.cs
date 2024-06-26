using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionShopDemo.Models;
using FashionShopDemo.Repositories;

namespace FashionShopDemo.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context;

        public CategoriesController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }
        public async Task<IActionResult> Index()
        {
            var category = await _categoryRepository.GetAllAsync();
            return View(category);
        }
        public async Task<IActionResult> Display(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        public IActionResult Add()
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Add(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryRepository.AddAsync(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        public async Task<IActionResult> Update(int id)
        {
           /* if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return RedirectToAction("AccessDenied", "Home");
            } */
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                return RedirectToAction("NotFound", "Categories");
            }
            return View(category);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                _categoryRepository.UpdateAsync(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category != null)
            {
                _categoryRepository.DeleteAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Search(int categoryId)
        {
            var searchResults = _context.Products.Where(p => p.CategoryId == categoryId).ToList();
            return View("SearchResult", searchResults);
        }
        public IActionResult NotFound()
        {
            return View();
        }
    }
}
