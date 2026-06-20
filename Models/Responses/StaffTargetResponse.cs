using System.ComponentModel.DataAnnotations.Schema;
using WebMVC.Entities;

namespace WebMVC.Models.Responses
{
    public class StaffTargetResponse : StaffTarget
    {
        public string StaffAvatar { get; set; }
        public string Username { get; set; }
        public int? OrderReal { get; set; }
        public double? WeightReal { get; set; }
        public double? VolumeReal { get; set; }
        public int? NewAccountReal { get; set; }
        public decimal? TotalPriceVNDReal { get; set; }
        public int? TotalAccount { get; set; }
        public int? TotalNewAccountHasOrder { get; set; }
        public decimal? TotalPriceVNDNewAccountHasOrder { get; set; }

        public double Profit { get; set; }
        public string Status { get; set; }

        public decimal? TotalPriceVNDRealCN { get; set; }
        public decimal? TotalPriceVNDRealHT { get; set; }
        public decimal? TotalPriceVNDRealDT { get; set; }

    }
}
