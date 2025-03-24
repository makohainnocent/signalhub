using Application.Common.Abstractions;
using Application.TagsManagement.Abstractions;
using Dapper;
using DataAccess.Common.Exceptions;
using Domain.Common.Responses;
using Domain.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.TagsManagement.Repositories
{
    public class TagsManagementRepository : ITagsManagementRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public TagsManagementRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        // Tag-related methods

        public async Task<Tag> CreateTagAsync(Tag tag)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                INSERT INTO [Tags] (TagNumber, TagType, Manufacturer, BatchNumber, CreatedAt)
                VALUES (@TagNumber, @TagType, @Manufacturer, @BatchNumber, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    tag.TagNumber,
                    tag.TagType,
                    tag.Manufacturer,
                    tag.BatchNumber,
                    tag.CreatedAt
                };

                tag.TagId = await connection.QuerySingleAsync<int>(insertQuery, parameters);
                return tag;
            }
        }

        public async Task<Tag?> GetTagByIdAsync(int tagId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                SELECT *
                FROM [Tags]
                WHERE TagId = @TagId";

                return await connection.QuerySingleOrDefaultAsync<Tag>(query, new { TagId = tagId });
            }
        }

        public async Task<PagedResultResponse<Tag>> GetTagsAsync(int pageNumber, int pageSize, string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
                SELECT *
                FROM [Tags]
                WHERE 1 = 1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                    AND (TagNumber LIKE @Search
                    OR TagType LIKE @Search
                    OR Manufacturer LIKE @Search)");
                }

                query.Append(@"
                ORDER BY CreatedAt DESC
                OFFSET @Skip ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*)
                FROM [Tags]
                WHERE 1 = 1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                    AND (TagNumber LIKE @Search
                    OR TagType LIKE @Search
                    OR Manufacturer LIKE @Search)");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%"
                }))
                {
                    var tags = multi.Read<Tag>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<Tag>
                    {
                        Items = tags,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<bool> UpdateTagAsync(Tag tag)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var updateQuery = @"
                UPDATE [Tags]
                SET TagNumber = @TagNumber,
                    TagType = @TagType,
                    Manufacturer = @Manufacturer,
                    BatchNumber = @BatchNumber
                WHERE TagId = @TagId";

                var rowsAffected = await connection.ExecuteAsync(updateQuery, tag);
                return rowsAffected > 0;
            }
        }

        public async Task<bool> DeleteTagAsync(int tagId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var deleteQuery = @"
                DELETE FROM [Tags]
                WHERE TagId = @TagId";

                var rowsAffected = await connection.ExecuteAsync(deleteQuery, new { TagId = tagId });
                return rowsAffected > 0;
            }
        }

        // TagApplication-related methods

        public async Task<TagApplication> CreateTagApplicationAsync(TagApplication application)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                INSERT INTO [TagApplications] (ApplicantType, ApplicantId, NumberOfTags, Purpose, Status, AppliedBy, AppliedAt, ReviewedBy, ReviewedAt, Comments)
                VALUES (@ApplicantType, @ApplicantId, @NumberOfTags, @Purpose, @Status, @AppliedBy, @AppliedAt, @ReviewedBy, @ReviewedAt, @Comments);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    application.ApplicantType,
                    application.ApplicantId,
                    application.NumberOfTags,
                    application.Purpose,
                    application.Status,
                    application.AppliedBy,
                    application.AppliedAt,
                    application.ReviewedBy,
                    application.ReviewedAt,
                    application.Comments
                };

                application.ApplicationId = await connection.QuerySingleAsync<int>(insertQuery, parameters);
                return application;
            }
        }

        public async Task<TagApplication?> GetTagApplicationByIdAsync(int applicationId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                SELECT *
                FROM [TagApplications]
                WHERE ApplicationId = @ApplicationId";

                return await connection.QuerySingleOrDefaultAsync<TagApplication>(query, new { ApplicationId = applicationId });
            }
        }

        public async Task<PagedResultResponse<TagApplication>> GetTagApplicationsAsync(int pageNumber, int pageSize,int? applicantId, string? search = null, string? agent = "no")
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
                SELECT *
                FROM [TagApplications]
                WHERE 1 = 1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                    AND (ApplicantType LIKE @Search
                    OR Purpose LIKE @Search
                    OR Status LIKE @Search)");
                }

                if (applicantId.HasValue && applicantId>0 && agent!="yes")
                {
                    query.Append(@"
                    AND (ApplicantId=@ApplicantId)");
                }

                if (applicantId.HasValue && applicantId > 0 && agent == "yes")
                {
                    query.Append(@"
                    AND (requestFrom=@ApplicantId)");
                }

                query.Append(@"
                ORDER BY AppliedAt DESC
                OFFSET @Skip ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*)
                FROM [TagApplications]
                WHERE 1 = 1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                    AND (ApplicantType LIKE @Search
                    OR Purpose LIKE @Search
                    OR Status LIKE @Search)");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%",
                    ApplicantId=applicantId
                }))
                {
                    var applications = multi.Read<TagApplication>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<TagApplication>
                    {
                        Items = applications,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<bool> UpdateTagApplicationAsync(TagApplication application)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Initialize the base query
                var updateQuery = "UPDATE [TagApplications] SET ";

                // Build the SET clause dynamically based on non-null or non-default values
                var setClauses = new List<string>();
                var parameters = new DynamicParameters();

                if (application.ApplicantType != default)
                {
                    setClauses.Add("ApplicantType = @ApplicantType");
                    parameters.Add("ApplicantType", application.ApplicantType);
                }

                if (application.ApplicantId != default)
                {
                    setClauses.Add("ApplicantId = @ApplicantId");
                    parameters.Add("ApplicantId", application.ApplicantId);
                }

                if (application.NumberOfTags != default)
                {
                    setClauses.Add("NumberOfTags = @NumberOfTags");
                    parameters.Add("NumberOfTags", application.NumberOfTags);
                }

                if (!string.IsNullOrWhiteSpace(application.Purpose))
                {
                    setClauses.Add("Purpose = @Purpose");
                    parameters.Add("Purpose", application.Purpose);
                }

                if (!string.IsNullOrWhiteSpace(application.Status))
                {
                    setClauses.Add("Status = @Status");
                    parameters.Add("Status", application.Status);
                }

                if (application.AppliedBy != default)
                {
                    setClauses.Add("AppliedBy = @AppliedBy");
                    parameters.Add("AppliedBy", application.AppliedBy);
                }

                if (application.AppliedAt != default)
                {
                    setClauses.Add("AppliedAt = @AppliedAt");
                    parameters.Add("AppliedAt", application.AppliedAt);
                }

                if (application.ReviewedBy != default)
                {
                    setClauses.Add("ReviewedBy = @ReviewedBy");
                    parameters.Add("ReviewedBy", application.ReviewedBy);
                }

                if (application.ReviewedAt != default)
                {
                    setClauses.Add("ReviewedAt = @ReviewedAt");
                    parameters.Add("ReviewedAt", application.ReviewedAt);
                }

                if (!string.IsNullOrWhiteSpace(application.Comments))
                {
                    setClauses.Add("Comments = @Comments");
                    parameters.Add("Comments", application.Comments);
                }

                // If no fields are provided to update, return false
                if (setClauses.Count == 0)
                {
                    return false;
                }

                // Add the WHERE clause
                updateQuery += string.Join(", ", setClauses) + " WHERE ApplicationId = @ApplicationId";
                parameters.Add("ApplicationId", application.ApplicationId);

                // Execute the query
                var rowsAffected = await connection.ExecuteAsync(updateQuery, parameters);
                return rowsAffected > 0;
            }
        }

        public async Task<bool> DeleteTagApplicationAsync(int applicationId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var deleteQuery = @"
                DELETE FROM [TagApplications]
                WHERE ApplicationId = @ApplicationId";

                var rowsAffected = await connection.ExecuteAsync(deleteQuery, new { ApplicationId = applicationId });
                return rowsAffected > 0;
            }
        }

        // TagIssuance-related methods

        public async Task<TagIssuance> CreateTagIssuanceAsync(TagIssuance issuance)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                INSERT INTO [TagIssuance] (TagId, ApplicationId, IssuedToType, IssuedToId, IssuedBy, IssueDate,Status, ExpiryDate)
                VALUES (@TagId, @ApplicationId, @IssuedToType, @IssuedToId, @IssuedBy, @IssueDate,@Status, @ExpiryDate);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    issuance.TagId,
                    issuance.ApplicationId,
                    issuance.IssuedToType,
                    issuance.IssuedToId,
                    issuance.IssuedBy,
                    issuance.IssueDate,
                    issuance.Status,
                    issuance.ExpiryDate,
                    
                };

                issuance.IssuanceId = await connection.QuerySingleAsync<int>(insertQuery, parameters);
                return issuance;
            }
        }

        public async Task<TagIssuance?> GetTagIssuanceByIdAsync(int issuanceId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                SELECT *
                FROM [TagIssuance]
                WHERE IssuanceId = @IssuanceId";

                return await connection.QuerySingleOrDefaultAsync<TagIssuance>(query, new { IssuanceId = issuanceId });
            }
        }

        public async Task<PagedResultResponse<TagIssuance>> GetTagIssuancesAsync(int pageNumber, int pageSize,int? issuedToId, string? search = null, string? agent = "no")
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
        SELECT ti.IssuanceId, ti.ApplicationId, ti.IssuedToType, ti.IssuedToId, 
       ti.IssuedBy, ti.IssueDate, ti.ExpiryDate, ti.status AS Status,ti.TagId, t.TagNumber, t.TagType, t.Manufacturer, t.BatchNumber
        FROM [TagIssuance] ti
        INNER JOIN [Tags] t ON ti.TagId = t.TagId
        WHERE 1 = 1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
            AND (ti.IssuedToType LIKE @Search
            OR ti.Status LIKE @Search
            OR t.TagNumber LIKE @Search
            OR t.TagType LIKE @Search
            OR t.Manufacturer LIKE @Search)");
                }


                if (issuedToId.HasValue && issuedToId>0 && agent!="yes")
                {
                    query.Append(@"
            AND (ti.IssuedToId=@IssuedToId)");
                }

                if (issuedToId.HasValue && issuedToId > 0 && agent == "yes")
                {
                    query.Append(@"
            AND (ti.IssuedBy=@IssuedToId)");
                }

                query.Append(@"
        ORDER BY ti.IssueDate DESC
        OFFSET @Skip ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(*)
        FROM [TagIssuance] ti
        INNER JOIN [Tags] t ON ti.TagId = t.TagId
        WHERE 1 = 1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
            AND (ti.IssuedToType LIKE @Search
            OR ti.Status LIKE @Search
            OR t.TagNumber LIKE @Search
            OR t.TagType LIKE @Search
            OR t.Manufacturer LIKE @Search)");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%",
                    IssuedToId=issuedToId
                }))
                {
                    var issuances = multi.Read<TagIssuance, Tag, TagIssuance>((issuance, tag) =>
                    {
                        issuance.Tag = tag; // Assign the tag details to the TagIssuance object
                        return issuance;
                    }, splitOn: "TagNumber").ToList();

                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<TagIssuance>
                    {
                        Items = issuances,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }
        public async Task<bool> UpdateTagIssuanceAsync(TagIssuance issuance)
        {
            if (issuance == null)
            {
                throw new ArgumentNullException(nameof(issuance), "The issuance object cannot be null.");
            }

            if (issuance.IssuanceId <= 0)
            {
                throw new ArgumentException("Invalid IssuanceId.", nameof(issuance.IssuanceId));
            }

            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Dynamically build the UPDATE query based on non-null or non-default values
                var updateFields = new List<string>();
                var parameters = new DynamicParameters();

                if (issuance.TagId != 0) // Assuming 0 is the default value for TagId
                {
                    updateFields.Add("TagId = @TagId");
                    parameters.Add("TagId", issuance.TagId);
                }
                if (issuance.ApplicationId != 0) // Assuming 0 is the default value for ApplicationId
                {
                    updateFields.Add("ApplicationId = @ApplicationId");
                    parameters.Add("ApplicationId", issuance.ApplicationId);
                }
                if (issuance.IssuedToType != null)
                {
                    updateFields.Add("IssuedToType = @IssuedToType");
                    parameters.Add("IssuedToType", issuance.IssuedToType);
                }
                if (issuance.IssuedToId != 0) // Assuming 0 is the default value for IssuedToId
                {
                    updateFields.Add("IssuedToId = @IssuedToId");
                    parameters.Add("IssuedToId", issuance.IssuedToId);
                }
                if (issuance.IssuedBy != null)
                {
                    updateFields.Add("IssuedBy = @IssuedBy");
                    parameters.Add("IssuedBy", issuance.IssuedBy);
                }
                if (issuance.IssueDate != default) // Assuming default(DateTime) is the default value for IssueDate
                {
                    updateFields.Add("IssueDate = @IssueDate");
                    parameters.Add("IssueDate", issuance.IssueDate);
                }
                if (issuance.ExpiryDate != default) // Assuming default(DateTime) is the default value for ExpiryDate
                {
                    updateFields.Add("ExpiryDate = @ExpiryDate");
                    parameters.Add("ExpiryDate", issuance.ExpiryDate);
                }
                if (issuance.Status != null)
                {
                    updateFields.Add("Status = @Status");
                    parameters.Add("Status", issuance.Status);
                }
                if (issuance.RevokedAt != default) // Assuming default(DateTime) is the default value for RevokedAt
                {
                    updateFields.Add("RevokedAt = @RevokedAt");
                    parameters.Add("RevokedAt", issuance.RevokedAt);
                }
                if (issuance.RevokedBy != null)
                {
                    updateFields.Add("RevokedBy = @RevokedBy");
                    parameters.Add("RevokedBy", issuance.RevokedBy);
                }
                if (issuance.UpdatedAt != default) // Assuming default(DateTime) is the default value for UpdatedAt
                {
                    updateFields.Add("UpdatedAt = @UpdatedAt");
                    parameters.Add("UpdatedAt", issuance.UpdatedAt);
                }

                // If no fields are provided to update, return false
                if (updateFields.Count == 0)
                {
                    return false;
                }

                // Build the final UPDATE query
                var updateQuery = $@"
        UPDATE [TagIssuance]
        SET {string.Join(", ", updateFields)}
        WHERE IssuanceId = @IssuanceId";

                parameters.Add("IssuanceId", issuance.IssuanceId);

                var rowsAffected = await connection.ExecuteAsync(updateQuery, parameters);
                return rowsAffected > 0;
            }
        }

        public async Task<bool> DeleteTagIssuanceAsync(int issuanceId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var deleteQuery = @"
                DELETE FROM [TagIssuance]
                WHERE IssuanceId = @IssuanceId";

                var rowsAffected = await connection.ExecuteAsync(deleteQuery, new { IssuanceId = issuanceId });
                return rowsAffected > 0;
            }
        }
    }
}