using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class AccountSignature : BaseEntity
    {
        public int AccountId { get; set; }
        public string Body { get; set; }
        public string Logo { get; set; }

    }
}
