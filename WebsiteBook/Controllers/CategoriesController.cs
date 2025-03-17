using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionShopDemo.Models;
using FashionShopDemo.Repositories;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FashionShopDemo.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<IActionResult> TreeView()
        {
            var categories = await _categoryRepository.GetCategoryTreeAsync();
            return Json(categories); // Trả về JSON thay vì View
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();

            var parentCategories = categories.Where(c => c.ParentId == null).ToList();

            return View(parentCategories);
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

        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = categories.Where(c => c.ParentId == null).ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Category category)
        {
            // Kiểm tra dữ liệu từ form
            Console.WriteLine($"ParentId nhận được: {(category.ParentId.HasValue ? category.ParentId.Value.ToString() : "NULL")}");


            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState không hợp lệ!");
                foreach (var error in ModelState)
                {
                    foreach (var subError in error.Value.Errors)
                    {
                        Console.WriteLine($"Lỗi: {subError.ErrorMessage}");
                    }
                }

                var categories = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = categories.Where(c => c.ParentId == null).ToList();
                return View(category);
            }

            // Lưu dữ liệu vào database
            await _categoryRepository.AddAsync(category);
            return RedirectToAction(nameof(Index));
        }









        public async Task<IActionResult> Update(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
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
                await _categoryRepository.UpdateAsync(category);
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
            await _categoryRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
