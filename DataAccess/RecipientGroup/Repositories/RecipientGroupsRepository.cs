using Domain.Common.Responses;
using Domain.RecipientGroups;
using Domain.RecipientGroups.Requests;
using Domain.Recipients;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Abstractions;

namespace Application.RecipientGroups.Abstractions
{
    public class RecipientGroupsRepository : IRecipientGroupsRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public RecipientGroupsRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        // Create a new recipient group
        public async Task<RecipientGroup> CreateRecipientGroupAsync(RecipientGroupCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    INSERT INTO [RecipientGroups] (TenantId, Name, Description, CreatedAt, CreatedByUserId)
                    VALUES (@TenantId, @Name, @Description, @CreatedAt, @CreatedByUserId);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var groupId = await connection.ExecuteScalarAsync<int>(query, new
                {
                    request.TenantId,
                    request.Name,
                    request.Description,
                    CreatedAt = DateTime.UtcNow,
                    request.CreatedByUserId
                });

                return new RecipientGroup
                {
                    GroupId = groupId,
                    TenantId = request.TenantId,
                    Name = request.Name,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = request.CreatedByUserId
                };
            }
        }

        // Get recipient groups with pagination and optional filters
        public async Task<PagedResultResponse<RecipientGroup>> GetRecipientGroupsAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            int? tenantId = null,
            int? createdByUserId = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = @"
                    SELECT * FROM [RecipientGroups]
                    WHERE (@Search IS NULL OR Name LIKE @Search)
                    AND (@TenantId IS NULL OR TenantId = @TenantId)
                    AND (@CreatedByUserId IS NULL OR CreatedByUserId = @CreatedByUserId)
                    ORDER BY CreatedAt
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*) FROM [RecipientGroups]
                    WHERE (@Search IS NULL OR Name LIKE @Search)
                    AND (@TenantId IS NULL OR TenantId = @TenantId)
                    AND (@CreatedByUserId IS NULL OR CreatedByUserId = @CreatedByUserId);";

                using (var multi = await connection.QueryMultipleAsync(query, new
                {
                    Search = string.IsNullOrEmpty(search) ? null : $"%{search}%",
                    TenantId = tenantId,
                    CreatedByUserId = createdByUserId,
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

        // Get a specific recipient group by its ID
        public async Task<RecipientGroup?> GetRecipientGroupByIdAsync(int groupId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT * FROM [RecipientGroups]
                    WHERE GroupId = @GroupId";

                return await connection.QuerySingleOrDefaultAsync<RecipientGroup>(query, new { GroupId = groupId });
            }
        }

        // Update an existing recipient group
        public async Task<RecipientGroup> UpdateRecipientGroupAsync(RecipientGroupUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [RecipientGroups]
                    SET Name = @Name,
                        Description = @Description
                    WHERE GroupId = @GroupId;

                    SELECT * FROM [RecipientGroups] WHERE GroupId = @GroupId;";

                var updatedGroup = await connection.QuerySingleOrDefaultAsync<RecipientGroup>(query, new
                {
                    request.GroupId,
                    request.Name,
                    request.Description
                });

                return updatedGroup!;
            }
        }

        // Delete a recipient group by ID
        public async Task<bool> DeleteRecipientGroupAsync(int groupId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    DELETE FROM [RecipientGroups]
                    WHERE GroupId = @GroupId";

                var result = await connection.ExecuteAsync(query, new { GroupId = groupId });

                return result > 0;
            }
        }

        // Count the total number of recipient groups
        public async Task<int> CountRecipientGroupsAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = "SELECT COUNT(*) FROM [RecipientGroups]";
                return await connection.ExecuteScalarAsync<int>(query);
            }
        }

        // Add a recipient to a group
        public async Task<bool> AddRecipientToGroupAsync(int groupId, int recipientId, int addedByUserId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    INSERT INTO [RecipientGroupMembers] (GroupId, RecipientId, AddedAt, AddedByUserId)
                    VALUES (@GroupId, @RecipientId, @AddedAt, @AddedByUserId)";

                var result = await connection.ExecuteAsync(query, new
                {
                    GroupId = groupId,
                    RecipientId = recipientId,
                    AddedAt = DateTime.UtcNow,
                    AddedByUserId = addedByUserId
                });

                return result > 0;
            }
        }

        // Remove a recipient from a group
        public async Task<bool> RemoveRecipientFromGroupAsync(int groupId, int recipientId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    DELETE FROM [RecipientGroupMembers]
                    WHERE GroupId = @GroupId AND RecipientId = @RecipientId";

                var result = await connection.ExecuteAsync(query, new
                {
                    GroupId = groupId,
                    RecipientId = recipientId
                });

                return result > 0;
            }
        }

        // Get recipients of a specific group with pagination
        public async Task<PagedResultResponse<Recipient>> GetGroupRecipientsAsync(
            int groupId,
            int pageNumber,
            int pageSize,
            string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;
                var query = @"
                    SELECT * FROM [Recipients] r
                    JOIN [RecipientGroupMembers] rgm ON r.RecipientId = rgm.RecipientId
                    WHERE rgm.GroupId = @GroupId
                    AND (@Search IS NULL OR r.Name LIKE @Search)
                    ORDER BY r.Name
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*) FROM [RecipientGroupMembers] WHERE GroupId = @GroupId";

                using (var multi = await connection.QueryMultipleAsync(query, new
                {
                    GroupId = groupId,
                    Search = string.IsNullOrEmpty(search) ? null : $"%{search}%",
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

        // Count the number of recipients in a group
        public async Task<int> CountGroupRecipientsAsync(int groupId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT COUNT(*)
                    FROM [RecipientGroupMembers]
                    WHERE GroupId = @GroupId";

                return await connection.ExecuteScalarAsync<int>(query, new { GroupId = groupId });
            }
        }
    }
}
