namespace Domain.UserDevices.Requests
{
    public class UserDeviceCreationRequest
    {
        public int UserId { get; set; }
        public string DeviceToken { get; set; }
        public string Platform { get; set; }
    }
}