
using BookReadingService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookReadingService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookController : Controller
    {
        private readonly NamNguyenBookDBContext _context;
        public BookController(NamNguyenBookDBContext context)
        {
            _context = context;
        }   

        //Lấy danh sách tất cả các đầu sách
        [HttpGet("get")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBook()
        {
            var books = await _context.Books
                .OrderBy(b => b.BookId)  // You may want to order by a specific property
                .Select(b => new BookDTO
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    AuthorId = b.Author.AuthorId,
                    Author = b.Author.AuthorName,
                    Publisher = b.Publisher,
                    ReleaseDate = b.ReleaseDate,
                    Content = b.Content,
                    Description = b.Description,
                    Imgsrc = b.Imgsrc,
                    Views = b.Views,
                    price = b.price,
                    CategoryId = b.CategoryId,
                    Category = b.Category.CategoryName,
                    ForMembership = b.ForMembership,
                }).ToListAsync();
            return Ok(books);
        }

        //Lấy sách theo id
        [HttpGet("{id}")]
        public async Task<ActionResult<BookDTO>> GetBook(int id)
        {
            var book = await _context.Books
              .Include(b => b.Author)  // Tải dữ liệu tác giả eager loading
              .Include(b => b.Category)  // Tải dữ liệu danh mục eager loading
              .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            var bookDTO = new BookDTO
            {
                BookId = book.BookId,
                Title = book.Title,
                AuthorId = book.Author?.AuthorId,  // Sử dụng optional chaining ở đây
                Author = book.Author?.AuthorName,
                Publisher = book.Publisher,
                Content = book.Content,
                price = book.price,
                Description = book.Description,
                Imgsrc = book.Imgsrc,
                Views = book.Views,
                ReleaseDate = book.ReleaseDate,
                Category = book.Category?.CategoryName,
                CategoryId = book.Category?.CategoryId,
                ForMembership = book.ForMembership,
            };

            return Ok(bookDTO);
        }

        //Tạo 1 sách mới
        [HttpPost("post")]
        public async Task<ActionResult<Book>> PostProduct(BookDTO book, int? userId = null)
        {
            if (ModelState.IsValid)
            {
               
                var existingCategory = await _context.Categories.FindAsync(book.CategoryId);
                if (existingCategory == null)
                { 
                    
                    return BadRequest($"Category with ID {book.CategoryId} does not exist.");
                }
                var existingAuthor = await _context.Authors.FindAsync(book.AuthorId);
                if (existingAuthor == null)
                {

                    return BadRequest($"Author with ID {book.AuthorId} does not exist.");
                }

                Book newBook = new Book();

                newBook.Title = book.Title;
                newBook.Author = existingAuthor;
                newBook.Publisher = book.Publisher;
                newBook.Content = book.Content;
                newBook.price = book.price;
                newBook.Description = book.Description;
                newBook.Imgsrc = book.Imgsrc;
                newBook.Views = book.Views;
                newBook.ReleaseDate = DateTime.Now;
                newBook.Category = existingCategory;
                newBook.ForMembership = book.ForMembership;

                try
                {

                    _context.Books.Add(newBook);
                    await _context.SaveChangesAsync();

                    return Ok();

                }
                catch (Exception ex)
                {
                    return BadRequest($"Error updating book: {ex.Message}");
                }
            }

            return BadRequest(ModelState);
        }

        //Sửa sách theo id
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, BookDTO bookDTO, int? userId = null)
        {
            
            var existingBook = await _context.Books.FindAsync(id);
            if (existingBook == null)
            {
                return BadRequest($"Book ID {bookDTO.BookId} does not exist.");
            }

            
            var existingCategory = await _context.Categories.FindAsync(bookDTO.CategoryId);
            if (existingCategory == null)
            {
                
                return BadRequest($"Category with ID {bookDTO.CategoryId} does not exist.");
            }

            var existingAuthor = await _context.Authors.FindAsync(bookDTO.AuthorId);
            if (existingAuthor == null)
            {

                return BadRequest($"Author with ID {bookDTO.AuthorId} does not exist.");
            }


            existingBook.Title = bookDTO.Title;
            existingBook.Publisher = bookDTO.Publisher;
            existingBook.Content = bookDTO.Content;
            existingBook.Title = bookDTO.Title;
            existingBook.Description = bookDTO.Description;
            existingBook.Imgsrc = bookDTO.Imgsrc;
            existingBook.Views = bookDTO.Views;
            existingBook.price = bookDTO.price;
            existingBook.ReleaseDate = bookDTO.ReleaseDate;
            existingBook.Category = existingCategory;
            existingBook.Author = existingAuthor;
            existingBook.ForMembership = bookDTO.ForMembership;
            try
            {
                await _context.SaveChangesAsync();

                return Ok();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating book: {ex.Message}");
            }
        }

        //Xoá sách theo id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                var book = await _context.Books.FindAsync(id);

                if (book == null)
                {
                    return NotFound();
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("free")]
        public async Task<ActionResult<IEnumerable<Book>>> GetFreeBooks()
        {
            var freeBooks = await _context.Books
                .Where(b => b.ForMembership == false) // Chỉ lấy sách không yêu cầu thành viên
                .ToListAsync();

            return Ok(freeBooks);
        }

        [HttpGet("forMembership")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooksForMembership()
        {
            var freeBooks = await _context.Books
                .Where(b => b.ForMembership == true) // Chỉ lấy sách yêu cầu thành viên
                .ToListAsync();

            return Ok(freeBooks);
        }

        [HttpGet("new")]
        public async Task<ActionResult<IEnumerable<Book>>> GetNewBooks()
        {
            var newBooks = await _context.Books
                .OrderByDescending(b => b.ReleaseDate) // Sắp xếp theo ngày xuất bản giảm dần (sách mới nhất đầu tiên)
                .ToListAsync();

            return Ok(newBooks);
        }

        [HttpGet("most-viewed")]
        public async Task<ActionResult<Book>> GetMostViewedBook()
        {
            var mostViewedBook = await _context.Books
                .OrderByDescending(b => b.Views)
                .FirstOrDefaultAsync();

            if (mostViewedBook == null)
            {
                return NotFound();
            }

            var bookDTO = new BookDTO
            {
                BookId = mostViewedBook.BookId,
                Title = mostViewedBook.Title,
                Description = mostViewedBook.Description,
                Publisher = mostViewedBook.Publisher,
                Content = mostViewedBook.Content,
                Imgsrc = mostViewedBook.Imgsrc,
                Views = mostViewedBook.Views,
                ReleaseDate = mostViewedBook.ReleaseDate,
                ForMembership = mostViewedBook.ForMembership,
                price = mostViewedBook.price,
                CategoryId = mostViewedBook.CategoryId,
                Category = mostViewedBook.Category?.CategoryName,
                AuthorId = mostViewedBook.AuthorId,
                Author = mostViewedBook.Author?.AuthorName
            };

            return Ok(bookDTO);
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }

        public class BookDTO
        {
            public int BookId { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public string? Publisher { get; set; } 
            public string? Content { get; set; } 
            public string? Imgsrc { get; set; }
            public int Views { get; set; }
            public DateTime? ReleaseDate { get; set; }
            public bool? ForMembership { get; set; }
            public int? price { get; set; }
            public int? CategoryId { get; set; }
            public string? Category { get; set; }
            public int? AuthorId { get; set; }
            public string? Author { get; set; } = null;
        }
    }
}
