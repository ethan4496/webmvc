using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Ultilities
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class WebRoleFilter : Attribute, IAuthorizationFilter
    {
        private readonly int[] _notAllowedRoles;
        private readonly bool _isStaff;

        public WebRoleFilter(bool isStaff, params int[] notAllowedRoles)
        {
            _notAllowedRoles = notAllowedRoles;
            _isStaff = isStaff;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new RedirectResult("/dang-nhap");
                return;
            }

            var roleClaim = user.Claims.FirstOrDefault(c => c.Type == "Role");

            if (roleClaim == null || !int.TryParse(roleClaim.Value, out int userRoleId))
            {
                context.Result = new RedirectResult("/dang-nhap");
                return;
            }

            var isStaffCookie = Convert.ToInt32(context.HttpContext?.Request.Cookies["IsStaff"] ?? "0");
            if (Convert.ToBoolean(isStaffCookie) != _isStaff)
            {
                context.Result = new RedirectResult("/dang-nhap");
                return;
            }

            if (_isStaff && userRoleId != (int)ERoleId.User)
            {
                return;
            }

            if (_notAllowedRoles.Contains(userRoleId))
            {
                context.Result = new RedirectResult("/dang-nhap");
                return;
            }
        }
    }
}
