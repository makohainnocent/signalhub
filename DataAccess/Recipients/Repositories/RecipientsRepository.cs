using Domain.Common.Responses;
using Domain.RecipientGroups;
using Domain.Recipients;
using Domain.Recipients.Requests;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Abstractions;

namespace Application.Recipients.Abstractions
{
    public class RecipientsRepository : IRecipientsRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public RecipientsRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        // Basic CRUD Operations
        public async Task<Recipient> CreateRecipientAsync(RecipientCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    INSERT INTO [Recipients] (TenantId, ExternalId, Email, PhoneNumber, DeviceToken, FullName, PreferencesJson, IsActive, CreatedAt, UserId)
                    VALUES (@TenantId, @ExternalId, @Email, @PhoneNumber, @DeviceToken, @FullName, @PreferencesJson, @IsActive, @CreatedAt, @UserId);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var recipientId = await connection.ExecuteScalarAsync<int>(query, new
                {
                    request.TenantId,
                    request.ExternalId,
                    request.Email,
                    request.PhoneNumber,
                    request.DeviceToken,
                    request.FullName,
                    request.PreferencesJson,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    request.UserId
                });

                return new Recipient
                {
                    RecipientId = recipientId,
                    TenantId = request.TenantId,
                    ExternalId = request.ExternalId,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    DeviceToken = request.DeviceToken,
                    FullName = request.FullName,
                    PreferencesJson = request.PreferencesJson,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UserId = request.UserId
                };
            }
        }

        public async Task<PagedResultResponse<Recipient>> GetRecipientsAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            int? tenantId = null,
            int? userId = null,
            bool? isActive = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;
                var query = @"
                    SELECT * FROM [Recipients]
                    WHERE (@Search IS NULL OR FullName LIKE @Search)
                    AND (@TenantId IS NULL OR TenantId = @TenantId)
                    AND (@UserId IS NULL OR UserId = @UserId)
                    AND (@IsActive IS NULL OR IsActive = @IsActive)
                    ORDER BY CreatedAt
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*) FROM [Recipients]
                    WHERE (@Search IS NULL OR FullName LIKE @Search)
                    AND (@TenantId IS NULL OR TenantId = @TenantId)
                    AND (@UserId IS NULL OR UserId = @UserId)
                    AND (@IsActive IS NULL OR IsActive = @IsActive);";

                using (var multi = await connection.QueryMultipleAsync(query, new
                {
                    Search = string.IsNullOrEmpty(search) ? null : $"%{search}%",
                    TenantId = tenantId,
                    UserId = userId,
                    IsActive = isActive,
                    Skip = skip,
                    PageSize = pageSize
                }))
                {
                    var recipients = multi.Read<Recipient>().ToList();
                    var totalCount = multi.ReadSingle<int>();

                    return new PagedResultResponse<Recipient>
                    {
                        Items = recipients,
                        TotalCount = totalCount,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<Recipient?> GetRecipientByIdAsync(int recipientId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT * FROM [Recipients]
                    WHERE RecipientId = @RecipientId";

                return await connection.QuerySingleOrDefaultAsync<Recipient>(query, new { RecipientId = recipientId });
            }
        }

        public async Task<Recipient> UpdateRecipientAsync(RecipientUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [Recipients]
                    SET FullName = @FullName,
                        Email = @Email,
                        PhoneNumber = @PhoneNumber,
                        DeviceToken = @DeviceToken,
                        PreferencesJson = @PreferencesJson,
                        IsActive = @IsActive,
                        UpdatedAt = @UpdatedAt
                    WHERE RecipientId = @RecipientId;

                    SELECT * FROM [Recipients] WHERE RecipientId = @RecipientId;";

                var updatedRecipient = await connection.QuerySingleOrDefaultAsync<Recipient>(query, new
                {
                    request.RecipientId,
                    request.FullName,
                    request.Email,
                    request.PhoneNumber,
                    request.DeviceToken,
                    request.PreferencesJson,
                    request.IsActive,
                    UpdatedAt = DateTime.UtcNow
                });

                return updatedRecipient!;
            }
        }

        public async Task<bool> DeleteRecipientAsync(int recipientId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    DELETE FROM [Recipients]
                    WHERE RecipientId = @RecipientId";

                var result = await connection.ExecuteAsync(query, new { RecipientId = recipientId });

                return result > 0;
            }
        }

        public async Task<int> CountRecipientsAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = "SELECT COUNT(*) FROM [Recipients]";
                return await connection.ExecuteScalarAsync<int>(query);
            }
        }

        // Specialized Lookups
        public async Task<Recipient?> GetRecipientByEmailAsync(string email, int tenantId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT * FROM [Recipients]
                    WHERE Email = @Email AND TenantId = @TenantId";

                return await connection.QuerySingleOrDefaultAsync<Recipient>(query, new { Email = email, TenantId = tenantId });
            }
        }

        public async Task<Recipient?> GetRecipientByPhoneAsync(string phoneNumber, int tenantId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT * FROM [Recipients]
                    WHERE PhoneNumber = @PhoneNumber AND TenantId = @TenantId";

                return await connection.QuerySingleOrDefaultAsync<Recipient>(query, new { PhoneNumber = phoneNumber, TenantId = tenantId });
            }
        }

        public async Task<Recipient?> GetRecipientByExternalIdAsync(string externalId, int tenantId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT * FROM [Recipients]
                    WHERE ExternalId = @ExternalId AND TenantId = @TenantId";

                return await connection.QuerySingleOrDefaultAsync<Recipient>(query, new { ExternalId = externalId, TenantId = tenantId });
            }
        }

        public async Task<Recipient?> GetRecipientByUserIdAsync(int userId, int tenantId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT * FROM [Recipients]
                    WHERE UserId = @UserId AND TenantId = @TenantId";

                return await connection.QuerySingleOrDefaultAsync<Recipient>(query, new { UserId = userId, TenantId = tenantId });
            }
        }

        // Status Management
        public async Task<bool> DeactivateRecipientAsync(int recipientId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [Recipients]
                    SET IsActive = 0
                    WHERE RecipientId = @RecipientId";

                var result = await connection.ExecuteAsync(query, new { RecipientId = recipientId });

                return result > 0;
            }
        }

        public async Task<bool> ActivateRecipientAsync(int recipientId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [Recipients]
                    SET IsActive = 1
                    WHERE RecipientId = @RecipientId";

                var result = await connection.ExecuteAsync(query, new { RecipientId = recipientId });

                return result > 0;
            }
        }

        // Preference Management
        public async Task<bool> UpdatePreferencesAsync(int recipientId, string preferencesJson)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [Recipients]
                    SET PreferencesJson = @PreferencesJson
                    WHERE RecipientId = @RecipientId";

                var result = await connection.ExecuteAsync(query, new { RecipientId = recipientId, PreferencesJson = preferencesJson });

                return result > 0;
            }
        }

        // Group Membership
        public async Task<PagedResultResponse<RecipientGroup>> GetRecipientGroupsAsync(
            int recipientId,
            int pageNumber,
            int pageSize)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;
                var query = @"
                    SELECT g.* FROM [RecipientGroups] g
                    JOIN [RecipientGroupMembers] rgm ON g.GroupId = rgm.GroupId
                    WHERE rgm.RecipientId = @RecipientId
                    ORDER BY g.Name
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*) FROM [RecipientGroupMembers] WHERE RecipientId = @RecipientId";

                using (var multi = await connection.QueryMultipleAsync(query, new
                {
                    RecipientId = recipientId,
                    Skip = skip,
                    PageSize = pageSize
                }))
                {
                    var groups = multi.Read<RecipientGroup>().ToList();
                    var totalCount = multi.ReadSingle<int>();

                    return new PagedResultResponse<RecipientGroup>
                    {
                        Items = groups,
                        TotalCount = totalCount,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<int> CountRecipientGroupsAsync(int recipientId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT COUNT(*) FROM [RecipientGroupMembers]
                    WHERE RecipientId = @RecipientId";

                return await connection.ExecuteScalarAsync<int>(query, new { RecipientId = recipientId });
            }
        }
    }
}
