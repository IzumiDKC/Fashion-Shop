using FashionShopDemo.Models;
using FashionShopDemo.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using System.Threading.Tasks;

namespace FashionShopDemo.Controllers
{
    public class BrandController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ApplicationDbContext _context;

        public BrandController(IProductRepository productRepository, IBrandRepository brandRepository)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
        }

        public async Task<IActionResult> Index()
        {
            var brand = await _brandRepository.GetAllAsync();
            return View(brand);
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
        public async Task<IActionResult> Add(Brand brand)
        {
            if (ModelState.IsValid)
            {
                 _brandRepository.AddAsync(brand);
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }
        public async Task<IActionResult> Display(int id)
        {
            
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }
        

        public async Task<IActionResult> Update(int id)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, Brand brand)
        {
            if (id != brand.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _brandRepository.UpdateAsync(brand);
                return RedirectToAction(nameof(Index));
            }

            return View(brand);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _brandRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
