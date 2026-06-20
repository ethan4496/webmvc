namespace WebMVC.Models.Responses
{
    public class ChatConversationResponse
    {
        public int Id { get; set; }
        public string LastMessage { get; set; }
        public string LastMessageSecondLanguage { get; set; }
        public bool IsRead { get; set; }
        public string ReceiverUsername { get; set; }
        public string SenderUsername { get; set; }
        public int User1Id { get; set; }
        public int User2Id { get; set; }
    }
}
