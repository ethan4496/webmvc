namespace WebMVC.Models.Requests.Creates
{
    public class CreatePermissionRequest
    {
        public string Name { get; set; }
        public string FrontendComponent { get; set; }
        public int? ParentId { get; set; }
        public string Controller { get; set; }
        public string Acction { get; set; }
    }
}
