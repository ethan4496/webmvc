namespace WebMVC.Ultilities
{
    public static class FormatDate
    {
        public static string FormatNullDate(DateTime? date)
        {
            return date.HasValue ? date.Value.ToString("dd/MM/yyyy HH:mm") : "";
        }
    }
}
