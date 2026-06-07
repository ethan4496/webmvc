namespace WebMVC.Ultilities.Enums
{
    public class DeliveryMethodName
    {
        public static List<string> GetDeliveryMethod()
        {
            return new List<string> { "Lấy tại Kho", "Gửi Ahamove", "Gửi Vietel", "Gửi J&T", "Khác" };
        }
    }
}
