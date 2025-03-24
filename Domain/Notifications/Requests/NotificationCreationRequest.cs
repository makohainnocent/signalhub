namespace Domain.Notifications.Requests
{
    public class NotificationCreationRequest
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}