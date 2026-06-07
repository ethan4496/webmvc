using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;
using WebMVC.Entities;
using WebMVC.Interfaces;
using WebMVC.Ultilities.Enums;
using ZaloDotNetSDK;

public class ZaloWebhookController : Controller
{
    private readonly IWebConfigurationService _webConfigurationService;
    private readonly ITransportationService _transportationService;
    private readonly IZaloAPIService _zaloAPIService;

    public ZaloWebhookController(IWebConfigurationService webConfigurationService, ITransportationService transportationService,
        IZaloAPIService zaloAPIService)
    {
        _webConfigurationService = webConfigurationService;
        _transportationService = transportationService;
        _zaloAPIService = zaloAPIService;
    }

    [Route("zaloapp")]
    public async Task<IActionResult> Index()
    {
        var code = HttpContext.Request.Query["code"];
        var OAId = HttpContext.Request.Query["oa_id"];
        if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(code))
        {
            var zaloCode = await _zaloAPIService.GetByKeyCode("code");
            var zaloOA = await _zaloAPIService.GetByKeyCode("oa_id");
            await _zaloAPIService.Update(zaloCode.Id, zaloCode.Key, code);
            await _zaloAPIService.Update(zaloOA.Id, zaloOA.Key, OAId);
            await _zaloAPIService.GetTokenFromCode();
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveWebhook()
    {
        using (var reader = new StreamReader(Request.Body))
        {
            string json = await reader.ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(json);

            if (data?.event_name == "user_send_text")
            {
                string userId = data.sender.id;
                string textMessage = data.message.text;

                string responseMessage = await ProcessTrackingCode(textMessage);
                await SendMessageToUser(userId, responseMessage);
            }
        }
        return Ok();
    }

    private async Task<string> ProcessTrackingCode(string message)
    {
        string barcode = ExtractSubstringAfterTPK(message.ToLower());
        if (!string.IsNullOrEmpty(barcode))
        {
            var config = await _webConfigurationService.GetById();
            var transporation = await _transportationService.GetByBarcode(barcode);
            if (transporation != null && transporation?.Status > (int)ETransportationStatus.New)
            {
                string trangThai = "";
                string thoiGian = "";
                string noiDungSau = "";
                switch (transporation.Status)
                {
                    case (int)ETransportationStatus.ArrivedAtTQWarehouse:
                        trangThai = ETransportationStatusName.GetStatusName(transporation.Status);
                        thoiGian = transporation.DateArrivedAtTQWarehouse.HasValue ? $"ngày {transporation.DateArrivedAtTQWarehouse:dd/MM/yyyy HH:mm}" : "";
                        noiDungSau = config.Hook02;
                        break;
                    case (int)ETransportationStatus.ExitedFromTQWarehouse:
                        trangThai = ETransportationStatusName.GetStatusName(transporation.Status);
                        thoiGian = transporation.DateExitedFromTQWarehouse.HasValue ? $"ngày {transporation.DateExitedFromTQWarehouse:dd/MM/yyyy HH:mm}" : "";
                        noiDungSau = config.Hook03;
                        break;
                    case (int)ETransportationStatus.CustomsInspectedGoods:
                        trangThai = ETransportationStatusName.GetStatusName(transporation.Status);
                        thoiGian = transporation.DateCustomsInspectedGoods.HasValue ? $"ngày {transporation.DateCustomsInspectedGoods:dd/MM/yyyy HH:mm}" : "";
                        noiDungSau = config.Hook04;
                        break;
                    case (int)ETransportationStatus.ReturningToVNWarehouse:
                        trangThai = ETransportationStatusName.GetStatusName(transporation.Status);
                        thoiGian = transporation.DateReturningToVNWarehouse.HasValue ? $"ngày {transporation.DateReturningToVNWarehouse:dd/MM/yyyy HH:mm}" : "";
                        noiDungSau = config.Hook05;
                        break;
                    case (int)ETransportationStatus.ArrivedAtVNWarehouse:
                        trangThai = ETransportationStatusName.GetStatusName(transporation.Status);
                        thoiGian = transporation.DateArrivedAtVNWarehouse.HasValue ? $"ngày {transporation.DateArrivedAtVNWarehouse:dd/MM/yyyy HH:mm}" : "";
                        noiDungSau = config.Hook06;
                        break;
                    case (int)ETransportationStatus.Completed:
                        trangThai = ETransportationStatusName.GetStatusName(transporation.Status);
                        thoiGian = transporation.DateCompleted.HasValue ? $"ngày {transporation.DateCompleted:dd/MM/yyyy HH:mm}" : "";
                        noiDungSau = config.Hook07;
                        break;
                }
                return $"Vận đơn {barcode}_{transporation?.UserNote} của Anh Chị {trangThai}_{thoiGian}. {noiDungSau}";
            }
            return config.Hook01;
        }
        return "";
    }

    private async Task SendMessageToUser(string userId, string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        var apiZalo = await _zaloAPIService.GetByKeyCode("access_token");
        string accessToken = apiZalo.Value;

        try
        {
            ZaloClient client = new ZaloClient(accessToken);
            JObject response = client.sendTextMessageToUserIdV3(userId, message);
        }
        catch
        {
        }
    }

    private string ExtractSubstringAfterTPK(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        int tpkIndex = input.IndexOf("tpk", StringComparison.OrdinalIgnoreCase);
        if (tpkIndex == -1) return string.Empty;

        string remaining = input[(tpkIndex + 3)..].Trim();
        int spaceIndex = remaining.IndexOf(' ');

        return spaceIndex == -1 ? remaining : remaining[..spaceIndex];
    }
}
