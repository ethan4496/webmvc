using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models.Responses;
using WebMVC.Services;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Controllers
{
    [Route("WebService1.asmx")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly ITransportationService _transportationService;
        private readonly IAccountService _accountService;
        private readonly IAppApiService _appApiService;

        public ServiceController(ITransportationService transportationService, IAccountService accountService, IAppApiService appApiService)
        {
            _transportationService = transportationService;
            _accountService = accountService;
            _appApiService = appApiService;
        }

        [HttpPost("UploadFile")]
        public async Task<ResponseClass> UploadFile([FromForm] IFormFile file)
        {
            return await _appApiService.UploadFile(file);
        }
    }
}
