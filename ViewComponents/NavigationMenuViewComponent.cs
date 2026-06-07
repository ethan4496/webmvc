using Microsoft.AspNetCore.Mvc;
using WebMVC.Entities;
using WebMVC.Interfaces;
using WebMVC.Models;
using WebMVC.Services;

namespace WebMVC.ViewComponents
{
    public class NavigationMenuViewComponent : ViewComponent
    {
        private readonly IHttpContextService _httpContextService;

        public NavigationMenuViewComponent(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var account = await Task.FromResult(_httpContextService.GetLoggedModel());
            return View(account);
        }
    }
}