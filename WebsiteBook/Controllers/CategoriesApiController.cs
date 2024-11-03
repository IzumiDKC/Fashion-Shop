using FashionShopDemo.Models;
using FashionShopDemo.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/categories")] // Đặt đường dẫn là api/categories
[ApiController]
public class CategoriesController : ControllerBase // Đổi tên thành CategoriesController
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoriesController(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    // GET: api/categories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return Ok(categories);
    }

    // GET: api/categories/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Category>> GetCategory(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return category;
    }

    // POST: api/categories
    [HttpPost]
    public async Task<ActionResult<Category>> PostCategory(Category category)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _categoryRepository.AddAsync(category);
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    // PUT: api/categories/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCategory(int id, Category category)
    {
        if (id != category.Id)
        {
            return BadRequest();
        }

        await _categoryRepository.UpdateAsync(category);
        return NoContent();
    }

    // DELETE: api/categories/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        await _categoryRepository.DeleteAsync(id);
        return NoContent();
    }
}
