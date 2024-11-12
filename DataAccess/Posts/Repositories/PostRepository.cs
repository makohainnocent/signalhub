using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Domain.Core.Models;
using Domain.Common.Responses;
using DataAccess.Common.Exceptions;
using Application.Common.Abstractions;
using Domain.Post.Requests;
using Application.Posts.Abstractions;

namespace DataAccess.Posts.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public PostRepository(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        // Create a new post
        public async Task<Post> CreatePostAsync(CreatePostRequest request)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var insertQuery = @"
                    INSERT INTO [Post] (UserId, Title, PostType, Files, Description, CreatedAt, UpdatedAt, IsPublished, LikesCount)
                    VALUES (@UserId, @Title, @PostType, @Files, @Description, @CreatedAt, @UpdatedAt, @IsPublished, @LikesCount);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    UserId = request.UserId,
                    Title = request.Title,
                    PostType = request.PostType,
                    Files = request.Files,
                    Description=request.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsPublished = true,
                    LikesCount = 0
                };

                var postId = await connection.QuerySingleAsync<int>(insertQuery, parameters);

                return new Post
                {
                    PostId = postId,
                    UserId = request.UserId,
                    Title = request.Title,
                    Description = request.Description,
                    PostType = request.PostType,
                    Files = request.Files,
                    CreatedAt = parameters.CreatedAt,
                    UpdatedAt = parameters.UpdatedAt,
                    IsPublished = parameters.IsPublished,
                    LikesCount = parameters.LikesCount
                };
            }
        }

        // Get a post by ID
        public async Task<Post?> GetPostByIdAsync(int postId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var query = @"
                    SELECT *
                    FROM [Post]
                    WHERE PostId = @PostId";

                return await connection.QuerySingleOrDefaultAsync<Post>(query, new { PostId = postId });
            }
        }

        // Update a post
        public async Task<Post> UpdatePostAsync(Post post)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var updateQuery = @"
                    UPDATE [Post]
                    SET Title = @Title,
                        PostType = @PostType,
                        Files = @Files,
                        Description = @Description,
                        IsPublished = @IsPublished,
                        UpdatedAt = @UpdatedAt,
                        LikesCount = @LikesCount
                    WHERE PostId = @PostId";

                var parameters = new
                {
                    post.PostId,
                    post.Title,
                    post.PostType,
                    post.Files,
                    post.Description,
                    post.IsPublished,
                    UpdatedAt = DateTime.UtcNow,
                    post.LikesCount
                };

                await connection.ExecuteAsync(updateQuery, parameters);

                post.UpdatedAt = parameters.UpdatedAt;
                return post;
            }
        }

        // Delete a post
        public async Task<bool> DeletePostAsync(int postId)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var checkQuery = "SELECT COUNT(*) FROM [Post] WHERE PostId = @PostId";
                var exists = await connection.QuerySingleAsync<int>(checkQuery, new { PostId = postId });

                if (exists == 0)
                {
                    return false;
                }

                var deleteQuery = "DELETE FROM [Post] WHERE PostId = @PostId";
                await connection.ExecuteAsync(deleteQuery, new { PostId = postId });

                return true;
            }
        }

        public async Task<PagedResultResponse<Post>> GetAllPostsAsync(
    int pageNumber,
    int pageSize,
    string? search = null,
    int? userId = null)
        {
            using (var connection = _dbConnectionProvider.CreateConnection())
            {
                connection.Open();

                var skip = (pageNumber - 1) * pageSize;

                var query = new StringBuilder(@"
    SELECT 
        p.PostId, p.files, p.description, p.title, p.PostType, p.UserId,p.CreatedAt, 
        u.UserId, u.Username, u.ProfilePhoto AS ProfilePhoto, u.CoverPhoto AS CoverPhoto, u.username, u.fullname
    FROM [Post] p
    INNER JOIN [User] u ON p.UserId = u.UserId
    WHERE 1=1");

                if (userId.HasValue&userId!=0)
                {
                    query.Append(" AND p.UserId = @UserId");
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(" AND (p.Title LIKE @Search OR p.Description LIKE @Search)");
                }

                query.Append(@"
            ORDER BY p.CreatedAt DESC
            OFFSET @Skip ROWS
            FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM [Post] p
            WHERE 1=1");

                if (userId.HasValue & userId != 0)
                {
                    query.Append(" AND p.UserId = @UserId");
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query.Append(" AND (p.Title LIKE @Search OR p.Description LIKE @Search)");
                }

                var parameters = new
                {
                    Skip = skip,
                    PageSize = pageSize,
                    Search = $"%{search}%",
                    UserId = userId
                };

                using (var multi = await connection.QueryMultipleAsync(query.ToString(), parameters))
                {
                    var posts = multi.Read<Post, User, Post>((post, user) =>
                    {
                        post.User = user;
                        return post;
                    }, splitOn: "UserId").ToList();

                    var totalRecords = multi.ReadSingle<int>();

                    return new PagedResultResponse<Post>
                    {
                        Items = posts,
                        TotalCount = totalRecords,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }

            }
        }

    }
}
