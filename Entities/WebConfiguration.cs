using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class WebConfiguration : BaseEntity
    {
        public string WebsiteName { get; set; }

        public string WebsiteUrl { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Currency { get; set; }

        public string Hook01 { get; set; }
        public string Hook02 { get; set; }
        public string Hook03 { get; set; }
        public string Hook04 { get; set; }
        public string Hook05 { get; set; }
        public string Hook06 { get; set; }
        public string Hook07 { get; set; }

        public int IsShowSignUpApp { get; set; }
        public bool IsSendZaloOA { get; set; }
        public string ZaloLink { get; set; }
        public string Hotline { get; set; }
        public string AppNotiImage { get; set; }
    }
}
