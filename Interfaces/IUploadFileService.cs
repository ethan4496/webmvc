
namespace WebMVC.Interfaces
{
    public interface IUploadFileService
    {
        Task<string> SavePXKToPdf(string fileHtml, string id);
        Task<string> UploadFile(IFormFile file);
        Task<string> UploadImage(IFormFile file);
    }
}
