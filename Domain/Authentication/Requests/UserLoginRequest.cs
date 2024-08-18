namespace Domain.Authentication.Requests
{
    public class UserLoginRequest
    {
        public string UsernameOrEmail { get; set; }
        public string Password { get; set; }

    }
}
