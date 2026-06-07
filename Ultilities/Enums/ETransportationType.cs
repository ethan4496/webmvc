namespace WebMVC.Ultilities.Enums
{
    public enum ETransportationType
    {
        HangLe1 = 1,
        HangLe2 = 2,
        HangLo = 3,
        ChinhNgach = 4,
        Khac = 5,
    }
    public class ETransportationTypeName
    {
        public static string GetTypeName(int status)
        {
            switch (status)
            {
                case (int)ETransportationType.HangLe1:
                    return "快递1号";
                case (int)ETransportationType.HangLe2:
                    return "快递2号";
                case (int)ETransportationType.HangLo:
                    return "物流";
                case (int)ETransportationType.ChinhNgach:
                    return "正报";
                case (int)ETransportationType.Khac:
                    return "其他";
                default:
                    return "";
            };
        }
    }

}
