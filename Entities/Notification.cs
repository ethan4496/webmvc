using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class Notification : BaseEntity
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string WebUrl { get; set; }
        public int? Type { get; set; }
        public bool IsRead { get; set; } = false;
        public bool IsStaff { get; set; } = false;
        public int? AccountId { get; set; }
    }
}
