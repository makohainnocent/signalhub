using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Authentication.Exceptions
{
    public class ItemDoesNotExistException : Exception
    {
        public ItemDoesNotExistException(int id)
            : base($"The item with ID {id} does not exist.")
        {
        }

        public ItemDoesNotExistException(string msg)
            : base(msg)
        {
        }
    }

}
