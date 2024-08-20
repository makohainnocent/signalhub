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
using System.Security.Authentication;
using DataAccess.Authentication.Exceptions;
using Domain.Common;


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
                    throw new UserAlreadyExistsException("Username or email already exists.");
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

        public async Task<User> LoginUser(UserLoginRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
            SELECT UserId, Username, HashedPassword, Email, FullName, Address, CreatedAt, LastLoginAt
            FROM [User]
            WHERE Username = @UsernameOrEmail OR Email = @UsernameOrEmail";

                
                var user = await connection.QuerySingleOrDefaultAsync<User>(query, new { UsernameOrEmail = request.UsernameOrEmail });

                
                if (user == null)
                {
                    throw new InvalidCredentialsException("Invalid username or email.");
                }

                
                bool isPasswordValid = PasswordHasher.VerifyHashedPassword(user.HashedPassword, request.Password);

                
                if (!isPasswordValid)
                {
                    throw new InvalidCredentialsException("Invalid password.");
                }

                
                var updateQuery = @"
            UPDATE [User]
            SET LastLoginAt = @LastLoginAt
            WHERE UserId = @UserId";

                await connection.ExecuteAsync(updateQuery, new { LastLoginAt = DateTime.UtcNow, UserId = user.UserId });

                
                return user;
            }
        }

        public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                const string query = @"
                    SELECT r.RoleId, r.RoleName
                    FROM Roles r
                    INNER JOIN UserRoles ur ON r.RoleId = ur.RoleId
                    WHERE ur.UserId = @UserId";

                return await connection.QueryAsync<Role>(query, new { UserId = userId });
            }
            
        }


        public async Task<IEnumerable<Claim>> GetClaimsByUserIdAsync(int userId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                const string query = @"
                    SELECT DISTINCT c.ClaimId, c.ClaimType, c.ClaimValue
                    FROM Claims c
                    INNER JOIN UserClaims uc ON c.ClaimId = uc.ClaimId
                    WHERE uc.UserId = @UserId

                    UNION

                    SELECT DISTINCT c.ClaimId, c.ClaimType, c.ClaimValue
                    FROM Claims c
                    INNER JOIN RoleClaims rc ON c.ClaimId = rc.ClaimId
                    INNER JOIN UserRoles ur ON rc.RoleId = ur.RoleId
                    WHERE ur.UserId = @UserId";

                return await connection.QueryAsync<Claim>(query, new { UserId = userId });
            }
                
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                const string query = @"
                    SELECT UserId, Username, HashedPassword, Email, FullName, Address, CreatedAt, LastLoginAt
                    FROM [User]
                    WHERE UserId = @UserId";

                    return await connection.QuerySingleOrDefaultAsync<User>(query, new { UserId = userId });
            }
                
        }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                const string query = "SELECT * FROM RefreshTokens WHERE Token = @Token AND RevokedAt IS NULL";
                return await connection.QuerySingleOrDefaultAsync<RefreshToken>(query, new { Token = token });
            }

                
        }

        public async Task AddAsync(RefreshToken refreshToken)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                const string query = @"
                INSERT INTO RefreshTokens (UserId, Token, ExpiresAt, CreatedAt)
                VALUES (@UserId, @Token, @ExpiresAt, @CreatedAt)";

                await connection.ExecuteAsync(query, refreshToken);
            }
               
        }

        public async Task RevokeAsync(string token)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                const string query = "UPDATE RefreshTokens SET RevokedAt = @RevokedAt WHERE Token = @Token";

                await connection.ExecuteAsync(query, new { Token = token, RevokedAt = DateTime.UtcNow });
            }
                
        }

        public async Task StoreVerificationCode(string email, string code)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                const string query = @"
                INSERT INTO VerificationCodes (Email, Code, ExpiryDate)
                VALUES (@Email, @Code, @ExpiryDate)";

                var parameters = new
                {
                    Email = email,
                    Code = code,
                    ExpiryDate = DateTime.UtcNow.AddMinutes(10)
                };

                await connection.ExecuteAsync(query, parameters);
            }
        }


        public async Task<bool> ValidateVerificationCode(string email, string code)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                const string query = @"
                SELECT COUNT(*)
                FROM VerificationCodes
                WHERE Email = @Email AND Code = @Code AND ExpiryDate > @CurrentDate";

                var parameters = new
                {
                    Email = email,
                    Code = code,
                    CurrentDate = DateTime.UtcNow
                };

                var count = await connection.QuerySingleAsync<int>(query, parameters);
                return count > 0;
            }
        }

        public async Task UpdatePassword(string email, string newPassword)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                const string query = @"
                UPDATE [User]
                SET HashedPassword = @PasswordHash
                WHERE Email = @Email";

                var parameters = new
                {
                    Email = email,
                    PasswordHash = PasswordHasher.HashPassword(newPassword) 
                };

                await connection.ExecuteAsync(query, parameters);
            }
        }

        public async Task<bool> ValidatePassword(string email, string password)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                const string query = @"
                SELECT HashedPassword
                FROM [User]
                WHERE Email = @Email";

                var storedPasswordHash = await connection.QuerySingleOrDefaultAsync<string>(query, new { Email = email });

                if (storedPasswordHash == null)
                {
                    return false;
                }

                return PasswordHasher.VerifyHashedPassword(storedPasswordHash, password);
            }
        }

        public async Task UpdateUser(int userId, UpdateUserRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                
                var checkQuery = @"
                SELECT COUNT(*)
                FROM [User]
                WHERE (Username = @Username OR Email = @Email) AND UserId <> @UserId";

                var existingCount = await connection.QuerySingleAsync<int>(checkQuery, new
                {
                    Username = request.Username,
                    Email = request.Email,
                    UserId = userId
                });

                if (existingCount > 0)
                {
                    throw new UserAlreadyExistsException("Username or email already exists.");
                }

                // Build the update query dynamically
                var updateFields = new List<string>();
                var parameters = new DynamicParameters();
                parameters.Add("UserId", userId);

                if (!string.IsNullOrEmpty(request.Username))
                {
                    updateFields.Add("Username = @Username");
                    parameters.Add("Username", request.Username);
                }

                if (!string.IsNullOrEmpty(request.Email))
                {
                    updateFields.Add("Email = @Email");
                    parameters.Add("Email", request.Email);
                }

                if (!string.IsNullOrEmpty(request.FullName))
                {
                    updateFields.Add("FullName = @FullName");
                    parameters.Add("FullName", request.FullName);
                }

                if (!string.IsNullOrEmpty(request.Address))
                {
                    updateFields.Add("Address = @Address");
                    parameters.Add("Address", request.Address);
                }

                if (updateFields.Count == 0)
                {
                    return;
                }

                var updateQuery = $@"
                UPDATE [User]
                SET {string.Join(", ", updateFields)}
                WHERE UserId = @UserId";

                await connection.ExecuteAsync(updateQuery, parameters);
            }
        }


        public async Task<User> GetUserByUsernameAsync(string username)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                const string query = @"
            SELECT UserId, Username, HashedPassword, Email, FullName, Address, CreatedAt, LastLoginAt
            FROM [User]
            WHERE Username = @Username";

                return await connection.QuerySingleOrDefaultAsync<User>(query, new { Username = username });
            }
        }

        public async Task<PagedResultResponse<User>> GetUsersAsync(int pageNumber, int pageSize, string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Calculate the number of records to skip
                var skip = (pageNumber - 1) * pageSize;

                // Define the base query with pagination
                var query = new StringBuilder(@"
                SELECT *
                FROM [User]
                WHERE 1=1");

                // Add search condition if search term is provided
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                    AND (Username LIKE @Search
                    OR Email LIKE @Search
                    OR FullName LIKE @Search
                    OR Address LIKE @Search)");
                }

                // Add pagination and ordering
                query.Append(@"
                ORDER BY Username
                OFFSET @Skip ROWS
                FETCH NEXT @PageSize ROWS ONLY;
    
                SELECT COUNT(*)
                FROM [User]
                WHERE 1=1");

                // Add search condition to count query if search term is provided
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                    AND (Username LIKE @Search
                    OR Email LIKE @Search
                    OR FullName LIKE @Search
                    OR Address LIKE @Search)");
                }

                
                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%"
                }))
                {
                    var users = multi.Read<User>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<User>
                    {
                        Items = users,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }


        public async Task DeleteUserAsync(int userId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                
                const string query = @"
                DELETE FROM [User]
                WHERE UserId = @UserId";

                var affectedRows = await connection.ExecuteAsync(query, new { UserId = userId });

                if (affectedRows == 0)
                {
                    throw new UserNotFoundException("User not found.");
                }
            }
        }

        public async Task<int> AddRoleAsync(Role newRole)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if a role with the same name already exists
                const string checkQuery = "SELECT COUNT(1) FROM [Roles] WHERE RoleName = @RoleName";
                var exists = await connection.ExecuteScalarAsync<bool>(checkQuery, new { newRole.RoleName });

                if (exists)
                {
                    throw new RoleAlreadyExistsException(newRole.RoleName);
                }

                // If not, insert the new role
                const string insertQuery = @"
                INSERT INTO [Roles] (RoleName)
                VALUES (@RoleName);
                SELECT CAST(SCOPE_IDENTITY() as int)";

                var roleId = await connection.QuerySingleAsync<int>(insertQuery, new { newRole.RoleName });

                return roleId;
            }
        }


        public async Task<Role?> UpdateRoleAsync(UpdateRoleRequest roleRequest)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if a role with the new name already exists (excluding the current role being updated)
                const string checkQuery = @"
                SELECT COUNT(1) 
                FROM [Roles] 
                WHERE RoleName = @RoleName AND RoleId != @RoleId";

                var exists = await connection.ExecuteScalarAsync<bool>(checkQuery, new { RoleName = roleRequest.RoleName, RoleId = roleRequest.RoleId });

                if (exists)
                {
                    throw new RoleAlreadyExistsException(roleRequest.RoleName);
                }

                
                const string updateQuery = @"
                UPDATE [Roles]
                SET RoleName = @RoleName
                WHERE RoleId = @RoleId;

                SELECT RoleId, RoleName
                FROM [Roles]
                WHERE RoleId = @RoleId";

                var updatedRole = await connection.QuerySingleOrDefaultAsync<Role>(updateQuery, new { RoleName = roleRequest.RoleName, RoleId = roleRequest.RoleId });

                return updatedRole;
            }
        }

        public async Task<PagedResultResponse<Role>> GetRolesAsync(int page, int pageSize, string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                
                var query = new StringBuilder("SELECT RoleId, RoleName FROM [Roles]");
                var countQuery = new StringBuilder("SELECT COUNT(*) FROM [Roles]");

                
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(" WHERE RoleName LIKE @Search");
                    countQuery.Append(" WHERE RoleName LIKE @Search");
                }

                
                query.Append(" ORDER BY RoleName OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");

                
                var roles = await connection.QueryAsync<Role>(query.ToString(), new
                {
                    Search = $"%{search}%",
                    Offset = (page - 1) * pageSize,
                    PageSize = pageSize
                });

                
                var totalCount = await connection.ExecuteScalarAsync<int>(countQuery.ToString(), new
                {
                    Search = $"%{search}%"
                });

                
                return new PagedResultResponse<Role>
                {
                    Items = roles,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                };
            }
        }






    }
}
