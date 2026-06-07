namespace WebMVC.Ultilities.Enums
{
    public enum EBigPackageStatus
    {
        Cancel = 0,
        ArrivedAtTQWarehouse = 2,
        ExitedFromTQWarehouse = 3,
        CustomsInspectedGoods = 4,
        ReturningToVNWarehouse = 5,
        ArrivedAtVNWarehouse = 6,
        Completed = 7,
    }
    public class EBigPackageStatusName
    {
        public static string GetStatusName(int status)
        {
            switch (status)
            {
                case (int)EBigPackageStatus.Cancel:
                    return "Đã hủy";
                case (int)EBigPackageStatus.ArrivedAtTQWarehouse:
                    return "Nhập kho TQ";
                case (int)EBigPackageStatus.ExitedFromTQWarehouse:
                    return "Đã phát hàng";
                case (int)EBigPackageStatus.CustomsInspectedGoods:
                    return "Kiểm hóa";
                case (int)EBigPackageStatus.ReturningToVNWarehouse:
                    return "Đang Nhập Khẩu";
                case (int)EBigPackageStatus.ArrivedAtVNWarehouse:
                    return "Nhập kho HN";
                case (int)EBigPackageStatus.Completed:
                    return "Hoàn thành";
                default:
                    return "";
            };
        }
    }

}
