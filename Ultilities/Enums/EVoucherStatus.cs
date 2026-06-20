namespace WebMVC.Ultilities.Enums
{
    public enum EVoucherStatus
    {
        Active = 1,
        InActive = 0,
    }

    public class EVoucherStatusName
    {
        public static string GetStatusName(int status)
        {
            switch (status)
            {
                case (int)EVoucherStatus.Active:
                    return "Hiện";
                case (int)EVoucherStatus.InActive:
                    return "Ẩn";
                default:
                    return "";
            };
        }
    }

    public enum EVoucherAccountStatus
    {
        Expired = 0,
        New = 1,
        Used = 2,
    }

    public class EVoucherAccountStatusName
    {
        public static string GetStatusName(int status)
        {
            switch (status)
            {
                case (int)EVoucherAccountStatus.Expired:
                    return "Hết hạn";
                case (int)EVoucherAccountStatus.New:
                    return "Mới";
                case (int)EVoucherAccountStatus.Used:
                    return "Đã dùng";
                default:
                    return "";
            };
        }
    }
}
