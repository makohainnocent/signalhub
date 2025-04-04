using Application.Common.Abstractions;
using Dapper;
using Domain.Common.Responses;
using Domain.Tenants;
using Domain.Tenants.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Tenants.Abtstractions
{
    public class TenantsRepository : ITenantsRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public TenantsRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        // Create Tenant
        public async Task<Tenant> CreateTenantAsync(TenantCreationRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    INSERT INTO [Tenants] (Name, Slug, Description, IsActive, OwnerUserId, CreatedAt)
                    VALUES (@Name, @Slug, @Description, @IsActive, @OwnerUserId, @CreatedAt);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var tenantId = await connection.ExecuteScalarAsync<int>(query, new
                {
                    request.Name,
                    request.Slug,
                    request.Description,
                    IsActive = true,
                    request.OwnerUserId,
                    CreatedAt = DateTime.UtcNow
                });

                return new Tenant
                {
                    TenantId = tenantId,
                    Name = request.Name,
                    Slug = request.Slug,
                    Description = request.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    OwnerUserId = request.OwnerUserId
                };
            }
        }

        // Get Tenants with pagination
        public async Task<PagedResultResponse<Tenant>> GetTenantsAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            bool? isActive = null,
            int? ownerUserId = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = @"
                    SELECT * FROM [Tenants]
                    WHERE (@Search IS NULL OR Name LIKE '%' + @Search + '%')
                    AND (@IsActive IS NULL OR IsActive = @IsActive)
                    AND (@OwnerUserId IS NULL OR OwnerUserId = @OwnerUserId)
                    ORDER BY CreatedAt
                    OFFSET @Skip ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*) FROM [Tenants]
                    WHERE (@Search IS NULL OR Name LIKE '%' + @Search + '%')
                    AND (@IsActive IS NULL OR IsActive = @IsActive)
                    AND (@OwnerUserId IS NULL OR OwnerUserId = @OwnerUserId);";

                using (var multi = await connection.QueryMultipleAsync(query, new
                {
                    Search = search,
                    IsActive = isActive,
                    OwnerUserId = ownerUserId,
                    Skip = skip,
                    PageSize = pageSize
                }))
                {
                    var tenants = multi.Read<Tenant>().ToList();
                    var totalCount = multi.ReadSingle<int>();

                    return new PagedResultResponse<Tenant>
                    {
                        Items = tenants,
                        TotalCount = totalCount,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }

        // Get Tenant by ID
        public async Task<Tenant?> GetTenantByIdAsync(int tenantId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT * FROM [Tenants]
                    WHERE TenantId = @TenantId";

                return await connection.QuerySingleOrDefaultAsync<Tenant>(query, new { TenantId = tenantId });
            }
        }

        // Get Tenant by Slug
        public async Task<Tenant?> GetTenantBySlugAsync(string slug)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT * FROM [Tenants]
                    WHERE Slug = @Slug";

                return await connection.QuerySingleOrDefaultAsync<Tenant>(query, new { Slug = slug });
            }
        }

        // Update Tenant
        public async Task<Tenant> UpdateTenantAsync(TenantUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [Tenants]
                    SET Name = @Name,
                        Slug = @Slug,
                        Description = @Description,
                        IsActive = @IsActive,
                        UpdatedAt = @UpdatedAt
                    WHERE TenantId = @TenantId;

                    SELECT * FROM [Tenants] WHERE TenantId = @TenantId;";

                var updatedTenant = await connection.QuerySingleOrDefaultAsync<Tenant>(query, new
                {
                    request.TenantId,
                    request.Name,
                    request.Slug,
                    request.Description,
                    request.IsActive,
                    UpdatedAt = DateTime.UtcNow
                });

                return updatedTenant!;
            }
        }

        // Deactivate Tenant
        public async Task<bool> DeactivateTenantAsync(int tenantId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [Tenants]
                    SET IsActive = 0
                    WHERE TenantId = @TenantId";

                var result = await connection.ExecuteAsync(query, new { TenantId = tenantId });

                return result > 0;
            }
        }

        // Activate Tenant
        public async Task<bool> ActivateTenantAsync(int tenantId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    UPDATE [Tenants]
                    SET IsActive = 1
                    WHERE TenantId = @TenantId";

                var result = await connection.ExecuteAsync(query, new { TenantId = tenantId });

                return result > 0;
            }
        }

        // Delete Tenant
        public async Task<bool> DeleteTenantAsync(int tenantId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    DELETE FROM [Tenants]
                    WHERE TenantId = @TenantId";

                var result = await connection.ExecuteAsync(query, new { TenantId = tenantId });

                return result > 0;
            }
        }

        // Count Tenants
        public async Task<int> CountTenantsAsync()
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT COUNT(*) FROM [Tenants]";

                return await connection.ExecuteScalarAsync<int>(query);
            }
        }
    }
}
