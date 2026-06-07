using Microsoft.AspNetCore.Mvc;
using WebMVC.Interfaces;

namespace WebMVC.ViewComponents
{
    public class SidebarMenuViewComponent : ViewComponent
    {
        private readonly IHttpContextService _httpContextService;

        public SidebarMenuViewComponent(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;
        }

        public IViewComponentResult Invoke()
        {
            var account = _httpContextService.GetLoggedModel();
            return View(account);
        }
    }
}
