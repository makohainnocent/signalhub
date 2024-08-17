using Application.Abstractions;
using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Data.SqlClient;

namespace DataAccess.DbConnection
{
    public class DbConnection : IDbConnectionProvider
    {
        private readonly IConfiguration _configuration;

        public DbConnection(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection CreateConnection()
        {
            string connectionString = _configuration.GetConnectionString("sqlServerConnectionString");
            return new SqlConnection(connectionString);
        }
    }
}
