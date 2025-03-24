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
using System.Transactions;
using Domain.Common.Responses;
using Domain.Authentication.Responses;
using DataAccess.Common.Exceptions;


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
                FROM [Users]
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
                INSERT INTO [Users] (
                    Username, HashedPassword, Salt, Email, FullName, Address, PhoneNumber, Role, 
                    FailedLoginAttempts, IsLocked, PasswordResetToken, PasswordResetTokenExpiry, 
                    CreatedAt, UpdatedAt, LastLoginAt, IsDeleted, CreatedBy, CoverPhoto, ProfilePhoto
                )
                VALUES (
                    @Username, @HashedPassword, @Salt, @Email, @FullName, @Address, @PhoneNumber, @Role, 
                    @FailedLoginAttempts, @IsLocked, @PasswordResetToken, @PasswordResetTokenExpiry, 
                    @CreatedAt, @UpdatedAt, @LastLoginAt, @IsDeleted, @CreatedBy, @CoverPhoto, @ProfilePhoto
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

                // Prepare the parameters
                var parameters = new
                {
                    Username = request.Username,
                    HashedPassword = hashedPassword,
                    Salt = Guid.NewGuid().ToString("N"), // Generate a new salt
                    Email = request.Email,
                    FullName = request.FullName,
                    Address = request.Address,
                    PhoneNumber = request.PhoneNumber,
                    Role = "User", // Default role
                    FailedLoginAttempts = 0,
                    IsLocked = false,
                    PasswordResetToken = (string)null,
                    PasswordResetTokenExpiry = (DateTime?)null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = (DateTime?)null,
                    LastLoginAt = DateTime.UtcNow,
                    IsDeleted = false,
                    CreatedBy = (int?)null, // Nullable for self-registered users
                    CoverPhoto = request.CoverPhoto,
                    ProfilePhoto = request.ProfilePhoto
                };

                // Execute the query and get the new user's ID
                var userId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                // Return the created user
                return new User
                {
                    UserId = userId,
                    Username = request.Username,
                    HashedPassword = hashedPassword,
                    Salt = parameters.Salt,
                    Email = request.Email,
                    FullName = request.FullName,
                    Address = request.Address,
                    PhoneNumber = request.PhoneNumber,
                    Role = parameters.Role,
                    FailedLoginAttempts = parameters.FailedLoginAttempts,
                    IsLocked = parameters.IsLocked,
                    PasswordResetToken = parameters.PasswordResetToken,
                    PasswordResetTokenExpiry = parameters.PasswordResetTokenExpiry,
                    CreatedAt = parameters.CreatedAt,
                    UpdatedAt = parameters.UpdatedAt,
                    LastLoginAt = parameters.LastLoginAt,
                    IsDeleted = parameters.IsDeleted,
                    CreatedBy = parameters.CreatedBy,
                    CoverPhoto = request.CoverPhoto,
                    ProfilePhoto = request.ProfilePhoto
                };
            }
        }

        public async Task<User> LoginUser(UserLoginRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                SELECT UserId, Username, HashedPassword, Salt, Email, FullName, Address, PhoneNumber, Role, 
                       FailedLoginAttempts, IsLocked, PasswordResetToken, PasswordResetTokenExpiry, 
                       CreatedAt, UpdatedAt, LastLoginAt, IsDeleted, CreatedBy, CoverPhoto, ProfilePhoto
                FROM [Users]
                WHERE Username = @UsernameOrEmail OR Email = @UsernameOrEmail";

                var user = await connection.QuerySingleOrDefaultAsync<User>(query, new { UsernameOrEmail = request.UsernameOrEmail });

                if (user == null)
                {
                    throw new InvalidCredentialsException("Invalid username or email.");
                }

                // Verify the password
                bool isPasswordValid = PasswordHasher.VerifyHashedPassword(user.HashedPassword, request.Password);

                if (!isPasswordValid)
                {
                    // Increment failed login attempts
                    var updateFailedAttemptsQuery = @"
                    UPDATE [Users]
                    SET FailedLoginAttempts = FailedLoginAttempts + 1
                    WHERE UserId = @UserId";

                    await connection.ExecuteAsync(updateFailedAttemptsQuery, new { UserId = user.UserId });

                    throw new InvalidCredentialsException("Invalid password.");
                }

                // Reset failed login attempts on successful login
                var resetFailedAttemptsQuery = @"
                UPDATE [Users]
                SET FailedLoginAttempts = 0, LastLoginAt = @LastLoginAt
                WHERE UserId = @UserId";

                await connection.ExecuteAsync(resetFailedAttemptsQuery, new { LastLoginAt = DateTime.UtcNow, UserId = user.UserId });

                return user;
            }
        }

        public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                const string query = @"
                    SELECT r.RoleId, r.RoleName, ur.Status
                    FROM Roles r
                    INNER JOIN UserRoles ur ON r.RoleId = ur.RoleId
                    WHERE ur.UserId = @UserId ORDER BY CreatedAt ASC" ;

                return await connection.QueryAsync<Role>(query, new { UserId = userId });
            }
            
        }


        public async Task<int> CountRolesWithPendingStatusAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Query to count roles with status 'Pending'
                var query = "SELECT COUNT(*) FROM [UserRoles] WHERE Status = @Status";

                // Execute the query
                var pendingCount = await connection.ExecuteScalarAsync<int>(query, new
                {
                    Status = "Pending"
                });

                return pendingCount;
            }
        }


        public async Task<int> CountUsersAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Query to count all users
                var query = "SELECT COUNT(*) FROM [Users]";

                // Execute the query and retrieve the count
                var userCount = await connection.ExecuteScalarAsync<int>(query);

                return userCount;
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
                    SELECT UserId, Username, HashedPassword,PhoneNumber, Email, FullName, Address,CoverPhoto,ProfilePhoto, CreatedAt, LastLoginAt
                    FROM [Users]
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
                UPDATE [Users]
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
                FROM [Users]
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
                FROM [Users]
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

                if (!string.IsNullOrEmpty(request.PhoneNumber))
                {
                    updateFields.Add("PhoneNumber = @PhoneNumber");
                    parameters.Add("PhoneNumber", request.PhoneNumber);
                }

                if (!string.IsNullOrEmpty(request.CoverPhoto))
                {
                    updateFields.Add("CoverPhoto = @CoverPhoto");
                    parameters.Add("CoverPhoto", request.CoverPhoto);
                }

                if (!string.IsNullOrEmpty(request.ProfilePhoto))
                {
                    updateFields.Add("ProfilePhoto = @ProfilePhoto");
                    parameters.Add("ProfilePhoto", request.ProfilePhoto);
                }

                if (updateFields.Count == 0)
                {
                    return;
                }

                var updateQuery = $@"
                UPDATE [Users]
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
            SELECT UserId, Username, HashedPassword,PhoneNumber, Email, FullName, Address,CoverPhoto,ProfilePhoto, CreatedAt, LastLoginAt
            FROM [Users]
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
                FROM [Users]
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
                FROM [Users]
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
                DELETE FROM [Users]
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


        public async Task<int> AddClaimAsync(AddClaimRequest claim)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        
                        const string checkQuery = "SELECT COUNT(*) FROM [Claims] WHERE ClaimType = @ClaimType";
                        var existingCount = await connection.ExecuteScalarAsync<int>(checkQuery, new { ClaimType = claim.ClaimType }, transaction);

                        if (existingCount > 0)
                        {
                            
                            throw new ItemAlreadyExistsException($"A claim with type '{claim.ClaimType}' already exists.");
                        }

                        
                        const string insertQuery = @"
                        INSERT INTO [Claims] (ClaimType, ClaimValue)
                        VALUES (@ClaimType, @ClaimValue);
                        SELECT SCOPE_IDENTITY();";

                        var newClaimId = await connection.ExecuteScalarAsync<int>(insertQuery, new
                        {
                            ClaimType = claim.ClaimType,
                            ClaimValue = claim.ClaimValue
                        }, transaction);

                        transaction.Commit();
                        return newClaimId;
                    }
                    catch (ItemAlreadyExistsException)
                    {
                        
                        throw;
                    }
                    catch (Exception ex)
                    {
                        
                        transaction.Rollback();
                        throw; 
                    }
                }
            }
        }

        public async Task<Claim?> UpdateClaimAsync(UpdateClaimRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                
                const string checkQuery = "SELECT COUNT(*) FROM [Claims] WHERE ClaimId = @ClaimId";
                var existingCount = await connection.ExecuteScalarAsync<int>(checkQuery, new { ClaimId = request.ClaimId });

                if (existingCount == 0)
                {
                    return null;
                }

                // Build the dynamic update query based on provided fields
                var updateQuery = new StringBuilder("UPDATE [Claims] SET ");
                var parameters = new DynamicParameters();

                // Conditionally add parameters
                if (!string.IsNullOrWhiteSpace(request.ClaimType))
                {
                    updateQuery.Append("ClaimType = @ClaimType, ");
                    parameters.Add("ClaimType", request.ClaimType);
                }

                if (!string.IsNullOrWhiteSpace(request.ClaimValue))
                {
                    updateQuery.Append("ClaimValue = @ClaimValue, ");
                    parameters.Add("ClaimValue", request.ClaimValue);
                }

                // Remove the last comma and space
                if (updateQuery.Length > 0)
                {
                    updateQuery.Length -= 2; // Remove trailing ", "
                }
                else
                {
                    
                    return null;
                }

                updateQuery.Append(" WHERE ClaimId = @ClaimId");
                parameters.Add("ClaimId", request.ClaimId);

                
                var affectedRows = await connection.ExecuteAsync(updateQuery.ToString(), parameters);

                if (affectedRows == 0)
                {
                    return null;
                }

                
                const string selectQuery = "SELECT ClaimId, ClaimType, ClaimValue FROM [Claims] WHERE ClaimId = @ClaimId";
                var updatedClaim = await connection.QuerySingleOrDefaultAsync<Claim>(selectQuery, new { ClaimId = request.ClaimId });

                return updatedClaim;
            }
        }

        public async Task<PagedResultResponse<Claim>> GetClaimsAsync(int pageNumber, int pageSize, string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Calculate the number of records to skip
                var skip = (pageNumber - 1) * pageSize;

                // Define the query with pagination and search
                var queryBuilder = new StringBuilder(@"
                SELECT ClaimId, ClaimType, ClaimValue
                FROM [Claims]
                ");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    queryBuilder.Append("WHERE ClaimType LIKE @Search OR ClaimValue LIKE @Search ");
                }

                queryBuilder.Append(@"
                ORDER BY ClaimType
                OFFSET @Skip ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*)
                FROM [Claims]
                ");

                var query = queryBuilder.ToString();

                
                using (var multi = await connection.QueryMultipleAsync(query, new
                {
                    Search = $"%{search}%",
                    Skip = skip,
                    PageSize = pageSize
                }))
                {
                    var claims = multi.Read<Claim>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<Claim>
                    {
                        Items = claims,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }


        public async Task<Claim?> GetClaimByIdAsync(int claimId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                const string query = "SELECT ClaimId, ClaimType, ClaimValue FROM [Claims] WHERE ClaimId = @ClaimId";

                var claim = await connection.QuerySingleOrDefaultAsync<Claim>(query, new { ClaimId = claimId });

                return claim;
            }
        }

        public async Task DeleteClaimAsync(int claimId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        
                        const string checkQuery = "SELECT COUNT(*) FROM Claims WHERE ClaimId = @ClaimId";
                        var exists = await connection.ExecuteScalarAsync<int>(checkQuery, new { ClaimId = claimId }, transaction);

                        if (exists == 0)
                        {
                            throw new ItemDoesNotExistException(claimId);
                        }

                       
                        await connection.ExecuteAsync("DELETE FROM UserClaims WHERE ClaimId = @ClaimId", new { ClaimId = claimId }, transaction);
                        await connection.ExecuteAsync("DELETE FROM RoleClaims WHERE ClaimId = @ClaimId", new { ClaimId = claimId }, transaction);

                       
                        await connection.ExecuteAsync("DELETE FROM Claims WHERE ClaimId = @ClaimId", new { ClaimId = claimId }, transaction);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw; 
                    }
                }
            }
        }

        public async Task DeleteRoleAsync(int roleId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        
                        const string checkQuery = "SELECT COUNT(*) FROM Roles WHERE RoleId = @RoleId";
                        var exists = await connection.ExecuteScalarAsync<int>(checkQuery, new { RoleId = roleId }, transaction);

                        if (exists == 0)
                        {
                            throw new ItemDoesNotExistException(roleId);
                        }

                        
                        await connection.ExecuteAsync("DELETE FROM UserRoles WHERE RoleId = @RoleId", new { RoleId = roleId }, transaction);
                        await connection.ExecuteAsync("DELETE FROM RoleClaims WHERE RoleId = @RoleId", new { RoleId = roleId }, transaction);

                        
                        await connection.ExecuteAsync("DELETE FROM Roles WHERE RoleId = @RoleId", new { RoleId = roleId }, transaction);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw; 
                    }
                }
            }
        }

        public async Task<Role?> GetRoleAsync(int roleId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                const string query = "SELECT RoleId, RoleName FROM Roles WHERE RoleId = @RoleId";

                return await connection.QuerySingleOrDefaultAsync<Role>(query, new { RoleId = roleId });
            }
        }

        public async Task<bool> AddRoleToUserAsync(AddRoleToUserRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                const string checkUserQuery = "SELECT COUNT(*) FROM [Users] WHERE UserId = @UserId";
                const string checkRoleQuery = "SELECT COUNT(*) FROM [Roles] WHERE RoleId = @RoleId";
                const string checkUserRoleQuery = "SELECT COUNT(*) FROM UserRoles WHERE UserId = @UserId AND RoleId = @RoleId";

                var userExists = await connection.ExecuteScalarAsync<int>(checkUserQuery, new { UserId = request.UserId });
                var roleExists = await connection.ExecuteScalarAsync<int>(checkRoleQuery, new { RoleId = request.RoleId });
                var userRoleExists = await connection.ExecuteScalarAsync<int>(checkUserRoleQuery, new { UserId = request.UserId, RoleId = request.RoleId });

                if (userExists == 0)
                {
                    throw new ItemDoesNotExistException($"User with UserId:{request.UserId} does not exist");
                }

                if (roleExists == 0)
                {
                    throw new ItemDoesNotExistException($"Role with RoleId:{request.RoleId} does not exist");
                }

                if (userRoleExists > 0)
                {
                    throw new ItemAlreadyExistsException($"User with userId:{request.UserId} was already assigned this role with roleId:{request.RoleId}");
                }

                const string insertQuery = @"
                INSERT INTO UserRoles (UserId, RoleId)
                VALUES (@UserId, @RoleId);";

                await connection.ExecuteAsync(insertQuery, request);
                return true;
            }
        }

        public async Task<bool> UpdateUserRoleStatusAsync(int userId, int roleId, string status)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the role exists for the user
                const string checkQuery = @"
        SELECT COUNT(*) 
        FROM UserRoles 
        WHERE UserId = @UserId AND RoleId = @RoleId";

                var existingCount = await connection.ExecuteScalarAsync<int>(checkQuery, new { UserId = userId, RoleId = roleId });

                if (existingCount == 0)
                {
                    throw new ItemDoesNotExistException($"Role ID {roleId} is not assigned to User ID {userId}.");
                }

                // Update the Status column for the specified user and role
                const string updateQuery = @"
        UPDATE UserRoles 
        SET Status = @Status
        WHERE UserId = @UserId AND RoleId = @RoleId";

                var affectedRows = await connection.ExecuteAsync(updateQuery, new { UserId = userId, RoleId = roleId, Status = status });

                return affectedRows > 0;
            }
        }


        public async Task<bool> RemoveRoleFromUserAsync(RemoveRoleFromUserRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the role exists for the user
                const string checkQuery = @"
            SELECT COUNT(*)
            FROM UserRoles
            WHERE UserId = @UserId AND RoleId = @RoleId";

                var existingCount = await connection.ExecuteScalarAsync<int>(checkQuery, new { UserId = request.UserId, RoleId = request.RoleId });

                if (existingCount == 0)
                {
                    throw new ItemDoesNotExistException($"Role ID {request.RoleId} is not assigned to User ID {request.UserId}.");
                }

                // Remove the role from the user
                const string deleteQuery = @"
            DELETE FROM UserRoles
            WHERE UserId = @UserId AND RoleId = @RoleId";

                var affectedRows = await connection.ExecuteAsync(deleteQuery, new { UserId = request.UserId, RoleId = request.RoleId });

                return affectedRows > 0;
            }
        }

        public async Task<PagedResultResponse<UserRoleResponse>> GetUserRolesAsync(int pageNumber, int pageSize, string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Calculate the number of records to skip
                var skip = (pageNumber - 1) * pageSize;

                // Define the base query with pagination
                var query = new StringBuilder(@"
        WITH LatestUserRoles AS (
            SELECT 
                u.UserId, 
                u.Username, 
                u.Email, 
                r.RoleId, 
                r.RoleName, 
                ur.CreatedAt, 
                ur.Status,
                ROW_NUMBER() OVER (PARTITION BY u.UserId ORDER BY ur.CreatedAt DESC) AS RowNum
            FROM [Users] u
            JOIN UserRoles ur ON u.UserId = ur.UserId
            JOIN Roles r ON ur.RoleId = r.RoleId
            WHERE 1=1");

                // Add search condition if search term is provided
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
            AND (u.Username LIKE @Search
            OR u.Email LIKE @Search
            OR u.FullName LIKE @Search
            OR r.RoleName LIKE @Search
            OR ur.Status LIKE @Search)");
                }

                query.Append(@"
        )
        SELECT 
            UserId, 
            Username, 
            Email, 
            RoleId, 
            RoleName, 
            CreatedAt, 
            Status
        FROM LatestUserRoles
        WHERE RowNum = 1
        ORDER BY CreatedAt DESC, Username DESC
        OFFSET @Skip ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(*)
        FROM (
            SELECT 
                u.UserId, 
                ROW_NUMBER() OVER (PARTITION BY u.UserId ORDER BY ur.CreatedAt DESC) AS RowNum
            FROM [Users] u
            JOIN UserRoles ur ON u.UserId = ur.UserId
            JOIN Roles r ON ur.RoleId = r.RoleId
            WHERE 1=1");

                // Add search condition to count query if search term is provided
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
            AND (u.Username LIKE @Search
            OR u.Email LIKE @Search
            OR u.FullName LIKE @Search
            OR r.RoleName LIKE @Search
            OR ur.Status LIKE @Search)");
                }

                query.Append(@"
        ) AS LatestUserRoles
        WHERE RowNum = 1;");

                // Execute the query
                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%"
                }))
                {
                    var userRoles = multi.Read<UserRoleResponse>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<UserRoleResponse>
                    {
                        Items = userRoles,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }
        public async Task<bool> AddClaimToRoleAsync(AddClaimToRoleRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the role exists
                const string checkRoleExistsQuery = @"
                SELECT COUNT(*)
                FROM Roles
                WHERE RoleId = @RoleId";

                var roleExists = await connection.ExecuteScalarAsync<int>(checkRoleExistsQuery, new
                {
                    request.RoleId
                });

                if (roleExists == 0)
                {
                    throw new ItemDoesNotExistException($"The role with ID {request.RoleId} does not exist.");
                }

                // Check if the claim exists
                const string checkClaimExistsQuery = @"
                SELECT COUNT(*)
                FROM Claims
                WHERE ClaimId = @ClaimId";

                var claimExists = await connection.ExecuteScalarAsync<int>(checkClaimExistsQuery, new
                {
                    request.ClaimId
                });

                if (claimExists == 0)
                {
                    throw new ItemDoesNotExistException($"The claim with ID {request.ClaimId} does not exist.");
                }

                // Check if the claim is already associated with the role
                const string checkRoleClaimExistsQuery = @"
                SELECT COUNT(*)
                FROM RoleClaims
                WHERE RoleId = @RoleId AND ClaimId = @ClaimId";

                var roleClaimExists = await connection.ExecuteScalarAsync<int>(checkRoleClaimExistsQuery, new
                {
                    request.RoleId,
                    request.ClaimId
                });

                if (roleClaimExists > 0)
                {
                    throw new ItemAlreadyExistsException("This claim is already associated with the role.");
                }

                // Associate the claim with the role
                const string insertRoleClaimQuery = @"
                INSERT INTO RoleClaims (RoleId, ClaimId)
                VALUES (@RoleId, @ClaimId);";

                var rowsAffected = await connection.ExecuteAsync(insertRoleClaimQuery, new
                {
                    request.RoleId,
                    request.ClaimId
                });

                return rowsAffected > 0;
            }
        }

        public async Task<bool> RemoveClaimFromRoleAsync(RemoveClaimFromRoleRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the role exists
                const string checkRoleExistsQuery = @"
                SELECT COUNT(*)
                FROM Roles
                WHERE RoleId = @RoleId";

                var roleExists = await connection.ExecuteScalarAsync<int>(checkRoleExistsQuery, new
                {
                    request.RoleId
                });

                if (roleExists == 0)
                {
                    throw new ItemDoesNotExistException($"The role with ID {request.RoleId} does not exist.");
                }

                // Check if the claim exists
                const string checkClaimExistsQuery = @"
                SELECT COUNT(*)
                FROM Claims
                WHERE ClaimId = @ClaimId";

                var claimExists = await connection.ExecuteScalarAsync<int>(checkClaimExistsQuery, new
                {
                    request.ClaimId
                });

                if (claimExists == 0)
                {
                    throw new ItemDoesNotExistException($"The claim with ID {request.ClaimId} does not exist.");
                }

                // Check if the claim is associated with the role
                const string checkRoleClaimExistsQuery = @"
                SELECT COUNT(*)
                FROM RoleClaims
                WHERE RoleId = @RoleId AND ClaimId = @ClaimId";

                var roleClaimExists = await connection.ExecuteScalarAsync<int>(checkRoleClaimExistsQuery, new
                {
                    request.RoleId,
                    request.ClaimId
                });

                if (roleClaimExists == 0)
                {
                    throw new ItemDoesNotExistException($"The claim with ID {request.ClaimId} is not associated with the role.");
                }

                // Remove the claim from the role
                const string deleteRoleClaimQuery = @"
                DELETE FROM RoleClaims
                WHERE RoleId = @RoleId AND ClaimId = @ClaimId;";

                var rowsAffected = await connection.ExecuteAsync(deleteRoleClaimQuery, new
                {
                    request.RoleId,
                    request.ClaimId
                });

                return rowsAffected > 0;
            }
        }

        public async Task<IEnumerable<RoleClaimResponse>> GetClaimsByRoleIdAsync(int roleId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Retrieve the role and its associated claims
                const string query = @"
                SELECT r.RoleId, r.RoleName, c.ClaimId, c.ClaimType, c.ClaimValue
                FROM RoleClaims rc
                JOIN Claims c ON rc.ClaimId = c.ClaimId
                JOIN Roles r ON rc.RoleId = r.RoleId
                WHERE r.RoleId = @RoleId";

                var roleClaims = await connection.QueryAsync<RoleClaimResponse>(query, new
                {
                    RoleId = roleId
                });

                if (!roleClaims.Any())
                {
                    throw new ItemDoesNotExistException($"The role with ID {roleId} does not exist or has no associated claims.");
                }

                return roleClaims;
            }
        }

        public async Task<PagedResultResponse<RoleClaimResponse>> GetRoleClaimsAsync(int pageNumber, int pageSize, string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                
                var skip = (pageNumber - 1) * pageSize;

                
                var query = new StringBuilder(@"
                SELECT r.RoleId, r.RoleName, c.ClaimId, c.ClaimType, c.ClaimValue
                FROM RoleClaims rc
                JOIN Roles r ON rc.RoleId = r.RoleId
                JOIN Claims c ON rc.ClaimId = c.ClaimId
                WHERE 1=1");

               
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                    AND (r.RoleName LIKE @Search
                    OR c.ClaimType LIKE @Search
                    OR c.ClaimValue LIKE @Search)");
                }

                
                query.Append(@"
                ORDER BY r.RoleName, c.ClaimType
                OFFSET @Skip ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*)
                FROM RoleClaims rc
                JOIN Roles r ON rc.RoleId = r.RoleId
                JOIN Claims c ON rc.ClaimId = c.ClaimId
                WHERE 1=1");

                
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                    AND (r.RoleName LIKE @Search
                    OR c.ClaimType LIKE @Search
                    OR c.ClaimValue LIKE @Search)");
                }

                
                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%"
                }))
                {
                    var roleClaims = multi.Read<RoleClaimResponse>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<RoleClaimResponse>
                    {
                        Items = roleClaims,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<bool> AddClaimToUserAsync(AddClaimToUserRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                
                const string checkUserQuery = "SELECT COUNT(*) FROM [Users] WHERE UserId = @UserId";
                const string checkClaimQuery = "SELECT COUNT(*) FROM [Claims] WHERE ClaimId = @ClaimId";
                const string checkUserClaimQuery = "SELECT COUNT(*) FROM UserClaims WHERE UserId = @UserId AND ClaimId = @ClaimId";

                var userExists = await connection.ExecuteScalarAsync<int>(checkUserQuery, new { UserId = request.UserId });
                var claimExists = await connection.ExecuteScalarAsync<int>(checkClaimQuery, new { ClaimId = request.ClaimId });
                var userClaimExists = await connection.ExecuteScalarAsync<int>(checkUserClaimQuery, new { UserId = request.UserId, ClaimId = request.ClaimId });

                if (userExists == 0)
                {
                    throw new ItemDoesNotExistException($"User with UserId:{request.UserId} does not exist.");
                }

                if (claimExists == 0)
                {
                    throw new ItemDoesNotExistException($"Claim with ClaimId:{request.ClaimId} does not exist.");
                }

                if (userClaimExists > 0)
                {
                    throw new ItemAlreadyExistsException($"User with UserId:{request.UserId} already has the claim with ClaimId:{request.ClaimId}.");
                }

                
                const string insertQuery = @"
                INSERT INTO UserClaims (UserId, ClaimId)
                VALUES (@UserId, @ClaimId);";

                await connection.ExecuteAsync(insertQuery, request);
                return true;
            }
        }

        public async Task<bool> RemoveClaimFromUserAsync(RemoveClaimFromUserRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                const string checkUserQuery = "SELECT COUNT(*) FROM [Users] WHERE UserId = @UserId";
                const string checkClaimQuery = "SELECT COUNT(*) FROM [Claims] WHERE ClaimId = @ClaimId";
                const string checkUserClaimQuery = "SELECT COUNT(*) FROM UserClaims WHERE UserId = @UserId AND ClaimId = @ClaimId";

                var userExists = await connection.ExecuteScalarAsync<int>(checkUserQuery, new { UserId = request.UserId });
                var claimExists = await connection.ExecuteScalarAsync<int>(checkClaimQuery, new { ClaimId = request.ClaimId });
                var userClaimExists = await connection.ExecuteScalarAsync<int>(checkUserClaimQuery, new { UserId = request.UserId, ClaimId = request.ClaimId });

                if (userExists == 0)
                {
                    throw new ItemDoesNotExistException($"User with UserId:{request.UserId} does not exist");
                }

                if (claimExists == 0)
                {
                    throw new ItemDoesNotExistException($"Claim with ClaimId:{request.ClaimId} does not exist");
                }

                if (userClaimExists == 0)
                {
                    throw new ItemDoesNotExistException($"Claim with ClaimId:{request.ClaimId} is not assigned to User with UserId:{request.UserId}");
                }

                const string deleteQuery = @"
                DELETE FROM UserClaims
                WHERE UserId = @UserId AND ClaimId = @ClaimId;";

                var affectedRows = await connection.ExecuteAsync(deleteQuery, request);
                return affectedRows > 0;
            }
        }


        public async Task<PagedResultResponse<UserClaimResponse>> GetUserClaimsAsync(int pageNumber, int pageSize, string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
                SELECT u.UserId, u.Username, c.ClaimId, c.ClaimType, c.ClaimValue
                FROM UserClaims uc
                JOIN [Users] u ON uc.UserId = u.UserId
                JOIN Claims c ON uc.ClaimId = c.ClaimId
                WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                    AND (u.Username LIKE @Search
                    OR c.ClaimType LIKE @Search
                    OR c.ClaimValue LIKE @Search)");
                }

                query.Append(@"
                ORDER BY u.Username, c.ClaimType
                OFFSET @Skip ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*)
                FROM UserClaims uc
                JOIN [Users] u ON uc.UserId = u.UserId
                JOIN Claims c ON uc.ClaimId = c.ClaimId
                WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                    AND (u.Username LIKE @Search
                    OR c.ClaimType LIKE @Search
                    OR c.ClaimValue LIKE @Search)");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%"
                }))
                {
                    var userClaims = multi.Read<UserClaimResponse>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<UserClaimResponse>
                    {
                        Items = userClaims,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the user exists
                var user = await connection.QuerySingleOrDefaultAsync<User>(
                    "SELECT UserId FROM [Users] WHERE Email = @Email",
                    new { Email = email }
                );

                if (user == null)
                {
                    throw new UserNotFoundException("User not found.");
                }

                // Generate a unique token
                var token = Guid.NewGuid().ToString("N"); // Example: "550e8400e29b41d4a716446655440000"
                var expiryDate = DateTime.UtcNow.AddMinutes(15); // Token expires in 15 minutes

                // Store the token and expiry date in the database
                await connection.ExecuteAsync(
                    "UPDATE [Users] SET PasswordResetToken = @Token, PasswordResetTokenExpiry = @ExpiryDate WHERE UserId = @UserId",
                    new { Token = token, ExpiryDate = expiryDate, UserId = user.UserId }
                );

                return token;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the user exists
                var user = await connection.QuerySingleOrDefaultAsync<User>(
                    "SELECT UserId, PasswordResetToken, PasswordResetTokenExpiry FROM [Users] WHERE Email = @Email",
                    new { Email = email }
                );

                if (user == null)
                {
                    throw new UserNotFoundException("User not found.");
                }

                // Validate the token and expiry date
                if (user.PasswordResetToken != token || user.PasswordResetTokenExpiry < DateTime.UtcNow)
                {
                    throw new InvalidTokenException("Invalid or expired token.");
                }

                // Hash the new password
                string hashedPassword = PasswordHasher.HashPassword(newPassword);

                // Update the user's password and clear the reset token
                await connection.ExecuteAsync(
                    "UPDATE [Users] SET HashedPassword = @HashedPassword, PasswordResetToken = NULL, PasswordResetTokenExpiry = NULL WHERE UserId = @UserId",
                    new { HashedPassword = hashedPassword, UserId = user.UserId }
                );

                return true;
            }
        }






    }
}
