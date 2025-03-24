// DataAccess/Transportation/Repositories/TransportationRepository.cs
using Application.Transportations.Abstractions;
using Domain.Transportation.Models;
using Domain.Transportation.Requests;
using Dapper;
using System.Data;
using Application.Common.Abstractions;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;
using System.Text;

namespace DataAccess.Transportations.Repositories
{
    public class TransportationRepository : ITransportationRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public TransportationRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<Transportation> CreateTransportationAsync(TransportationCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Insert the new transportation
                var insertQuery = @"
            INSERT INTO [Transportation] (
                PermitId, SourcePremisesId, SourceAddress, DestinationPremisesId, DestinationAddress,
                TransporterId, VehicleDetails, StartDate, EndDate, ItemsDocument, ReasonForTransport,
                Description, Status, CreatedAt, UpdatedAt, UserId,AgentId
            )
            VALUES (
                @PermitId, @SourcePremisesId, @SourceAddress, @DestinationPremisesId, @DestinationAddress,
                @TransporterId, @VehicleDetails, @StartDate, @EndDate, @ItemsDocument, @ReasonForTransport,
                @Description, @Status, @CreatedAt, @UpdatedAt, @UserId,@AgentId
            );
            SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    PermitId = request.PermitId,
                    SourcePremisesId = request.SourcePremisesId,
                    SourceAddress = request.SourceAddress,
                    DestinationPremisesId = request.DestinationPremisesId,
                    DestinationAddress = request.DestinationAddress,
                    TransporterId = request.TransporterId,
                    VehicleDetails = request.VehicleDetails,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    ItemsDocument = request.ItemsDocument,
                    ReasonForTransport = request.ReasonForTransport,
                    Description = request.Description,
                    Status = "Pending", // Default status
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = (DateTime?)null,
                    UserId = request.UserId ,
                    AgentId=request.AgentId
                };

                var transportId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                // Return the created transportation object
                return new Transportation
                {
                    TransportId = transportId,
                    PermitId = request.PermitId,
                    SourcePremisesId = request.SourcePremisesId,
                    SourceAddress = request.SourceAddress,
                    DestinationPremisesId = request.DestinationPremisesId,
                    DestinationAddress = request.DestinationAddress,
                    TransporterId = request.TransporterId,
                    VehicleDetails = request.VehicleDetails,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    ItemsDocument = request.ItemsDocument,
                    ReasonForTransport = request.ReasonForTransport,
                    Description = request.Description,
                    Status = "Pending",
                    CreatedAt = parameters.CreatedAt,
                    UpdatedAt = parameters.UpdatedAt,
                    UserId = request.UserId,
                    AgentId= request.AgentId
                };
            }
        }

        public async Task<PagedResultResponse<Transportation>> GetTransportationsAsync(
       int pageNumber,
       int pageSize,
       string? search = null,
       int? userId = null,
       int? frompremiseId = null,
       string? agent= "no",
        string? vet = "no")
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
            SELECT *
            FROM [Transportation]
            WHERE 1=1");

                // Add search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (SourceAddress LIKE @Search
                OR DestinationAddress LIKE @Search
                OR ReasonForTransport LIKE @Search)");
                }

                if (userId.HasValue)
                {
                    if (agent == "yes")
                    {
                        query.Append(@"
        AND AgentId = @UserId");
                    }
                    else if (vet == "yes")
                    {
                        query.Append(@"
        AND RequestFrom = @UserId");
                    }
                    else
                    {
                        query.Append(@"
        AND UserId = @UserId");
                    }
                }


                // Add frompremiseId filter
                if (frompremiseId.HasValue)
                {
                    query.Append(@"
                AND SourcePremisesId = @FrompremiseId");
                }

                query.Append(@"
            ORDER BY CreatedAt DESC
            OFFSET @Skip ROWS
            FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM [Transportation]
            WHERE 1=1");

                // Add search filter for count query
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (SourceAddress LIKE @Search
                OR DestinationAddress LIKE @Search
                OR ReasonForTransport LIKE @Search)");
                }

                // Add userId filter for count query
                if (userId.HasValue)
                {
                    query.Append(@"
                AND UserId = @UserId");
                }

                // Add frompremiseId filter for count query
                if (frompremiseId.HasValue)
                {
                    query.Append(@"
                AND SourcePremisesId = @FrompremiseId");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%",
                    UserId = userId,
                    FrompremiseId = frompremiseId
                }))
                {
                    var transportations = multi.Read<Transportation>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<Transportation>
                    {
                        Items = transportations,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<Transportation?> GetTransportationByIdAsync(int transportId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT *
                    FROM [Transportation]
                    WHERE TransportId = @TransportId";

                return await connection.QuerySingleOrDefaultAsync<Transportation>(query, new { TransportId = transportId });
            }
        }

        public async Task<Transportation> UpdateTransportationAsync(TransportationUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the transportation exists and belongs to the user
                var checkQuery = @"
            SELECT COUNT(*)
            FROM [Transportation]
            WHERE TransportId = @TransportId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new
                {
                    TransportId = request.TransportId,
                });

                if (exists == 0)
                {
                    throw new UnauthorizedAccessException("You are not authorized to update this record or it does not exist.");
                }

                // Prepare the SQL query to update the transportation
                var updateQuery = @"
            UPDATE [Transportation]
            SET PermitId = COALESCE(@PermitId, PermitId),
                SourcePremisesId = COALESCE(@SourcePremisesId, SourcePremisesId),
                SourceAddress = COALESCE(@SourceAddress, SourceAddress),
                DestinationPremisesId = COALESCE(@DestinationPremisesId, DestinationPremisesId),
                DestinationAddress = COALESCE(@DestinationAddress, DestinationAddress),
                TransporterId = COALESCE(@TransporterId, TransporterId),
                VehicleDetails = COALESCE(@VehicleDetails, VehicleDetails),
                StartDate = COALESCE(@StartDate, StartDate),
                EndDate = COALESCE(@EndDate, EndDate),
                ItemsDocument = COALESCE(@ItemsDocument, ItemsDocument),
                ReasonForTransport = COALESCE(@ReasonForTransport, ReasonForTransport),
                Description = COALESCE(@Description, Description),
                Status = COALESCE(@Status, Status),
                VetId = COALESCE(@VetId, VetId),
                UpdatedAt = @UpdatedAt
            WHERE TransportId = @TransportId"; // Ensure UserId is respected

                // Prepare the parameters
                var parameters = new
                {
                    TransportId = request.TransportId,
                    PermitId = request.PermitId,
                    SourcePremisesId = request.SourcePremisesId,
                    SourceAddress = request.SourceAddress,
                    DestinationPremisesId = request.DestinationPremisesId,
                    DestinationAddress = request.DestinationAddress,
                    TransporterId = request.TransporterId,
                    VehicleDetails = request.VehicleDetails,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    ItemsDocument = request.ItemsDocument,
                    ReasonForTransport = request.ReasonForTransport,
                    Description = request.Description,
                    Status = request.Status,
                    VetId=request.VetId,
                    UpdatedAt = DateTime.UtcNow,
                   
                };

                // Execute the update
                await connection.ExecuteAsync(updateQuery, parameters);

                // Retrieve the updated transportation details
                var query = @"
            SELECT *
            FROM [Transportation]
            WHERE TransportId = @TransportId";

                var transportation = await connection.QuerySingleOrDefaultAsync<Transportation>(query, new
                {
                    TransportId = request.TransportId,
                });

                if (transportation == null)
                {
                    throw new ItemDoesNotExistException(request.TransportId);
                }

                return transportation;
            }
        }

        public async Task<bool> DeleteTransportationAsync(int transportId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the transportation exists
                var checkQuery = @"
                    SELECT COUNT(*)
                    FROM [Transportation]
                    WHERE TransportId = @TransportId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { TransportId = transportId });

                if (exists == 0)
                {
                    return false;
                }

                // Delete the transportation
                var deleteQuery = @"
                    DELETE FROM [Transportation]
                    WHERE TransportId = @TransportId";

                await connection.ExecuteAsync(deleteQuery, new { TransportId = transportId });

                return true;
            }
        }

        public async Task<int> CountTransportationsAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = "SELECT COUNT(*) FROM [Transportation]";

                return await connection.ExecuteScalarAsync<int>(query);
            }
        }
    }
}