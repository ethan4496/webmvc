namespace WebMVC.Models.Requests.Updates
{
    public class UpdatePermissionRequest
    {
        public string Name { get; set; }
        public string FrontendComponent { get; set; }
        public int? ParentId { get; set; }
        public string Controller { get; set; }
        public string Acction { get; set; }
    }
}
