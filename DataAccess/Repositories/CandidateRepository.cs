using Application.Abstractions;
using Domain.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class CandidateRepository : ICandidateRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public CandidateRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        /*public async Task<Candidate> CreateCandidate(Candidate candidate)
        {
            const string selectSql = "SELECT * FROM Candidates WHERE Email = @Email";
            const string insertSql = @"
        INSERT INTO Candidates 
        (FirstName, LastName, Email, PhoneNumber, PreferredCallTime, LinkedInProfileUrl, GitHubProfileUrl, FreeTextComment, DateCreated)
        VALUES 
        (@FirstName, @LastName, @Email, @PhoneNumber, @PreferredCallTime, @LinkedInProfileUrl, @GitHubProfileUrl, @FreeTextComment, @DateCreated);
        SELECT last_insert_rowid();"; 

            candidate.DateCreated = DateTime.UtcNow;
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                var existingCandidate = await connection.QueryFirstOrDefaultAsync<Candidate>(selectSql, new { Email = candidate.Email });

                if (existingCandidate != null)
                {
                    return await UpdateCandidate(candidate, existingCandidate.Id);
                }
                else
                {
                    candidate.DateCreated = DateTime.UtcNow;
                    var newCandidateId = await connection.ExecuteScalarAsync<int>(insertSql, candidate);
                    candidate.Id = newCandidateId;
                    return candidate;
                }
            }
        }




        public async Task DeleteCandidate(int id)
        {
            const string sql = "DELETE FROM Candidates WHERE Id = @Id";

            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                await connection.ExecuteAsync(sql, new { Id = id });
            }
        }

        public async Task<Candidate> GetCandidate(int id)
        {
           
            const string sql = "SELECT * FROM Candidates WHERE Id = @Id";

            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Candidate>(sql, new { Id = id });
            }
        }

        public async Task<ICollection<Candidate>> GetCandidates()
        {
            const string sql = "SELECT * FROM Candidates";

            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                var candidates = await connection.QueryAsync<Candidate>(sql);
                return candidates.ToList();
            }
        }

        public async Task<Candidate> UpdateCandidate(Candidate candidate, int id)
        {
           
            const string sql = @"
        UPDATE Candidates 
        SET FirstName = @FirstName, 
            LastName = @LastName, 
            Email = @Email, 
            PhoneNumber = @PhoneNumber, 
            PreferredCallTime = @PreferredCallTime, 
            LinkedInProfileUrl = @LinkedInProfileUrl, 
            GitHubProfileUrl = @GitHubProfileUrl, 
            FreeTextComment = @FreeTextComment,
            LastUpdated   = @LastUpdated
        WHERE Id = @Id";

           
            candidate.Id = id;
            candidate.LastUpdated = DateTime.UtcNow;

            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                await connection.ExecuteAsync(sql, candidate);
            }

            
            return await GetCandidate(id);
        }*/

    }
}
