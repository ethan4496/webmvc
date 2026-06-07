using Serilog;
using System.Net;
using System.Text.Json;
using WebMVC.Extensions;
using WebMVC.Models.Responses;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(Microsoft.AspNetCore.Http.HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                switch (error)
                {
                    case AggregateException e: //423
                        response.StatusCode = (int)HttpStatusCode.Locked;
                        break;
                    case AppException e: //400
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                    case UnauthorizedAccessException e: //401
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        break;
                    case InvalidCastException e: //403
                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                        break;
                    case KeyNotFoundException e: //404
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    case TimeoutException e: //408
                        response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                        break;
                    default:
                        {
                            var RouteData = context.Request.Path.Value.Split("/");
                            string apiName = string.Empty;
                            string actionName = string.Empty;

                            if (RouteData.Count() >= 2)
                                apiName = RouteData[1];
                            if (RouteData.Count() >= 3)
                                actionName = RouteData[2];

                            // Lưu lại Stream ban đầu
                            var originalBodyStream = context.Response.Body;

                            // Sử dụng MemoryStream để ghi nội dung
                            using (var memoryStream = new MemoryStream())
                            {
                                // Đặt body của request vào MemoryStream để đọc
                                context.Request.Body = memoryStream;

                                // Đọc nội dung request (nếu là JSON, ví dụ)
                                await context.Request.Body.CopyToAsync(memoryStream);
                                memoryStream.Seek(0, SeekOrigin.Begin);

                                // Đọc nội dung của body trong request (để ghi log hoặc xử lý)
                                using (var reader = new StreamReader(memoryStream))
                                {
                                    var requestBody = await reader.ReadToEndAsync();
                                    if(String.IsNullOrEmpty(requestBody))
                                        break;
                                    // Log body vào nơi bạn muốn, ví dụ: log file, database, etc.
                                    Log.Error($"API: {apiName}; Action: {actionName}; ");
                                    Log.Error(requestBody);
                                }
                                // Khôi phục lại stream ban đầu sau khi đọc xong
                                memoryStream.Seek(0, SeekOrigin.Begin);
                                context.Request.Body = memoryStream;
                            }
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        }
                        break;
                }
                var result = JsonSerializer.Serialize(new ApiResponse()
                {
                    StatusCode = response.StatusCode,
                    Message = error?.Message,
                    Type = (int)EApiResponseType.Error,
                });
                await response.WriteAsync(result);
            }
        }
    }
}
