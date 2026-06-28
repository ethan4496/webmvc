using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
namespace WebMVC.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body, string from);
    }
}