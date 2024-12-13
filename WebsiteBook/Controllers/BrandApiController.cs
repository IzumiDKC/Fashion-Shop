using FashionShopDemo.Models;
using FashionShopDemo.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/brands")] 
[ApiController]
public class BrandsController : ControllerBase 
{
    private readonly IBrandRepository _brandRepository;

    public BrandsController(IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Brand>>> GetBrands()
    {
        var brands = await _brandRepository.GetAllAsync();
        return Ok(brands);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Brand>> GetBrand(int id)
    {
        var brand = await _brandRepository.GetByIdAsync(id);
        if (brand == null)
        {
            return NotFound();
        }
        return brand;
    }

    [HttpPost]
    public async Task<ActionResult<Brand>> PostBrand(Brand brand)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        await _brandRepository.AddAsync(brand);
        return CreatedAtAction(nameof(GetBrand), new { id = brand.Id }, brand);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutBrand(int id, Brand brand)
    {
        if (id != brand.Id)
        {
            return BadRequest();
        }
        await _brandRepository.UpdateAsync(brand);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        var brand = await _brandRepository.GetByIdAsync(id);
        if (brand == null)
        {
            return NotFound();
        }
        await _brandRepository.DeleteAsync(id);
        return NoContent();
    }
}
