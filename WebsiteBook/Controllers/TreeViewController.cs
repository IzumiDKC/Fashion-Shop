using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using FashionShopDemo.Models;
using FashionShopDemo.Repositories;

namespace FashionShopDemo.Controllers
{
    public class TreeViewController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public TreeViewController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy danh mục từ repository
            var categories = await _categoryRepository.GetAllAsync();

            // Gửi danh mục đến View
            return View(categories);
        }
    }
}
