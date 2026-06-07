using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebMVC.Data;
using WebMVC.Interfaces;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextService _httpContextService;

        public ChatController(AppDbContext context, IHttpContextService httpContextService)
        {
            _context = context;
            _httpContextService = httpContextService;       
        }
        [Route("chat")]
        public async Task<IActionResult> ChatMVC()
        {
            var currentAccount = await _httpContextService.GetCurrentAccount();
            string value = $"{currentAccount.Id}|{currentAccount.Username}|{currentAccount.RoleId}";
            string token = AesEncryptionHelper.Encrypt(value);
            return View(model: token);
        }
    }
}

