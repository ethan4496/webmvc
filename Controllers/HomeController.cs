using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IWebConfigurationService _webConfigurationService;
        private readonly ITrackingService _trackingService;

        public HomeController(IAccountService accountService, IWebConfigurationService webConfigurationService, ITrackingService trackingService)
        {
            _accountService = accountService;
            _webConfigurationService = webConfigurationService;
            _trackingService = trackingService;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var webConfiguration = await _webConfigurationService.GetById();
            return View(model: webConfiguration.AppNotiImage);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("gioi-thieu")]
        public IActionResult Introduce()
        {
            return View();
        }

        [Route("huong-dan")]
        public IActionResult Guide()
        {
            return View();
        }

        [Route("bieu-phi")]
        public IActionResult Pricing()
        {
            return View();
        }

        [Route("chinh-sach")]
        public IActionResult Policy()
        {
            return View();
        }

        [Route("dich-vu")]
        public IActionResult Services()
        {
            return View();
        }

        [Route("tin-tuc")]
        public IActionResult News()
        {
            return View();
        }
        [Route("dang-ky")]
        public async Task<IActionResult> SignUpAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (!(await _accountService.IsUserSession()))
                    return RedirectToAction("Home", "Account");
                else
                    return RedirectToAction("Dashboard", "Statistic");
            }
            return View();
        }

        [Route("dang-nhap")]
        public async Task<IActionResult> SignInAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (!(await _accountService.IsUserSession()))
                    return RedirectToAction("Home", "Account");
                else
                    return RedirectToAction("Dashboard", "Statistic");
            }

            return View();
        }
        [Route("quen-mat-khau")]
        public async Task<IActionResult> ForgotPasswordAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (!(await _accountService.IsUserSession()))
                    return RedirectToAction("Home", "Account");
                else
                    return RedirectToAction("Dashboard", "Statistic");
            }
            return View();
        }

        [HttpPost]
        public async Task<ApiResponse> SignUp(CreateAccountRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            var data = await _accountService.SignupAsync(request);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Đăng ký thành công",
                Type = (int)EApiResponseType.Success,
                Data = data
            };
        }

        [HttpPost]
        public async Task<ApiResponse> SignIn(string username, string password)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            var account = await _accountService.SigninCookieAsync(username.Trim().ToLower(), password);
            var claims = new List<Claim>
            {
                new Claim("Username", account.Username),
                new Claim("Id", account.Id.ToString()),
                new Claim("Role", account.RoleId.ToString()),
                new Claim("PostOffice", (account.PostOffice ?? "")),
                new Claim("TransportationType", (account.TransportationType ?? 0).ToString()),
                new Claim("SpecialShipId", account.SpecialShipId ?? "")
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
            if (account.RoleId != (int)ERoleId.User && account.RoleId > 0)
            {
                HttpContext.Response.Cookies.Append("IsStaff", "1", new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(30),
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict
                });
            }
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Đăng nhập thành công",
                Type = (int)EApiResponseType.Success,
                Data = account.RoleId
            };
        }

        [HttpPost]
        public async Task<ApiResponse> ForgotPassword(string email)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException("ModelState InValid");
            }
            await _accountService.SendEmailPassword(email);
            return new ApiResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success,
            };
        }

        public async Task<IActionResult> SignOffAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Response.Cookies.Delete("IsStaff");
            return Redirect("/dang-nhap");
        }
        public IActionResult Staff()
        {
            HttpContext.Response.Cookies.Append("IsStaff", "1", new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(30),
                HttpOnly = true,
                SameSite = SameSiteMode.Strict
            });

            return RedirectToAction("Dashboard", "Statistic");
        }

        public IActionResult Customer()
        {
            HttpContext.Response.Cookies.Append("IsStaff", "0", new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(30),
                HttpOnly = true,
                SameSite = SameSiteMode.Strict
            });

            return RedirectToAction("Home", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> GetInfo([FromBody] string ordecode)
        {
            var result = await _trackingService.GetInfo(ordecode);
            return Json(new { d = result });
        }
    }
}
