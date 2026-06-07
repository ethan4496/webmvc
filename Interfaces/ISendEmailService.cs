namespace WebMVC.Interfaces
{
    public interface ISendEmailService
    {
        void Send(string toEmail, string subject, string message);
        void Send(string toEmail, string subject, string message, byte[] attachmentData, string attachmentName);
    }
}
