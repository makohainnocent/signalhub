using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Authentication.Exceptions
{
    public class ItemAlreadyExistsException : Exception
    {
        
        public ItemAlreadyExistsException()
        {
        }

        
        public ItemAlreadyExistsException(string message)
            : base(message)
        {
        }

        
        public ItemAlreadyExistsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
