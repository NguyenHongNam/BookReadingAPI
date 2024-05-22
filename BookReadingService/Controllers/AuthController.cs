using BookReadingService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookReadingService.Controllers
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
                    Balance = a.Balance,
                    CreatedDate = a.CreatedDate,
                }).ToListAsync();
            return Ok(accounts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AccountDTO>> GetAccountById(int id)
        {
            var account = await _dbContext.Accounts
                .Where(a => a.AccountId == id)
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
                    Balance = a.Balance,
                    CreatedDate = a.CreatedDate,
                })
                .FirstOrDefaultAsync();

            if (account == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            return Ok(account);
        }

        [HttpPost("sign-in")]
        public IActionResult Post([FromBody] AccountForm user)
        {
            // Check if the user credentials are valid (you need to implement this logic)
            Account foundAccount = ValidateUserCredentials(user.Username, user.Password);

            if (foundAccount == null)
            {
                // Return some error response if the credentials are not valid
                return BadRequest("Sai thông tin tài khoản hoặc mật khẩu");
            }
            Account accountInfo = _dbContext.Accounts.FirstOrDefault(a => a.AccountId == foundAccount.AccountId);
            var currentAccount = new
            {
                foundAccount.Username,
                foundAccount.Role,
                foundAccount.AccountId,
                foundAccount.Fullname,
                foundAccount.Email,
                foundAccount.Balance,
                foundAccount.Gender,
                foundAccount.Membership
                // Add more properties as needed
            };

            var claims = new List<Claim>
        {
            new Claim("Username", foundAccount.Username),
            new Claim("Role", foundAccount.Role.ToString(), ClaimValueTypes.Integer32),
            new Claim("AccountId", foundAccount.AccountId.ToString(), ClaimValueTypes.Integer32),
            new Claim("Fullname", foundAccount.Fullname ),
            new Claim("Email", foundAccount.Email ),
            new Claim("Balance", foundAccount.Balance.ToString()),
            new Claim("Gender", foundAccount.Gender.ToString()),
            new Claim("Membership", foundAccount.Membership.ToString()),
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

            return Ok(new { AccessToken = authToken.AccessToken, Expires = authToken.Expires, AccountId = foundAccount.AccountId });
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
            var emailExists = await _dbContext.Accounts.AnyAsync(e => e.Email == user.Email);
            if (emailExists)
            {
                return BadRequest("Email đã được sử dụng");
            }

            var usernameExists = await _dbContext.Accounts.AnyAsync(u => u.Username == user.Username);
            if (usernameExists)
            {
                return BadRequest("Tên tài khoản đã được sử dụng");
            }

            // Create and save new account
            var account = new Account
            {
                Username = user.Username,
                Email = user.Email,
                Password = user.Password,
                Fullname = user.Fullname,
                Path = "null",
                Gender = user.Gender,
                Role = "User",
                Membership = false,
                Balance = 0,
                CreatedDate = DateTime.Now
            };

            await _dbContext.Accounts.AddAsync(account);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Đăng ký thành công" });
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

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> EditUser(int id, [FromBody] AccountDTO user)
        {
            // Check if user with the provided ID exists
            var account = await _dbContext.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound("Không tìm thấy tài khoản");
            }

            // Validate user input (optional, can be improved)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            account.Fullname = user.Fullname;
            account.Email = user.Email;
            account.Membership = user.Membership;
            account.Password = user.Password;
            account.Username = user.Username;
            account.Balance = user.Balance;
            account.Gender = (bool)user.Gender;

            // You can add logic to update other editable fields here

            _dbContext.Accounts.Update(account);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Cập nhật thông tin thành công" });
        }

        [HttpPost("register-membership/{accountId}")]
        public async Task<IActionResult> RegisterMembership(int accountId, [FromBody] int membershipId)
        {
            var account = await _dbContext.Accounts.FindAsync(accountId);
            if (account == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            var membership = await _dbContext.Memberships.FindAsync(membershipId);
            if (membership == null)
            {
                return NotFound("Không tìm thấy gói hội viên.");
            }

            if (account.Balance < membership.Price)
            {
                return BadRequest("Số dư trong tài khoản không đủ để đăng ký gói hội viên này.");
            }

            // Đủ số dư, đăng ký gói hội viên và cập nhật số dư mới
            account.Membership = true;
            account.Balance -= membership.Price;

            // Lưu thay đổi vào cơ sở dữ liệu
            _dbContext.Accounts.Update(account);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            // Find the account by ID
            var account = await _dbContext.Accounts.FindAsync(id);

            // Check if the account exists
            if (account == null)
            {
                return NotFound("Không tìm thấy tài khoản");
            }

            // Remove the account from the database
            _dbContext.Accounts.Remove(account);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Xóa tài khoản thành công" });
        }


        [HttpPut("deposit/{id}")]
        public ActionResult Deposit(int id, [FromBody] DepositRequest depositRequest)
        {
            if (depositRequest.Amount <= 0)
            {
                return BadRequest("Số tiền nạp phải lớn hơn 0.");
            }
            if (depositRequest.Series != "ABCDE12345")
            {
                return BadRequest("Mã series không đúng.");
            }
            var account = _dbContext.Accounts.FirstOrDefault(a => a.AccountId == id);
            if (account == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            account.Balance += depositRequest.Amount;
            _dbContext.SaveChanges();

            return Ok();
        }

        [HttpGet("membership-stats")]
        public async Task<IActionResult> GetMembershipStats()
        {
            var membershipStats = new
            {
                TotalWithMembership = await _dbContext.Accounts.CountAsync(a => a.Membership == true),
                TotalWithoutMembership = await _dbContext.Accounts.CountAsync(a => a.Membership == false)
            };

            return Ok(membershipStats);
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue()
        {
            var totalRevenue = await _dbContext.Accounts.SumAsync(a => a.Balance);
            return Ok(totalRevenue);
        }

        private Account ValidateUserCredentials(string username, string password)
        {
            var account = _dbContext.Accounts.FirstOrDefault(u => u.Username == username && u.Password == password);
            return account;
        }

        [HttpPut("change-password/{id}")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest changePasswordRequest)
        {
            // Kiểm tra xác thực người dùng, ví dụ: kiểm tra tên đăng nhập và mật khẩu hiện tại
            var account = await _dbContext.Accounts.FirstOrDefaultAsync(u => u.AccountId == id && u.Password == changePasswordRequest.CurrentPassword);
            if (account == null)
            {
                return BadRequest("Mật khẩu hiện tại không chính xác.");
            }

            // Cập nhật mật khẩu mới cho người dùng
            account.Password = changePasswordRequest.NewPassword;

            // Lưu thay đổi vào cơ sở dữ liệu
            await _dbContext.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Đổi mật khẩu thành công" });
        }

    }
}

public class AccountForm
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}
public class DepositRequest
{
    public int Amount { get; set; }
    public string Series { get; set; }
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

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
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
    public int? Balance { get; set; }
    public DateTime? CreatedDate { get; set; }
}
