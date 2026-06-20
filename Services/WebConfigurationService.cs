using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using WebMVC.Entities;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Updates;

namespace WebMVC.Services
{
    public class WebConfigurationService : IWebConfigurationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextService _httpContextService;
        private readonly IMapper _mapper;

        public WebConfigurationService(IUnitOfWork unitOfWork, IHttpContextService httpContextService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _httpContextService = httpContextService;
            _mapper = mapper;
        }

        public async Task<WebConfiguration> GetById(int id = 1)
        {
            return await _unitOfWork.Repository<WebConfiguration>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> Update(UpdateWebConfigurationRequest request)
        {
            var currentDate = DateTime.Now;
            var currentAccount = _httpContextService.GetLoggedModel();
            var webConfiguration = await _unitOfWork.Repository<WebConfiguration>().GetQueryable().SingleOrDefaultAsync(x => x.Id == 1);
            _mapper.Map(request, webConfiguration);
            _unitOfWork.Repository<WebConfiguration>().Update(webConfiguration, currentDate, currentAccount.Id);
            return await _unitOfWork.SaveAsync() > 0;
        }
    }
}
