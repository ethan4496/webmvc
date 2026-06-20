namespace WebMVC.Ultilities.Enums
{
    public enum EPricingType
    {
        Weight = 1,
        Volume = 2
    }

    public class EPricingTypeName
    {
        public static string GetPricingTypeName(int type)
        {
            switch (type)
            {
                case (int)EPricingType.Weight:
                    return "Cân nặng";
                case (int)EPricingType.Volume:
                    return "Số khối";
                default:
                    return "";
            };
        }
    }
}
