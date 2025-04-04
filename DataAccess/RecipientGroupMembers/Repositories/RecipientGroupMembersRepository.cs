using Domain.Common.Responses;
using Domain.RecipientGroups;
using Domain.RecipientGroupMembers;
using Domain.Recipients;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Abstractions;

namespace Application.RecipientGroups.Abstractions
{
    public class RecipientGroupMembersRepository : IRecipientGroupMembersRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public RecipientGroupMembersRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        // Membership Operations
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

        public async Task<bool> IsRecipientInGroupAsync(int groupId, int recipientId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT COUNT(1)
                    FROM [RecipientGroupMembers]
                    WHERE GroupId = @GroupId AND RecipientId = @RecipientId";

                var count = await connection.ExecuteScalarAsync<int>(query, new
                {
                    GroupId = groupId,
                    RecipientId = recipientId
                });

                return count > 0;
            }
        }

        // Bulk Operations
        public async Task<int> AddMultipleRecipientsToGroupAsync(int groupId, IEnumerable<int> recipientIds, int addedByUserId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    INSERT INTO [RecipientGroupMembers] (GroupId, RecipientId, AddedAt, AddedByUserId)
                    VALUES (@GroupId, @RecipientId, @AddedAt, @AddedByUserId)";

                var parameters = recipientIds.Select(recipientId => new
                {
                    GroupId = groupId,
                    RecipientId = recipientId,
                    AddedAt = DateTime.UtcNow,
                    AddedByUserId = addedByUserId
                });

                var result = await connection.ExecuteAsync(query, parameters);

                return result;
            }
        }

        public async Task<int> RemoveMultipleRecipientsFromGroupAsync(int groupId, IEnumerable<int> recipientIds)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    DELETE FROM [RecipientGroupMembers]
                    WHERE GroupId = @GroupId AND RecipientId IN @RecipientIds";

                var result = await connection.ExecuteAsync(query, new
                {
                    GroupId = groupId,
                    RecipientIds = recipientIds
                });

                return result;
            }
        }

        // Query Operations
        public async Task<PagedResultResponse<Recipient>> GetGroupMembersAsync(int groupId, int pageNumber, int pageSize, string? search = null, bool? isActive = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;
                var query = @"
                    SELECT * FROM [Recipients] r
                    JOIN [RecipientGroupMembers] rgm ON r.RecipientId = rgm.RecipientId
                    WHERE rgm.GroupId = @GroupId";

                if (!string.IsNullOrEmpty(search))
                {
                    query += " AND (r.Name LIKE @Search OR r.Email LIKE @Search)";
                }

                if (isActive.HasValue)
                {
                    query += " AND r.IsActive = @IsActive";
                }

                query += @"
                    ORDER BY r.Name
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*) FROM [RecipientGroupMembers] WHERE GroupId = @GroupId";

                using (var multi = await connection.QueryMultipleAsync(query, new
                {
                    GroupId = groupId,
                    Search = $"%{search}%",
                    IsActive = isActive,
                    Skip = skip,
                    PageSize = pageSize
                }))
                {
                    var members = multi.Read<Recipient>().ToList();
                    var totalCount = multi.ReadSingle<int>();

                    return new PagedResultResponse<Recipient>
                    {
                        Items = members,
                        TotalCount = totalCount,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<PagedResultResponse<RecipientGroup>> GetRecipientGroupsAsync(int recipientId, int pageNumber, int pageSize)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;
                var query = @"
                    SELECT * FROM [RecipientGroups] rg
                    JOIN [RecipientGroupMembers] rgm ON rg.GroupId = rgm.GroupId
                    WHERE rgm.RecipientId = @RecipientId
                    ORDER BY rg.Name
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

        // Count Operations
        public async Task<int> CountGroupMembersAsync(int groupId)
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

        public async Task<int> CountRecipientGroupsAsync(int recipientId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT COUNT(*)
                    FROM [RecipientGroupMembers]
                    WHERE RecipientId = @RecipientId";

                return await connection.ExecuteScalarAsync<int>(query, new { RecipientId = recipientId });
            }
        }

        // Membership History
        public async Task<PagedResultResponse<GroupMembershipRecord>> GetMembershipHistoryAsync(int groupId, int recipientId, int pageNumber, int pageSize)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;
                var query = @"
                    SELECT gm.GroupId, gm.RecipientId, gm.AddedAt, gm.AddedByUserId
                    FROM [RecipientGroupMembers] gm
                    WHERE gm.GroupId = @GroupId AND gm.RecipientId = @RecipientId
                    ORDER BY gm.AddedAt
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*) FROM [RecipientGroupMembers] WHERE GroupId = @GroupId AND RecipientId = @RecipientId";

                using (var multi = await connection.QueryMultipleAsync(query, new
                {
                    GroupId = groupId,
                    RecipientId = recipientId,
                    Skip = skip,
                    PageSize = pageSize
                }))
                {
                    var history = multi.Read<GroupMembershipRecord>().ToList();
                    var totalCount = multi.ReadSingle<int>();

                    return new PagedResultResponse<GroupMembershipRecord>
                    {
                        Items = history,
                        TotalCount = totalCount,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }
    }
}
