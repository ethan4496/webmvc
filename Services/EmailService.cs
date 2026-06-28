using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Newtonsoft.Json;
using WebMVC.Interfaces;
using WebMVC.Models;

namespace WebMVC.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string to, string subject, string body, string from)
        {
            var accounts = _configuration
            .GetSection("EmailAccounts")
            .Get<List<EmailAccount>>();
            var account = accounts?
            .FirstOrDefault(x =>
                x.Username.Equals(from,
                    StringComparison.OrdinalIgnoreCase));
            if (account == null)
            {
                throw new Exception(
                    $"Email account '{from}' not found.");
            }
            Console.WriteLine(
                "account"
            );
            Console.WriteLine(
                JsonConvert.SerializeObject(account, Formatting.Indented)
            );
            var message = new MimeMessage();

            message.From.Add(
            new MimeKit.MailboxAddress(
                account.Name,
                account.Username));

            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = body
            };

            using var client = new SmtpClient();

            await client.ConnectAsync(
            account.Host,
            account.Port,
            SecureSocketOptions.SslOnConnect);

            await client.AuthenticateAsync(
            account.Username,
            account.Password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}