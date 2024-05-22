using BookReadingService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookReadingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembershipController : ControllerBase
    {
        private readonly NamNguyenBookDBContext _context;

        public MembershipController(NamNguyenBookDBContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Membership>>> GetMemberships()
        {
            var memberships = await _context.Memberships
                .OrderBy(m => m.MembershipId)
                .ToListAsync();

            return memberships;
        }

        [HttpGet("{id}")]
        public ActionResult<Membership> GetMembership(int id)
        {
            var membership = _context.Memberships.Find(id);
            if (membership == null)
            {
                return NotFound();
            }
            return membership;
        }

        [HttpPost]
        public ActionResult<Membership> AddMembership(Membership membership)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.Memberships.Add(membership);
            _context.SaveChanges();
            return Ok(membership);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateMembership(int id, Membership membership)
        {
            // Kiểm tra dữ liệu đầu vào
            if (!ModelState.IsValid || id != membership.MembershipId)
            {
                return BadRequest(ModelState);
            }

            var existingMembership = _context.Memberships.Find(id);
            if (existingMembership == null)
            {
                return NotFound();
            }

            _context.Entry(existingMembership).CurrentValues.SetValues(membership);
            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteMembership(int id)
        {
            var membership = _context.Memberships.Find(id);
            if (membership == null)
            {
                return NotFound();
            }
            _context.Memberships.Remove(membership);
            _context.SaveChanges();
            return NoContent();
        }

        [HttpGet("search/{name}")]
        public ActionResult<IEnumerable<Membership>> SearchMemberships(string name)
        {
            var upperName = name.ToUpper();
            return _context.Memberships.Where(m => m.MembershipName.ToUpper().Contains(upperName)).ToList();
        }
    }
}
