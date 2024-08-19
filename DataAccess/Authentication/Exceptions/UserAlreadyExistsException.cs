namespace DataAccess.Authentication.Exceptions
{
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException() : base("Username or email already exists.")
        {
        }

        public UserAlreadyExistsException(string message) : base(message)
        {
        }

        public UserAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
