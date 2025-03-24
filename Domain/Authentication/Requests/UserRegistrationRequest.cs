namespace Domain.Authentication.Requests
{
    public class UserRegistrationRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string? CoverPhoto { get; set; } // URL or path to the cover photo, can be null
        public string? ProfilePhoto { get; set; }
    }
}
