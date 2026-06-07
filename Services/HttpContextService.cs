using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMVC.Entities;
using WebMVC.Interfaces;
using WebMVC.Models;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Services
{
    public class HttpContextService : IHttpContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public HttpContextService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }
        public async Task<Account> GetCurrentAccount()
        {
            try
            {
                var accountId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? "0");
                return await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == Convert.ToInt32(accountId));
            }
            catch
            {

                throw new UnauthorizedAccessException("Unauthorize");
            }
        }

        public LoggedModel GetLoggedModel()
        {
            var accountId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? "0");
            var roleId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value ?? "0");
            var transporationType = Convert.ToInt32(_httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "TransporationType")?.Value ?? "0");
            var postOffice = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "PostOffice")?.Value ?? "";
            if (roleId == (int)ERoleId.Admin || roleId == (int)ERoleId.Manager)
                postOffice = "";
            var isStaff = Convert.ToInt32(_httpContextAccessor.HttpContext?.Request.Cookies["IsStaff"] ?? "0");
            var specialShipId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "SpecialShipId")?.Value;
            return new LoggedModel
            {
                Id = accountId,
                RoleId = roleId,
                IsStaff = isStaff,
                PostOffice = postOffice,
                TransportationType = transporationType,
                SpecialShipId = specialShipId,
            };
        }
    }
}
