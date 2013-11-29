namespace Chat.Repositories
{
    public class MissedConversationModel
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}