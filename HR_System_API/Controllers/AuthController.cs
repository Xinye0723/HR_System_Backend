using HR_System_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HR_System_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly HrContext _context;
        private readonly IConfiguration _configuration;
        private readonly Services.IEmailService _emailService;
        // 注入資料庫連線 (HrContext) 和 設定檔讀取器 (IConfiguration)
        public AuthController(HrContext context, IConfiguration configuration, Services.IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService; 
        }

        // POST: api/Auth/Login
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            var user = _context.EmployeeInfos
                .FirstOrDefault(u => u.Account == loginDto.Account && u.Password == loginDto.Password);

            if (user == null) return Unauthorized("帳號或密碼錯誤");

            var tokenString = GenerateJwtToken(user);

            // ▼▼▼ 設定 Cookie 選項 ▼▼▼
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                // [修正] 改為 true。當 SameSite=None 時，必須開啟 Secure，否則瀏覽器會拒收。
                // (您的 Program.cs 有 app.UseHttpsRedirection()，代表後端是跑 HTTPS 的，所以沒問題)
                Secure = true,

                // [修正] 改為 SameSiteMode.None。
                // 這允許 Cookie 在不同 Port 之間傳送 (解決 Angular 4200 對接 API 的問題)。
                SameSite = SameSiteMode.None,

                Expires = DateTime.UtcNow.AddHours(2)
            };

            // 把 Token 寫入 Cookie
            Response.Cookies.Append("auth_token", tokenString, cookieOptions);

            // 回傳時，只回傳使用者資訊，不要再回傳 Token 字串了！
            return Ok(new
            {
                message = "登入成功",
                user = new { user.Account, user.Name, user.Role }
            });
        }

        // 私有方法：負責產生 Token 字串
        private string GenerateJwtToken(EmployeeInfo user)
        {
            // A. 讀取我們在 appsettings.json 設定的密鑰
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            // B. 設定 Token 裡面的「聲明 (Claims)」
            // 這些資訊會被加密在 Token 裡，前端解密後可以拿來用 (例如知道你是誰、權限是什麼)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.EmployeeId),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Account),
                    new Claim(JwtRegisteredClaimNames.Email, user.Account),
                    new Claim("Role", user.Role ?? "User"), // 把角色權限塞進去
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                // 設定過期時間 (例如 2 小時後過期)
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = issuer,
                Audience = audience,
                // 使用 HmacSha256 演算法簽名
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            // C. 產生字串
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("auth_token");
            return Ok(new { message = "已登出" });
        }
        // POST: api/Auth/ForgotPassword
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            // 1. 檢查使用者是否存在
            // (假設 Account 就是 Email)
            var user = _context.EmployeeInfos.FirstOrDefault(u => u.Account == request.Email);

            // 即使找不到人，為了安全起見 (避免被駭客試探帳號)，我們通常還是回傳 "成功"
            // 或者你可以回傳 BadRequest，看你的資安策略
            if (user == null)
            {
                return Ok(new { message = "如果此信箱存在，我們已發送重設信件。" });
            }

            // 2. 產生一個隨機的 Token (重設代碼)
            // 在正式系統中，你應該把這個 Token 存回資料庫，並設定有效期限 (例如 15 分鐘)
            // 這裡為了示範，我們先省略存資料庫的步驟，直接寄出去
            var resetToken = Guid.NewGuid().ToString();

            // 3. 組合前端的重設網址
            // 假設前端重設頁面是 /reset-password
            var resetLink = $"http://localhost:4200/reset-password?token={resetToken}&email={request.Email}";

            // 4. 寄信！
            var subject = "【HR 系統】重設密碼通知";
            var body = $@"
                <h3>您好，{user.Name}：</h3>
                <p>我們收到了您重設密碼的請求。</p>
                <p>請點擊下方連結以設定新密碼：</p>
                <a href='{resetLink}'>點此重設密碼</a>
                <br><br>
                <p>如果這不是您本人的操作，請忽略此信。</p>
            ";

            await _emailService.SendEmailAsync(request.Email, subject, body);

            return Ok(new { message = "重設信件已發送，請檢查您的信箱。" });
        }
        // POST: api/Auth/ResetPassword
        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword([FromBody] ResetPasswordDto request)
        {
            // 1. 找人
            var user = _context.EmployeeInfos.FirstOrDefault(u => u.Account == request.Email);
            if (user == null)
            {
                // 為了安全，通常不回傳明確的「找不到人」錯誤，但開發時可以回
                return BadRequest("無效的請求");
            }

            // 2. 驗證 Token (目前還沒做資料庫儲存，所以先假裝驗證)
            // 未來重構點：去資料庫查這個 Email 對應的 Token 是否吻合且沒過期
            if (string.IsNullOrEmpty(request.Token))
            {
                return BadRequest("無效的 Token");
            }

            // 3. 更新密碼
            // 正式環境請務必 Hash 密碼，這裡先明碼示範
            user.Password = request.NewPassword;

            // 4. 存檔
            _context.SaveChanges();

            return Ok(new { message = "密碼已更新" });
        }
    }
}