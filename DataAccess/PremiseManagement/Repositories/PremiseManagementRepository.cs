using Domain.Core.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using Application.Common.Abstractions;
using System.Text;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;
using Application.PremiseManagement.Abstractions;
using Domain.PremiseManagement.Requests;

namespace DataAccess.PremiseManagement.Repositories
{
    public class PremiseManagementRepository : Application.PremiseManagement.Abstractions.IPremiseManagementRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public PremiseManagementRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<Premise> CreatePremise(PremiseCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if a premise with the same name and owner already exists
                var checkQuery = @"
            SELECT COUNT(*)
            FROM [Premises]
            WHERE Name = @Name AND OwnerId = @OwnerId";

                var existingCount = await connection.QuerySingleAsync<int>(checkQuery, new
                {
                    request.Name,
                    request.OwnerId
                });

                if (existingCount > 0)
                {
                    throw new ItemAlreadyExistsException("Premise name already exists for this owner.");
                }

                // Insert the new premise
                var insertQuery = @"
            INSERT INTO [Premises] (Name, Coordinates, Type, OwnerId, Status, PremiseImage, Province, DistrictConstituency, Ward, VillageLocalityAddress, Chiefdom, Headman, VeterinaryCamp, CampOfficerNames, VeterinaryOfficerNames, PhysicalPostalAddress, HandlingFacility, AlternativeAddresses, RegisteredAt,AgentId, UpdatedAt)
            VALUES (@Name, @Coordinates, @Type, @OwnerId, @Status, @PremiseImage, @Province, @DistrictConstituency, @Ward, @VillageLocalityAddress, @Chiefdom, @Headman, @VeterinaryCamp, @CampOfficerNames, @VeterinaryOfficerNames, @PhysicalPostalAddress, @HandlingFacility, @AlternativeAddresses, @RegisteredAt,@AgentId, @UpdatedAt);
            SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    request.Name,
                    request.Coordinates,
                    request.Type,
                    request.OwnerId,
                    request.Status,
                    request.PremiseImage,
                    request.Province,
                    request.DistrictConstituency,
                    request.Ward,
                    request.VillageLocalityAddress,
                    request.Chiefdom,
                    request.Headman,
                    request.VeterinaryCamp,
                    request.CampOfficerNames,
                    request.VeterinaryOfficerNames,
                    request.PhysicalPostalAddress,
                    request.HandlingFacility,
                    request.AlternativeAddresses,
                    RegisteredAt = DateTime.UtcNow,
                    request.AgentId,
                    UpdatedAt = (DateTime?)null
                };

                var premiseId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                // Return the created premise object
                return new Premise
                {
                    PremisesId = premiseId,
                    Name = request.Name,
                    Coordinates = request.Coordinates,
                    Type = request.Type,
                    OwnerId = request.OwnerId,
                    Status = request.Status,
                    PremiseImage = request.PremiseImage,
                    Province = request.Province,
                    DistrictConstituency = request.DistrictConstituency,
                    Ward = request.Ward,
                    VillageLocalityAddress = request.VillageLocalityAddress,
                    Chiefdom = request.Chiefdom,
                    Headman = request.Headman,
                    VeterinaryCamp = request.VeterinaryCamp,
                    CampOfficerNames = request.CampOfficerNames,
                    VeterinaryOfficerNames = request.VeterinaryOfficerNames,
                    PhysicalPostalAddress = request.PhysicalPostalAddress,
                    HandlingFacility = request.HandlingFacility,
                    AlternativeAddresses = request.AlternativeAddresses,
                    RegisteredAt = DateTime.UtcNow,
                    UpdatedAt = null
                };
            }
        }

        public async Task<IEnumerable<Premise>> GetPremisesByUserId(int userId,string agent)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                string query;
                if (agent == "yes")
                {
                    query = @"
SELECT *
FROM [Premises]
WHERE AgentId = @OwnerId
ORDER BY Name;";
                }
                else
                {
                    query = @"
SELECT *
FROM [Premises]
WHERE OwnerId = @OwnerId
ORDER BY Name;";
                }

                var premises = await connection.QueryAsync<Premise>(query, new { OwnerId = userId });

                return premises;
            }
        }

        public async Task<PagedResultResponse<Premise>> GetAllPremisesAsync(int pageNumber, int pageSize, string? search = null, int? ownerId = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
            SELECT *
            FROM [Premises]
            WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (Name LIKE @Search
                OR Province LIKE @Search
                OR DistrictConstituency LIKE @Search
                OR Ward LIKE @Search
                OR VillageLocalityAddress LIKE @Search
                OR Chiefdom LIKE @Search
                OR Headman LIKE @Search
                OR VeterinaryCamp LIKE @Search
                OR CampOfficerNames LIKE @Search
                OR VeterinaryOfficerNames LIKE @Search
                OR PhysicalPostalAddress LIKE @Search
                OR HandlingFacility LIKE @Search
                OR AlternativeAddresses LIKE @Search)");
                }

                if (ownerId.HasValue)
                {
                    query.Append(@"
                AND OwnerId = @OwnerId");
                }

                query.Append(@"
            ORDER BY Name
            OFFSET @Skip ROWS
            FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM [Premises]
            WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (Name LIKE @Search
                OR Province LIKE @Search
                OR DistrictConstituency LIKE @Search
                OR Ward LIKE @Search
                OR VillageLocalityAddress LIKE @Search
                OR Chiefdom LIKE @Search
                OR Headman LIKE @Search
                OR VeterinaryCamp LIKE @Search
                OR CampOfficerNames LIKE @Search
                OR VeterinaryOfficerNames LIKE @Search
                OR PhysicalPostalAddress LIKE @Search
                OR HandlingFacility LIKE @Search
                OR AlternativeAddresses LIKE @Search)");
                }

                if (ownerId.HasValue)
                {
                    query.Append(@"
                AND OwnerId = @OwnerId");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%",
                    OwnerId = ownerId
                }))
                {
                    var premises = multi.Read<Premise>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<Premise>
                    {
                        Items = premises,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<Premise?> GetPremiseByIdAsync(int PremisesId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
            SELECT *
            FROM [Premises]
            WHERE PremisesId = @PremisesId";

                return await connection.QuerySingleOrDefaultAsync<Premise>(query, new { PremisesId = PremisesId });
            }
        }

        public async Task<int> CountPremisesAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = "SELECT COUNT(*) FROM [Premises]";

                // Execute the query and return the count of Premises
                return await connection.ExecuteScalarAsync<int>(query);
            }
        }

        public async Task<Premise> UpdatePremise(PremiseUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the Premise exists
                var checkQuery = @"
            SELECT COUNT(*)
            FROM [Premises]
            WHERE PremisesId = @PremisesId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { request.PremisesId });

                if (exists == 0)
                {
                    throw new ItemDoesNotExistException(request.PremisesId);
                }

                // Prepare the SQL query to update the Premise using COALESCE
                var updateQuery = @"
            UPDATE [Premises]
            SET Name = COALESCE(@Name, Name),
                Coordinates = COALESCE(@Coordinates, Coordinates),
                Type = COALESCE(@Type, Type),
                OwnerId = COALESCE(@OwnerId, OwnerId),
                Status = COALESCE(@Status, Status),
                PremiseImage = COALESCE(@PremiseImage, PremiseImage),
                Province = COALESCE(@Province, Province),
                DistrictConstituency = COALESCE(@DistrictConstituency, DistrictConstituency),
                Ward = COALESCE(@Ward, Ward),
                VillageLocalityAddress = COALESCE(@VillageLocalityAddress, VillageLocalityAddress),
                Chiefdom = COALESCE(@Chiefdom, Chiefdom),
                Headman = COALESCE(@Headman, Headman),
                VeterinaryCamp = COALESCE(@VeterinaryCamp, VeterinaryCamp),
                CampOfficerNames = COALESCE(@CampOfficerNames, CampOfficerNames),
                VeterinaryOfficerNames = COALESCE(@VeterinaryOfficerNames, VeterinaryOfficerNames),
                PhysicalPostalAddress = COALESCE(@PhysicalPostalAddress, PhysicalPostalAddress),
                HandlingFacility = COALESCE(@HandlingFacility, HandlingFacility),
                AlternativeAddresses = COALESCE(@AlternativeAddresses, AlternativeAddresses),
                UpdatedAt = @UpdatedAt
            WHERE PremisesId = @PremisesId";

                // Prepare the parameters
                var parameters = new
                {
                    request.PremisesId,
                    request.Name,
                    request.Coordinates,
                    request.Type,
                    request.OwnerId,
                    request.Status,
                    request.PremiseImage,
                    request.Province,
                    request.DistrictConstituency,
                    request.Ward,
                    request.VillageLocalityAddress,
                    request.Chiefdom,
                    request.Headman,
                    request.VeterinaryCamp,
                    request.CampOfficerNames,
                    request.VeterinaryOfficerNames,
                    request.PhysicalPostalAddress,
                    request.HandlingFacility,
                    request.AlternativeAddresses,
                    UpdatedAt = DateTime.UtcNow
                };

                // Execute the update
                await connection.ExecuteAsync(updateQuery, parameters);

                // Retrieve the updated Premise details
                var query = @"
            SELECT *
            FROM [Premises]
            WHERE PremisesId = @PremisesId";

                var premise = await connection.QuerySingleOrDefaultAsync<Premise>(query, new { request.PremisesId });

                if (premise == null)
                {
                    throw new ItemDoesNotExistException(request.PremisesId);
                }

                return premise;
            }
        }
        public async Task<bool> DeletePremise(int PremisesId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var checkQuery = @"
            SELECT COUNT(*)
            FROM [Premises]
            WHERE PremisesId = @PremisesId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { PremisesId = PremisesId });

                if (exists == 0)
                {
                    return false;
                }

                var deleteQuery = @"
            DELETE FROM [Premises]
            WHERE PremisesId = @PremisesId";

                await connection.ExecuteAsync(deleteQuery, new { PremisesId = PremisesId });

                return true;
            }
        }
    }
}