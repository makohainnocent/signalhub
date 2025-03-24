// Domain/Notifications/Models/Notification.cs
namespace Domain.Notifications.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Status { get; set; } = "Pending"; // Default status
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
    }
}