// DataAccess/NotificationTemplates/Repositories/NotificationTemplatesRepository.cs
using Application.NotificationTemplates.Abstractions;
using Domain.NotificationTemplates;
using Domain.NotificationTemplates.Requests;
using Domain.TemplateChannels;
using Dapper;
using System.Data;
using Application.Common.Abstractions;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;
using System.Text;

namespace DataAccess.NotificationTemplates.Repositories
{
    public class NotificationTemplatesRepository : INotificationTemplatesRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public NotificationTemplatesRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        // Basic CRUD Operations
        public async Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplateCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                    INSERT INTO [NotificationTemplates] (ApplicationId, Name, Description, Content, VariablesSchemaJson, CreatedByUserId, ApprovalStatus, IsActive)
                    VALUES (@ApplicationId, @Name, @Description, @Content, @VariablesSchemaJson, @CreatedByUserId, 'Draft', 1);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    request.ApplicationId,
                    request.Name,
                    request.Description,
                    request.Content,
                    request.VariablesSchemaJson,
                    request.CreatedByUserId
                };

                var templateId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                return new NotificationTemplate
                {
                    TemplateId = templateId,
                    ApplicationId = request.ApplicationId,
                    Name = request.Name,
                    Description = request.Description,
                    Content = request.Content,
                    VariablesSchemaJson = request.VariablesSchemaJson,
                    CreatedByUserId = request.CreatedByUserId,
                    ApprovalStatus = "Draft",
                    IsActive = true
                };
            }
        }

        public async Task<PagedResultResponse<NotificationTemplate>> GetTemplatesAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            int? applicationId = null,
            int? createdByUserId = null,
            string? approvalStatus = null,
            bool? isActive = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;
                var query = new StringBuilder(@"
                    SELECT *
                    FROM [NotificationTemplates]
                    WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                        AND (Name LIKE @Search OR Description LIKE @Search)");
                }

                if (applicationId.HasValue)
                {
                    query.Append(@"
                        AND ApplicationId = @ApplicationId");
                }

                if (createdByUserId.HasValue)
                {
                    query.Append(@"
                        AND CreatedByUserId = @CreatedByUserId");
                }

                if (!string.IsNullOrWhiteSpace(approvalStatus))
                {
                    query.Append(@"
                        AND ApprovalStatus = @ApprovalStatus");
                }

                if (isActive.HasValue)
                {
                    query.Append(@"
                        AND IsActive = @IsActive");
                }

                query.Append(@"
                    ORDER BY CreatedAt DESC
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*)
                    FROM [NotificationTemplates]
                    WHERE 1=1");

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%",
                    ApplicationId = applicationId,
                    CreatedByUserId = createdByUserId,
                    ApprovalStatus = approvalStatus,
                    IsActive = isActive
                }))
                {
                    var templates = multi.Read<NotificationTemplate>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<NotificationTemplate>
                    {
                        Items = templates,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<NotificationTemplate?> GetTemplateByIdAsync(int templateId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT *
                    FROM [NotificationTemplates]
                    WHERE TemplateId = @TemplateId";

                return await connection.QuerySingleOrDefaultAsync<NotificationTemplate>(query, new { TemplateId = templateId });
            }
        }

        public async Task<NotificationTemplate> UpdateTemplateAsync(NotificationTemplateUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var updateQuery = @"
                    UPDATE [NotificationTemplates]
                    SET Name = @Name,
                        Description = @Description,
                        Content = @Content,
                        VariablesSchemaJson = @VariablesSchemaJson,
                        UpdatedAt = @UpdatedAt
                    WHERE TemplateId = @TemplateId";

                var parameters = new
                {
                    request.TemplateId,
                    request.Name,
                    request.Description,
                    request.Content,
                    request.VariablesSchemaJson,
                    UpdatedAt = DateTime.UtcNow
                };

                await connection.ExecuteAsync(updateQuery, parameters);

                return new NotificationTemplate
                {
                    TemplateId = request.TemplateId,
                    Name = request.Name,
                    Description = request.Description,
                    Content = request.Content,
                    VariablesSchemaJson = request.VariablesSchemaJson,
                    UpdatedAt = DateTime.UtcNow
                };
            }
        }

        public async Task<bool> DeleteTemplateAsync(int templateId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var checkQuery = @"
                    SELECT COUNT(*)
                    FROM [NotificationTemplates]
                    WHERE TemplateId = @TemplateId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { TemplateId = templateId });

                if (exists == 0)
                {
                    return false;
                }

                var deleteQuery = @"
                    DELETE FROM [NotificationTemplates]
                    WHERE TemplateId = @TemplateId";

                await connection.ExecuteAsync(deleteQuery, new { TemplateId = templateId });

                return true;
            }
        }

        public async Task<int> CountTemplatesAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = "SELECT COUNT(*) FROM [NotificationTemplates]";

                return await connection.ExecuteScalarAsync<int>(query);
            }
        }

        // Template Versioning
        public async Task<NotificationTemplate> CreateNewVersionAsync(int templateId, int createdByUserId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var versionQuery = @"
                    INSERT INTO [NotificationTemplateVersions] (TemplateId, CreatedByUserId, CreatedAt)
                    VALUES (@TemplateId, @CreatedByUserId, @CreatedAt);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    TemplateId = templateId,
                    CreatedByUserId = createdByUserId,
                    CreatedAt = DateTime.UtcNow
                };

                var versionId = await connection.QuerySingleAsync<int>(versionQuery, parameters);

                var template = await GetTemplateByIdAsync(templateId);
                if (template != null)
                {
                    return new NotificationTemplate
                    {
                        TemplateId = template.TemplateId,
                        ApplicationId = template.ApplicationId,
                        Name = template.Name,
                        Description = template.Description,
                        Content = template.Content,
                        VariablesSchemaJson = template.VariablesSchemaJson,
                        Version = template.Version + 1, // Increment version
                        IsActive = template.IsActive,
                        CreatedAt = DateTime.UtcNow,
                        CreatedByUserId = createdByUserId,
                    };
                }

                throw new Exception("Template not found");
            }
        }

        public async Task<PagedResultResponse<NotificationTemplate>> GetTemplateVersionsAsync(int templateId, int pageNumber, int pageSize)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;
                var query = @"
                    SELECT *
                    FROM [NotificationTemplateVersions]
                    WHERE TemplateId = @TemplateId
                    ORDER BY CreatedAt DESC
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*)
                    FROM [NotificationTemplateVersions]
                    WHERE TemplateId = @TemplateId";

                using (var multi = await connection.QueryMultipleAsync(query, new
                {
                    TemplateId = templateId,
                    Skip = skip,
                    PageSize = pageSize
                }))
                {
                    var versions = multi.Read<NotificationTemplate>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<NotificationTemplate>
                    {
                        Items = versions,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        // Approval Workflow
        public async Task<bool> SubmitForApprovalAsync(int templateId, int submittedByUserId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var updateQuery = @"
                    UPDATE [NotificationTemplates]
                    SET ApprovalStatus = 'Pending', 
                        UpdatedAt = @UpdatedAt
                    WHERE TemplateId = @TemplateId AND ApprovalStatus = 'Draft'";

                var result = await connection.ExecuteAsync(updateQuery, new
                {
                    TemplateId = templateId,
                    UpdatedAt = DateTime.UtcNow
                });

                return result > 0;
            }
        }

        public async Task<bool> ApproveTemplateAsync(int templateId, int approvedByUserId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var updateQuery = @"
                    UPDATE [NotificationTemplates]
                    SET ApprovalStatus = 'Approved',
                        ApprovedByUserId = @ApprovedByUserId,
                        UpdatedAt = @UpdatedAt
                    WHERE TemplateId = @TemplateId";

                var result = await connection.ExecuteAsync(updateQuery, new
                {
                    TemplateId = templateId,
                    ApprovedByUserId = approvedByUserId,
                    UpdatedAt = DateTime.UtcNow
                });

                return result > 0;
            }
        }

        public async Task<bool> RejectTemplateAsync(int templateId, int rejectedByUserId, string comments)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var updateQuery = @"
                    UPDATE [NotificationTemplates]
                    SET ApprovalStatus = 'Rejected', 
                        UpdatedAt = @UpdatedAt
                    WHERE TemplateId = @TemplateId";

                var result = await connection.ExecuteAsync(updateQuery, new
                {
                    TemplateId = templateId,
                    UpdatedAt = DateTime.UtcNow
                });

                return result > 0;
            }
        }

        public async Task<bool> RevertToDraftAsync(int templateId, int revertedByUserId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var updateQuery = @"
                    UPDATE [NotificationTemplates]
                    SET ApprovalStatus = 'Draft',
                        UpdatedAt = @UpdatedAt
                    WHERE TemplateId = @TemplateId";

                var result = await connection.ExecuteAsync(updateQuery, new
                {
                    TemplateId = templateId,
                    UpdatedAt = DateTime.UtcNow
                });

                return result > 0;
            }
        }

        // Template Activation
        public async Task<bool> ActivateTemplateAsync(int templateId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var updateQuery = @"
                    UPDATE [NotificationTemplates]
                    SET IsActive = 1
                    WHERE TemplateId = @TemplateId";

                var result = await connection.ExecuteAsync(updateQuery, new
                {
                    TemplateId = templateId
                });

                return result > 0;
            }
        }

        public async Task<bool> DeactivateTemplateAsync(int templateId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var updateQuery = @"
                    UPDATE [NotificationTemplates]
                    SET IsActive = 0
                    WHERE TemplateId = @TemplateId";

                var result = await connection.ExecuteAsync(updateQuery, new
                {
                    TemplateId = templateId
                });

                return result > 0;
            }
        }

        // Template Content Operations
        public async Task<string> GetTemplateContentAsync(int templateId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT Content
                    FROM [NotificationTemplates]
                    WHERE TemplateId = @TemplateId";

                return await connection.QuerySingleOrDefaultAsync<string>(query, new { TemplateId = templateId });
            }
        }

        public async Task<bool> UpdateTemplateContentAsync(int templateId, string content, int updatedByUserId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var updateQuery = @"
                    UPDATE [NotificationTemplates]
                    SET Content = @Content, UpdatedByUserId = @UpdatedByUserId, UpdatedAt = @UpdatedAt
                    WHERE TemplateId = @TemplateId";

                var result = await connection.ExecuteAsync(updateQuery, new
                {
                    TemplateId = templateId,
                    Content = content,
                    UpdatedByUserId = updatedByUserId,
                    UpdatedAt = DateTime.UtcNow
                });

                return result > 0;
            }
        }

        public async Task<bool> UpdateVariablesSchemaAsync(int templateId, string schemaJson, int updatedByUserId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var updateQuery = @"
                    UPDATE [NotificationTemplates]
                    SET VariablesSchemaJson = @VariablesSchemaJson, UpdatedByUserId = @UpdatedByUserId, UpdatedAt = @UpdatedAt
                    WHERE TemplateId = @TemplateId";

                var result = await connection.ExecuteAsync(updateQuery, new
                {
                    TemplateId = templateId,
                    VariablesSchemaJson = schemaJson,
                    UpdatedByUserId = updatedByUserId,
                    UpdatedAt = DateTime.UtcNow
                });

                return result > 0;
            }
        }

        // Channel Templates
        public async Task<PagedResultResponse<TemplateChannel>> GetTemplateChannelsAsync(int templateId, int pageNumber, int pageSize)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;
                var query = @"
                    SELECT *
                    FROM [TemplateChannels]
                    WHERE TemplateId = @TemplateId
                    ORDER BY ChannelId
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*)
                    FROM [TemplateChannels]
                    WHERE TemplateId = @TemplateId";

                using (var multi = await connection.QueryMultipleAsync(query, new
                {
                    TemplateId = templateId,
                    Skip = skip,
                    PageSize = pageSize
                }))
                {
                    var channels = multi.Read<TemplateChannel>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<TemplateChannel>
                    {
                        Items = channels,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<int> CountTemplateChannelsAsync(int templateId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT COUNT(*)
                    FROM [TemplateChannels]
                    WHERE TemplateId = @TemplateId";

                return await connection.ExecuteScalarAsync<int>(query, new { TemplateId = templateId });
            }
        }
    }
}
