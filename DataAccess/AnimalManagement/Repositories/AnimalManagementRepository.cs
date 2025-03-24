using Application.Common.Abstractions;
using Dapper;
using DataAccess.Common.Exceptions;
using Domain.Common.Responses;
using Domain.Core.Models;
using Domain.AnimalManagement.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.AnimalManagement.Abstractions;

namespace DataAccess.AnimalManagement.Repositories
{
    public class AnimalManagementRepository : IAnimalManagementRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public AnimalManagementRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<Animal> CreateAnimalAsync(AnimalCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the OwnerId exists in the Users table
                var ownerExistsQuery = @"
                SELECT COUNT(*)
                FROM [Users]
                WHERE UserId = @OwnerId";

                var ownerExists = await connection.QuerySingleAsync<int>(ownerExistsQuery, new
                {
                    OwnerId = request.OwnerId
                });

                if (ownerExists == 0)
                {
                    throw new ItemDoesNotExistException($"The specified Owner ID: {request.OwnerId} does not exist.");
                }

                // Check if the PremisesId exists in the Premises table
                var premisesExistsQuery = @"
                SELECT COUNT(*)
                FROM [Premises]
                WHERE PremisesId = @PremisesId";

                var premisesExists = await connection.QuerySingleAsync<int>(premisesExistsQuery, new
                {
                    PremisesId = request.PremisesId
                });

                if (premisesExists == 0)
                {
                    throw new ItemDoesNotExistException($"The specified Premises ID: {request.PremisesId} does not exist.");
                }

                // Check if an animal with the same IdentificationMark already exists
                var animalExistsQuery = @"
                SELECT COUNT(*)
                FROM [Animals]
                WHERE IdentificationMark = @IdentificationMark";

                var existingCount = await connection.QuerySingleAsync<int>(animalExistsQuery, new
                {
                    IdentificationMark = request.IdentificationMark
                });

                if (existingCount > 0)
                {
                    throw new ItemAlreadyExistsException("An animal with this identification mark already exists.");
                }

                // Insert new Animal record
                var insertQuery = @"
                INSERT INTO [Animals] (
                    Species, Breed, BirthDate, Color, Description, Name, HealthStatus, IdentificationMark, OwnerId, PremisesId, Status, CreatedAt, AnimalImage
                )
                VALUES (
                    @Species, @Breed, @BirthDate, @Color, @Description, @Name, @HealthStatus, @IdentificationMark, @OwnerId, @PremisesId, @Status, @CreatedAt, @AnimalImage
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    Species = request.Species,
                    Breed = request.Breed,
                    BirthDate = request.BirthDate,
                    Color = request.Color,
                    Description = request.Description,
                    Name = request.Name,
                    HealthStatus = request.HealthStatus,
                    IdentificationMark = request.IdentificationMark,
                    OwnerId = request.OwnerId,
                    PremisesId = request.PremisesId,
                    Status = request.Status,
                    CreatedAt = DateTime.UtcNow,
                    AnimalImage = request.AnimalImage
                };

                var animalId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                return new Animal
                {
                    AnimalId = animalId,
                    Species = request.Species,
                    Breed = request.Breed,
                    BirthDate = request.BirthDate,
                    Color = request.Color,
                    Description = request.Description,
                    Name = request.Name,
                    HealthStatus = request.HealthStatus,
                    IdentificationMark = request.IdentificationMark,
                    OwnerId = request.OwnerId,
                    PremisesId = request.PremisesId,
                    Status = request.Status,
                    CreatedAt = parameters.CreatedAt,
                    AnimalImage = request.AnimalImage
                };
            }
        }

        public async Task<PagedResultResponse<Animal>> GetAnimalsByPremisesAsync(int premisesId, int pageNumber, int pageSize, string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
                SELECT *
                FROM [Animals]
                WHERE PremisesId = @PremisesId");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (Species LIKE @Search
                OR Breed LIKE @Search
                OR IdentificationMark LIKE @Search)");
                }

                query.Append(@"
                ORDER BY IdentificationMark
                OFFSET @Skip ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*)
                FROM [Animals]
                WHERE PremisesId = @PremisesId");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (Species LIKE @Search
                OR Breed LIKE @Search
                OR IdentificationMark LIKE @Search)");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    PremisesId = premisesId,
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%"
                }))
                {
                    var animals = multi.Read<Animal>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<Animal>
                    {
                        Items = animals,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<Animal?> GetAnimalByIdAsync(int animalId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                SELECT *
                FROM [Animals]
                WHERE AnimalId = @AnimalId";

                return await connection.QuerySingleOrDefaultAsync<Animal>(query, new { AnimalId = animalId });
            }
        }

        public async Task<Animal?> UpdateAnimalAsync(AnimalUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the animal exists
                var checkQuery = @"
                SELECT COUNT(*)
                FROM [Animals]
                WHERE AnimalId = @AnimalId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { AnimalId = request.AnimalId });

                if (exists == 0)
                {
                    throw new ItemDoesNotExistException("Animal does not exist.");
                }

                // Dynamically build the update query based on non-null values in request
                var updateQuery = new StringBuilder("UPDATE [Animals] SET ");
                var parameters = new DynamicParameters();

                if (!string.IsNullOrEmpty(request.Species))
                {
                    updateQuery.Append("Species = @Species, ");
                    parameters.Add("Species", request.Species);
                }
                if (!string.IsNullOrEmpty(request.Breed))
                {
                    updateQuery.Append("Breed = @Breed, ");
                    parameters.Add("Breed", request.Breed);
                }
                if (!string.IsNullOrEmpty(request.BirthDate))
                {
                    updateQuery.Append("BirthDate = @BirthDate, ");
                    parameters.Add("BirthDate", request.BirthDate);
                }
                if (!string.IsNullOrEmpty(request.Color))
                {
                    updateQuery.Append("Color = @Color, ");
                    parameters.Add("Color", request.Color);
                }
                if (!string.IsNullOrEmpty(request.Description))
                {
                    updateQuery.Append("Description = @Description, ");
                    parameters.Add("Description", request.Description);
                }
                if (!string.IsNullOrEmpty(request.Name))
                {
                    updateQuery.Append("Name = @Name, ");
                    parameters.Add("Name", request.Name);
                }
                if (!string.IsNullOrEmpty(request.HealthStatus))
                {
                    updateQuery.Append("HealthStatus = @HealthStatus, ");
                    parameters.Add("HealthStatus", request.HealthStatus);
                }
                if (!string.IsNullOrEmpty(request.IdentificationMark))
                {
                    updateQuery.Append("IdentificationMark = @IdentificationMark, ");
                    parameters.Add("IdentificationMark", request.IdentificationMark);
                }
                if (request.OwnerId != 0)
                {
                    updateQuery.Append("OwnerId = @OwnerId, ");
                    parameters.Add("OwnerId", request.OwnerId);
                }
                if (request.PremisesId != 0)
                {
                    updateQuery.Append("PremisesId = @PremisesId, ");
                    parameters.Add("PremisesId", request.PremisesId);
                }
                if (!string.IsNullOrEmpty(request.Status))
                {
                    updateQuery.Append("Status = @Status, ");
                    parameters.Add("Status", request.Status);
                }
                if (request.AnimalImage != null)
                {
                    updateQuery.Append("AnimalImage = @AnimalImage, ");
                    parameters.Add("AnimalImage", request.AnimalImage);
                }

                // Add the UpdatedAt and WHERE clause
                updateQuery.Append("UpdatedAt = @UpdatedAt WHERE AnimalId = @AnimalId");
                parameters.Add("UpdatedAt", DateTime.UtcNow);
                parameters.Add("AnimalId", request.AnimalId);

                // Execute the dynamically generated update query
                await connection.ExecuteAsync(updateQuery.ToString(), parameters);

                // Retrieve the updated animal record
                var animal = await GetAnimalByIdAsync(request.AnimalId);

                return animal;
            }
        }

        public async Task<bool> DeleteAnimalAsync(int animalId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the animal exists
                var checkQuery = @"
                SELECT COUNT(*)
                FROM [Animals]
                WHERE AnimalId = @AnimalId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { AnimalId = animalId });

                if (exists == 0)
                {
                    throw new ItemDoesNotExistException("Animal does not exist.");
                }

                // Delete the animal record
                var deleteQuery = @"
                DELETE FROM [Animals]
                WHERE AnimalId = @AnimalId";

                var rowsAffected = await connection.ExecuteAsync(deleteQuery, new { AnimalId = animalId });

                return rowsAffected > 0;
            }
        }

        public async Task<int> CountAnimalsAsync(int? ownerId = null, int? premisesId = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = new StringBuilder(@"
                SELECT COUNT(*)
                FROM [Animals]
                WHERE 1 = 1");

                if (ownerId.HasValue)
                {
                    query.Append(" AND OwnerId = @OwnerId");
                }

                if (premisesId.HasValue)
                {
                    query.Append(" AND PremisesId = @PremisesId");
                }

                var totalRecords = await connection.ExecuteScalarAsync<int>(query.ToString(), new
                {
                    OwnerId = ownerId,
                    PremisesId = premisesId
                });

                return totalRecords;
            }
        }

        public async Task<HealthRecord> CreateHealthRecordAsync(HealthRecordCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the animal exists
                var animalExistsQuery = @"
                SELECT COUNT(*)
                FROM [Animals]
                WHERE AnimalId = @AnimalId";

                var animalExists = await connection.QuerySingleAsync<int>(animalExistsQuery, new
                {
                    AnimalId = request.animalId
                });

                if (animalExists == 0)
                {
                    throw new ItemDoesNotExistException($"The specified Animal ID: {request.animalId} does not exist.");
                }

                // Insert new HealthRecord
                var insertQuery = @"
                INSERT INTO [HealthRecord] (
                    AnimalId, UserId, DateOfVisit, Diagnosis, Treatment, FollowUpDate, CreatedAt, UpdatedAt
                )
                VALUES (
                    @AnimalId, @UserId, @DateOfVisit, @Diagnosis, @Treatment, @FollowUpDate, @CreatedAt, @UpdatedAt
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    AnimalId = request.animalId,
                    UserId = request.UserId,
                    DateOfVisit = request.DateOfVisit,
                    Diagnosis = request.Diagnosis,
                    Treatment = request.Treatment,
                    FollowUpDate = request.FollowUpDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var healthRecordId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                return new HealthRecord
                {
                    HealthRecordId = healthRecordId,
                    AnimalId = request.animalId,
                    UserId = request.UserId,
                    DateOfVisit = request.DateOfVisit,
                    Diagnosis = request.Diagnosis,
                    Treatment = request.Treatment,
                    FollowUpDate = request.FollowUpDate,
                    CreatedAt = parameters.CreatedAt,
                    UpdatedAt = parameters.UpdatedAt
                };
            }
        }

        public async Task<PagedResultResponse<HealthRecord>> GetHealthRecordsByAnimalIdAsync(int animalId, int pageNumber, int pageSize, string? search = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
                SELECT *
                FROM [HealthRecord]
                WHERE AnimalId = @AnimalId");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (Diagnosis LIKE @Search
                OR Treatment LIKE @Search)");
                }

                query.Append(@"
                ORDER BY DateOfVisit
                OFFSET @Skip ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*)
                FROM [HealthRecord]
                WHERE AnimalId = @AnimalId");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (Diagnosis LIKE @Search
                OR Treatment LIKE @Search)");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    AnimalId = animalId,
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%"
                }))
                {
                    var healthRecords = multi.Read<HealthRecord>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<HealthRecord>
                    {
                        Items = healthRecords,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<HealthRecord?> GetHealthRecordByIdAsync(int healthRecordId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                SELECT *
                FROM [HealthRecord]
                WHERE HealthRecordId = @HealthRecordId";

                return await connection.QueryFirstOrDefaultAsync<HealthRecord>(query, new { HealthRecordId = healthRecordId });
            }
        }

        public async Task<bool> UpdateHealthRecordAsync(int healthRecordId, UpdateHealthRecordRequest updateRequest)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = new StringBuilder("UPDATE [HealthRecord] SET ");
                var parameters = new DynamicParameters();
                parameters.Add("HealthRecordId", healthRecordId);

                if (updateRequest.animalId.HasValue)
                {
                    query.Append("AnimalId = @AnimalId, ");
                    parameters.Add("AnimalId", updateRequest.animalId);
                }

                if (!string.IsNullOrWhiteSpace(updateRequest.Diagnosis))
                {
                    query.Append("Diagnosis = @Diagnosis, ");
                    parameters.Add("Diagnosis", updateRequest.Diagnosis);
                }

                if (!string.IsNullOrWhiteSpace(updateRequest.Treatment))
                {
                    query.Append("Treatment = @Treatment, ");
                    parameters.Add("Treatment", updateRequest.Treatment);
                }

                if (updateRequest.FollowUpDate.HasValue)
                {
                    query.Append("FollowUpDate = @FollowUpDate, ");
                    parameters.Add("FollowUpDate", updateRequest.FollowUpDate);
                }

                if (updateRequest.DateOfVisit.HasValue)
                {
                    query.Append("DateOfVisit = @DateOfVisit, ");
                    parameters.Add("DateOfVisit", updateRequest.DateOfVisit);
                }

                // Add the UpdatedAt and WHERE clause
                query.Append("UpdatedAt = @UpdatedAt ");
                parameters.Add("UpdatedAt", DateTime.UtcNow);

                query.Append("WHERE HealthRecordId = @HealthRecordId");

                var result = await connection.ExecuteAsync(query.ToString(), parameters);

                return result > 0;
            }
        }

        public async Task<int> CountHealthRecordsAsync(int? userId = null, int? animalId = null, int? premisesId = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = new StringBuilder(@"
                SELECT COUNT(*)
                FROM [HealthRecord] hr
                LEFT JOIN [Animals] a ON hr.AnimalId = a.AnimalId
                LEFT JOIN [Premises] p ON a.PremisesId = p.PremisesId
                WHERE 1 = 1");

                // Add conditions for userId, animalId, and premisesId
                if (userId.HasValue && userId.Value != 0)
                {
                    query.Append(" AND hr.UserId = @UserId");
                }

                if (animalId.HasValue && animalId.Value != 0)
                {
                    query.Append(" AND hr.AnimalId = @AnimalId");
                }

                if (premisesId.HasValue && premisesId.Value != 0)
                {
                    query.Append(" AND p.PremisesId = @PremisesId");
                }

                // Execute the count query with the appropriate parameters
                var totalRecords = await connection.ExecuteScalarAsync<int>(query.ToString(), new
                {
                    UserId = userId,
                    AnimalId = animalId,
                    PremisesId = premisesId
                });

                return totalRecords;
            }
        }

        public async Task<PagedResultResponse<HealthRecord>> GetAllHealthRecordsAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            int? userId = null,
            int? animalId = null,
            int? premisesId = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
                SELECT hr.*
                FROM [HealthRecord] hr
                LEFT JOIN [Animals] a ON hr.AnimalId = a.AnimalId
                LEFT JOIN [Premises] p ON a.PremisesId = p.PremisesId
                WHERE 1 = 1");

                // Optional filters
                if (userId.HasValue && userId.Value != 0)
                {
                    query.Append(" AND hr.UserId = @UserId");
                }

                if (animalId.HasValue && animalId.Value != 0)
                {
                    query.Append(" AND hr.AnimalId = @AnimalId");
                }

                if (premisesId.HasValue && premisesId.Value != 0)
                {
                    query.Append(" AND p.PremisesId = @PremisesId");
                }

                // Optional search in specific fields
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (hr.Diagnosis LIKE @Search 
                OR hr.Treatment LIKE @Search)");
                }

                query.Append(@"
                ORDER BY hr.CreatedAt DESC
                OFFSET @Skip ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*)
                FROM [HealthRecord] hr
                LEFT JOIN [Animals] a ON hr.AnimalId = a.AnimalId
                LEFT JOIN [Premises] p ON a.PremisesId = p.PremisesId
                WHERE 1 = 1");

                // Reapply filters for count query
                if (userId.HasValue && userId.Value != 0)
                {
                    query.Append(" AND hr.UserId = @UserId");
                }

                if (animalId.HasValue && animalId.Value != 0)
                {
                    query.Append(" AND hr.AnimalId = @AnimalId");
                }

                if (premisesId.HasValue && premisesId.Value != 0)
                {
                    query.Append(" AND p.PremisesId = @PremisesId");
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (hr.Diagnosis LIKE @Search 
                OR hr.Treatment LIKE @Search)");
                }

                // Execute the query with parameters
                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    UserId = userId,
                    AnimalId = animalId,
                    PremisesId = premisesId,
                    Search = $"%{search}%",
                    Skip = skip,
                    PageSize = pageSize
                }))
                {
                    var healthRecords = multi.Read<HealthRecord>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<HealthRecord>
                    {
                        Items = healthRecords,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<bool> DeleteHealthRecordAsync(int healthRecordId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Check if the health record exists
                var checkQuery = @"
                SELECT COUNT(*)
                FROM [HealthRecord]
                WHERE HealthRecordId = @HealthRecordId";

                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { HealthRecordId = healthRecordId });

                if (exists == 0)
                {
                    throw new ItemDoesNotExistException($"Health record with ID {healthRecordId} does not exist.");
                }

                // Delete the health record
                var deleteQuery = @"
                DELETE FROM [HealthRecord]
                WHERE HealthRecordId = @HealthRecordId";

                var rowsAffected = await connection.ExecuteAsync(deleteQuery, new { HealthRecordId = healthRecordId });

                return rowsAffected > 0;
            }
        }
    }
}