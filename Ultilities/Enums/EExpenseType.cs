namespace WebMVC.Ultilities.Enums
{
    public enum EExpenseType
    {
        Export = 0,
        Import = 1,
    }
    public class EExpenseTypeName
    {
        public static string GetTypeName(int type)
        {
            switch (type)
            {
                case (int)EExpenseType.Export:
                    return "Chi phí xuất";
                case (int)EExpenseType.Import:
                    return "Chi phí nhập";
                default:
                    return "";
            };
        }
    }
}
