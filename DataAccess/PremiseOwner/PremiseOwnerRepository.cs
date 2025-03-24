using Dapper;
using Domain.Core.Models;
using Domain.PremiseOwner.PremiseOwnerCreateRequest;
using Domain.PremiseOwner.Requests;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Abstractions;
using Domain.Common.Responses;
using Application.PremiseOwners.Abstraction;

namespace DataAccess.PremiseOwners
{
    public class PremiseOwnerRepository: IPremiseOwnerRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public PremiseOwnerRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<PremiseOwner> CreatePremiseOwnerAsync(PremiseOwnerCreateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                INSERT INTO [PremiseOwners] 
                    (RegisterdById, Province, District, VillageOrAddress, Names, Surname, OtherNames, Sex, NRC, PhoneNumber, Email, 
                     ArtificialPersonName, ContactPersonName, ContactPersonID, ContactPersonPhoneNumber, ContactPersonEmail, CreatedAt,AgentId)
                VALUES 
                    (@RegisterdById, @Province, @District, @VillageOrAddress, @Names, @Surname, @OtherNames, @Sex, @NRC, @PhoneNumber, @Email, 
                     @ArtificialPersonName, @ContactPersonName, @ContactPersonID, @ContactPersonPhoneNumber, @ContactPersonEmail, @CreatedAt, @AgentId);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    request.RegisterdById,
                    request.Province,
                    request.District,
                    request.VillageOrAddress,
                    request.Names,
                    request.Surname,
                    request.OtherNames,
                    request.Sex,
                    request.NRC,
                    request.PhoneNumber,
                    request.Email,
                    request.ArtificialPersonName,
                    request.ContactPersonName,
                    request.ContactPersonID,
                    request.ContactPersonPhoneNumber,
                    request.ContactPersonEmail,
                    CreatedAt = DateTime.UtcNow,
                    request.AgentId
                };

                var premiseOwnerId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                return new PremiseOwner
                {
                    Id = premiseOwnerId,
                    RegisterdById = request.RegisterdById,
                    Province = request.Province,
                    District = request.District,
                    VillageOrAddress = request.VillageOrAddress,
                    Names = request.Names,
                    Surname = request.Surname,
                    OtherNames = request.OtherNames,
                    Sex = request.Sex,
                    NRC = request.NRC,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    ArtificialPersonName = request.ArtificialPersonName,
                    ContactPersonName = request.ContactPersonName,
                    ContactPersonID = request.ContactPersonID,
                    ContactPersonPhoneNumber = request.ContactPersonPhoneNumber,
                    ContactPersonEmail = request.ContactPersonEmail,
                    CreatedAt = parameters.CreatedAt
                };
            }
        }

        public async Task<PagedResultResponse<PremiseOwner>> GetPremiseOwnersAsync(int pageNumber, int pageSize, string? search = null, int? registerdBy = 0)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
            SELECT *
            FROM [PremiseOwners]
            WHERE 1=1");

                if (registerdBy.HasValue && registerdBy.Value != 0)
                {
                    query.Append("AND AgentId = @RegisterdBy");
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (Names LIKE @Search
                OR Surname LIKE @Search
                OR PhoneNumber LIKE @Search
                OR Email LIKE @Search
                OR NRC LIKE @Search)");
                }

                query.Append(@"
            ORDER BY CreatedAt DESC
            OFFSET @Skip ROWS
            FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM [PremiseOwners]
            WHERE 1=1");

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(@"
                AND (Names LIKE @Search
                OR Surname LIKE @Search
                OR PhoneNumber LIKE @Search
                OR Email LIKE @Search
                OR NRC LIKE @Search)");
                }

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%",
                    RegisterdBy=registerdBy
                }))
                {
                    var owners = multi.Read<PremiseOwner>().ToList();
                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<PremiseOwner>
                    {
                        Items = owners,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
        }


        public async Task<PremiseOwner?> GetPremiseOwnerByIdAsync(int premiseOwnerId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"SELECT TOP 1 * FROM [PremiseOwners] WHERE RegisterdById = @PremiseOwnerId ORDER BY CreatedAt DESC ";
                return await connection.QuerySingleOrDefaultAsync<PremiseOwner>(query, new { PremiseOwnerId = premiseOwnerId });
            }
        }

        public async Task<PremiseOwner> UpdatePremiseOwnerAsync(int premiseOwnerId, PremiseOwnerUpdateRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var updateQuery = @"
                UPDATE [PremiseOwners]
                SET 
                    RegisterdById = @RegisterdById,
                    Province = @Province,
                    District = @District,
                    VillageOrAddress = @VillageOrAddress,
                    Names = @Names,
                    Surname = @Surname,
                    OtherNames = @OtherNames,
                    Sex = @Sex,
                    NRC = @NRC,
                    PhoneNumber = @PhoneNumber,
                    Email = @Email,
                    ArtificialPersonName = @ArtificialPersonName,
                    ContactPersonName = @ContactPersonName,
                    ContactPersonID = @ContactPersonID,
                    ContactPersonPhoneNumber = @ContactPersonPhoneNumber,
                    ContactPersonEmail = @ContactPersonEmail,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @PremiseOwnerId";

                var parameters = new
                {
                    PremiseOwnerId = premiseOwnerId,
                    request.RegisterdById,
                    request.Province,
                    request.District,
                    request.VillageOrAddress,
                    request.Names,
                    request.Surname,
                    request.OtherNames,
                    request.Sex,
                    request.NRC,
                    request.PhoneNumber,
                    request.Email,
                    request.ArtificialPersonName,
                    request.ContactPersonName,
                    request.ContactPersonID,
                    request.ContactPersonPhoneNumber,
                    request.ContactPersonEmail,
                    UpdatedAt = DateTime.UtcNow
                };

                await connection.ExecuteAsync(updateQuery, parameters);
                return await GetPremiseOwnerByIdAsync(premiseOwnerId);
            }
        }

        public async Task<bool> DeletePremiseOwnerAsync(int premiseOwnerId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();
                var deleteQuery = "DELETE FROM [PremiseOwners] WHERE Id = @PremiseOwnerId";
                var affectedRows = await connection.ExecuteAsync(deleteQuery, new { PremiseOwnerId = premiseOwnerId });
                return affectedRows > 0;
            }
        }
    }
}
