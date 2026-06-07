using Microsoft.AspNetCore.Mvc;
using WebMVC.Interfaces;
using WebMVC.Ultilities;

namespace WebMVC.ViewComponents
{
    public class ChatWindowViewComponent : ViewComponent
    {
        private readonly IHttpContextService _httpContextService;

        public ChatWindowViewComponent(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;

        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var currentPath = HttpContext.Request.Path.Value?.ToLower();
            if (currentPath != null && currentPath.StartsWith("/chat"))
            {
                return Content(string.Empty); // Không render gì
            }
            var currentAccount = await _httpContextService.GetCurrentAccount();
            string value = $"{currentAccount.Id}|{currentAccount.Username}|{currentAccount.RoleId}";
            string token = AesEncryptionHelper.Encrypt(value);

            ViewData["ChatToken"] = token;
            return View();
        }
    }
}
