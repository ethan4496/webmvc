namespace WebMVC.Models.Responses
{
    public class StatisticTable3Response
    {
        public int Id { get; set; }
        public bool IsOrder { get; set; }
        public string Username { get; set; }
        public string Phone { get; set; }
        public string Created { get; set; }
    }

    public class StatisticTable3TitleResponse
    {
        public int Title1 { get; set; }
        public int Title2 { get; set; }
        public double Title3
        {
            get
            {
                return Math.Round(((double)Title2 / Title1) * 100, 2);
            }
        }
    }
}
