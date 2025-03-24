namespace Domain.Notifications.Requests
{
    public class NotificationUpdateRequest
    {
        public int NotificationId { get; set; }
        public string Status { get; set; }
        public DateTime? SentAt { get; set; }
    }
}