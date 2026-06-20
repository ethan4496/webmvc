using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using WebMVC.Entities;
using WebMVC.Interfaces;

namespace WebMVC.Services
{
    public class ZaloAPIService : IZaloAPIService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ZaloAPIService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ZaloAPI> GetByKeyCode(string code)
        {
            return await _unitOfWork.Repository<ZaloAPI>().GetQueryable().FirstOrDefaultAsync(t => t.Key == code);
        }

        public async Task<bool> Update(int id, string key, string value)
        {
            var zaloApi = await _unitOfWork.Repository<ZaloAPI>().GetQueryable().SingleOrDefaultAsync(t => t.Id == id);
            zaloApi.Key = key;
            zaloApi.Value = value;
            _unitOfWork.Repository<ZaloAPI>().Update(zaloApi, DateTime.Now, 0);
            return true;
        }

        public async Task SendMessage(string phone, string templateId, TemplateData template)
        {
            var webConfig = await _unitOfWork.Repository<WebConfiguration>().GetQueryable().SingleOrDefaultAsync(x => x.Id == 1);
            if (webConfig?.IsSendZaloOA != true)
                return;
            try
            {
                var data = new RootData();
                data.template_id = templateId;
                data.phone = "84" + phone.Remove(0, 1);
                data.template_data = template;
                data.tracking_id = Guid.NewGuid().ToString();
                var datastr = JsonConvert.SerializeObject(data);
                await SendZalo(datastr);
            }
            catch
            {

            }

        }

        public async Task SendMessageOutStock(string phone, string templateId, TemplateOutStockData template)
        {
            var webConfig = await _unitOfWork.Repository<WebConfiguration>().GetQueryable().SingleOrDefaultAsync(x => x.Id == 1);
            if (webConfig?.IsSendZaloOA != true)
                return;
            try
            {
                var data = new RootOutStockData();
                data.template_id = templateId;
                data.phone = "84" + phone.Remove(0, 1);
                data.template_data = template;
                data.tracking_id = Guid.NewGuid().ToString();
                var datastr = JsonConvert.SerializeObject(data);
                await SendZalo(datastr);
            }
            catch
            {

            }

        }

        public async Task GetTokenFromCode()
        {

            var key = await GetByKeyCode("secret_key");
            var code = await GetByKeyCode("code");
            var appId = await GetByKeyCode("app_id");
            var codeVerifier = await GetByKeyCode("code_verifier");

            var url = "https://oauth.zaloapp.com/v4/oa/access_token";

            WebHeaderCollection aPIHeaderValues = new WebHeaderCollection();
            aPIHeaderValues.Add("secret_key", key.Value.Trim());

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                   | SecurityProtocolType.Tls11
                   | SecurityProtocolType.Tls12
                   | SecurityProtocolType.Ssl3;

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";
            httpRequest.Headers.Add(aPIHeaderValues);

            httpRequest.ContentType = "application/x-www-form-urlencoded";


            var data = @"code=" + code.Value.Trim() + "&app_id=" + appId.Value.Trim() + "&grant_type=authorization_code&code_verifier=" + codeVerifier.Value.Trim() + "";

            byte[] dataStream = Encoding.UTF8.GetBytes(data);



            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    var tokennn = JsonConvert.DeserializeObject<RootToken>(result);

                    var access_token = await GetByKeyCode("access_token");

                    var refresh_token = await GetByKeyCode("refresh_token");

                    await Update(access_token.Id, access_token.Key, tokennn.access_token);
                    await Update(refresh_token.Id, refresh_token.Key, tokennn.refresh_token);
                }
            }
        }

        private async Task SendZalo(string datastr)
        {
            var token = await GetByKeyCode("access_token");

            var url = "https://business.openapi.zalo.me/message/template";

            WebHeaderCollection aPIHeaderValues = new WebHeaderCollection();
            aPIHeaderValues.Add("access_token", token.Value);


            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";
            httpRequest.Headers.Add(aPIHeaderValues);

            httpRequest.ContentType = "application/json";

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(datastr);
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                var message = JsonConvert.DeserializeObject<RootMessage>(result);


                if (message != null && (message.error == (int)ResultMessage.TokenInvalid || message.error == (int)ResultMessage.AccessTokenInvalid))
                {
                    var success = await GetTokenFromRefeshTokenAsync();
                    if (success == "success")
                    {
                        await SendZalo(datastr);
                    }
                    else
                    {

                    }
                }


            }
        }

        private async Task<string> GetTokenFromRefeshTokenAsync()
        {
            try
            {
                var success = "";
                var key = await GetByKeyCode("secret_key");
                var refresh_token = await GetByKeyCode("refresh_token");
                var appId = await GetByKeyCode("app_id");
                var codeVerifier = await GetByKeyCode("code_verifier");

                var url = "https://oauth.zaloapp.com/v4/oa/access_token";

                WebHeaderCollection aPIHeaderValues = new WebHeaderCollection();
                aPIHeaderValues.Add("secret_key", key.Value.Trim());

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;

                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "POST";
                httpRequest.Headers.Add(aPIHeaderValues);

                httpRequest.ContentType = "application/x-www-form-urlencoded";


                var data = @"refresh_token=" + refresh_token.Value.Trim() + "&app_id=" + appId.Value.Trim() + "&grant_type=refresh_token&code_verifier=" + codeVerifier.Value.Trim()
                    + "";

                byte[] dataStream = Encoding.UTF8.GetBytes(data);

                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var token = JsonConvert.DeserializeObject<RootToken>(result);

                        if (!string.IsNullOrEmpty(token.access_token) && !string.IsNullOrEmpty(token.refresh_token))
                        {
                            var access_tokennew = await GetByKeyCode("access_token");

                            var refresh_tokennew = await GetByKeyCode("refresh_token");

                            await Update(access_tokennew.Id, access_tokennew.Key, token.access_token);
                            await Update(refresh_tokennew.Id, refresh_tokennew.Key, token.refresh_token);
                            success = "success";
                        }
                        if (!string.IsNullOrEmpty(token.error_name))
                        {
                            success = "fail";
                        }

                    }
                }
                return success;
            }
            catch
            {
                return "";
            }
        }

        public class RootToken
        {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public string expires_in { get; set; }

            public string error_name { get; set; }
            public string error_reason { get; set; }
        }

        public class RootMessage
        {
            public int error { get; set; }
            public string message { get; set; }
        }

        public enum ResultMessage
        {
            Success = 0,
            Error = -100,
            AppInvalid = -101,
            OutQuota = -115,
            TokenInvalid = -124,
            AccessTokenInvalid = -216
        }

        public abstract class BaseRootData
        {
            public string phone { get; set; }
            public string template_id { get; set; }
            public string tracking_id { get; set; }

        }

        public class RootData : BaseRootData
        {
            public TemplateData template_data { get; set; }
        }

        public class RootOutStockData : BaseRootData
        {
            public TemplateOutStockData template_data { get; set; }
        }

        public class TemplateData
        {
            public string maVanDon { get; set; }
            public string soKien { get; set; }
            public string thoiGian { get; set; }
            public string username { get; set; }
            public string ghiChu { get; set; }
        }

        public class TemplateOutStockData
        {
            public string customer_name { get; set; }
            public string id_phieu { get; set; }
        }
    }
}
