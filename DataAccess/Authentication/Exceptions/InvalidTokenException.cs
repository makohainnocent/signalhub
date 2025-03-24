using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Authentication.Exceptions
{
    public class InvalidTokenException : Exception
    {
        public InvalidTokenException(string message) : base(message) { }
    }
}
