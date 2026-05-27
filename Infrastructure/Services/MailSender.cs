using Application.IServices;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
namespace Infrastructure.Services
{
    public class MailSettings
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
    public class MailSender : IEmailSender, IDisposable
    {
        private readonly MailSettings _mailSettings;
        private readonly SmtpClient _client;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public MailSender(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
            _client = new SmtpClient("smtp.gmail.com", 587)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_mailSettings.UserName, _mailSettings.Password),
                EnableSsl = true,
            };
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await _semaphore.WaitAsync();
            try
            {
                var mailMessage = new MailMessage(_mailSettings.UserName!, email, subject, htmlMessage)
                {
                    IsBodyHtml = true
                };
                await _client.SendMailAsync(mailMessage);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
            _semaphore?.Dispose();
        }
    }
}

