namespace WebMVC.Models.Responses
{
    public class ApiResponse
    {
        public int? StatusCode { get; set; }
        public string Message { get; set; }
        public int? Type { get; set; }
        public object Data { get; set; }
    }
}
