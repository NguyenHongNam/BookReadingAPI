using BookReadingService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static BookReadingService.Controllers.BookController;

namespace BookReadingService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RatingController : Controller
    {
        private readonly NamNguyenBookDBContext _dbContext;

        public RatingController(NamNguyenBookDBContext context)
        {
            _dbContext = context;
        }
        [HttpGet("get")]
        public async Task<ActionResult<IEnumerable<RatingDTO>>> GetRating()
        {
            var ratings = await _dbContext.Ratings
                .OrderBy(r => r.RatingId)  // You may want to order by a specific property
                .Select(r => new RatingDTO
                {
                    RatingId = r.RatingId,
                    Comment = r.Comment,
                    AccountId = r.Account.AccountId,
                    BookId = r.Book.BookId,
                    CreatedDate = r.CreatedDate,
                    Status = r.Status,

                }).ToListAsync();
            return Ok(ratings);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RatingDTO>> GetRatingById(int id)
        {
            try
            {
                var rating = await _dbContext.Ratings.FindAsync(id);
                if (rating == null)
                {
                    return NotFound("Không tìm thấy đánh giá có ID: " + id);
                }
                return Ok(rating);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi lấy đánh giá: " + ex.Message);
            }
        }


        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateRating(int id, Rating rating)
        {
            try
            {
                if (id != rating.RatingId)
                {
                    return BadRequest("ID không khớp với đánh giá cần cập nhật");
                }

                _dbContext.Entry(rating).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Cập nhật đánh giá thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi cập nhật đánh giá: " + ex.Message);
            }
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateRating([FromBody] RatingDTO ratingDTO)
        {
            try
            {
                // Kiểm tra xem các thông tin bắt buộc đã được cung cấp chưa
                if (ratingDTO.BookId == null || ratingDTO.AccountId == null || string.IsNullOrEmpty(ratingDTO.Comment))
                {
                    return BadRequest("Vui lòng cung cấp đủ thông tin để đăng bình luận.");
                }

                // Tạo mới một đối tượng Rating từ DTO
                var rating = new Rating
                {
                    Comment = ratingDTO.Comment,
                    CreatedDate = DateTime.Now,
                    Status = true,
                    BookId = ratingDTO.BookId.Value,
                    AccountId = ratingDTO.AccountId.Value
                };

                // Thêm đối tượng Rating mới vào trong DbContext
                _dbContext.Ratings.Add(rating);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Đăng bình luận thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteRating(int id)
        {
            try
            {
                var rating = await _dbContext.Ratings.FindAsync(id);
                if (rating == null)
                {
                    return NotFound("Không tìm thấy đánh giá có ID: " + id);
                }

                _dbContext.Ratings.Remove(rating);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Xóa đánh giá thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi xóa đánh giá: " + ex.Message);
            }
        }



        [HttpGet("get-by-book/{bookId}")]
        public async Task<IActionResult> GetRatingsByBook(int bookId)
        {
            try
            {
                // Lấy danh sách các bình luận cho một cuốn sách cụ thể
                var ratings = await _dbContext.Ratings
                    .Where(r => r.BookId == bookId && r.Status == true) // Chỉ lấy những bình luận có trạng thái hiển thị là true
                    .ToListAsync();

                if (ratings == null || !ratings.Any())
                {
                    return NotFound("Không có bình luận nào cho cuốn sách này");
                }

                return Ok(ratings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        public class RatingDTO
        {
            public int RatingId { get; set; }
            public string? Comment { get; set; }
            public DateTime? CreatedDate { get; set; }
            public bool Status { get; set; }
            public int? BookId { get; set; }
            public int? AccountId { get; set; }
        }
    }
}
