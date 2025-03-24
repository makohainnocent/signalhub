
using Application.Common.Abstractions;
using Dapper;
using Domain.Vaccinations.Requests;
using Domain.Core.Models;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Core.Models.Domain.Core.Models;
using Application.Vaccinations.Abstractions;

namespace DataAccess.Vaccinations.Repositories
{
    public class VaccinationRepository : IVaccinationRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public VaccinationRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<Vaccination> CreateVaccinationAsync(CreateVaccinationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                INSERT INTO [Vaccination] (LivestockId, UserId, FarmId, VaccineName, Manufacturer, DateAdministered, NextDoseDueDate, Dosage, AdministeredBy, IsCompleted, CreatedAt, UpdatedAt, Notes)
                VALUES (@LivestockId, @UserId, @FarmId, @VaccineName, @Manufacturer, @DateAdministered, @NextDoseDueDate, @Dosage, @AdministeredBy, @IsCompleted, @CreatedAt, @UpdatedAt, @Notes);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    LivestockId = request.LivestockId,
                    UserId = request.UserId,
                    FarmId = request.FarmId,
                    VaccineName = request.VaccineName,
                    Manufacturer = request.Manufacturer,
                    DateAdministered = request.DateAdministered,
                    NextDoseDueDate = request.NextDoseDueDate,
                    Dosage = request.Dosage,
                    AdministeredBy = request.AdministeredBy,
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Notes = request.Notes
                };

                var vaccinationId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                return new Vaccination
                {
                    VaccinationId = vaccinationId,
                    LivestockId = request.LivestockId,
                    UserId = request.UserId,
                    FarmId = request.FarmId,
                    VaccineName = request.VaccineName,
                    Manufacturer = request.Manufacturer,
                    DateAdministered = request.DateAdministered,
                    NextDoseDueDate = request.NextDoseDueDate,
                    Dosage = request.Dosage,
                    AdministeredBy = request.AdministeredBy,
                    IsCompleted = false,
                    CreatedAt = parameters.CreatedAt,
                    UpdatedAt = parameters.UpdatedAt,
                    Notes = request.Notes
                };
            }
        }

        public async Task<Vaccination?> GetVaccinationByIdAsync(int vaccinationId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                SELECT *
                FROM [Vaccination]
                WHERE VaccinationId = @VaccinationId";

                return await connection.QuerySingleOrDefaultAsync<Vaccination>(query, new { VaccinationId = vaccinationId });
            }
        }

        public async Task<PagedResultResponse<Vaccination>> GetAllVaccinationsAsync(int pageNumber, int pageSize,int? farmId = null, int? userId = null, string? search = null,int?livestockId=null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
        SELECT *
        FROM [Vaccination]
        WHERE 1=1");

              

                if (farmId.HasValue && userId !=0)
                {
                    query.Append(" AND FarmId = @FarmId");
                }

                if (userId.HasValue && userId != 0)
                {
                    query.Append(" AND UserId = @UserId");
                }

                if (livestockId.HasValue && livestockId != 0)
                {
                    query.Append(" AND LivestockId = @LivestockId");
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    // Search in both VaccineName and Manufacturer fields
                    query.Append(" AND (VaccineName LIKE @Search OR Manufacturer LIKE @Search)");
                }

                query.Append(@"
        ORDER BY DateAdministered DESC
        OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(*)
        FROM [Vaccination]
        WHERE 1=1");



                if (farmId.HasValue && farmId != 0)
                {
                    query.Append(" AND FarmId = @FarmId");
                }

                if (livestockId.HasValue && livestockId != 0)
                {
                    query.Append(" AND LivestockId = @LivestockId");
                }

                if (userId.HasValue && userId != 0)
                {
                    query.Append(" AND UserId = @UserId");
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(" AND (VaccineName LIKE @Search OR Manufacturer LIKE @Search)");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    FarmId = farmId,
                    UserId = userId,
                    LivestockId = livestockId,
                    Search = $"%{search}%",
                    Skip = skip,
                    PageSize = pageSize
                }))
                {
                    var vaccinations = multi.Read<Vaccination>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<Vaccination>
                    {
                        Items = vaccinations,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        public async Task<Vaccination> UpdateVaccinationAsync(Vaccination vaccination)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var updateQuery = @"
                UPDATE [Vaccination]
                SET LivestockId = @LivestockId,
                    UserId = @UserId,
                    FarmId = @FarmId,
                    VaccineName = @VaccineName,
                    Manufacturer = @Manufacturer,
                    DateAdministered = @DateAdministered,
                    NextDoseDueDate = @NextDoseDueDate,
                    Dosage = @Dosage,
                    AdministeredBy = @AdministeredBy,
                    IsCompleted = @IsCompleted,
                    UpdatedAt = @UpdatedAt,
                    Notes = @Notes
                WHERE VaccinationId = @VaccinationId";

                var parameters = new
                {
                    vaccination.VaccinationId,
                    vaccination.LivestockId,
                    vaccination.UserId,
                    vaccination.FarmId,
                    vaccination.VaccineName,
                    vaccination.Manufacturer,
                    vaccination.DateAdministered,
                    vaccination.NextDoseDueDate,
                    vaccination.Dosage,
                    vaccination.AdministeredBy,
                    vaccination.IsCompleted,
                    UpdatedAt = DateTime.UtcNow,
                    vaccination.Notes
                };

                await connection.ExecuteAsync(updateQuery, parameters);
                vaccination.UpdatedAt = parameters.UpdatedAt;
                return vaccination;
            }
        }

        public async Task<bool> DeleteVaccinationAsync(int vaccinationId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var deleteQuery = @"
                DELETE FROM [Vaccination]
                WHERE VaccinationId = @VaccinationId";

                var affectedRows = await connection.ExecuteAsync(deleteQuery, new { VaccinationId = vaccinationId });

                return affectedRows > 0;
            }
        }

        public async Task<int> CountVaccinationsAsync(int? userId = null, int? farmId = null, int? livestockId = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = new StringBuilder(@"
        SELECT COUNT(*)
        FROM [Vaccination]
        WHERE 1 = 1");

                // Add conditions only if values are not null or zero
                if (userId.HasValue && userId.Value != 0)
                {
                    query.Append(" AND UserId = @UserId");
                }

                if (farmId.HasValue && farmId.Value != 0)
                {
                    query.Append(" AND FarmId = @FarmId");
                }

                if (livestockId.HasValue && livestockId.Value != 0)
                {
                    query.Append(" AND LivestockId = @LivestockId");
                }

                // Execute the count query with the appropriate parameters
                var totalRecords = await connection.ExecuteScalarAsync<int>(query.ToString(), new
                {
                    UserId = userId,
                    FarmId = farmId,
                    LivestockId = livestockId       
                });

                return totalRecords;
            }
        }

    }
}
