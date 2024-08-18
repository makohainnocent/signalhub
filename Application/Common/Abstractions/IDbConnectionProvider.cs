using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Abstractions
{

    public interface IDbConnectionProvider
    {
        IDbConnection CreateConnection();
    }

}
