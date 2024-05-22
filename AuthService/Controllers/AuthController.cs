using AuthService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly NamNguyenBookDBContext _dbContext;

        public AuthController(IConfiguration configuration, NamNguyenBookDBContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpGet("get-all")]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccount()
        {
            var accounts = await _dbContext.Accounts
                .OrderBy(a => a.AccountId)  // You may want to order by a specific property
                .Select(a => new AccountDTO
                {
                    AccountId = a.AccountId,
                    Username = a.Username,
                    Password = a.Password,
                    Fullname = a.Fullname,
                    Gender = a.Gender,
                    Email = a.Email,
                    Path = a.Path,
                    Role = a.Role,
                    Membership = a.Membership,
                    CreatedDate = a.CreatedDate,
                }).ToListAsync();
            return Ok(accounts);
        }

        [HttpPost("sign-in")]
        public IActionResult Post([FromBody] AccountForm user)
        {
            // Check if the user credentials are valid (you need to implement this logic)
            Account foundAccount = ValidateUserCredentials(user.Username, user.Password);

            if (foundAccount == null)
            {
                // Return some error response if the credentials are not valid
                return BadRequest("Invalid credentials");
            }

            var claims = new List<Claim>
        {
            new Claim("Username", foundAccount.Username),
            new Claim("Role", foundAccount.Role.ToString(), ClaimValueTypes.Integer32),
            new Claim("UserId", foundAccount.AccountId.ToString(), ClaimValueTypes.Integer32)
        };
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                 notBefore: now,
                 expires: now.Add(TimeSpan.FromMinutes(2)),
                 claims: claims,
                 signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT"])), SecurityAlgorithms.HmacSha256)
             );

            AuthToken authToken = new AuthToken
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(jwt),
                Expires = TimeSpan.FromMinutes(30).TotalSeconds
            };

            return Ok(authToken);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Register([FromBody] Account user)
        {
            // Validate user input
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if email already exists
            var emailExists = await _dbContext.Accounts.AnyAsync(u => u.Email == user.Email);
            if (emailExists)
            {
                return BadRequest("Email đã được sử dụng");
            }

            var usernameExists = await _dbContext.Accounts.AnyAsync(u => u.Username == user.Username);
            if (usernameExists)
            {
                return BadRequest("Email đã được sử dụng");
            }

            // Create and save new account
            var account = new Account
            {
                Username = user.Username,
                Email = user.Email,
                Password = user.Password,
                Fullname = user.Fullname,
                Path = user.Path,
                Gender = user.Gender,
                Role = user.Role,
                CreatedDate = DateTime.Now
            };

            await _dbContext.Accounts.AddAsync(account);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Đăng ký thành công" });
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditAccount([FromBody] Account model)
        {
            // Get user from token
            var user = await GetUserFromToken();

            // Validate user input
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update account information
            user.Username = model.Username;
            user.Password = model.Password;
            user.Fullname = model.Fullname;
            user.Path = model.Path;
            user.Gender = model.Gender;
            user.Email = model.Email;
            user.Membership = model.Membership;

            await _dbContext.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Cập nhật thông tin tài khoản thành công" });
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAccount()
        {
            // Get user from token
            var user = await GetUserFromToken();

            // Delete account
            _dbContext.Accounts.Remove(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Xóa tài khoản thành công" });
        } 

        [HttpPost("validate-token")]
        public IActionResult ValidateToken([FromBody] TokenValidationRequest tokenRequest)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JWT"]);

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(tokenRequest.AccessToken, validationParameters, out validatedToken);

                var expiresClaim = principal.FindFirst("exp");
                if (expiresClaim == null)
                {
                    return BadRequest(new { Valid = false, Message = "Token does not contain expiration claim." });
                }

                var expirationTime = UnixTimeStampToDateTime(double.Parse(expiresClaim.Value));

                if (expirationTime < DateTime.UtcNow)
                {
                    return BadRequest(new { Valid = false, Message = "Token has expired." });
                }

                return Ok(new { Valid = true, Expires = expirationTime });
            }
            catch (Exception)
            {
                // Token validation failed
                return BadRequest(new { Valid = false, Message = "Invalid token." });
            }
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)unixTimeStamp);
            return dateTimeOffset.UtcDateTime;
        }

        private async Task<Account> GetUserFromToken()
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Bạn cần cung cấp token truy cập");
            }

            var key = Encoding.ASCII.GetBytes(_configuration["JWT"]);
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                var userIdClaim = principal.FindFirst("UserId");
                if (userIdClaim == null)
                {
                    throw new UnauthorizedAccessException("Token không hợp lệ");
                }

                var userId = int.Parse(userIdClaim.Value);
                var user = await _dbContext.Accounts.FindAsync(userId);
                if (user == null)
                {
                    throw new UnauthorizedAccessException("Tài khoản không tồn tại");
                }

                return user;
            }
            catch (Exception)
            {
                throw new UnauthorizedAccessException("Token không hợp lệ");
            }
        }


        private Account ValidateUserCredentials(string username, string password)
        {
            // Implement logic to validate the user credentials against your database
            // For example, query the database to check if the username and password match a user record

            // Replace the logic below with your actual database validation logic
            var account = _dbContext.Accounts.FirstOrDefault(u => u.Username == username && u.Password == password);

            return account;
        }
    }
}
public class AccountForm
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class AuthToken
{
    public string? AccessToken { get; set; }
    public double Expires { get; set; }
}
public class TokenValidationRequest
{
    public string? AccessToken { get; set; }
}

public partial class AccountDTO
{
    public int AccountId { get; set; }
    public string Fullname { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Path { get; set; } = null!;
    public bool? Gender { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!;
    public bool? Membership { get; set; }
    public DateTime? CreatedDate { get; set; }
}
