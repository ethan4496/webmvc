using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class BigPackageHistory : BaseEntity
    {
        public string Content { get; set; }

        public int BigPackageId { get; set; }
    }
}
