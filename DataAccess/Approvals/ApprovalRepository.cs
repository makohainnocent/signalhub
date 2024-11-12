
using Application.Common.Abstractions;
using Dapper;
using Domain.Approvals.Requests;
using Domain.Core.Models;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Approvals.Abstractions;

namespace DataAccess.Approvals.Repositories
{
    public class ApprovalRepository : IApprovalsRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public ApprovalRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<Approval> CreateApprovalAsync(CreateApprovalRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                INSERT INTO [Approval] (UserId, FarmId, LivestockIds, ApprovalDocument, CreatedAt, UpdatedAt, Notes)
                VALUES (@UserId, @FarmId, @LivestockIds, @ApprovalDocument, @CreatedAt, @UpdatedAt, @Notes);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    request.UserId,
                    request.FarmId,
                    request.LivestockIds,
                    request.ApprovalDocument,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    request.Notes
                };

                var approvalId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                return new Approval
                {
                    ApprovalId = approvalId,
                    UserId = request.UserId,
                    FarmId = request.FarmId,
                    LivestockIds = request.LivestockIds,
                    ApprovalDocument = request.ApprovalDocument,
                    CreatedAt = parameters.CreatedAt,
                    UpdatedAt = parameters.UpdatedAt,
                    Notes = request.Notes
                };
            }
        }

        public async Task<Approval?> GetApprovalByIdAsync(int approvalId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                SELECT *
                FROM [Approval]
                WHERE ApprovalId = @ApprovalId";

                return await connection.QuerySingleOrDefaultAsync<Approval>(query, new { ApprovalId = approvalId });
            }
        }

        public async Task<PagedResultResponse<Approval>> GetAllApprovalsAsync(int pageNumber, int pageSize, string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
                SELECT *
                FROM [Approval]
                WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(" AND (LivestockIds LIKE @Search OR Notes LIKE @Search)");
                }

                query.Append(@"
                ORDER BY CreatedAt DESC
                OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*)
                FROM [Approval]
                WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(" AND (LivestockIds LIKE @Search OR Notes LIKE @Search)");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Search = $"%{search}%",
                    Skip = skip,
                    PageSize = pageSize
                }))
                {
                    var approvals = multi.Read<Approval>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<Approval>
                    {
                        Items = approvals,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<Approval> UpdateApprovalAsync(Approval approval)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var updateQuery = @"
                UPDATE [Approval]
                SET UserId = @UserId,
                    FarmId = @FarmId,
                    LivestockIds = @LivestockIds,
                    ApprovalDocument = @ApprovalDocument,
                    UpdatedAt = @UpdatedAt,
                    Notes = @Notes
                WHERE ApprovalId = @ApprovalId";

                var parameters = new
                {
                    approval.ApprovalId,
                    approval.UserId,
                    approval.FarmId,
                    approval.LivestockIds,
                    approval.ApprovalDocument,
                    UpdatedAt = DateTime.UtcNow,
                    approval.Notes
                };

                await connection.ExecuteAsync(updateQuery, parameters);
                approval.UpdatedAt = parameters.UpdatedAt;
                return approval;
            }
        }

        public async Task<bool> DeleteApprovalAsync(int approvalId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var deleteQuery = @"
                DELETE FROM [Approval]
                WHERE ApprovalId = @ApprovalId";

                var affectedRows = await connection.ExecuteAsync(deleteQuery, new { ApprovalId = approvalId });

                return affectedRows > 0;
            }
        }
    }
}
