using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using Serilog;
using System.Net;
using WebMVC.BackgroundWorkers;
using WebMVC.Controllers;
using WebMVC.Entities;
using WebMVC.Interfaces;
using WebMVC.Models.Requests.Searchs;
using WebMVC.Models.Responses;
using WebMVC.Ultilities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IHttpContextService _httpContextService;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IBackgroundTaskQueue backgroundTaskQueue, IServiceScopeFactory serviceScopeFactory,
            IHttpContextService httpContextService, IUnitOfWork unitOfWork)
        {
            _backgroundTaskQueue = backgroundTaskQueue;
            _serviceScopeFactory = serviceScopeFactory;
            _httpContextService = httpContextService;
            _unitOfWork = unitOfWork;
        }

        public async Task PushMessage(PushMessageRequest request)
        {
            foreach (var item in request.AccountId)
            {
                var account = await _unitOfWork.Repository<Account>().GetQueryable().SingleOrDefaultAsync(x => x.Id == item);
                if (account != null)
                {
                    await PushOneSignalUser(account.OneSignalId, request.Title, request.Message);
                }
            }
        }

        public async Task<PagedList<Notification>> GetPaging(NotificationSearch search)
        {
            var currentLogged = _httpContextService.GetLoggedModel();
            var query = _unitOfWork.Repository<Notification>().GetQueryable().Where(x =>
                x.AccountId == currentLogged.Id
                && (search.Type < 1 || search.Type == null || search.Type == x.Type)
                && (search.IsRead == null || search.IsRead == x.IsRead)
                && ((currentLogged.IsStaff == 0 && x.IsStaff == false)
                    || (currentLogged.IsStaff == 1 && x.IsStaff == true))

            ).OrderByDescending(x => x.Id);
            var notifications = await query.Skip((search.PageIndex - 1) * search.PageSize).Take(search.PageSize).ToListAsync();
            var total = await query.CountAsync();
            return new PagedList<Notification>
            {
                PageIndex = search.PageIndex,
                PageSize = search.PageSize,
                TotalItem = total,
                Items = notifications
            };
        }

        public async Task<int> GetUnRead()
        {
            var currentLogged = _httpContextService.GetLoggedModel();
            var query = _unitOfWork.Repository<Notification>().GetQueryable().Where(x =>
                x.AccountId == currentLogged.Id && x.IsRead == false
                && ((currentLogged.IsStaff == 0 && x.IsStaff == false)
                    || (currentLogged.IsStaff == 1 && x.IsStaff == true))

            );
            return await query.CountAsync();
        }

        public async Task<bool> Read(int id)
        {
            var currentDate = DateTime.Now;
            var currentAccount = _httpContextService.GetLoggedModel();
            var notification = await _unitOfWork.Repository<Notification>().GetQueryable().SingleOrDefaultAsync(x => x.Id == id && x.AccountId == currentAccount.Id);
            notification.IsRead = true;
            _unitOfWork.Repository<Notification>().Update(notification, currentDate, currentAccount.Id);
            return await _unitOfWork.SaveAsync() > 0;
        }

        public async Task<bool> ReadAll()
        {
            var currentDate = DateTime.Now;
            var currentAccount = _httpContextService.GetLoggedModel();
            string sql = $"UPDATE Notifications SET IsRead = 1, Updated = '{currentDate}', UpdateBy = {currentAccount.Id} WHERE AccountId = {currentAccount.Id} AND IsRead = 0 AND IsStaff = {currentAccount.IsStaff}";
            await _unitOfWork.ExecuteSqlRawAsync(sql);
            return await _unitOfWork.SaveAsync() > 0;
        }

        public Task SendNotification(Notification notification, DateTime currentDate, int currentAccountId, int customerId = 0, int staffId = 0, List<int> staffRoleIds = null)
        {
            _backgroundTaskQueue.QueueBackgroundWorkItem(async token =>
            {
                using var scope = _serviceScopeFactory.CreateScope(); // Sử dụng scope factory
                var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var _signalRService = scope.ServiceProvider.GetRequiredService<ISignalRService>();

                try
                {
                    var notifications = new List<Notification>();
                    var receivers = new List<Account>();

                    var accountIds = new List<int>();
                    if (customerId > 0) accountIds.Add(customerId);
                    //if (staffId > 0) accountIds.Add(staffId);
                    if (staffRoleIds != null)
                    {
                        var staffOfRoles = await _unitOfWork.Repository<Account>()
                            .GetQueryable()
                            .Where(x => staffRoleIds.Contains(x.RoleId) && x.RoleId != (int)ERoleId.TQWarehouseStaff)
                            .ToListAsync();
                        accountIds.AddRange(staffOfRoles.Select(x => x.Id));
                    }

                    var accounts = await _unitOfWork.Repository<Account>()
                        .GetQueryable()
                        .Where(x => accountIds.Contains(x.Id))
                        .ToListAsync();

                    receivers.AddRange(accounts);

                    foreach (var account in receivers)
                    {

                        notifications.Add(new Notification
                        {
                            AccountId = account.Id,
                            IsStaff = notification.IsStaff,
                            Title = notification.Title,
                            Content = notification.Content,
                            Type = notification.Type,
                            WebUrl = notification.WebUrl
                        });
                    }

                    await _unitOfWork.Repository<Notification>().AddRange(notifications, currentDate, currentAccountId);
                    if (await _unitOfWork.SaveAsync() > 0)
                    {
                        foreach (var receiver in receivers)
                        {
                            var userId = receiver.Id.ToString();
                            var oneSignalId = receiver.OneSignalId;

                            _ = Task.Run(async () =>
                            {
                                await _signalRService.SendToastNotificationToUser(userId, notification.Content);
                                if (!string.IsNullOrEmpty(oneSignalId))
                                {
                                    await PushOneSignalUser(oneSignalId, notification.Title, notification.Content);
                                }
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Send notification failed: {JsonConvert.SerializeObject(notification)}; Error: {ex.Message};");
                }
            });

            return Task.CompletedTask;
        }

        public async Task PushOneSignalAllUser(string title, string content)
        {
            try
            {
                string onesignalAppId = "ca1d8fa1-3e74-4c4e-aec8-4bb3ae571306";//cái này sửa lại
                string onesignalRestId = "NTk4MWUzZGEtNmRkMy00YjZjLWI5ZjgtNDJkMjJlYWVmZGQ3";//cái này sửa lại
                RestClient client = new RestClient();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                var request = new RestRequest(new Uri("https://onesignal.com/api/v1/notifications"), Method.Post);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", "Basic " + onesignalRestId);
                request.AddHeader("content-type", "application/json");

                request.AddParameter("application/json", "{\"app_id\": \"" + onesignalAppId + "\",\"included_segments\": [\"All\"],\"contents\": { \"en\": \"" + content + "\"},\"headings\": {\"en\": \"" + title + "\"},\"android_large_icon\" : \"https://tpkexpress.com/Uploads/NewsIMG/6-7-2023-91156-AM.png\",\"large_icon\" : \"https://tpkexpress.com/Uploads/NewsIMG/6-7-2023-91156-AM.png\" }", ParameterType.RequestBody);
                RestResponse response = await client.ExecutePostAsync(request);
            }
            catch (Exception ex)
            {
                Log.Error($"PushOneSignalAllUser failed: title: {title}; content: {content}; Error: {ex.Message};");
            }
            finally { }
        }

        private async Task PushOneSignalUser(string deviceId, string title, string content)
        {
            try
            {
                if (!string.IsNullOrEmpty(deviceId))
                {
                    string onesignalAppId = "ca1d8fa1-3e74-4c4e-aec8-4bb3ae571306";//cái này sửa lại
                    string onesignalRestId = "NTk4MWUzZGEtNmRkMy00YjZjLWI5ZjgtNDJkMjJlYWVmZGQ3";//cái này sửa lại
                    RestClient client = new RestClient();
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    var request = new RestRequest(new Uri("https://onesignal.com/api/v1/notifications"), Method.Post);
                    request.AddHeader("accept", "*/*");
                    request.AddHeader("Basic", onesignalRestId);
                    request.AddHeader("content-type", "application/json");

                    request.AddParameter("application/json", "{\"app_id\": \"" + onesignalAppId + "\",\"include_player_ids\": [\"" + deviceId + "\"],\"contents\": { \"en\": \"" + content + "\"},\"headings\": {\"en\": \"" + title + "\"},\"android_large_icon\" : \"https://tpkexpress.com/Uploads/NewsIMG/6-7-2023-91156-AM.png\",\"large_icon\" : \"https://tpkexpress.com/Uploads/NewsIMG/6-7-2023-91156-AM.png\" }", ParameterType.RequestBody);
                    RestResponse response = await client.ExecutePostAsync(request);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"PushOneSignalAllUser failed: deviceId: {deviceId}; title: {title}; content: {content}; Error: {ex.Message};");
            }
            finally { }
        }


    }
}
