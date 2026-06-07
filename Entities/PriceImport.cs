using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities.Base;

namespace WebMVC.Entities
{
    public class PriceImport : BaseEntity
    {
        public string Name { get; set; }

        public string Hscode { get; set; }

        public decimal TaxVAT { get; set; }

        public decimal TaxNK { get; set; }

        public string Price { get; set; }

        public string Origin { get; set; }
        public bool Policy { get; set; }
    }
}
