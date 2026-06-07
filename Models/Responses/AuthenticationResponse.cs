namespace WebMVC.Models.Responses
{
    public class AuthenticationResponse
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string Token { get; set; }
        public string PostOffice { get; set; }
        public int? TransportationType { get; set; }
    }
}
