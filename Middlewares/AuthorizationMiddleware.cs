using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebMVC.Entities;
using WebMVC.Interfaces;

namespace WebMVC.Middlewares
{
    public class JwtValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public JwtValidationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
        {


            var token = context.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(token))
            {
                token = context.Session.GetString("JWTToken");
            }
            if (string.IsNullOrEmpty(token))
            {
                var endpoint = context.GetEndpoint();
                if (endpoint != null)
                {
                    var authorizeAttribute = endpoint.Metadata.GetMetadata<AuthorizeAttribute>();
                    if (authorizeAttribute == null)
                    {
                        await _next(context);
                        return;
                    }
                }
                await _next(context); // Continue the request processing
                return;
            }
            else
            {
                var handler = new JwtSecurityTokenHandler();
                try
                {
                    // Validate token
                    var principal = ValidateToken(token);

                    // Add user information to the context
                    context.User = principal;

                    var jwtToken = handler.ReadJwtToken(token);
                    var accountId = jwtToken?.Claims?.FirstOrDefault(c => c.Type == "Id")?.Value ?? "0";
                    var roleId = Convert.ToInt32(jwtToken?.Claims?.FirstOrDefault(c => c.Type == "Role")?.Value ?? "0");
                    // Lưu userId vào HttpContext để truy cập sau
                    context.Items["AccountId"] = accountId;
                    context.Items["RoleId"] = roleId;
                    //var routeValues = context.Request.RouteValues;
                    //string apiName = routeValues["controller"]?.ToString();
                    //string actionName = routeValues["action"]?.ToString();
                    //if (string.IsNullOrEmpty(apiName) && string.IsNullOrEmpty(actionName))
                    //{
                    //    var routeData = context.Request.Path.Value.Split("/");
                    //    if (routeData.Count() >= 2)
                    //        apiName = routeData[1];
                    //    if (routeData.Count() >= 3)
                    //        actionName = routeData[2];
                    //}
                    //if (apiName != "Home")
                    //{
                    //    var permission = await unitOfWork.Repository<RolePermission>()
                    //                .GetQueryable()
                    //                .Where(rp => rp.RoleId == roleId)
                    //                .Join(
                    //                    unitOfWork.Repository<Permission>().GetQueryable().Where(x => x.Controller == apiName && x.Acction == actionName),
                    //                    rolePermission => rolePermission.PermissionId,
                    //                    permission => permission.Id,
                    //                    (rolePermission, permission) => permission
                    //                )
                    //                .FirstOrDefaultAsync();
                    //    if (permission == null)
                    //    {
                    //        throw new Exception();
                    //    }
                    //}
                    await _next(context); // Continue the request processing
                }
                catch (Exception)
                {
                    throw new UnauthorizedAccessException("Unauthorize");
                }
            }
        }

        private ClaimsPrincipal ValidateToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = key
            };

            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
    }

}
