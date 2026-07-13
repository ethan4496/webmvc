namespace WebMVC.Models.Requests.Updates
{
    public class UpdateContactRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int Id { get; set; }
    }
}
