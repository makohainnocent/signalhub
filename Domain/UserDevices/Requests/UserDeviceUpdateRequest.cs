namespace Domain.UserDevices.Requests
{
    public class UserDeviceUpdateRequest
    {
        public int DeviceId { get; set; }
        public string DeviceToken { get; set; }
        public string Platform { get; set; }
    }
}