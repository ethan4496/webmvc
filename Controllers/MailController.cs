using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WebMVC.Extensions;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Creates;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Requests.Updates;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;
using WebMVC.Data;
using Newtonsoft.Json;
using Microsoft.AspNetCore.DataProtection;

namespace WebMVC.Controllers
{
    [Route("api")]
    [ApiController]

    public class MailController : ControllerBase
    {
        private readonly ITemplateService _templateService;
        private readonly ICampaignService _campaignService;
        private readonly IContactService _contactService;
        private readonly IUploadFileService _uploadFileService;
        private readonly IServiceScopeFactory _scopeFactory;
        public MailController(ITemplateService templateService, ICampaignService campaignService, IContactService contactService, IUploadFileService uploadFileService, IServiceScopeFactory scopeFactory)
        {
            _templateService = templateService;
            _campaignService = campaignService;
            _contactService = contactService;
            _uploadFileService = uploadFileService;
            _scopeFactory = scopeFactory;
        }

        [Route("templates")]
        [HttpPost]
        public async Task<ApiResponse> GetTemplatePaging(TemplateSearch search)
        {
            var data = await _templateService.GetPagingApi(search);
            return new ApiResponse
            {
                Data = data,
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [Route("templates-status")]
        [HttpPost]
        public async Task<ApiResponse> GetStatusCountApi(TemplateSearch appUser)
        {
            var data = await _templateService.GetStatusCountApi(appUser);
            return new ApiResponse
            {
                Data = data,
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [Route("all-templates")]
        [HttpPost]
        public async Task<ApiResponse> GetAllTemplates(AppUser appUser)
        {
            var data = await _templateService.GetAllApi(appUser);
            return new ApiResponse
            {
                Data = data,
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [Route("create-templates")]
        [HttpPost]
        public async Task<ApiResponse> Create(CreateTemplateApiRequest request)
        {
            try
            {
                var data = await _templateService.CreateApi(request);
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.OK,
                    Type = (int)EApiResponseType.Success
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadGateway,
                    Type = (int)EApiResponseType.Error
                };
            }
        }
        [Route("upload-logo")]
        [HttpPost]
        public async Task<IActionResult> UploadSignatureLogo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file provided" });

            var path = await _uploadFileService.UploadImage(file);
            return Ok(new { path });
        }

        [Route("signature")]
        [HttpPost]
        public async Task<ApiResponse> Signature(AppUser request)
        {
            try
            {
                var signature = await _templateService.GetSignatureApi(request);
                return new ApiResponse
                {
                    Data = signature,
                    StatusCode = (int)HttpStatusCode.OK,
                    Type = (int)EApiResponseType.Success
                };
            }catch(Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadGateway,
                    Type = (int)EApiResponseType.Error
                };
            }
            
        }

        [Route("save-signature")]
        [HttpPost]
        public async Task<ApiResponse> SaveSignature(CreateSignatureApiRequest request)
        {
            try
            {
                await _templateService.saveSignatureApi(request);
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.OK,
                    Type = (int)EApiResponseType.Success
                };
            }catch(Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadGateway,
                    Type = (int)EApiResponseType.Error
                };
            }
        }
        [Route("edit-template/{id}")]
        [HttpGet]
        public async Task<ApiResponse> Edit(int id, AppUser request)
        {
            try
            {
                var template = await _templateService.GetTemplate(id, request);
                return new ApiResponse
                {
                    Data = template,
                    StatusCode = (int)HttpStatusCode.OK,
                    Type = (int)EApiResponseType.Success
                };
            }catch(Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadGateway,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }
        }

        [Route("edit-template/{id}")]
        [HttpPost]
        public async Task<ApiResponse> Save(int id, CreateTemplateApiRequest request)
        {
            try
            {
                var rs = await _templateService.SaveApiAsync(id, request);
                return new ApiResponse
                {
                    Data = rs,
                    StatusCode = (int)HttpStatusCode.OK,
                    Type = (int)EApiResponseType.Success
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadGateway,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }
        }
        [Route("template/{id}")]
        [HttpDelete]
        public async Task<ApiResponse> Delete(int id, AppUser request)
        {
            try
            {
                var rs = await _templateService.DeleteApiAsync(id, request);
                return new ApiResponse()
                {
                    Data = rs,
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Xóa thành công",
                    Type = (int)EApiResponseType.Success,
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadGateway,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }
        }
        [Route("campaigns")]
        [HttpPost]
        public async Task<ApiResponse> GetCampaignPaging(CampaignSearchApi search)
        {
            var data = await _campaignService.GetPagingApi(search);
            return new ApiResponse
            {
                Data = data,
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }
        [Route("campaigns-status")]
        [HttpPost]
        public async Task<ApiResponse> GetStatusCampaignCountApi(AppUser appUser)
        {
            var data = await _campaignService.GetStatusCampaignCountApi(appUser);
            return new ApiResponse
            {
                Data = data,
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }
        [Route("all-campaigns")]
        [HttpPost]
        public async Task<ApiResponse> GetAllCampaign(AppUser request)
        {
            var data = await _campaignService.GetAllCampaigns(request);
            return new ApiResponse
            {
                Data = data,
                StatusCode = (int)HttpStatusCode.OK,
                Type = (int)EApiResponseType.Success
            };
        }

        [Route("create-campaign")]
        [HttpPost]
        public async Task<ApiResponse> CreateCampaign(CreateCampaignApiRequest body)
        {
            try
            {
                var data = await _campaignService.CreateApiAsync(body);
                return new ApiResponse()
                {
                    Data = data,
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Thêm thành công",
                    Type = (int)EApiResponseType.Success,
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadGateway,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }
        }
        [Route("campaign/{id}")]
        [HttpGet]
        public async Task<ApiResponse> GetCampaign(int id, AppUser userApp)
        {
            try
            {
                var data = await _campaignService.GetCampaign(id, userApp);
                return new ApiResponse
                {
                    Data = data,
                    StatusCode = (int)HttpStatusCode.OK,
                    Type = (int)EApiResponseType.Success
                };
            }catch(Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadGateway,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }
        }
        [Route("campaign/{id}")]
        [HttpPost]
        public async Task<ApiResponse> SaveCampaign(int id, CreateCampaignApiRequest request)
        {
            try
            {
                var data = await _campaignService.SaveApiAsync(id, request);
                return new ApiResponse
                {
                    Data = data,
                    StatusCode = (int)HttpStatusCode.OK,
                    Type = (int)EApiResponseType.Success
                };
            }catch(Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }
            
        }
        [Route("campaign/{id}")]
        [HttpDelete]
        public async Task<ApiResponse> DeleteCampaign(int id, AppUser request)
        {
            try
            {
                var rs = await _campaignService.DeleteApiAsync(id, request);
                return new ApiResponse()
                {
                    Data = rs,
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Xóa thành công",
                    Type = (int)EApiResponseType.Success,
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }
        }
        [Route("report/statistic")]
        [HttpPost]
        public async Task<ApiResponse> getStatistic(ReportStatistic request)
        {
            try
            {
                var data = await _campaignService.GetReportStatsApi(request);
                return new ApiResponse
                {
                    Data = data,
                    StatusCode = (int)HttpStatusCode.OK,
                    Type = (int)EApiResponseType.Success
                };
            }catch(Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }
        }
        [Route("report/bar-by-month")]
        [HttpPost]
        public async Task<ApiResponse> getMonthData(AppUser request)
        {
            try
            {
                var data = await _campaignService.GetMonthData(request);
                return new ApiResponse
                {
                    Data = data,
                    StatusCode = (int)HttpStatusCode.OK,
                    Type = (int)EApiResponseType.Success
                };
            }catch(Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }
            
        }
        [Route("report/logs")]
        [HttpPost]
        public async Task<ApiResponse> getLog(ReportLogs request)
        {
            try
            {
                var data = await _campaignService.GetApiLogs(request);
                return new ApiResponse
                {
                    Data = data,
                    StatusCode = (int)HttpStatusCode.OK,
                    Type = (int)EApiResponseType.Success
                };
            }catch(Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }
            
        }
        [Route("contact-lists")]
        [HttpPost]
        public async Task<ApiResponse> ContactLists(ContactListApiSearch request)
        {
            try
            {
                var data = await _contactService.GetPagingApi(request);
                return new ApiResponse
                {
                    Data = data,
                    StatusCode = (int)HttpStatusCode.OK,
                    Type = (int)EApiResponseType.Success
                };
            }catch(Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }
        }
        [Route("save-contact-lists")]
        [HttpPost]
        public async Task<ApiResponse> SaveContactLists([FromForm] CreateContactListApiRequest request)
        {
            try
            {
                var data = await _contactService.CreateApiAsync(request);
                return new ApiResponse
                {
                    Data = data,
                    StatusCode = (int)HttpStatusCode.OK,
                    Type = (int)EApiResponseType.Success
                };
            }catch(Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }
            
        }
        [Route("contact-lists/{id}")]
        [HttpPost]
        public async Task<ApiResponse> getContactList(int id, AppUser request)
        {
            try
            {
                var data = await _contactService.GetContactList(id, request);
                return new ApiResponse
                {
                    Data = data,
                    StatusCode = (int)HttpStatusCode.OK,
                    Type = (int)EApiResponseType.Success
                };
            }catch(Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }
        }
        [Route("update-list/{id}")]
        [HttpPut]
        public async Task<ApiResponse> UpdateList(int id, [FromForm] CreateContactListApiRequest request)
        {
            try
            {
                var data = await _contactService.UpdateContactList(id, request);
                return new ApiResponse
                {
                    Data = data,
                    StatusCode = (int)HttpStatusCode.OK,
                    Type = (int)EApiResponseType.Success
                };
            }catch(Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }
        }

        [Route("email-lists/{id}")]
        [HttpPost]
        public async Task<ApiResponse> GetEmailContactList(int id, EmailContactListSearch request)
        {
            try
            {
                var data = await _contactService.GetEmailContactList(id, request);
                return new ApiResponse
                {
                    Data = data,
                    StatusCode = (int)HttpStatusCode.OK,
                    Type = (int)EApiResponseType.Success
                };
            }catch(Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }   
        }
        [Route("add-email/{id}")]
        [HttpPost]
        public async Task<ApiResponse> AddEmailContact(int id, AddContactRequestApi request)
        {
            try
            {
                var data = await _contactService.AddEmailContactList(id, request);
                return new ApiResponse
                {
                    Data = data,
                    StatusCode = (int)HttpStatusCode.OK,
                    Type = (int)EApiResponseType.Success
                };
            }catch(Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.Message,
                };
            }   
        }
        [Route("contact-list/{id}")]
        [HttpDelete]
        public async Task<ApiResponse> DeleteContactList(int id, AppUser request)
        {
            try
            {
                var rs = await _contactService.DeleteApiAsync(id, request);
                return new ApiResponse()
                {
                    Data = rs,
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Xóa thành công",
                    Type = (int)EApiResponseType.Success,
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }
        }
        [Route("contact/{id}")]
        [HttpDelete]
        public async Task<ApiResponse> DeleteContact(int id, DeleteContactApi request)
        {
            try
            {
                var rs = await _contactService.DeleteContactApi(id, request);
                return new ApiResponse()
                {
                    Data = rs,
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Xóa thành công",
                    Type = (int)EApiResponseType.Success,
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }
        }
        [Route("contact/{id}")]
        [HttpPut]
        public async Task<ApiResponse> UpdateContact(int id, UpdateContactRequestApi request)
        {
            try
            {
                var rs = await _contactService.UpdateContactApi(id, request);
                return new ApiResponse()
                {
                    Data = rs,
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Xóa thành công",
                    Type = (int)EApiResponseType.Success,
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }
        }
        [Route("trackings")]
        [HttpPost]
        public async Task<ApiResponse> EmailTrackings(EmailTrackingApiSearch request)
        {
            try
            {
                var data = await _campaignService.GetEmailTrackingApi(request);
                return new ApiResponse
                {
                    Data = data,
                    StatusCode = (int)HttpStatusCode.OK,
                    Type = (int)EApiResponseType.Success
                };
            }catch(Exception ex)
            {
                return new ApiResponse
                {
                    Data = null,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Type = (int)EApiResponseType.Error,
                    Message = ex.ToString(),
                };
            }
        }
        [Route("campaign-report/export-tracking/{CampaignId}")]
        [HttpPost]
        public async Task<IActionResult> ExportTrackings(int CampaignId)
        {
            var db = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
            var campaign = await db.Campaigns.FindAsync(CampaignId);
            var contactList = await db.ContactLists.FindAsync(campaign.ContactId);
            var data = await db.EmailTrackings
                .Where(x => x.CampaignId == CampaignId)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("EmailTracking");

                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Email";
                worksheet.Cell(1, 3).Value = "Campaign Name";
                worksheet.Cell(1, 4).Value = "Email Sent";
                worksheet.Cell(1, 5).Value = "Created At";

                int row = 2;
                int stt = data.Count();
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = stt;
                    worksheet.Cell(row, 2).Value = item.RecipientEmail;
                    worksheet.Cell(row, 3).Value = campaign?.Name;
                    worksheet.Cell(row, 4).Value = campaign?.EmailSent;
                    worksheet.Cell(row, 5).Value = item.CreatedAt;
                    row++;
                    stt--;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"{FormatDate.Format(campaign.SendAt)}-CampaignId-{CampaignId}.xlsx");
                }
            }
        }
    }
}
