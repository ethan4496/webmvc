namespace WebMVC.Ultilities.Enums
{
    public enum EOutOfStockStatus
    {
        Cancel = 0,
        New = 1,
        Done = 2
    }

    public class EOutOfStockStatusName
    {
        public static string GetStatusName(int status)
        {
            switch (status)
            {
                case (int)EOutOfStockStatus.Cancel:
                    return "Đã hủy";
                case (int)EOutOfStockStatus.New:
                    return "Chưa xuất kho";
                case (int)EOutOfStockStatus.Done:
                    return "Đã xuất kho";
                default:
                    return "";
            }
        }

        public static string GetStatusNameWithColor(int status)
        {
            return status switch
            {
                (int)EOutOfStockStatus.Cancel => "<span class=\"badge bg-dark text-white p-2\">Đã hủy</span>",
                (int)EOutOfStockStatus.New => "<span class=\"badge bg-danger text-white p-2\">Chưa xuất kho</span>",
                (int)EOutOfStockStatus.Done => "<span class=\"badge bg-primary text-white p-2\">Đã xuất kho</span>",
                _ => ""
            };
        }
    }


    public enum EPaymentOutOfStockStatus
    {
        New = 1,
        Paied = 2
    }

    public class EPaymentOutOfStockStatusName
    {
        public static string GetStatusName(int status)
        {
            switch (status)
            {
                case (int)EPaymentOutOfStockStatus.New:
                    return "Chưa thanh toán";
                case (int)EPaymentOutOfStockStatus.Paied:
                    return "Đã thanh toán";
                default:
                    return "";
            }
        }
        public static string GetStatusNameWithColor(int status)
        {
            return status switch
            {
                (int)EPaymentOutOfStockStatus.New => "<span class=\"badge bg-danger text-white p-2\">Chưa thanh toán</span>",
                (int)EPaymentOutOfStockStatus.Paied => "<span class=\"badge bg-primary text-white p-2\">Đã thanh toán</span>",
                _ => ""
            };
        }
    }
}
