using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using WebMVC.Interfaces;

namespace WebMVC.Ultilities
{
    //public class JwtValidationFilter : ActionFilterAttribute
    //{
    //    public override async void OnActionExecuting(ActionExecutingContext filterContext)
    //    {
    //        var context = filterContext.HttpContext;
    //        var _accountService = context.RequestServices.GetService<IAccountService>();
    //        var token = context.Session.GetString("JWTToken"); ;
    //        string refreshToken = context.Session.GetString("RefreshToken");

    //        if (!string.IsNullOrEmpty(token))
    //        {
    //            var handler = new JwtSecurityTokenHandler();
    //            try
    //            {
    //                var jwtToken = handler.ReadJwtToken(token);
    //                if (jwtToken.ValidTo < DateTime.UtcNow) // Token hết hạn
    //                {
    //                    if (!string.IsNullOrEmpty(refreshToken))
    //                    {
    //                        var data = await _accountService.RefreshToken(refreshToken, context);
    //                        if (data != null)
    //                        {
    //                            context.Session.SetString("JWTToken", data.Token);
    //                        }
    //                        else
    //                        {
    //                            context.Session.Clear();
    //                            filterContext.Result = new RedirectResult("/signin");
    //                        }
    //                    }
    //                    else
    //                    {
    //                        context.Session.Clear();
    //                        filterContext.Result = new RedirectResult("/signin");
    //                    }
    //                }
    //                var accountId = jwtToken?.Claims?.FirstOrDefault(c => c.Type == "Id")?.Value ?? "0";
    //                var roleId = Convert.ToInt32(jwtToken?.Claims?.FirstOrDefault(c => c.Type == "Role")?.Value ?? "0");
    //                // Lưu userId vào HttpContext để truy cập sau
    //                context.Items["AccountId"] = accountId;
    //                context.Items["RoleId"] = roleId;
    //            }
    //            catch (Exception)
    //            {
    //                context.Session.Clear();
    //                filterContext.Result = new RedirectResult("/signin");
    //            }
    //        }
    //        base.OnActionExecuting(filterContext);
    //    }
    //}
}
