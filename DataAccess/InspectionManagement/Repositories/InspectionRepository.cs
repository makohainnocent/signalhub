using Dapper;
using Domain.InspectionManagement.Requests;
using Domain.Core.Models;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Abstractions;
using Application.InspectionManagement.Abstractions;

namespace DataAccess.InspectionManagement.Repositories
{
    public class InspectionRepository : IInspectionRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public InspectionRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<Inspection> CreateInspectionAsync(CreateInspectionRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                INSERT INTO [Inspections] 
                    (InspectorId, EntityId, InspectionType, InspectionDate, Status, Comments, InspectionReportPdfBase64, CreatedAt)
                VALUES 
                    (@InspectorId, @EntityId, @InspectionType, @InspectionDate, @Status, @Comments, @InspectionReportPdfBase64, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    request.InspectorId,
                    request.EntityId,
                    request.InspectionType,
                    request.InspectionDate,
                    request.Status,
                    request.Comments,
                    request.InspectionReportPdfBase64,
                    CreatedAt = DateTime.UtcNow
                };

                var inspectionId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                return new Inspection
                {
                    InspectionId = inspectionId,
                    InspectorId = request.InspectorId,
                    EntityId = int.Parse(request.EntityId),
                    InspectionType = request.InspectionType,
                    InspectionDate = request.InspectionDate,
                    Status = request.Status,
                    Comments = request.Comments,
                    InspectionReportPdfBase64 = request.InspectionReportPdfBase64,
                    CreatedAt = parameters.CreatedAt
                };
            }
        }

        public async Task<Inspection?> GetInspectionByIdAsync(int inspectionId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                SELECT *
                FROM [Inspections]
                WHERE InspectionId = @InspectionId";

                return await connection.QuerySingleOrDefaultAsync<Inspection>(query, new { InspectionId = inspectionId });
            }
        }

        public async Task<Inspection> UpdateInspectionAsync(Inspection inspection)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var updateQuery = @"
                UPDATE [Inspections]
                SET 
                    InspectorId = @InspectorId,
                    EntityId = @EntityId,
                    InspectionType = @InspectionType,
                    InspectionDate = @InspectionDate,
                    Status = @Status,
                    Comments = @Comments,
                    InspectionReportPdfBase64 = @InspectionReportPdfBase64,
                    UpdatedAt = @UpdatedAt
                WHERE InspectionId = @InspectionId";

                var parameters = new
                {
                    inspection.InspectionId,
                    inspection.InspectorId,
                    inspection.EntityId,
                    inspection.InspectionType,
                    inspection.InspectionDate,
                    inspection.Status,
                    inspection.Comments,
                    inspection.InspectionReportPdfBase64,
                    UpdatedAt = DateTime.UtcNow
                };

                await connection.ExecuteAsync(updateQuery, parameters);
                inspection.UpdatedAt = parameters.UpdatedAt;
                return inspection;
            }
        }

        public async Task<bool> DeleteInspectionAsync(int inspectionId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var deleteQuery = @"
                DELETE FROM [Inspections]
                WHERE InspectionId = @InspectionId";

                var affectedRows = await connection.ExecuteAsync(deleteQuery, new { InspectionId = inspectionId });

                return affectedRows > 0;
            }
        }

        public async Task<PagedResultResponse<Inspection>> GetAllInspectionsAsync
            (int pageNumber, 
            int pageSize, 
            string? search = null,
            int? userId = 0, 
            int? animalId = 0,
            int? premiseId = 0,
            int? tagId = 0,
            int? productId = 0,
            int? transportId = 0

            )
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
                SELECT *
                FROM [Inspections] i
                WHERE 1 = 1");

                // Search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(" AND (i.Status LIKE @Search OR i.Comments LIKE @Search)");
                }

                // UserId filter
                if (userId.HasValue && userId != 0)
                {
                    query.Append(" AND i.InspectorId = @UserId");
                }

                
                if (animalId.HasValue && animalId.Value != 0)
                {
                    query.Append(" AND i.EntityId = @AnimalId");
                }

                if (productId.HasValue && productId.Value != 0)
                {
                    query.Append(" AND i.EntityId = @ProductId");
                }

                if (tagId.HasValue && tagId.Value != 0)
                {
                    query.Append(" AND i.EntityId = @TagId");
                }

                if (transportId.HasValue && transportId.Value != 0)
                {
                    query.Append(" AND i.EntityId = @TransportId");
                }

                if (premiseId.HasValue && premiseId.Value != 0)
                {
                    query.Append(" AND i.EntityId = @premiseId");
                }


                // Pagination
                query.Append(@"
                ORDER BY i.InspectionDate DESC
                OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

                -- Count total records
                SELECT COUNT(*)
                FROM [Inspections] i
                WHERE 1 = 1");

                // Duplicate conditions for total count query
                // Search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(" AND (i.Status LIKE @Search OR i.Comments LIKE @Search)");
                }

                // UserId filter
                if (userId.HasValue && userId != 0)
                {
                    query.Append(" AND i.InspectorId = @UserId");
                }


                if (animalId.HasValue && animalId.Value != 0)
                {
                    query.Append(" AND i.EntityId = @AnimalId");
                }

                if (productId.HasValue && productId.Value != 0)
                {
                    query.Append(" AND i.EntityId = @ProductId");
                }

                if (tagId.HasValue && tagId.Value != 0)
                {
                    query.Append(" AND i.EntityId = @TagId");
                }

                if (transportId.HasValue && transportId.Value != 0)
                {
                    query.Append(" AND i.EntityId = @TransportId");
                }

                if (premiseId.HasValue && premiseId.Value != 0)
                {
                    query.Append(" AND i.EntityId = @premiseId");
                }

            

                    try
                        {
                            using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                            {
                                Search = $"%{search}%",
                                Skip = skip,
                                PageSize = pageSize,
                                UserId = userId,
                                PremiseId = premiseId?.ToString(),
                                AnimalId = animalId?.ToString(),
                                TagId = tagId?.ToString(),
                                ProductId = productId?.ToString(),
                                TransportId = transportId?.ToString(),

                            }))
                            {
                                var inspections = multi.Read<Inspection>().ToList();
                                var totalRecords = multi.ReadSingle<int>();

                                return new PagedResultResponse<Inspection>
                                {
                                    Items = inspections,
                                    TotalCount = totalRecords,
                                    PageNumber = pageNumber,
                                    PageSize = pageSize
                                };
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log and handle the exception appropriately
                            throw new Exception("An error occurred while retrieving inspections.", ex);
                        }
                    }
        }


      
        public async Task<int> CountInspectionsAsync
            (
            int? userId = 0,
            int? animalId = 0,
            int? premiseId = 0,
            int? tagId = 0,
            int? productId = 0,
            int? transportId = 0

            )
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = new StringBuilder(@"
                SELECT COUNT(*)
                FROM [Inspections] i
                WHERE 1 = 1");

               

                // UserId filter
                if (userId.HasValue && userId != 0)
                {
                    query.Append(" AND i.InspectorId = @UserId");
                }


                if (animalId.HasValue && animalId.Value != 0)
                {
                    query.Append(" AND i.EntityId = @AnimalId");
                }

                if (productId.HasValue && productId.Value != 0)
                {
                    query.Append(" AND i.EntityId = @ProductId");
                }

                if (tagId.HasValue && tagId.Value != 0)
                {
                    query.Append(" AND i.EntityId = @TagId");
                }

                if (transportId.HasValue && transportId.Value != 0)
                {
                    query.Append(" AND i.EntityId = @TransportId");
                }

                if (premiseId.HasValue && premiseId.Value != 0)
                {
                    query.Append(" AND i.EntityId = @premiseId");
                }



                // Execute the count query with the appropriate parameters
                var totalRecords = await connection.ExecuteScalarAsync<int>(query.ToString(), new
                {
                    UserId = userId,
                    PremiseId = premiseId?.ToString(),
                    AnimalId = animalId?.ToString(),
                    TagId = tagId?.ToString(),
                    ProductId = productId?.ToString(),
                    TransportId = transportId?.ToString(),
                });

                return totalRecords;
            }
        }

        public async Task<IEnumerable<object>> GetInspectionsThisWeekAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Get the start and end of the current week
                var startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);

                var query = @"
                    SELECT 
                        DATEPART(WEEKDAY, CreatedAt) AS DayOfWeek,
                        COUNT(*) AS InspectionCount
                    FROM [Inspections]
                    WHERE CreatedAt BETWEEN @StartOfWeek AND @EndOfWeek
                    GROUP BY DATEPART(WEEKDAY, CreatedAt)
                    ORDER BY DayOfWeek";

                var rawResults = await connection.QueryAsync<DayOfWeekInspectionCount>(query, new
                {
                    StartOfWeek = startOfWeek,
                    EndOfWeek = endOfWeek
                });

                // Ensure we have entries for all days of the week, filling in 0 counts if missing
                var weekDays = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>();
                var results = weekDays.Select(day => new
                {
                    Day = day.ToString(), // Day name as string
                    Count = rawResults.FirstOrDefault(r => r.DayOfWeek == ((int)day + 1))?.InspectionCount ?? 0
                });

                return results;
            }
        }

        public async Task<IEnumerable<object>> GetCompliantInspectionsThisWeekAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Get the start and end of the current week
                var startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);

                var query = @"
            SELECT 
                DATEPART(WEEKDAY, CreatedAt) AS DayOfWeek,
                COUNT(*) AS InspectionCount
            FROM [Inspections]
            WHERE CreatedAt BETWEEN @StartOfWeek AND @EndOfWeek
              AND Status = @Status
            GROUP BY DATEPART(WEEKDAY, CreatedAt)
            ORDER BY DayOfWeek";

                var rawResults = await connection.QueryAsync<DayOfWeekInspectionCount>(query, new
                {
                    StartOfWeek = startOfWeek,
                    EndOfWeek = endOfWeek,
                    Status = "Compliant"
                });

                // Ensure we have entries for all days of the week, filling in 0 counts if missing
                var weekDays = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>();
                var results = weekDays.Select(day => new
                {
                    Day = day.ToString(), // Day name as string
                    Count = rawResults.FirstOrDefault(r => r.DayOfWeek == ((int)day + 1))?.InspectionCount ?? 0
                });

                return results;
            }
        }

        public async Task<IEnumerable<object>> GetNonCompliantInspectionsThisWeekAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                // Get the start and end of the current week
                var startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);

                var query = @"
            SELECT 
                DATEPART(WEEKDAY, CreatedAt) AS DayOfWeek,
                COUNT(*) AS InspectionCount
            FROM [Inspections]
            WHERE CreatedAt BETWEEN @StartOfWeek AND @EndOfWeek
              AND Status != @Status
            GROUP BY DATEPART(WEEKDAY, CreatedAt)
            ORDER BY DayOfWeek";

                var rawResults = await connection.QueryAsync<DayOfWeekInspectionCount>(query, new
                {
                    StartOfWeek = startOfWeek,
                    EndOfWeek = endOfWeek,
                    Status = "Compliant"
                });

                // Ensure we have entries for all days of the week, filling in 0 counts if missing
                var weekDays = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>();
                var results = weekDays.Select(day => new
                {
                    Day = day.ToString(), // Day name as string
                    Count = rawResults.FirstOrDefault(r => r.DayOfWeek == ((int)day + 1))?.InspectionCount ?? 0
                });

                return results;
            }
        }







    }
}
