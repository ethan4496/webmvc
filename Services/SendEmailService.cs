using System.Net.Mail;
using System.Net;
using System.Text;
using WebMVC.Interfaces;
using WebMVC.Extensions;
using System.Net.Mime;

namespace WebMVC.Services
{
    public class SendEmailService : ISendEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _email;
        private readonly string _passWord;
        public SendEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _email = _configuration["EmailSettings:FromEmail"];
            _passWord = _configuration["EmailSettings:MailPassword"];
        }
        public void Send(string toEmail, string subject, string message, byte[] attachmentData, string attachmentName)
        {
            var send_mail = new MailMessage();
            send_mail.IsBodyHtml = true;
            send_mail.To.Add(new MailAddress(toEmail));
            send_mail.Subject = subject;
            send_mail.Body = message;

            var attachment = new Attachment(new MemoryStream(attachmentData), attachmentName, MediaTypeNames.Application.Pdf);
            send_mail.Attachments.Add(attachment);
            SmptSend(send_mail);
        }
        public void Send(string toEmail, string subject, string message)
        {
            var send_mail = new MailMessage();
            send_mail.IsBodyHtml = true;
            send_mail.To.Add(new MailAddress(toEmail));
            send_mail.Subject = subject;
            send_mail.Body = message;
            SmptSend(send_mail);
        }

        public void SmptSend(MailMessage mailMessage)
        {
            try
            {
                SmtpClient client = new SmtpClient();
                client.Port = 587;//outgoing port for the mail.
                client.Host = "smtp.gmail.com";
                client.EnableSsl = true;
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_email, _passWord);

                mailMessage.From = new MailAddress(_email);
                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                throw new AppException($"Gửi mail thất bại: {ex.Message}");
            }
        }
    }
}
