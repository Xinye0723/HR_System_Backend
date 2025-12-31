namespace HR_System_API.Services
{
    public interface IEmailService
    {
        // 定義一個非同步的寄信方法
        // to: 收件人, subject: 主旨, body: 內容 (支援 HTML)
        Task SendEmailAsync(string to, string subject, string body);
    }
}