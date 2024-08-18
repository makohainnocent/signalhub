using Domain.Core.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Abstractions;
using Application.Authentication.Abstractions;
using Domain.Authentication.Requests;
using System.Security.Cryptography;
using System.Text;
using DataAccess.Common.Utilities;

namespace DataAccess.Authentication.Repositories
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public AuthenticationRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<User> CreateUser(UserRegistrationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the username or email already exists
                var checkQuery = @"
            SELECT COUNT(*)
            FROM [User]
            WHERE Username = @Username OR Email = @Email";

                var existingCount = await connection.QuerySingleAsync<int>(checkQuery, new
                {
                    Username = request.Username,
                    Email = request.Email
                });

                if (existingCount > 0)
                {
                    throw new InvalidOperationException("Username or email already exists.");
                }

                // Hash the password
                string hashedPassword = PasswordHasher.HashPassword(request.Password);

                // Prepare the SQL query to insert a new user
                var insertQuery = @"
            INSERT INTO [User] (Username, HashedPassword, Email, FullName, Address, CreatedAt, LastLoginAt)
            VALUES (@Username, @HashedPassword, @Email, @FullName, @Address, @CreatedAt, @LastLoginAt);
            SELECT CAST(SCOPE_IDENTITY() as int);";

                // Prepare the parameters
                var parameters = new
                {
                    Username = request.Username,
                    HashedPassword = hashedPassword,
                    Email = request.Email,
                    FullName = request.FullName,
                    Address = request.Address,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                // Execute the query and get the new user's ID
                var userId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                // Return the created user
                return new User
                {
                    UserId = userId,
                    Username = request.Username,
                    HashedPassword = hashedPassword,
                    Email = request.Email,
                    FullName = request.FullName,
                    Address = request.Address,
                    CreatedAt = parameters.CreatedAt,
                    LastLoginAt = parameters.LastLoginAt
                };
            }
        }



    }
}
