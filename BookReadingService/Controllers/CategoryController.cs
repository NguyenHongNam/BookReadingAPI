using BookReadingService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static BookReadingService.Controllers.BookController;

namespace BookReadingService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController : Controller
    {
        private readonly NamNguyenBookDBContext _context;
        public CategoryController(NamNguyenBookDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategory()
        {
            var categories = await _context.Categories
                .OrderBy(c =>c.CategoryId)  // You may want to order by a specific property
                .Select(c => new Category
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                }).ToListAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategoryById(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category, int? userId = null)
        {
            if (ModelState.IsValid)
            {

                Category newCategory = new Category();

                newCategory.CategoryName = category.CategoryName;
                try
                {

                    _context.Categories.Add(newCategory);
                    await _context.SaveChangesAsync();

                    return Ok();

                }
                catch (Exception ex)
                {
                    return BadRequest($"Error updating category: {ex.Message}");
                }
            }

            return BadRequest(ModelState);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category, int? userId = null)
        {

            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                return BadRequest($"category id {category.CategoryId} does not exist.");
            }


            existingCategory.CategoryName = category.CategoryName;

            try
            {
                await _context.SaveChangesAsync();

                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating category: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                {
                    return NotFound();
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
