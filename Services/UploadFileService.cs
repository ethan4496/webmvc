using SelectPdf;
using WebMVC.Interfaces;

namespace WebMVC.Services
{
    public class UploadFileService : IUploadFileService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env;

        public UploadFileService(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env)
        {
            _httpContextAccessor = httpContextAccessor;
            _env = env;
        }
        public async Task<string> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return "";
            }

            // Kiểm tra định dạng ảnh
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!Array.Exists(validExtensions, ext => ext == extension))
            {
                return "";
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Giảm kích thước ảnh trước khi lưu
            //using (var image = Image.Load(file.OpenReadStream()))
            //{
            //    image.Mutate(x => x.Resize(new ResizeOptions
            //    {
            //        Size = new Size(500, 500), // Resize về kích thước tối đa 500x500 px
            //        Mode = ResizeMode.Max
            //    }));

            //    await using var outputStream = new FileStream(filePath, FileMode.Create);
            //    await image.SaveAsync(outputStream, new JpegEncoder { Quality = 80 }); // Giảm chất lượng xuống 80%
            //}

            // Ghi file vào ổ đĩa
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            // Lấy Scheme và Host từ HttpContext
            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = request != null ? $"{request.Scheme}://{request.Host}" : "";

            var fileUrl = $"{baseUrl}/uploads/images/{uniqueFileName}";
            return fileUrl;
        }
        public async Task<string> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return "";
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/files");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Ghi file vào ổ đĩa
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            // Lấy Scheme và Host từ HttpContext
            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = request != null ? $"{request.Scheme}://{request.Host}" : "";
            var fileUrl = $"{baseUrl}/uploads/files/{uniqueFileName}";
            return fileUrl;
        }

        public async Task<string> SavePXKToPdf(string fileHtml, string id)
        {
            var converter = new HtmlToPdf();
            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.MarginTop = 0;
            converter.Options.MarginBottom = 0;
            converter.Options.MarginLeft = 0;
            converter.Options.MarginRight = 0;

            var pdfBytes = converter.ConvertHtmlString(fileHtml);

            // Đọc nội dung file HTML
            string emailTemplatePath = Path.Combine(_env.WebRootPath, "templates", "EmailPXKTemplate.html");
            string emailContent = await File.ReadAllTextAsync(emailTemplatePath);

            string fileName = "pxk" + id.Trim() + ".pdf";
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/pxks");
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (MemoryStream pdfStream = new MemoryStream())
            {
                pdfBytes.Save(pdfStream);
                pdfStream.Position = 0;
                using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    pdfStream.WriteTo(file);
                }
            }
            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = request != null ? $"{request.Scheme}://{request.Host}" : "";
            var fileUrl = $"{baseUrl}/uploads/pxks/{fileName}";
            return fileUrl;
        }
    }
}
