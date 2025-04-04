using Application.Common.Abstractions;
using Dapper;
using Domain.Common.Responses;
using Domain.TemplateChannels;
using Domain.TemplateChannels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.NotificationTemplates.Abstractions
{
    public class TemplateChannelsRepository : ITemplateChannelsRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public TemplateChannelsRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        // Channel CRUD Operations
        public async Task<TemplateChannel> CreateTemplateChannelAsync(TemplateChannelCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    INSERT INTO [TemplateChannels] (TemplateId, ChannelType, ChannelSpecificContentJson, IsActive, CreatedByUserId, CreatedAt)
                    VALUES (@TemplateId, @ChannelType, @ChannelSpecificContentJson, @IsActive, @CreatedByUserId, @CreatedAt);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var templateChannelId = await connection.ExecuteScalarAsync<int>(query, new
                {
                    request.TemplateId,
                    request.ChannelType,
                    request.ChannelSpecificContentJson,
                    IsActive = true,
                    request.CreatedByUserId,
                    CreatedAt = DateTime.UtcNow
                });

                return new TemplateChannel
                {
                    TemplateChannelId = templateChannelId,
                    TemplateId = request.TemplateId,
                    ChannelType = request.ChannelType,
                    ChannelSpecificContentJson = request.ChannelSpecificContentJson,
                    IsActive = true,
                    CreatedByUserId = request.CreatedByUserId,
                    CreatedAt = DateTime.UtcNow
                };
            }
        }

        public async Task<TemplateChannel?> GetTemplateChannelByIdAsync(int templateChannelId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT * FROM [TemplateChannels]
                    WHERE TemplateChannelId = @TemplateChannelId";

                return await connection.QuerySingleOrDefaultAsync<TemplateChannel>(query, new { TemplateChannelId = templateChannelId });
            }
        }

        public async Task<TemplateChannel> UpdateTemplateChannelAsync(TemplateChannelUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [TemplateChannels]
                    SET ChannelType = @ChannelType,
                        ChannelSpecificContentJson = @ChannelSpecificContentJson,
                        IsActive = @IsActive
                    WHERE TemplateChannelId = @TemplateChannelId;

                    SELECT * FROM [TemplateChannels] WHERE TemplateChannelId = @TemplateChannelId;";

                var updatedChannel = await connection.QuerySingleOrDefaultAsync<TemplateChannel>(query, new
                {
                    request.TemplateChannelId,
                    request.ChannelType,
                    request.ChannelSpecificContentJson,
                    request.IsActive
                });

                return updatedChannel!;
            }
        }

        public async Task<bool> DeleteTemplateChannelAsync(int templateChannelId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    DELETE FROM [TemplateChannels]
                    WHERE TemplateChannelId = @TemplateChannelId";

                var result = await connection.ExecuteAsync(query, new { TemplateChannelId = templateChannelId });

                return result > 0;
            }
        }

        // Template Channel Management
        public async Task<PagedResultResponse<TemplateChannel>> GetChannelsByTemplateAsync(
            int templateId,
            int pageNumber,
            int pageSize,
            string? channelType = null,
            bool? isActive = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;
                var query = @"
                    SELECT * FROM [TemplateChannels]
                    WHERE TemplateId = @TemplateId
                    AND (@ChannelType IS NULL OR ChannelType = @ChannelType)
                    AND (@IsActive IS NULL OR IsActive = @IsActive)
                    ORDER BY CreatedAt
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*) FROM [TemplateChannels]
                    WHERE TemplateId = @TemplateId
                    AND (@ChannelType IS NULL OR ChannelType = @ChannelType)
                    AND (@IsActive IS NULL OR IsActive = @IsActive);";

                using (var multi = await connection.QueryMultipleAsync(query, new
                {
                    TemplateId = templateId,
                    ChannelType = channelType,
                    IsActive = isActive,
                    Skip = skip,
                    PageSize = pageSize
                }))
                {
                    var channels = multi.Read<TemplateChannel>().ToList();
                    var totalCount = multi.ReadSingle<int>();

                    return new PagedResultResponse<TemplateChannel>
                    {
                        Items = channels,
                        TotalCount = totalCount,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<TemplateChannel?> GetChannelByTypeAsync(int templateId, string channelType)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT * FROM [TemplateChannels]
                    WHERE TemplateId = @TemplateId AND ChannelType = @ChannelType";

                return await connection.QuerySingleOrDefaultAsync<TemplateChannel>(query, new { TemplateId = templateId, ChannelType = channelType });
            }
        }

        // Content Management
        public async Task<bool> UpdateChannelContentAsync(
            int templateChannelId,
            string channelContentJson,
            int updatedByUserId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [TemplateChannels]
                    SET ChannelSpecificContentJson = @ChannelContentJson
                    WHERE TemplateChannelId = @TemplateChannelId";

                var result = await connection.ExecuteAsync(query, new
                {
                    TemplateChannelId = templateChannelId,
                    ChannelContentJson = channelContentJson
                });

                return result > 0;
            }
        }

        // Status Management
        public async Task<bool> ActivateChannelAsync(int templateChannelId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [TemplateChannels]
                    SET IsActive = 1
                    WHERE TemplateChannelId = @TemplateChannelId";

                var result = await connection.ExecuteAsync(query, new { TemplateChannelId = templateChannelId });

                return result > 0;
            }
        }

        public async Task<bool> DeactivateChannelAsync(int templateChannelId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [TemplateChannels]
                    SET IsActive = 0
                    WHERE TemplateChannelId = @TemplateChannelId";

                var result = await connection.ExecuteAsync(query, new { TemplateChannelId = templateChannelId });

                return result > 0;
            }
        }

        // Validation
        public async Task<bool> ChannelTypeExistsForTemplateAsync(int templateId, string channelType)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT COUNT(*) FROM [TemplateChannels]
                    WHERE TemplateId = @TemplateId AND ChannelType = @ChannelType";

                var count = await connection.ExecuteScalarAsync<int>(query, new { TemplateId = templateId, ChannelType = channelType });

                return count > 0;
            }
        }

        // Count Operations
        public async Task<int> CountChannelsByTemplateAsync(int templateId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT COUNT(*) FROM [TemplateChannels]
                    WHERE TemplateId = @TemplateId";

                return await connection.ExecuteScalarAsync<int>(query, new { TemplateId = templateId });
            }
        }

        public async Task<int> CountActiveChannelsByTemplateAsync(int templateId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT COUNT(*) FROM [TemplateChannels]
                    WHERE TemplateId = @TemplateId AND IsActive = 1";

                return await connection.ExecuteScalarAsync<int>(query, new { TemplateId = templateId });
            }
        }
    }
}
