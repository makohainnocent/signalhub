// Domain/UserDevices/Models/UserDevice.cs
namespace Domain.UserDevices.Models
{
    public class UserDevice
    {
        public int DeviceId { get; set; }
        public int UserId { get; set; }
        public string DeviceToken { get; set; }
        public string Platform { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}