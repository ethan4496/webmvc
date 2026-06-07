using System.ComponentModel.DataAnnotations.Schema;

namespace WebMVC.Models.Requests.Creates
{
    public class CreatePricingSeparateRequest
    {
        public int AccountId { get; set; }
        public int ShipId { get; set; }
        public double? RangeWeightMin { get; set; }
        public double? RangeWeightMax { get; set; }
        public double? PricePerWeightUnit { get; set; }
        public double? RangeVolumeMin { get; set; }
        public double? RangeVolumeMax { get; set; }
        public double? PricePerVolumeUnit { get; set; }
    }
}
