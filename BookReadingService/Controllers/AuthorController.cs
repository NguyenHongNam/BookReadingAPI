using BookReadingService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookReadingService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthorController : Controller
    {
        private readonly NamNguyenBookDBContext _context;
        public AuthorController(NamNguyenBookDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Author>>> GetAuthor()
        {
            var authors = await _context.Authors
                .OrderBy(a => a.AuthorId)  // Bạn có thể muốn sắp xếp theo một thuộc tính cụ thể
                .ToListAsync();

            foreach (var author in authors)
            {
                // Tính nhuận bút cho từng tác giả và gán giá trị vào trường Royalties
                author.Royalties = await CalculateRoyalties(author.AuthorId);
            }

            return Ok(authors);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Author>> GetAuthorById(int id)
        {
            var author = await _context.Authors.FindAsync(id);

            if (author == null)
            {
                return NotFound();
            }

            return Ok(author);
        }

        [HttpPost]
        public async Task<ActionResult<Author>> PostAuthor(Author author)
        {
            if (ModelState.IsValid)
            {
                Author newAuthor = new Author();

                newAuthor.AuthorName = author.AuthorName;
                newAuthor.Royalties = 0; 
                try
                {
                    _context.Authors.Add(newAuthor);
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
        public async Task<IActionResult> PutAuthor(int id, Author author)
        {
             
            var existingAuthor = await _context.Authors.FindAsync(id);
            if (existingAuthor == null)
            {
                return BadRequest($"author id {author.AuthorId} does not exist.");
            }


            existingAuthor.AuthorName = author.AuthorName;
            existingAuthor.Royalties = author.Royalties;

            try
            {
                await _context.SaveChangesAsync();

                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating author: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            try
            {
                var author = await _context.Authors.FindAsync(id);

                if (author == null)
                {
                    return NotFound();
                }

                _context.Authors.Remove(author);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<int> CalculateRoyalties(int authorId)
        {
            // Tìm sách có id tác giả tương ứng
            var booksByAuthor = await _context.Books.Where(b => b.AuthorId == authorId).ToListAsync();

            // Tính tổng số lượt xem của tất cả các sách
            int totalViews = booksByAuthor.Sum(b => b.Views);

            // Tính nhuận bút
            int royalties = totalViews * 5000;

            return royalties;
        }

        [HttpGet("royalties/{id}")]
        public async Task<ActionResult<int>> GetAuthorRoyalties(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }

            // Tính nhuận bút của tác giả
            int royalties = await CalculateRoyalties(id);

            return royalties;
        }

        [HttpGet("top-4-royalties")]
        public async Task<ActionResult<IEnumerable<Author>>> GetTopFourAuthorsByRoyalties()
        {
            var topAuthors = await _context.Authors
                .OrderByDescending(a => a.Royalties)
                .Take(4)
                .ToListAsync();
            foreach (var author in topAuthors)
            {
                // Tính nhuận bút cho từng tác giả và gán giá trị vào trường Royalties
                author.Royalties = await CalculateRoyalties(author.AuthorId);
            }
            return Ok(topAuthors);
        }


    }
}
