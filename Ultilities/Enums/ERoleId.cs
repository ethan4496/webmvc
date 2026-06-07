namespace WebMVC.Ultilities.Enums
{
    public enum ERoleId
    {
        Admin = 1,
        User = 2,
        Manager = 3,
        Sale = 4,
        TQWarehouseStaff = 5,
        VNWarehouseStaff = 6,
    }

    public class ERoleIdName
    {
        public static string GetRoleName(int role)
        {
            switch (role)
            {
                case (int)ERoleId.Admin:
                    return "Admin";
                case (int)ERoleId.User:
                    return "Khách hàng";
                case (int)ERoleId.Manager:
                    return "Quản lý";
                case (int)ERoleId.Sale:
                    return "Sale";
                case (int)ERoleId.TQWarehouseStaff:
                    return "Kho TQ";
                case (int)ERoleId.VNWarehouseStaff:
                    return "Kho VN";
                default:
                    return "";
            }
        }
    }
}
