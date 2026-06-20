namespace WebMVC.Ultilities.Enums
{
    public enum ETransportationStatus
    {
        Cancel = 0,
        New = 1,
        ArrivedAtTQWarehouse = 2,
        ExitedFromTQWarehouse = 3,
        CustomsInspectedGoods = 4,
        ReturningToVNWarehouse = 5,
        ArrivedAtVNWarehouse = 6,
        Completed = 7,
    }

    public class ETransportationStatusName
    {
        public static string GetStatusName(int status)
        {
            switch (status)
            {
                //case (int)ETransportationStatus.Cancel:
                //    return "Đã hủy";
                case (int)ETransportationStatus.New:
                    return "Đơn hàng mới";
                case (int)ETransportationStatus.ArrivedAtTQWarehouse:
                    return "Nhập kho TQ";
                case (int)ETransportationStatus.ExitedFromTQWarehouse:
                    return "Đã phát hàng";
                case (int)ETransportationStatus.CustomsInspectedGoods:
                    return "Kiểm hóa";
                case (int)ETransportationStatus.ReturningToVNWarehouse:
                    return "Đang nhập khẩu";
                case (int)ETransportationStatus.ArrivedAtVNWarehouse:
                    return "Nhập kho HN";
                case (int)ETransportationStatus.Completed:
                    return "Đã xuất kho";
                default:
                    return "";
            };
        }

        public static string GetStatusNameWithColor(int status)
        {
            return status switch
            {
                (int)ETransportationStatus.Cancel => "<span class=\"badge bg-dark text-white p-2\">Đã hủy</span>",
                (int)ETransportationStatus.New => "<span class=\"badge bg-danger text-white p-2\">Đơn hàng mới</span>",
                (int)ETransportationStatus.ArrivedAtTQWarehouse => "<span class=\"badge bg-warning text-white p-2\">Nhập kho TQ</span>",
                (int)ETransportationStatus.ExitedFromTQWarehouse => "<span class=\"badge bg-primary text-white p-2\">Đã phát hàng</span>",
                (int)ETransportationStatus.CustomsInspectedGoods => "<span class=\"badge bg-secondary text-white p-2\">Hải quan kiểm hoá</span>",
                (int)ETransportationStatus.ReturningToVNWarehouse => "<span class=\"badge bg-info text-white p-2\">Đang nhập khẩu</span>",
                (int)ETransportationStatus.ArrivedAtVNWarehouse => "<span class=\"badge bg-pink text-white p-2\">Nhập kho HN</span>",
                (int)ETransportationStatus.Completed => "<span class=\"badge bg-success text-white p-2\">Đã xuất kho</span>",
                _ => ""
            };
        }

        public static int GetStatusByBigPackageStatus(int bigPackageStatus)
        {
            switch (bigPackageStatus)
            {
                case (int)EBigPackageStatus.ArrivedAtTQWarehouse:
                    return (int)ETransportationStatus.ArrivedAtTQWarehouse;
                case (int)EBigPackageStatus.ExitedFromTQWarehouse:
                    return (int)ETransportationStatus.ExitedFromTQWarehouse;
                case (int)EBigPackageStatus.CustomsInspectedGoods:
                    return (int)ETransportationStatus.CustomsInspectedGoods;
                case (int)EBigPackageStatus.ReturningToVNWarehouse:
                    return (int)ETransportationStatus.ReturningToVNWarehouse;
                case (int)EBigPackageStatus.ArrivedAtVNWarehouse:
                    return (int)ETransportationStatus.ArrivedAtVNWarehouse;
                case (int)EBigPackageStatus.Completed:
                    return (int)ETransportationStatus.Completed;
                default:
                    return 0;

            }
        }
    }
}
