using System.Net;
using System.Net.Mail;

namespace HR_System_API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // 1. 從 appsettings.json 讀取設定
            var host = _configuration["Smtp:Host"];
            var port = int.Parse(_configuration["Smtp:Port"] ?? "25");
            var username = _configuration["Smtp:Username"]; // 開發環境通常是空的
            var password = _configuration["Smtp:Password"]; // 開發環境通常是空的
            var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@hr-system.com";

            // 2. 設定 SMTP 客戶端
            using (var client = new SmtpClient(host, port))
            {
                // 如果有設定帳號密碼，就進行驗證 (正式環境會用到)
                if (!string.IsNullOrEmpty(username))
                {
                    client.Credentials = new NetworkCredential(username, password);
                    client.EnableSsl = true; // 正式環境通常需要 SSL
                }

                // 3. 建立信件內容
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true // 支援 HTML 格式
                };
                mailMessage.To.Add(to);

                // 4. 寄出！
                await client.SendMailAsync(mailMessage);
            }
        }
    }
}