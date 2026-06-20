using WebMVC.Entities;
using WebMVC.Services;

namespace WebMVC.Interfaces
{
    public interface IZaloAPIService
    {
        Task<ZaloAPI> GetByKeyCode(string code);
        Task GetTokenFromCode();
        Task SendMessage(string phone, string templateId, ZaloAPIService.TemplateData template);
        Task SendMessageOutStock(string phone, string templateId, ZaloAPIService.TemplateOutStockData template);
        Task<bool> Update(int id, string key, string value);
    }
}
