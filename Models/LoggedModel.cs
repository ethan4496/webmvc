namespace WebMVC.Models
{
    public class LoggedModel
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string PostOffice { get; set; }
        public int TransportationType{ get; set; }
        public int IsStaff { get; set; }
        public string SpecialShipId { get; set; }
    }
}
